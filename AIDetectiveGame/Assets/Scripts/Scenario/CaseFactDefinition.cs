using UnityEngine;

[CreateAssetMenu(
    fileName = "Fact_New",
    menuName = "AI Detective/Scenario/Case Fact Definition")]
public class CaseFactDefinition : ScriptableObject
{
    [Header("Fact Identity")]

    [SerializeField]
    private string factId;

    [SerializeField]
    private string category;

    [Header("Developer Information")]

    [SerializeField]
    [TextArea(2, 5)]
    private string developerDescription;

    public string FactId => factId;

    public string Category => category;

    public string DeveloperDescription =>
        developerDescription;
}