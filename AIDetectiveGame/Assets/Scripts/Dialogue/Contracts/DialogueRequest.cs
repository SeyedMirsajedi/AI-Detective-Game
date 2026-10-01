using System;
using System.Collections.Generic;

[Serializable]
public class DialogueRequest
{
    public string ScenarioId;
    public string ModelId;

    public string NpcId;

    public string NpcDisplayName;

    public string NpcRole;

    public string PlayerMessage;

    public List<DialogueFactData> KnownFacts =
        new List<DialogueFactData>();

    public List<DialogueFactData> PrivateFacts =
        new List<DialogueFactData>();

    public List<DialogueFactData> FalseBeliefs =
        new List<DialogueFactData>();

    public string LieOrEvasionNotes;

    public List<DialogueHistoryMessageData>
        PreviousConversationHistory =
            new List<DialogueHistoryMessageData>();
}

[Serializable]
public class DialogueFactData
{
    public string FactId;

    public string Description;
}

[Serializable]
public class DialogueHistoryMessageData
{
    public string Speaker;

    public string Text;
}