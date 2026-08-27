using NullBorn.Player;
using NullBorn.RPG.Inventory;
using NullBorn.World;
using UnityEngine;

namespace NullBorn.RPG.Economy
{
    /// <summary>
    /// Tienda del hub (Runtime Market). E para abrir; 1-3 para comprar.
    /// </summary>
    public class Shop : MonoBehaviour, IInteractable
    {
        public string Prompt => browsing ? "1-3 comprar · E cerrar" : "E · Runtime Market";
        public bool CanInteract => true;

        private bool browsing;
        private ItemDefinition[] stock;

        private void Awake()
        {
            ItemCatalog catalog = ItemCatalog.Ensure();
            stock = new[]
            {
                catalog.Get("debug_patch"),
                catalog.Get("cpu_boost"),
                catalog.Get("ram_expander")
            };
        }

        public void Interact(GameObject actor)
        {
            browsing = !browsing;
            if (browsing)
            {
                Time.timeScale = 0.0001f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Show();
            }
            else Close();
        }

        private void Update()
        {
            if (!browsing) return;
            if (Input.GetKeyDown(KeyCode.Alpha1)) Buy(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) Buy(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) Buy(2);
            if (Input.GetKeyDown(KeyCode.Escape)) Close();
        }

        private void Buy(int index)
        {
            if (index < 0 || index >= stock.Length || stock[index] == null) return;
            ItemDefinition item = stock[index];
            CpuCredits credits = CpuCredits.Instance ?? FindAnyObjectByType<CpuCredits>();
            Inventory inv = Inventory.Instance ?? FindAnyObjectByType<Inventory>();
            if (credits == null || inv == null) return;
            if (!credits.TrySpend(item.price))
            {
                CombatFeedback.NotifyDeny(transform.position);
                NullBorn.Demo.TutorialDirector.Instance?.ShowRaw("Créditos de CPU insuficientes.", 2f);
                return;
            }

            inv.Add(item);
            NullBorn.Demo.TutorialDirector.Instance?.ShowRaw($"Comprado: {item.displayName}", 2.5f);
            Show();
        }

        private void Show()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("RUNTIME MARKET");
            int credits = CpuCredits.Instance != null ? CpuCredits.Instance.Amount : 0;
            sb.AppendLine($"Créditos de CPU: {credits}");
            for (int i = 0; i < stock.Length; i++)
            {
                if (stock[i] == null) continue;
                sb.AppendLine($"{i + 1}. {stock[i].displayName}  ({stock[i].price} cr) — {stock[i].description}");
            }

            FindAnyObjectByType<NullBorn.UI.CanvasHUD>()?.SetDialogue(sb.ToString());
        }

        private void Close()
        {
            browsing = false;
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            FindAnyObjectByType<NullBorn.UI.CanvasHUD>()?.SetDialogue(string.Empty);
        }
    }
}
