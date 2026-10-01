using UnityEngine;

[CreateAssetMenu(
    fileName = "NewClue",
    menuName = "AI Detective/Clue Definition")]
public class ClueDefinition : ScriptableObject
{
    [Header("Clue Information")]

    [SerializeField]
    private string clueId;

    [SerializeField]
    private string displayName;

    [SerializeField]
    [TextArea(3, 6)]
    private string description;

    public string ClueId => clueId;

    public string DisplayName => displayName;

    public string Description => description;
}