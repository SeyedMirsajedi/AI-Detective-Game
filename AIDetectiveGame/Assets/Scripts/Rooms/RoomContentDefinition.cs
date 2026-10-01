using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RoomContentDefinition",
    menuName = "AI Detective/Rooms/Room Content Definition"
)]
public class RoomContentDefinition : ScriptableObject
{
    [Serializable]
    public class CluePlacement
    {
        [SerializeField] private string spawnPointId;
        [SerializeField] private GameObject worldCluePrefab;

        public string SpawnPointId => spawnPointId;
        public GameObject WorldCluePrefab => worldCluePrefab;
    }

    [Serializable]
    public class ContainerPlacement
    {
        [SerializeField] private string spawnPointId;
        [SerializeField] private GameObject containerPrefab;

        public string SpawnPointId => spawnPointId;
        public GameObject ContainerPrefab => containerPrefab;
    }

    [Serializable]
    public class EnvironmentVariantSelection
    {
        [SerializeField] private string variantGroupId;
        [SerializeField] private string variantId;
        [SerializeField] private bool enabled = true;

        public string VariantGroupId => variantGroupId;
        public string VariantId => variantId;
        public bool Enabled => enabled;
    }

    [Header("Room")]
    [SerializeField] private RoomId roomId;

    [Header("Clue Placements")]
    [SerializeField] private CluePlacement[] cluePlacements;

    [Header("Container Placements")]
    [SerializeField] private ContainerPlacement[] containerPlacements;

    [Header("Environment Variant Selections")]
    [SerializeField]
    private EnvironmentVariantSelection[] environmentVariantSelections;

    public RoomId RoomId => roomId;

    public CluePlacement[] CluePlacements => cluePlacements;

    public ContainerPlacement[] ContainerPlacements => containerPlacements;

    public EnvironmentVariantSelection[] EnvironmentVariantSelections =>
        environmentVariantSelections;
}