using System.Collections.Generic;
using NullBorn.Core;
using NullBorn.Persistence;
using UnityEngine;

namespace NullBorn.RPG.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        public ItemDefinition item;
        public int count;
    }

    /// <summary>
    /// Memory Cache — slots limitados. Tab abre/cierra el panel.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance { get; private set; }

        [SerializeField] private int capacity = 12;

        private readonly List<InventorySlot> slots = new List<InventorySlot>();
        public IReadOnlyList<InventorySlot> Slots => slots;
        public bool PanelOpen { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                PanelOpen = !PanelOpen;
            }

            if (!PanelOpen) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseAt(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) UseAt(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UseAt(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) UseAt(3);
        }

        public bool Add(ItemDefinition item, int count = 1)
        {
            if (item == null) return false;
            InventorySlot existing = slots.Find(s => s.item != null && s.item.id == item.id);
            if (existing != null)
            {
                existing.count += count;
                return true;
            }

            if (slots.Count >= capacity) return false;
            slots.Add(new InventorySlot { item = item, count = count });
            return true;
        }

        public void UseAt(int index)
        {
            if (index < 0 || index >= slots.Count) return;
            InventorySlot slot = slots[index];
            if (slot.item == null) return;
            SystemIntegrity hp = GetComponent<SystemIntegrity>();
            CpuCycles cpu = GetComponent<CpuCycles>();
            if (slot.item.integrityRestore > 0f) hp?.Restore(slot.item.integrityRestore);
            if (slot.item.cpuRestore > 0f) cpu?.Restore(slot.item.cpuRestore);
            slot.count--;
            if (slot.count <= 0) slots.RemoveAt(index);
            NullBorn.Demo.TutorialDirector.Instance?.ShowRaw($"Usado: {slot.item.displayName}", 2.5f);
        }

        public string PanelText()
        {
            if (slots.Count == 0) return "Memory Cache vacía";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("MEMORY CACHE  (1-4 usar · Tab cerrar)");
            for (int i = 0; i < slots.Count; i++)
            {
                sb.AppendLine($"{i + 1}. {slots[i].item.displayName} x{slots[i].count}");
            }

            return sb.ToString();
        }

        public InventoryItemSave[] ToSave()
        {
            InventoryItemSave[] arr = new InventoryItemSave[slots.Count];
            for (int i = 0; i < slots.Count; i++)
            {
                arr[i] = new InventoryItemSave { itemId = slots[i].item.id, count = slots[i].count };
            }

            return arr;
        }

        public void FromSave(InventoryItemSave[] saved)
        {
            slots.Clear();
            if (saved == null) return;
            ItemCatalog catalog = ItemCatalog.Ensure();
            for (int i = 0; i < saved.Length; i++)
            {
                ItemDefinition def = catalog.Get(saved[i].itemId);
                if (def != null) Add(def, saved[i].count);
            }
        }
    }

    public class ItemCatalog
    {
        private static ItemCatalog instance;
        private readonly Dictionary<string, ItemDefinition> map = new Dictionary<string, ItemDefinition>();

        public static ItemCatalog Ensure()
        {
            if (instance != null) return instance;
            instance = new ItemCatalog();
            instance.Register(ItemDefinition.CreateRuntime("debug_patch", "Debug Patch", "Restaura Vitalidad.", ItemType.Consumable, 15, 28f, 0f));
            instance.Register(ItemDefinition.CreateRuntime("cpu_boost", "CPU Boost", "Restaura Energía.", ItemType.Consumable, 18, 0f, 40f));
            instance.Register(ItemDefinition.CreateRuntime("ram_expander", "RAM Expander", "Módulo de umbral RAM (sabor).", ItemType.Module, 40, 0f, 0f, true));
            return instance;
        }

        public void Register(ItemDefinition item) => map[item.id] = item;

        public ItemDefinition Get(string id)
        {
            map.TryGetValue(id, out ItemDefinition item);
            return item;
        }

        public ItemDefinition[] All
        {
            get
            {
                ItemDefinition[] arr = new ItemDefinition[map.Count];
                map.Values.CopyTo(arr, 0);
                return arr;
            }
        }
    }
}
