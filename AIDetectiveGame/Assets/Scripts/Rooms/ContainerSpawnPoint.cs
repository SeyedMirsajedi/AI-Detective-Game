using UnityEngine;

[DisallowMultipleComponent]
public class ContainerSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point")]
    [SerializeField]
    private string spawnPointId;

    public string SpawnPointId => spawnPointId;
}