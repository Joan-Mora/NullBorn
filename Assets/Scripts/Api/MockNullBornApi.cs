using System;
using System.Collections.Generic;
using NullBorn.Persistence;
using UnityEngine;

namespace NullBorn.Api
{
    /// <summary>
    /// Mock in-memory. No red, no PostgreSQL. Documenta el contrato REST.
    /// </summary>
    public class MockNullBornApi : INullBornApi
    {
        private readonly List<ApiSessionDto> sessions = new List<ApiSessionDto>();
        private readonly List<ApiGcEventDto> events = new List<ApiGcEventDto>();
        private string activeId;

        public string StartSession(int threadPriority)
        {
            activeId = Guid.NewGuid().ToString("N").Substring(0, 8);
            sessions.Add(new ApiSessionDto
            {
                sessionId = activeId,
                startedAt = DateTime.UtcNow.ToString("o"),
                threadPriority = threadPriority
            });
            Debug.Log($"[API mock] POST /sesiones → {activeId}");
            return activeId;
        }

        public void EndSession(string sessionId, float elapsedSeconds, bool victory)
        {
            ApiSessionDto found = sessions.Find(s => s.sessionId == sessionId);
            if (found != null)
            {
                found.endedAt = DateTime.UtcNow.ToString("o");
                found.elapsedSeconds = elapsedSeconds;
            }

            Debug.Log($"[API mock] POST /sesiones/{sessionId}/cierre victory={victory}");
        }

        public void RecordGcEvent(string kind, int fragmentsCleared, float damage)
        {
            events.Add(new ApiGcEventDto
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 8),
                kind = kind,
                fragmentsCleared = fragmentsCleared,
                damage = damage,
                at = DateTime.UtcNow.ToString("o")
            });
        }

        public void SyncProgress(SaveData data)
        {
            Debug.Log("[API mock] PUT /progreso (local only — sin PostgreSQL)");
        }

        public IReadOnlyList<ApiSessionDto> GetSessions() => sessions;
        public IReadOnlyList<ApiGcEventDto> GetGcEvents() => events;
    }
}
