using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Edgegap.NakamaServersPlugin
{
    using L = Logger;

    public class ServerAgent<IM>
        where IM : InstanceBaseMetadata
    {
        private Api<IM> NakamaApi;

        public MonoBehaviour Handler;

        #region ServerAgent Configuration
        // BaseUrl may only be set with constructor
        public string BaseUrl { get; }
        public string AuthToken { private get; set; }

        public double ConnectionUpdateFrequencySeconds;
        public int RequestTimeoutSeconds;

        public bool LogInstanceEvents;
        public bool LogConnectionEvents;
        #endregion

        #region ServerAgent State
        public IM InstanceMetadata;
        public List<string> UserIDs { get; private set; }
        internal bool InstanceReady;
        internal double UpdatedAt;
        internal bool PendingConnectionsUpdate;
        internal bool OngoingConncectionsUpdate;

        #endregion

        public ServerAgent(
            MonoBehaviour handler,
            string baseUrl,
            string authToken,
            double connectionUpdateFrequencySeconds = 1,
            int requestTimeoutSeconds = 5,
            bool logInstanceEvents = true,
            bool logConnectionEvents = false,
            IM instanceMetadata = null
        )
        {
            if (handler is null)
            {
                throw new Exception("Handler not assigned.");
            }

            Handler = handler;
            BaseUrl = baseUrl;
            AuthToken = authToken;
            ConnectionUpdateFrequencySeconds = connectionUpdateFrequencySeconds;
            RequestTimeoutSeconds = requestTimeoutSeconds;
            LogInstanceEvents = logInstanceEvents;
            LogConnectionEvents = logConnectionEvents;

            InstanceMetadata = (IM)(
                instanceMetadata is null ? new InstanceBaseMetadata() : instanceMetadata
            );

            L._Log($"Initialized with Instance Metadata '{InstanceMetadata}'");

            UserIDs = new List<string>();
            InstanceReady = false;
            UpdatedAt = Time.fixedUnscaledTimeAsDouble;
            PendingConnectionsUpdate = false;
            OngoingConncectionsUpdate = false;
        }

        public void Initialize()
        {
            if (string.IsNullOrEmpty(BaseUrl.Trim()))
            {
                throw new Exception("BaseUrl not declared.");
            }

            NakamaApi = new Api<IM>(Handler, AuthToken, BaseUrl);

            InstanceEventDTO<IM> instanceEvent = new InstanceEventDTO<IM>(
                InstanceMetadata.DeploymentID,
                "READY",
                "deployment is ready",
                InstanceMetadata
            );
            L._Log($"Sending Instance Event to Nakama. '{instanceEvent}'");
            NakamaApi.UpdateInstance(
                instanceEvent,
                (InstanceEventResponseDTO res, UnityWebRequest req) =>
                {
                    InstanceReady = true;
                },
                (string err, UnityWebRequest req) =>
                {
                    L._Error($"Couldn't send Instance Event to Nakama. '{err}'");
                }
            );
        }

        #region Nakama Edgegap API
        public void AddUser(string userID)
        {
            if (UserIDs.Contains(userID))
            {
                return;
            }
            UserIDs.Add(userID);
            PendingConnectionsUpdate = true;
            L._Log($"Added User. '{userID}'");
        }

        public void RemoveUser(string userID)
        {
            if (!UserIDs.Contains(userID))
            {
                return;
            }
            UserIDs.RemoveAll((string id) => id == userID);
            PendingConnectionsUpdate = true;
            L._Log($"Removed User. '{userID}'");
        }

        public void StopInstance(string message = "deployment stopped", string err = null)
        {
            InstanceEventDTO<IM> instanceEvent = new InstanceEventDTO<IM>(
                InstanceMetadata.DeploymentID,
                string.IsNullOrEmpty(err) ? "STOP" : "ERROR",
                message,
                InstanceMetadata
            );
            L._Log($"Sending Instance Event to Nakama. {instanceEvent}");
            NakamaApi.UpdateInstance(
                instanceEvent,
                (InstanceEventResponseDTO res, UnityWebRequest req) =>
                {
                    L._Log($"Instance Event processed by Nakama. '{instanceEvent}'");
                },
                (string err, UnityWebRequest req) =>
                {
                    L._Error($"Couldn't send Instance Event to Nakama. '{err}'");
                }
            );
        }

        public void FixedUpdate()
        {
            if (
                !PendingConnectionsUpdate
                || OngoingConncectionsUpdate
                || Time.fixedUnscaledTimeAsDouble - UpdatedAt < ConnectionUpdateFrequencySeconds
            )
            {
                return;
            }

            ConnectionEventDTO connectionEvent = new ConnectionEventDTO(
                InstanceMetadata.DeploymentID,
                UserIDs
            );
            L._Log($"Sending Connection Event to Nakama. '{connectionEvent}'");
            NakamaApi.UpdateConnections(
                connectionEvent,
                (ConnectionEventResponseDTO res, UnityWebRequest req) =>
                {
                    L._Log($"Connection Event processed by Nakama. '{connectionEvent}'");
                },
                (string err, UnityWebRequest req) =>
                {
                    L._Error($"Couldn't send Connection Event to Nakama. '{err}'");
                }
            );
        }
        #endregion
    }
}
