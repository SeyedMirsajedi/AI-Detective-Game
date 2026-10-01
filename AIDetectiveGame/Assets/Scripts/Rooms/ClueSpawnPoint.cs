using UnityEngine;

[DisallowMultipleComponent]
public class ClueSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField]
    private string spawnPointId;

    public string SpawnPointId => spawnPointId;
}