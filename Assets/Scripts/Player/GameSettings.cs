using System;
using UnityEngine;

namespace NullBorn.Game
{
    public enum ControlScheme
    {
        /// <summary>Mouse aims camera; A/D turn the character body only.</summary>
        WithMouse = 0,
        /// <summary>No mouse aim; A/D turn the camera (OTS locked).</summary>
        KeyboardOnly = 1
    }

    public static class GameSettings
    {
        const string MouseSensKey = "nullborn.mouseSensitivity";
        const string SchemeKey = "nullborn.controlScheme";

        static bool loaded;
        static float mouseSensitivity = 1f;
        static ControlScheme scheme = ControlScheme.WithMouse;

        public static float MouseSensitivity
        {
            get
            {
                EnsureLoaded();
                return mouseSensitivity;
            }
            set
            {
                EnsureLoaded();
                mouseSensitivity = Mathf.Clamp(value, 0.1f, 2f);
                PlayerPrefs.SetFloat(MouseSensKey, mouseSensitivity);
                Changed?.Invoke();
            }
        }

        public static ControlScheme Scheme
        {
            get
            {
                EnsureLoaded();
                return scheme;
            }
            set
            {
                EnsureLoaded();
                scheme = value;
                PlayerPrefs.SetInt(SchemeKey, (int)scheme);
                Changed?.Invoke();
            }
        }

        public static bool UsesMouse
        {
            get
            {
                EnsureLoaded();
                return scheme == ControlScheme.WithMouse;
            }
        }

        public static event Action Changed;

        public static void Save()
        {
            EnsureLoaded();
            PlayerPrefs.SetFloat(MouseSensKey, mouseSensitivity);
            PlayerPrefs.SetInt(SchemeKey, (int)scheme);
            PlayerPrefs.Save();
        }

        static void EnsureLoaded()
        {
            if (loaded) return;
            mouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(MouseSensKey, 1f), 0.1f, 2f);
            scheme = (ControlScheme)PlayerPrefs.GetInt(SchemeKey, (int)ControlScheme.WithMouse);
            loaded = true;
        }
    }
}
