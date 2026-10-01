namespace AIDetectiveGame.Backend.Contracts;

public sealed class DialogueRequestDto
{
    public string ScenarioId { get; set; } = string.Empty;

    public string ModelId { get; set; } = string.Empty;

    public string NpcId { get; set; } = string.Empty;

    public string NpcDisplayName { get; set; } = string.Empty;

    public string NpcRole { get; set; } = string.Empty;

    public string PlayerMessage { get; set; } = string.Empty;

    public List<DialogueFactDto> KnownFacts { get; set; } = new();

    public List<DialogueFactDto> PrivateFacts { get; set; } = new();

    public List<DialogueFactDto> FalseBeliefs { get; set; } = new();

    public string LieOrEvasionNotes { get; set; } = string.Empty;

    public List<DialogueHistoryMessageDto> PreviousConversationHistory { get; set; } = new();
}

public sealed class DialogueFactDto
{
    public string FactId { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}

public sealed class DialogueHistoryMessageDto
{
    public string Speaker { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}