using System;
using UnityEngine;

namespace NullBorn.Core
{
    /// <summary>
    /// Estado global de la partida del mini-demo.
    /// La victoria la decide DemoFlowController (boss), no cualquier leak.
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public enum State
        {
            Playing,
            Victory,
            Defeat
        }

        public static GameSession Instance { get; private set; }

        private SystemIntegrity integrity;
        private float elapsed;
        private State state = State.Playing;

        public State CurrentState => state;
        public float ElapsedSeconds => elapsed;
        public event Action<State> StateChanged;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Bind(SystemIntegrity systemIntegrity)
        {
            integrity = systemIntegrity;
        }

        private void Update()
        {
            if (state != State.Playing) return;

            elapsed += Time.deltaTime;

            if (integrity != null && integrity.IsCollapsed)
            {
                SetState(State.Defeat);
            }
        }

        public void NotifyVictory()
        {
            if (state != State.Playing) return;
            SetState(State.Victory);
        }

        /// <summary>Compat: leaks normales ya no ganan la partida solos.</summary>
        public void NotifyLeakCleared()
        {
            // Intentional no-op for basic leaks. Boss calls NotifyVictory via DemoFlow.
        }

        public void ForceDefeat()
        {
            SetState(State.Defeat);
        }

        private void SetState(State next)
        {
            if (state == next) return;
            state = next;
            Time.timeScale = next == State.Playing ? 1f : (next == State.Victory ? 0.35f : 0.2f);
            StateChanged?.Invoke(state);
        }

        public void ResetForCheckpoint()
        {
            // No cambia estado; solo utilidad de tiempo si se necesita.
        }
    }
}
