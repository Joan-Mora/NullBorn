namespace NullBorn.World
{
    public interface IInteractable
    {
        string Prompt { get; }
        bool CanInteract { get; }
        void Interact(UnityEngine.GameObject actor);
    }
}
