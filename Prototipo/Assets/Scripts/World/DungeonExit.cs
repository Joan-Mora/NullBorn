using NullBorn.World;
using UnityEngine;

namespace NullBorn.World
{
    public class DungeonExit : MonoBehaviour, IInteractable
    {
        public string Prompt => "E · volver al overworld";
        public bool CanInteract => true;

        public void Interact(GameObject actor)
        {
            AdditiveDungeonLoader.Exit();
        }
    }
}
