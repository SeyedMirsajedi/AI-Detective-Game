using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string interactionName = "Test Interactable";

    [SerializeField]
    private InteractionMode interactionMode = InteractionMode.Press;

    [SerializeField]
    [Min(0.1f)]
    private float holdDuration = 1f;

    [SerializeField]
    private string interactionPrompt = "interact";

    public InteractionMode Mode => interactionMode;

    public float HoldDuration => holdDuration;

    public string InteractionPrompt => interactionPrompt;

    public void Interact()
    {
        Debug.Log(interactionName + " activated.");
    }
}