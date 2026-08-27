using System.Collections.Generic;
using NullBorn.UI;
using UnityEngine;

namespace NullBorn.World
{
    /// <summary>
    /// Región activa, fog/ambient y terminales de fast travel desbloqueados.
    /// </summary>
    public class WorldMapController : MonoBehaviour
    {
        public static WorldMapController Instance { get; private set; }

        private readonly HashSet<string> unlocked = new HashSet<string>();
        private readonly Dictionary<string, Vector3> terminals = new Dictionary<string, Vector3>();

        public RegionId CurrentRegionId { get; private set; } = RegionId.RuntimePlaza;
        public string CurrentRegionName { get; private set; } = "Runtime Plaza";
        public string[] UnlockedTerminalIds
        {
            get
            {
                string[] arr = new string[unlocked.Count];
                unlocked.CopyTo(arr);
                return arr;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void EnterRegion(RegionDefinition def)
        {
            if (def == null) return;
            CurrentRegionId = def.id;
            CurrentRegionName = def.displayName;
            RenderSettings.ambientLight = def.ambientColor;
            RenderSettings.fogColor = def.fogColor;
            RenderSettings.fogDensity = def.fogDensity;
            FindAnyObjectByType<CanvasHUD>()?.SetRegion(def.displayName);
        }

        public void RegisterTerminal(string id, Vector3 position)
        {
            terminals[id] = position;
        }

        public void UnlockTerminal(string id)
        {
            unlocked.Add(id);
        }

        public bool IsUnlocked(string id) => unlocked.Contains(id);

        public bool TryGetTerminal(string id, out Vector3 pos) => terminals.TryGetValue(id, out pos);

        public IEnumerable<string> KnownTerminals => terminals.Keys;

        public void RestoreUnlocks(string[] ids)
        {
            unlocked.Clear();
            if (ids == null) return;
            for (int i = 0; i < ids.Length; i++) unlocked.Add(ids[i]);
        }
    }
}
