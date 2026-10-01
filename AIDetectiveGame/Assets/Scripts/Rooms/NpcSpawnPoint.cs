using UnityEngine;

[DisallowMultipleComponent]
public class NpcSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField]
    private string spawnPointId;

    public string SpawnPointId => spawnPointId;
}