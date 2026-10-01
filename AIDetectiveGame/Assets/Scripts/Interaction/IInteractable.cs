public enum InteractionMode
{
    Press,
    Hold
}

public interface IInteractable
{
    InteractionMode Mode { get; }

    float HoldDuration { get; }

    string InteractionPrompt { get; }

    void Interact();
}