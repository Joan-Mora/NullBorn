using UnityEngine;

namespace NullBorn.RPG.Inventory
{
    public enum ItemType
    {
        Consumable,
        Module
    }

    [CreateAssetMenu(fileName = "Item", menuName = "NullBorn/Item")]
    public class ItemDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemType type;
        public int price = 15;
        public float integrityRestore;
        public float cpuRestore;
        public bool ramExpander;

        public static ItemDefinition CreateRuntime(string id, string name, string desc, ItemType type, int price, float hp, float cpu, bool ram = false)
        {
            ItemDefinition item = CreateInstance<ItemDefinition>();
            item.id = id;
            item.displayName = name;
            item.description = desc;
            item.type = type;
            item.price = price;
            item.integrityRestore = hp;
            item.cpuRestore = cpu;
            item.ramExpander = ram;
            return item;
        }
    }
}
