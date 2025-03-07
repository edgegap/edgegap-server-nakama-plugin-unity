using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Edgegap.NakamaServersPlugin
{
    using L = Logger;

    public class Api<IM>
        where IM : InstanceBaseMetadata
    {
        internal MonoBehaviour Parent;
        internal string BaseUrl;
        internal string AuthToken;

        internal int RequestTimeoutSeconds;
        internal Func<float> BackoffSeconds = () => 1 + (0.1f * Random.value);

        internal string PATH_CONNECTION_EVENT = "connection";
        internal string PATH_INSTANCE_EVENT = "instance";

        public Api(
            MonoBehaviour parent,
            string authToken,
            string baseUrl,
            int requestTimeoutSeconds = 3
        )
        {
            AuthToken = authToken;
            BaseUrl = baseUrl;
        }

        public Api(
            MonoBehaviour parent,
            string authToken,
            string baseUrl,
            Func<float> backoffSeconds,
            int requestTimeoutSeconds = 3
        )
        {
            AuthToken = authToken;
            BaseUrl = baseUrl;
            RequestTimeoutSeconds = requestTimeoutSeconds;
            BackoffSeconds = backoffSeconds;
        }

        public void UpdateInstance(
            InstanceEventDTO<IM> instanceEvent,
            Action<InstanceEventResponseDTO, UnityWebRequest> onSuccessDelegate,
            Action<string, UnityWebRequest> onErrorDelegate
        )
        {
            Post(
                $"{BaseUrl}/{PATH_INSTANCE_EVENT}",
                JsonConvert.SerializeObject(instanceEvent),
                (string response, UnityWebRequest request) =>
                {
                    try
                    {
                        InstanceEventResponseDTO result =
                            JsonConvert.DeserializeObject<InstanceEventResponseDTO>(response);
                        onSuccessDelegate(result, request);
                    }
                    catch (Exception e)
                    {
                        L._Error($"Couldn't parse Nakama Instance Event response. {e.Message}");
                        throw;
                    }
                },
                onErrorDelegate
            );
        }

        public void UpdateConnections(
            ConnectionEventDTO connectionEvent,
            Action<ConnectionEventResponseDTO, UnityWebRequest> onSuccessDelegate,
            Action<string, UnityWebRequest> onErrorDelegate
        )
        {
            Post(
                $"{BaseUrl}/{PATH_CONNECTION_EVENT}",
                JsonConvert.SerializeObject(connectionEvent),
                (string response, UnityWebRequest request) =>
                {
                    try
                    {
                        ConnectionEventResponseDTO result =
                            JsonConvert.DeserializeObject<ConnectionEventResponseDTO>(response);
                        onSuccessDelegate(result, request);
                    }
                    catch (Exception e)
                    {
                        L._Error($"Couldn't parse Nakama Connection Event response. {e.Message}");
                        throw;
                    }
                },
                onErrorDelegate
            );
        }

        #region WebGL-friendly WebRequest
        internal void Post(
            string Url,
            string requestBody,
            Action<string, UnityWebRequest> onSuccessDelegate,
            Action<string, UnityWebRequest> onErrorDelegate
        )
        {
#if UNITY_2022_3_OR_NEWER
            UnityWebRequest request = UnityWebRequest.Post(Url, requestBody, "application/json");
#else
            UnityWebRequest request = UnityWebRequest.Post(Url, requestBody);
#endif
            request.SetRequestHeader("Authorization", AuthToken);
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = RequestTimeoutSeconds;

            Parent.StartCoroutine(
                _SendRequestEnumerator(request, onSuccessDelegate, onErrorDelegate)
            );
        }

        internal IEnumerator _SendRequestEnumerator(
            UnityWebRequest request,
            Action<string, UnityWebRequest> onSuccessDelegate,
            Action<string, UnityWebRequest> onErrorDelegate
        )
        {
            yield return new WaitForSeconds(BackoffSeconds());

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onErrorDelegate($"HTTP {request.responseCode}: {request.error}", request);
            }
            else
            {
                string stringResponse =
                    request.downloadHandler == null ? "" : request.downloadHandler.text;
                onSuccessDelegate(stringResponse, request);
            }
        }
        #endregion
    }
}
