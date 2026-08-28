using NullBorn.Art;
using NullBorn.RPG.Dialogue;
using NullBorn.RPG.Economy;
using NullBorn.World;
using UnityEngine;

namespace NullBorn.NPC
{
    /// <summary>
    /// NPC greybox (cápsula) + diálogo. Opcional tienda o patrol.
    /// </summary>
    public class NpcController : MonoBehaviour, IInteractable
    {
        [SerializeField] private string npcId = "npc";
        [SerializeField] private string displayName = "Servicio";
        [SerializeField] private bool isShop;

        private DialogueNode[] tree;

        public string NpcId => npcId;
        public string Prompt => $"E · hablar con {displayName}";
        public bool CanInteract => DialogueRunner.Instance == null || !DialogueRunner.Instance.IsOpen;

        public void Configure(string id, string name, DialogueNode[] dialogue, bool shop, Material mat)
        {
            npcId = id;
            displayName = name;
            tree = dialogue;
            isShop = shop;
            BuildVisual(mat);
        }

        public void Interact(GameObject actor)
        {
            if (isShop)
            {
                Shop shop = GetComponent<Shop>();
                shop?.Interact(actor);
                return;
            }

            if (tree == null || tree.Length == 0)
            {
                tree = DefaultLines();
            }

            DialogueRunner runner = FindAnyObjectByType<DialogueRunner>();
            runner?.Play(tree);
        }

        private DialogueNode[] DefaultLines()
        {
            return new[]
            {
                new DialogueNode
                {
                    id = "hello",
                    text = $"{displayName}: el Runtime aguanta, pero apenas.",
                    questTalkId = npcId
                }
            };
        }

        private void BuildVisual(Material mat)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Visual_Npc";
            body.transform.SetParent(transform, false);
            body.transform.localScale = new Vector3(0.7f, 0.9f, 0.7f);
            Object.Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = Vector3.up * 1.15f;
            head.transform.localScale = Vector3.one * 0.45f;
            Object.Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().sharedMaterial = mat;

            GameObject label = new GameObject("Name");
            label.transform.SetParent(transform, false);
            label.transform.localPosition = Vector3.up * 2.1f;
            TextMesh tm = label.AddComponent<TextMesh>();
            tm.text = displayName;
            tm.fontSize = 32;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
        }
    }
}
