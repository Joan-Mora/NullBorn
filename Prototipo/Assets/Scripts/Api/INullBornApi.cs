using System;
using System.Collections.Generic;
using UnityEngine;

namespace NullBorn.Api
{
    [Serializable]
    public class ApiSessionDto
    {
        public string sessionId;
        public string startedAt;
        public string endedAt;
        public int threadPriority;
        public float elapsedSeconds;
    }

    [Serializable]
    public class ApiGcEventDto
    {
        public string id;
        public string kind;
        public int fragmentsCleared;
        public float damage;
        public string at;
    }

    public interface INullBornApi
    {
        string StartSession(int threadPriority);
        void EndSession(string sessionId, float elapsedSeconds, bool victory);
        void RecordGcEvent(string kind, int fragmentsCleared, float damage);
        void SyncProgress(NullBorn.Persistence.SaveData data);
        IReadOnlyList<ApiSessionDto> GetSessions();
        IReadOnlyList<ApiGcEventDto> GetGcEvents();
    }

    /// <summary>
    /// Punto de acceso. Por defecto usa mock in-process (RF-17–20 parcial).
    /// Sustituir Current por NullBornApiClient cuando exista backend.
    /// </summary>
    public static class NullBornApi
    {
        public static INullBornApi Current { get; set; } = new MockNullBornApi();
    }
}
