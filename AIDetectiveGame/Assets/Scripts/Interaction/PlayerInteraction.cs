using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction UI")]
    [SerializeField]
    private TMP_Text interactionPromptText;

    private readonly List<IInteractable> nearbyInteractables =
        new List<IInteractable>();

    private IInteractable currentInteractable;
    private IInteractable previousInteractable;

    private bool holdStarted;
    private bool holdInteractionTriggered;
    private float holdTimer;

    private void Start()
    {
        HideInteractionPrompt();
    }

    private void Update()
    {
        RemoveInvalidInteractables();
        SelectClosestInteractable();

        if (currentInteractable != previousInteractable)
        {
            ResetHoldState();
            previousInteractable = currentInteractable;
        }

        UpdateInteractionPrompt();
        HandleInteractionInput();
    }

    private void HandleInteractionInput()
    {
        if (currentInteractable == null || Keyboard.current == null)
        {
            ResetHoldState();
            return;
        }

        if (currentInteractable.Mode == InteractionMode.Press)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                currentInteractable.Interact();
            }

            return;
        }

        HandleHoldInteraction();
    }

    private void HandleHoldInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            holdStarted = true;
            holdTimer = 0f;
            holdInteractionTriggered = false;
        }

        if (Keyboard.current.eKey.wasReleasedThisFrame)
        {
            ResetHoldState();
            return;
        }

        if (!holdStarted)
        {
            return;
        }

        if (!Keyboard.current.eKey.isPressed)
        {
            ResetHoldState();
            return;
        }

        if (holdInteractionTriggered)
        {
            return;
        }

        holdTimer += Time.deltaTime;

        float requiredHoldDuration =
            Mathf.Max(0.1f, currentInteractable.HoldDuration);

        if (holdTimer >= requiredHoldDuration)
        {
            currentInteractable.Interact();
            holdInteractionTriggered = true;
        }
    }

    private void UpdateInteractionPrompt()
    {
        if (interactionPromptText == null)
        {
            return;
        }

        if (currentInteractable == null)
        {
            HideInteractionPrompt();
            return;
        }

        if (!interactionPromptText.gameObject.activeSelf)
        {
            interactionPromptText.gameObject.SetActive(true);
        }

        string actionText = currentInteractable.InteractionPrompt;

        if (string.IsNullOrWhiteSpace(actionText))
        {
            actionText = "interact";
        }

        if (currentInteractable.Mode == InteractionMode.Hold)
        {
            interactionPromptText.text =
                "Hold E to " + actionText;
        }
        else
        {
            interactionPromptText.text =
                "Press E to " + actionText;
        }
    }

    private void HideInteractionPrompt()
    {
        if (interactionPromptText != null)
        {
            interactionPromptText.gameObject.SetActive(false);
        }
    }

    public void SetInteractionEnabled(bool isEnabled)
    {
        if (!isEnabled)
        {
            ResetHoldState();
            HideInteractionPrompt();
        }

        enabled = isEnabled;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractable interactable = FindInteractable(other);

        if (interactable != null &&
            !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractable interactable = FindInteractable(other);

        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
        }
    }

    private IInteractable FindInteractable(Collider2D other)
    {
        MonoBehaviour[] behaviours =
            other.GetComponentsInParent<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private void SelectClosestInteractable()
    {
        currentInteractable = null;

        float closestDistanceSquared = float.MaxValue;

        foreach (IInteractable interactable in nearbyInteractables)
        {
            MonoBehaviour behaviour =
                interactable as MonoBehaviour;

            if (behaviour == null)
            {
                continue;
            }

            Vector2 direction =
                behaviour.transform.position - transform.position;

            float distanceSquared = direction.sqrMagnitude;

            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                currentInteractable = interactable;
            }
        }
    }

    private void RemoveInvalidInteractables()
    {
        for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
        {
            MonoBehaviour behaviour =
                nearbyInteractables[i] as MonoBehaviour;

            if (behaviour == null)
            {
                nearbyInteractables.RemoveAt(i);
            }
        }
    }

    private void ResetHoldState()
    {
        holdStarted = false;
        holdTimer = 0f;
        holdInteractionTriggered = false;
    }
}