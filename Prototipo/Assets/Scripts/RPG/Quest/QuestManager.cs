using System.Collections.Generic;
using NullBorn.Enemies;
using NullBorn.Persistence;
using NullBorn.Player;
using NullBorn.RPG.Economy;
using NullBorn.UI;
using UnityEngine;

namespace NullBorn.RPG.Quest
{
    [System.Serializable]
    public class QuestRuntime
    {
        public QuestDefinition def;
        public QuestState state = QuestState.NotStarted;
        public int progress;
    }

    /// <summary>
    /// Tickets de mantenimiento. Journal compacto en HUD.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        private readonly List<QuestRuntime> quests = new List<QuestRuntime>();
        private readonly List<string> completed = new List<string>();

        public string[] CompletedIds => completed.ToArray();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void LoadStarterQuests()
        {
            if (quests.Count > 0) return;
            Add(QuestDefinition.CreateRuntime(
                "ticket_daemon", "Ticket: reportarse al Daemon",
                "Habla con el Daemon en Runtime Plaza. Es el servicio de orquestación del hub.",
                QuestObjectiveType.Talk, "npc_daemon", 1, 25, 10, true));
            Add(QuestDefinition.CreateRuntime(
                "ticket_purge_heap", "Ticket: contener Goteos del Heap",
                "Purga 3 Goteos en Heap Wilds. Cada leak es una fuga de memoria viva.",
                QuestObjectiveType.Kill, "Basic", 3, 60, 30, true, "ticket_daemon"));
            Add(QuestDefinition.CreateRuntime(
                "ticket_heap_cave", "Ticket: inspeccionar Heap Cave",
                "Entra al dungeon Heap Cave y limpia la fragmentación.",
                QuestObjectiveType.ClearDungeon, "Dungeon_HeapCave", 1, 80, 40, true, "ticket_purge_heap"));
            Add(QuestDefinition.CreateRuntime(
                "ticket_logger", "Secundaria: archivar con Logger",
                "Habla con Logger en el borde del Heap. Quiere fragmentos catalogados.",
                QuestObjectiveType.Talk, "npc_logger", 1, 20, 15, false, "ticket_daemon"));
            Add(QuestDefinition.CreateRuntime(
                "ticket_fragments", "Secundaria: recolectar fragmentos",
                "Reúne 5 fragmentos de memoria (púrgalos o recógelos).",
                QuestObjectiveType.Collect, "fragment", 5, 35, 25, false, "ticket_logger"));
            Add(QuestDefinition.CreateRuntime(
                "ticket_io_tower", "Secundaria: Relay I/O",
                "Alcanza la torre de I/O Wastes y registra el POI.",
                QuestObjectiveType.Reach, "poi_io_tower", 1, 40, 20, false, "ticket_daemon"));
        }

        private void Add(QuestDefinition def)
        {
            quests.Add(new QuestRuntime { def = def, state = QuestState.NotStarted });
        }

        public void TryAutoStart()
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].state != QuestState.NotStarted) continue;
                if (PrereqsMet(quests[i].def))
                {
                    quests[i].state = QuestState.Active;
                }
            }
        }

        public void NotifyTalk(string npcId)
        {
            Advance(QuestObjectiveType.Talk, npcId, 1);
        }

        public void NotifyKill(LeakVariant variant)
        {
            Advance(QuestObjectiveType.Kill, variant.ToString(), 1);
        }

        public void NotifyCollect(string itemId, int amount = 1)
        {
            Advance(QuestObjectiveType.Collect, itemId, amount);
        }

        public void NotifyReach(string poiId)
        {
            Advance(QuestObjectiveType.Reach, poiId, 1);
        }

        public void NotifyDungeonCleared(string sceneName)
        {
            Advance(QuestObjectiveType.ClearDungeon, sceneName, 1);
        }

        public string TrackerLine()
        {
            QuestRuntime main = null;
            QuestRuntime side = null;
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].state != QuestState.Active) continue;
                if (quests[i].def.isMain && main == null) main = quests[i];
                else if (side == null) side = quests[i];
            }

            string a = Format(main);
            string b = Format(side);
            if (string.IsNullOrEmpty(a)) return b;
            if (string.IsNullOrEmpty(b)) return a;
            return a + "\n" + b;
        }

        public QuestSave[] ToSave()
        {
            QuestSave[] arr = new QuestSave[quests.Count];
            for (int i = 0; i < quests.Count; i++)
            {
                arr[i] = new QuestSave
                {
                    id = quests[i].def.id,
                    progress = quests[i].progress,
                    state = (int)quests[i].state
                };
            }

            return arr;
        }

        public void FromSave(QuestSave[] saved, string[] done)
        {
            completed.Clear();
            if (done != null) completed.AddRange(done);
            if (saved == null) return;
            for (int i = 0; i < saved.Length; i++)
            {
                QuestRuntime rt = quests.Find(q => q.def.id == saved[i].id);
                if (rt == null) continue;
                rt.progress = saved[i].progress;
                rt.state = (QuestState)saved[i].state;
            }
        }

        private void Advance(QuestObjectiveType type, string target, int amount)
        {
            bool any = false;
            for (int i = 0; i < quests.Count; i++)
            {
                QuestRuntime rt = quests[i];
                if (rt.state != QuestState.Active) continue;
                if (rt.def.objectiveType != type) continue;
                if (!string.IsNullOrEmpty(rt.def.targetId) && rt.def.targetId != target && type != QuestObjectiveType.Collect)
                    continue;
                if (type == QuestObjectiveType.Collect && rt.def.targetId != target && target != "fragment") continue;

                rt.progress += amount;
                any = true;
                if (rt.progress >= rt.def.targetCount)
                {
                    Complete(rt);
                }
            }

            if (any)
            {
                TryAutoStart();
                RefreshHud();
            }
        }

        private void Complete(QuestRuntime rt)
        {
            rt.state = QuestState.Completed;
            if (!completed.Contains(rt.def.id)) completed.Add(rt.def.id);
            PlayerProgression.Instance?.AddXp(rt.def.xpReward, rt.def.id);
            CpuCredits credits = FindAnyObjectByType<CpuCredits>();
            credits?.Add(rt.def.creditReward);
            NullBorn.Demo.TutorialDirector.Instance?.ShowRaw($"Ticket cerrado: {rt.def.title}", 4f);
            NullBorn.Persistence.SaveManager.Instance?.Save();
        }

        private bool PrereqsMet(QuestDefinition def)
        {
            if (def.prerequisiteIds == null || def.prerequisiteIds.Length == 0) return true;
            for (int i = 0; i < def.prerequisiteIds.Length; i++)
            {
                if (!completed.Contains(def.prerequisiteIds[i])) return false;
            }

            return true;
        }

        private static string Format(QuestRuntime rt)
        {
            if (rt == null) return string.Empty;
            return $"{rt.def.title} ({rt.progress}/{rt.def.targetCount})";
        }

        private void RefreshHud()
        {
            CanvasHUD hud = FindAnyObjectByType<CanvasHUD>();
            hud?.SetQuestTracker(TrackerLine());
        }

        private void Update()
        {
            if (Time.frameCount % 20 == 0) RefreshHud();
        }
    }
}
