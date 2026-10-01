using System;

[Serializable]
public class DialogueResponse
{
    public string ReplyText;

    public DialogueEmotion Emotion =
        DialogueEmotion.Neutral;
}

public enum DialogueEmotion
{
    Neutral,
    Calm,
    Guarded,
    Concerned,
    Defensive,
    Irritated,
    Nervous,
    Sad
}