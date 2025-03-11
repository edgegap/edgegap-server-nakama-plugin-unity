using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Edgegap.NakamaServersPlugin
{
    using L = Logger;

    public class ServerAgent<IM>
        where IM : InstanceBaseMetadata
    {
        private Api<IM> NakamaApi;

        public ServerHandler<IM> Handler;

        #region ServerAgent Configuration
        public string AuthToken { private get; set; }
        internal string UrlConnectionEvent { get; }
        internal string UrlInstanceEvent { get; }
        public IM InstanceMetadata { get; }

        public double ConnectionUpdateFrequencySeconds;
        public int RequestTimeoutSeconds;
        #endregion

        #region ServerAgent State
        public List<string> UserIDs { get; private set; }
        internal bool InstanceReady;
        internal double UpdatedAt;
        internal bool PendingConnectionsUpdate;
        internal bool OngoingConnectionsUpdate;

        #endregion

        public ServerAgent(
            ServerHandler<IM> handler,
            string authToken,
            string urlConnectionEvent = null,
            string urlInstanceEvent = null,
            IM instanceMetadata = null,
            double connectionUpdateFrequencySeconds = 1,
            int requestTimeoutSeconds = 5
        )
        {
            if (handler is null)
            {
                throw new Exception("Handler not assigned.");
            }

            Handler = handler;
            AuthToken = authToken;
            UrlConnectionEvent =
                urlConnectionEvent ?? GetEnvVariable("NAKAMA_CONNECTION_EVENT_URL");
            UrlInstanceEvent = urlInstanceEvent ?? GetEnvVariable("NAKAMA_INSTANCE_EVENT_URL");

            string metadata = GetEnvVariable("NAKAMA_INSTANCE_METADATA", false);

            InstanceMetadata =
                instanceMetadata
                ?? (
                    string.IsNullOrEmpty(metadata)
                        ? (IM)(new InstanceBaseMetadata())
                        : JsonConvert.DeserializeObject<IM>(
                            GetEnvVariable("NAKAMA_INSTANCE_METADATA")
                        )
                );
            if (InstanceMetadata is null)
            {
                L._Warn("Starting with null (missing) Instance Metadata.");
            }
            else
            {
                L._Log($"Starting with Instance Metadata '{InstanceMetadata}'");
            }

            ConnectionUpdateFrequencySeconds = connectionUpdateFrequencySeconds;
            RequestTimeoutSeconds = requestTimeoutSeconds;

            UserIDs = new List<string>();
            InstanceReady = false;
            UpdatedAt = Time.fixedUnscaledTimeAsDouble;
            PendingConnectionsUpdate = false;
            OngoingConnectionsUpdate = false;
        }

        public string GetEnvVariable(string name, bool throwOnEmpty = true)
        {
            string value = Environment.GetEnvironmentVariable(name);
            if (throwOnEmpty && string.IsNullOrEmpty(value))
            {
                throw new Exception($"Required environment variable '{name}' not initialized.");
            }
            return value;
        }

        public void Initialize()
        {
            NakamaApi = new Api<IM>(Handler, AuthToken, UrlConnectionEvent, UrlInstanceEvent);

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
                    L._Log($"Instance Event processed by Nakama. '{instanceEvent}'");
                    Handler.OnInstanceEvent(instanceEvent, res);
                },
                (string err, UnityWebRequest req) =>
                {
                    L._Error($"Couldn't send Instance Event to Nakama. '{err}'");
                    Handler.OnInstanceEvent(instanceEvent, null, err);
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
                    Handler.OnInstanceEvent(instanceEvent, res);
                },
                (string err, UnityWebRequest req) =>
                {
                    L._Error($"Couldn't send Instance Event to Nakama. '{err}'");
                    Handler.OnInstanceEvent(instanceEvent, null, err);
                }
            );
        }

        public void FixedUpdate()
        {
            if (
                !PendingConnectionsUpdate
                || OngoingConnectionsUpdate
                || Time.fixedUnscaledTimeAsDouble - UpdatedAt < ConnectionUpdateFrequencySeconds
            )
            {
                return;
            }
            PendingConnectionsUpdate = false;
            OngoingConnectionsUpdate = true;

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
                    OngoingConnectionsUpdate = false;
                },
                (string err, UnityWebRequest req) =>
                {
                    L._Error($"Couldn't send Connection Event to Nakama. '{err}'");
                }
            );
        }
        #endregion
    }

    public abstract class ServerHandler<IM> : MonoBehaviour
    {
        public abstract void OnInstanceEvent(
            InstanceEventDTO<IM> payload,
            InstanceEventResponseDTO response,
            string error = null
        );
    }
}
