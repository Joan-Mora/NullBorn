using UnityEngine;
using UnityEngine.InputSystem;
using NullBorn.Game;

namespace NullBorn.Game.Map
{
    /// <summary>
    /// Toggles fullscreen tactical map with M. Keeps corner minimap visible otherwise.
    /// </summary>
    public class MapHudController : MonoBehaviour
    {
        [SerializeField] RectTransform minimapRoot;
        [SerializeField] RectTransform fullMapRoot;
        [SerializeField] CanvasGroup fullMapGroup;

        bool fullOpen;

        public bool IsFullMapOpen => fullOpen;

        public void Wire(RectTransform mini, RectTransform full, CanvasGroup fullGroup)
        {
            minimapRoot = mini;
            fullMapRoot = full;
            fullMapGroup = fullGroup;
            SetFullMap(false);
        }

        void Update()
        {
            if (PauseMenuController.IsPaused) return;

            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.mKey.wasPressedThisFrame)
                SetFullMap(!fullOpen);
            if (fullOpen && kb.escapeKey.wasPressedThisFrame)
                SetFullMap(false);
        }

        void SetFullMap(bool open)
        {
            fullOpen = open;
            if (fullMapRoot != null)
                fullMapRoot.gameObject.SetActive(open);
            if (fullMapGroup != null)
            {
                fullMapGroup.alpha = open ? 1f : 0f;
                fullMapGroup.blocksRaycasts = open;
                fullMapGroup.interactable = open;
            }

            if (minimapRoot != null)
                minimapRoot.gameObject.SetActive(!open);
        }
    }
}
