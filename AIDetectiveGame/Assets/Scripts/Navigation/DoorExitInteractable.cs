using UnityEngine;

public class DoorExitInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField]
    private InteractionMode interactionMode = InteractionMode.Hold;

    [SerializeField]
    [Min(0.1f)]
    private float holdDuration = 1f;

    [SerializeField]
    private string interactionPrompt = "leave room";

    [Header("Navigation")]
    [SerializeField]
    private RoomSceneController roomSceneController;

    public InteractionMode Mode => interactionMode;

    public float HoldDuration => holdDuration;

    public string InteractionPrompt => interactionPrompt;

    public void Interact()
    {
        if (roomSceneController == null)
        {
            Debug.LogError(
                $"DoorExitInteractable on \"{gameObject.name}\" " +
                "has no RoomSceneController assigned."
            );

            return;
        }

        roomSceneController.OpenHouseMap();
    }
}