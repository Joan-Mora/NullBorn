using NullBorn.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullBorn.World
{
    public class DungeonPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private string dungeonScene = "Dungeon_HeapCave";
        [SerializeField] private string label = "Heap Cave";
        [SerializeField] private bool bossGate;

        public string Prompt => bossGate ? "E · Kernel Gate (Núcleo Corrupto)" : $"E · entrar a {label}";
        public bool CanInteract => true;

        public void Configure(string scene, string display, bool boss = false)
        {
            dungeonScene = scene;
            label = display;
            bossGate = boss;
        }

        public void Interact(GameObject actor)
        {
            if (bossGate)
            {
                AdditiveDungeonLoader.EnterInWorldBoss(new Vector3(0f, 1.2f, 155f));
                return;
            }

            AdditiveDungeonLoader.Enter(dungeonScene, actor.transform.position);
        }
    }
}
