using UnityEngine;

[CreateAssetMenu(
    fileName = "NPC_New",
    menuName = "AI Detective/Scenario/NPC Definition")]
public class NpcDefinition : ScriptableObject
{
    [Header("Identity")]

    [SerializeField]
    private string npcId;

    [SerializeField]
    private string displayName;

    [SerializeField]
    [Min(0)]
    private int age;

    [SerializeField]
    private string role;

    [Header("Presentation")]

    [SerializeField]
    private GameObject worldPrefab;

    [SerializeField]
    private Sprite portrait;

    [Header("Default Dialogue Limits")]

    [SerializeField]
    [Min(1)]
    private int defaultMaxQuestions = 5;

    [SerializeField]
    [Min(1)]
    private int defaultMaxPlayerCharacters = 500;

    public string NpcId => npcId;

    public string DisplayName => displayName;

    public int Age => age;

    public string Role => role;

    public GameObject WorldPrefab => worldPrefab;

    public Sprite Portrait => portrait;

    public int DefaultMaxQuestions => defaultMaxQuestions;

    public int DefaultMaxPlayerCharacters =>
        defaultMaxPlayerCharacters;
}