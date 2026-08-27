using NullBorn.Core;
using NullBorn.Persistence;
using NullBorn.Player;
using UnityEngine;

namespace NullBorn.World
{
    /// <summary>
    /// Terminal de fast travel / save. Coste de Energía (latencia de red).
    /// </summary>
    public class FastTravelTerminal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string terminalId = "hub";
        [SerializeField] private string displayName = "Terminal";
        [SerializeField] private float energyCost = 12f;

        private bool menuOpen;

        public string Prompt => menuOpen ? "1-4 viajar · E cerrar" : $"E · {displayName} (save / viaje)";
        public bool CanInteract => true;

        public void Configure(string id, string name)
        {
            terminalId = id;
            displayName = name;
        }

        private void Start()
        {
            WorldMapController.Instance?.RegisterTerminal(terminalId, transform.position + Vector3.forward * 2f);
        }

        public void Interact(GameObject actor)
        {
            WorldMapController.Instance?.UnlockTerminal(terminalId);
            SaveManager.Instance?.Save();
            menuOpen = !menuOpen;
            if (menuOpen)
            {
                Time.timeScale = 0.0001f;
                BuildOptions();
                Show();
            }
            else Close();
        }

        private void Update()
        {
            if (!menuOpen) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Travel(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Travel(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Travel(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) Travel(3);
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void BuildOptions()
        {
            ListBuffer();
        }

        private readonly System.Collections.Generic.List<string> ids = new System.Collections.Generic.List<string>();

        private void ListBuffer()
        {
            ids.Clear();
            WorldMapController map = WorldMapController.Instance;
            if (map == null) return;
            foreach (string id in map.KnownTerminals)
            {
                if (map.IsUnlocked(id) && id != terminalId) ids.Add(id);
            }
        }

        private void Travel(int index)
        {
            ListBuffer();
            if (index < 0 || index >= ids.Count) return;
            WorldMapController map = WorldMapController.Instance;
            if (map == null || !map.TryGetTerminal(ids[index], out Vector3 pos)) return;

            GameObject player = GameObject.Find("Player_Thread");
            CpuCycles cpu = player != null ? player.GetComponent<CpuCycles>() : null;
            if (cpu != null && !cpu.TrySpend(energyCost))
            {
                CombatFeedback.NotifyDeny(transform.position);
                return;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = pos + Vector3.up;
            if (cc != null) cc.enabled = true;
            Close();
        }

        private void Show()
        {
            ListBuffer();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"TERMINAL {displayName} — progreso guardado.");
            if (ids.Count == 0) sb.AppendLine("No hay otros nodos desbloqueados.");
            for (int i = 0; i < ids.Count; i++)
            {
                sb.AppendLine($"{i + 1}. Viajar a {ids[i]} (coste Energía)");
            }

            FindAnyObjectByType<NullBorn.UI.CanvasHUD>()?.SetDialogue(sb.ToString());
        }

        private void Close()
        {
            menuOpen = false;
            Time.timeScale = 1f;
            FindAnyObjectByType<NullBorn.UI.CanvasHUD>()?.SetDialogue(string.Empty);
        }
    }
}
