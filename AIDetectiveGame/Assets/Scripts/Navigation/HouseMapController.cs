using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HouseMapController : MonoBehaviour
{
    [Serializable]
    public class RoomButtonUI
    {
        public RoomId roomId;
        public Button button;
        public TMP_Text statusText;
        public Image buttonImage;
    }

    [Header("UI References")]
    [SerializeField] private TMP_Text currentRoomText;
    [SerializeField] private Button backButton;

    [SerializeField]
    private List<RoomButtonUI> roomButtons =
        new List<RoomButtonUI>();

    [Header("Overlay Colors")]
    [SerializeField]
    private Color availableOverlayColor =
        new Color(0f, 0f, 0f, 0.08f);

    [SerializeField]
    private Color visitedOverlayColor =
        new Color(0f, 0.15f, 0f, 0.12f);

    [SerializeField]
    private Color currentOverlayColor =
        new Color(0.55f, 0.4f, 0f, 0.18f);

    [SerializeField]
    private Color restrictedOverlayColor =
        new Color(0.2f, 0f, 0f, 0.18f);

    [Header("Status Text Colors")]
    [SerializeField]
    private Color availableTextColor =
        new Color(0.55f, 1f, 0.55f);

    [SerializeField]
    private Color visitedTextColor =
        new Color(0.75f, 1f, 0.75f);

    [SerializeField]
    private Color currentTextColor =
        new Color(1f, 0.92f, 0.65f);

    [SerializeField]
    private Color restrictedTextColor =
        new Color(1f, 0.45f, 0.45f);

    private RoomNavigationState navigationState;

    private void Start()
    {
        navigationState = RoomNavigationState.Instance;

        if (navigationState == null)
        {
            Debug.LogError(
                "HouseMapController could not find RoomNavigationState.Instance."
            );

            return;
        }

        SetupButtonListeners();
        RefreshUI();
        SelectInitialButton();
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame ||
            Keyboard.current.mKey.wasPressedThisFrame)
        {
            ReturnToCurrentRoom();
        }
    }

    private void SetupButtonListeners()
    {
        foreach (RoomButtonUI roomUI in roomButtons)
        {
            if (roomUI == null || roomUI.button == null)
            {
                continue;
            }

            RoomId roomId = roomUI.roomId;

            roomUI.button.onClick.RemoveAllListeners();
            roomUI.button.onClick.AddListener(
                () => TryEnterRoom(roomId)
            );
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(ReturnToCurrentRoom);
        }
    }

    public void RefreshUI()
    {
        if (navigationState == null)
        {
            return;
        }

        if (currentRoomText != null)
        {
            currentRoomText.text =
                "Current Room: " +
                GetDisplayName(navigationState.CurrentRoom);
        }

        foreach (RoomButtonUI roomUI in roomButtons)
        {
            if (roomUI == null || roomUI.button == null)
            {
                continue;
            }

            bool isUnlocked =
                navigationState.IsRoomUnlocked(roomUI.roomId);

            bool isVisited =
                navigationState.IsRoomVisited(roomUI.roomId);

            bool isCurrent =
                navigationState.CurrentRoom == roomUI.roomId;

            // Keep every room focusable so the player can see
            // and select the restricted Study.
            // TryEnterRoom() prevents locked rooms from loading.
            roomUI.button.interactable = true;

            if (!isUnlocked)
            {
                if (roomUI.statusText != null)
                {
                    roomUI.statusText.text = "Restricted";
                    roomUI.statusText.color = restrictedTextColor;
                }

                if (roomUI.buttonImage != null)
                {
                    roomUI.buttonImage.color = restrictedOverlayColor;
                }
            }
            else if (isCurrent)
            {
                if (roomUI.statusText != null)
                {
                    roomUI.statusText.text = "Current Room";
                    roomUI.statusText.color = currentTextColor;
                }

                if (roomUI.buttonImage != null)
                {
                    roomUI.buttonImage.color = currentOverlayColor;
                }
            }
            else if (isVisited)
            {
                if (roomUI.statusText != null)
                {
                    roomUI.statusText.text = "Visited";
                    roomUI.statusText.color = visitedTextColor;
                }

                if (roomUI.buttonImage != null)
                {
                    roomUI.buttonImage.color = visitedOverlayColor;
                }
            }
            else
            {
                if (roomUI.statusText != null)
                {
                    roomUI.statusText.text = "Available";
                    roomUI.statusText.color = availableTextColor;
                }

                if (roomUI.buttonImage != null)
                {
                    roomUI.buttonImage.color = availableOverlayColor;
                }
            }
        }
    }

    private void TryEnterRoom(RoomId roomId)
    {
        if (!navigationState.IsRoomUnlocked(roomId))
        {
            Debug.Log(
                GetDisplayName(roomId) +
                " is currently restricted."
            );

            return;
        }

        navigationState.SetCurrentRoom(roomId);

        SceneManager.LoadScene(GetSceneName(roomId));
    }

    private void ReturnToCurrentRoom()
    {
        if (navigationState == null)
        {
            return;
        }

        SceneManager.LoadScene(
            GetSceneName(navigationState.CurrentRoom)
        );
    }

    private void SelectInitialButton()
    {
        if (EventSystem.current == null)
        {
            return;
        }

        // Prefer the current room.
        foreach (RoomButtonUI roomUI in roomButtons)
        {
            if (roomUI != null &&
                roomUI.button != null &&
                roomUI.roomId == navigationState.CurrentRoom)
            {
                EventSystem.current.SetSelectedGameObject(
                    roomUI.button.gameObject
                );

                return;
            }
        }

        // Fallback to the first room button.
        foreach (RoomButtonUI roomUI in roomButtons)
        {
            if (roomUI != null && roomUI.button != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    roomUI.button.gameObject
                );

                return;
            }
        }
    }

    private string GetSceneName(RoomId roomId)
    {
        switch (roomId)
        {
            case RoomId.LivingRoom:
                return "LivingRoom";

            case RoomId.DiningRoom:
                return "DiningRoom";

            case RoomId.Study:
                return "Study";

            case RoomId.Kitchen:
                return "Kitchen";

            case RoomId.MasterBedroom:
                return "MasterBedroom";

            case RoomId.AdrianBedroom:
                return "AdrianBedroom";

            default:
                return "LivingRoom";
        }
    }

    private string GetDisplayName(RoomId roomId)
    {
        switch (roomId)
        {
            case RoomId.LivingRoom:
                return "Living Room";

            case RoomId.DiningRoom:
                return "Dining Room";

            case RoomId.Study:
                return "Study";

            case RoomId.Kitchen:
                return "Kitchen";

            case RoomId.MasterBedroom:
                return "Master Bedroom";

            case RoomId.AdrianBedroom:
                return "Adrian's Bedroom";

            default:
                return roomId.ToString();
        }
    }
}