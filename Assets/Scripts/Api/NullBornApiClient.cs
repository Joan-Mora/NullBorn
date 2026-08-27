using System;
using System.Collections;
using System.Collections.Generic;
using NullBorn.Persistence;
using UnityEngine;
using UnityEngine.Networking;

namespace NullBorn.Api
{
    /// <summary>
    /// Cliente REST stub. No se usa por defecto; apunta a baseUrl cuando el backend exista.
    /// </summary>
    public class NullBornApiClient : MonoBehaviour, INullBornApi
    {
        [SerializeField] private string baseUrl = "http://localhost:8080/api";
        private readonly MockNullBornApi fallback = new MockNullBornApi();
        private bool useFallback = true;

        private void Awake()
        {
            NullBornApi.Current = this;
        }

        public string StartSession(int threadPriority) => fallback.StartSession(threadPriority);

        public void EndSession(string sessionId, float elapsedSeconds, bool victory)
        {
            fallback.EndSession(sessionId, elapsedSeconds, victory);
            if (!useFallback) StartCoroutine(PostJson($"{baseUrl}/sesiones/{sessionId}/cierre", $"{{\"elapsed\":{elapsedSeconds},\"victory\":{victory.ToString().ToLowerInvariant()}}}"));
        }

        public void RecordGcEvent(string kind, int fragmentsCleared, float damage)
        {
            fallback.RecordGcEvent(kind, fragmentsCleared, damage);
            if (!useFallback)
            {
                StartCoroutine(PostJson($"{baseUrl}/eventos-gc",
                    $"{{\"kind\":\"{kind}\",\"fragments\":{fragmentsCleared},\"damage\":{damage}}}"));
            }
        }

        public void SyncProgress(SaveData data)
        {
            fallback.SyncProgress(data);
            if (!useFallback) StartCoroutine(PostJson($"{baseUrl}/progreso", JsonUtility.ToJson(data)));
        }

        public IReadOnlyList<ApiSessionDto> GetSessions() => fallback.GetSessions();
        public IReadOnlyList<ApiGcEventDto> GetGcEvents() => fallback.GetGcEvents();

        private IEnumerator PostJson(string url, string json)
        {
            using UnityWebRequest req = new UnityWebRequest(url, "POST");
            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[API] {url} falló ({req.error}) — se conserva mock local.");
                useFallback = true;
            }
        }
    }
}
