using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RoomSceneController : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private RoomId roomId = RoomId.LivingRoom;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private const string HouseMapSceneName = "HouseMap";

    public RoomId RoomId => roomId;

    private void Start()
    {
        if (RoomNavigationState.Instance == null)
        {
            Debug.LogError(
                "RoomSceneController could not find RoomNavigationState.Instance."
            );

            return;
        }

        RoomNavigationState.Instance.SetCurrentRoom(roomId);
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.mKey.wasPressedThisFrame)
        {
            return;
        }

        OpenHouseMap();
    }

    public void OpenHouseMap()
    {
        if (playerMovement != null && !playerMovement.enabled)
        {
            return;
        }

        SceneManager.LoadScene(HouseMapSceneName);
    }
}