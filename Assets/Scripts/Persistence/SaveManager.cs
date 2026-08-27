using NullBorn.Api;
using NullBorn.Core;
using NullBorn.Player;
using NullBorn.RPG.Economy;
using NullBorn.RPG.Inventory;
using NullBorn.RPG.Quest;
using NullBorn.World;
using UnityEngine;

namespace NullBorn.Persistence
{
    /// <summary>
    /// Save/load JSON local. Auto-save en terminales y al completar quest.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private readonly LocalSaveProvider provider = new LocalSaveProvider();
        private Transform player;
        private string apiSessionId;

        public bool HasSave => provider.Exists;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void RegisterPlayer(Transform playerTransform)
        {
            player = playerTransform;
        }

        public void BeginApiSession(int level)
        {
            apiSessionId = NullBornApi.Current?.StartSession(level);
        }

        public void Save()
        {
            if (player == null) player = GameObject.Find("Player_Thread")?.transform;
            if (player == null) return;

            SystemIntegrity integrity = player.GetComponent<SystemIntegrity>();
            CpuCycles cpu = player.GetComponent<CpuCycles>();
            PlayerProgression prog = player.GetComponent<PlayerProgression>();
            CpuCredits credits = player.GetComponent<CpuCredits>();
            Inventory inv = player.GetComponent<Inventory>();

            SaveData data = new SaveData
            {
                posX = player.position.x,
                posY = player.position.y,
                posZ = player.position.z,
                rotY = player.eulerAngles.y,
                threadPriority = prog != null ? prog.Level : 1,
                xp = prog != null ? prog.Xp : 0f,
                integrity = integrity != null ? integrity.CurrentIntegrity : 100f,
                cpu = cpu != null ? cpu.CurrentCycles : 100f,
                credits = credits != null ? credits.Amount : 0,
                regionId = WorldMapController.Instance != null ? WorldMapController.Instance.CurrentRegionId.ToString() : "RuntimePlaza",
                unlockedTerminals = WorldMapController.Instance != null ? WorldMapController.Instance.UnlockedTerminalIds : System.Array.Empty<string>(),
                items = inv != null ? inv.ToSave() : System.Array.Empty<InventoryItemSave>(),
                quests = QuestManager.Instance != null ? QuestManager.Instance.ToSave() : System.Array.Empty<QuestSave>(),
                completedQuests = QuestManager.Instance != null ? QuestManager.Instance.CompletedIds : System.Array.Empty<string>()
            };

            provider.Write(data);
            NullBornApi.Current?.SyncProgress(data);
            Debug.Log($"NullBorn save → {provider.Path}");
        }

        public bool TryLoad()
        {
            SaveData data = provider.Read();
            if (data == null) return false;
            Apply(data);
            return true;
        }

        public void Apply(SaveData data)
        {
            if (player == null) player = GameObject.Find("Player_Thread")?.transform;
            if (player == null || data == null) return;

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.SetPositionAndRotation(new Vector3(data.posX, data.posY, data.posZ), Quaternion.Euler(0f, data.rotY, 0f));
            if (cc != null) cc.enabled = true;

            player.GetComponent<PlayerProgression>()?.Load(data.threadPriority, data.xp);
            player.GetComponent<SystemIntegrity>()?.SetCurrentIntegrity(data.integrity);
            player.GetComponent<CpuCycles>()?.SetCurrentCycles(data.cpu);
            player.GetComponent<CpuCredits>()?.SetAmount(data.credits);
            player.GetComponent<Inventory>()?.FromSave(data.items);
            QuestManager.Instance?.FromSave(data.quests, data.completedQuests);
            WorldMapController.Instance?.RestoreUnlocks(data.unlockedTerminals);
        }

        public void EndSession(bool victory)
        {
            GameSession session = GameSession.Instance;
            NullBornApi.Current?.EndSession(apiSessionId, session != null ? session.ElapsedSeconds : 0f, victory);
            Save();
        }
    }
}
