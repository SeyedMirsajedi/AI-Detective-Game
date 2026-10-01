namespace AIDetectiveGame.Backend.Contracts;

public sealed class DialogueResponseDto
{
    public string ReplyText { get; set; } = string.Empty;

    public DialogueEmotion Emotion { get; set; } = DialogueEmotion.Neutral;
}

public enum DialogueEmotion
{
    Neutral = 0,
    Calm = 1,
    Guarded = 2,
    Concerned = 3,
    Defensive = 4,
    Irritated = 5,
    Nervous = 6,
    Sad = 7
}