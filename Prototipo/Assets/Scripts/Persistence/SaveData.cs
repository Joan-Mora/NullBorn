using System;

namespace NullBorn.Persistence
{
    [Serializable]
    public class InventoryItemSave
    {
        public string itemId;
        public int count;
    }

    [Serializable]
    public class QuestSave
    {
        public string id;
        public int progress;
        public int state;
    }

    [Serializable]
    public class SaveData
    {
        public string version = "1.0";
        public float posX;
        public float posY;
        public float posZ;
        public float rotY;
        public int threadPriority = 1;
        public float xp;
        public float integrity = 100f;
        public float cpu = 100f;
        public int credits;
        public string regionId = "RuntimePlaza";
        public string[] unlockedTerminals = Array.Empty<string>();
        public InventoryItemSave[] items = Array.Empty<InventoryItemSave>();
        public QuestSave[] quests = Array.Empty<QuestSave>();
        public string[] completedQuests = Array.Empty<string>();
    }
}
