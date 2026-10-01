using System;
using System.Threading.Tasks;
using UnityEngine;

public class MockDialogueService :
    MonoBehaviour,
    IDialogueService
{
    [Header("Mock Network Behaviour")]

    [SerializeField]
    [Min(0f)]
    private float responseDelaySeconds = 0.5f;

    [SerializeField]
    private bool simulateFailure;

    public async Task<DialogueResponse>
        GenerateReplyAsync(
            DialogueRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(
                nameof(request));
        }

        if (responseDelaySeconds > 0f)
        {
            int delayMilliseconds =
                Mathf.RoundToInt(
                    responseDelaySeconds * 1000f);

            await Task.Delay(
                delayMilliseconds);
        }

        if (simulateFailure)
        {
            throw new InvalidOperationException(
                "Simulated dialogue provider failure.");
        }

        return CreateMockResponse(
            request);
    }

    private DialogueResponse CreateMockResponse(
        DialogueRequest request)
    {
        string displayName =
            request.NpcDisplayName == null
                ? string.Empty
                : request.NpcDisplayName.Trim();

        switch (displayName)
        {
            case "Evelyn Mercer":
                return new DialogueResponse
                {
                    ReplyText =
                        "I'll answer what I can, Detective.",
                    Emotion =
                        DialogueEmotion.Guarded
                };

            case "Daniel Mercer":
                return new DialogueResponse
                {
                    ReplyText =
                        "Of course, Detective. I'll cooperate where I can.",
                    Emotion =
                        DialogueEmotion.Calm
                };

            case "Adrian Bellamy":
                return new DialogueResponse
                {
                    ReplyText =
                        "If you have a question, just ask it.",
                    Emotion =
                        DialogueEmotion.Defensive
                };

            case "Anna Finch":
                return new DialogueResponse
                {
                    ReplyText =
                        "I heard Mr. Bellamy arguing with someone in the Study not long before I found him, but I couldn't tell who the other person was.",
                    Emotion =
                        DialogueEmotion.Concerned
                };

            case "Margaret Vale":
                return new DialogueResponse
                {
                    ReplyText =
                        "I'll tell you what I know, but please be precise.",
                    Emotion =
                        DialogueEmotion.Calm
                };

            default:
                return new DialogueResponse
                {
                    ReplyText =
                        "I'll answer what I can, Detective.",
                    Emotion =
                        DialogueEmotion.Neutral
                };
        }
    }
}