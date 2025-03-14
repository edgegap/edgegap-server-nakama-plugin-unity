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
        internal string UrlConnectionEvent;
        internal string UrlInstanceEvent;

        internal int RequestTimeoutSeconds;
        internal Func<float> BackoffSeconds = () => 1 + (0.1f * Random.value);

        public Api(
            MonoBehaviour parent,
            string urlConnectionEvent,
            string urlInstanceEvent,
            int requestTimeoutSeconds = 3
        )
        {
            Parent = parent;
            UrlConnectionEvent = urlConnectionEvent;
            UrlInstanceEvent = urlInstanceEvent;
            RequestTimeoutSeconds = requestTimeoutSeconds;
        }

        public Api(
            MonoBehaviour parent,
            string urlConnectionEvent,
            string urlInstanceEvent,
            Func<float> backoffSeconds,
            int requestTimeoutSeconds = 3
        )
        {
            Parent = parent;
            UrlConnectionEvent = urlConnectionEvent;
            UrlInstanceEvent = urlInstanceEvent;
            RequestTimeoutSeconds = requestTimeoutSeconds;
            BackoffSeconds = backoffSeconds;
        }

        public void UpdateInstance(
            InstanceEventDTO<IM> instanceEvent,
            Action<string, UnityWebRequest> onSuccessDelegate,
            Action<string, UnityWebRequest> onErrorDelegate
        )
        {
            Post(
                UrlInstanceEvent,
                JsonConvert.SerializeObject(instanceEvent),
                (string response, UnityWebRequest request) =>
                {
                    onSuccessDelegate(response, request);
                },
                onErrorDelegate
            );
        }

        public void UpdateConnections(
            ConnectionEventDTO connectionEvent,
            Action<string, UnityWebRequest> onSuccessDelegate,
            Action<string, UnityWebRequest> onErrorDelegate
        )
        {
            Post(
                UrlConnectionEvent,
                JsonConvert.SerializeObject(connectionEvent),
                (string response, UnityWebRequest request) =>
                {
                    onSuccessDelegate(response, request);
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
