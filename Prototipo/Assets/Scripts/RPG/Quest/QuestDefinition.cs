using UnityEngine;

namespace NullBorn.RPG.Quest
{
    public enum QuestState
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    public enum QuestObjectiveType
    {
        Kill,
        Collect,
        Talk,
        Reach,
        ClearDungeon
    }

    [CreateAssetMenu(fileName = "Quest", menuName = "NullBorn/Quest")]
    public class QuestDefinition : ScriptableObject
    {
        public string id;
        public string title;
        [TextArea] public string description;
        public QuestObjectiveType objectiveType;
        public string targetId;
        public int targetCount = 1;
        public int xpReward = 40;
        public int creditReward = 20;
        public string[] prerequisiteIds;
        public bool isMain;

        public static QuestDefinition CreateRuntime(
            string id, string title, string description, QuestObjectiveType type,
            string targetId, int count, int xp, int credits, bool main, params string[] prereq)
        {
            QuestDefinition q = CreateInstance<QuestDefinition>();
            q.id = id;
            q.title = title;
            q.description = description;
            q.objectiveType = type;
            q.targetId = targetId;
            q.targetCount = count;
            q.xpReward = xp;
            q.creditReward = credits;
            q.isMain = main;
            q.prerequisiteIds = prereq ?? System.Array.Empty<string>();
            return q;
        }
    }
}
