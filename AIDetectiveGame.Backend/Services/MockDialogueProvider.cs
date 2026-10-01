using AIDetectiveGame.Backend.Contracts;

namespace AIDetectiveGame.Backend.Services;

public sealed class MockDialogueProvider : IDialogueProvider
{
    public async Task<DialogueResponseDto> GenerateReplyAsync(
        DialogueRequestDto request,
        CancellationToken cancellationToken)
    {
        // Simulate a small amount of provider/network latency.
        // This makes the Unity "Thinking..." and input-lock state
        // visible during development.
        await Task.Delay(500, cancellationToken);

        string npcId =
            request.NpcId?.Trim().ToLowerInvariant()
            ?? string.Empty;

        DialogueResponseDto response = npcId switch
        {
            "npc_evelyn_mercer" => new DialogueResponseDto
            {
                ReplyText =
                    "I'll answer what I can, Detective.",
                Emotion = DialogueEmotion.Guarded
            },

            "npc_daniel_mercer" => new DialogueResponseDto
            {
                ReplyText =
                    "Of course, Detective. I'll cooperate where I can.",
                Emotion = DialogueEmotion.Calm
            },

            "npc_adrian_bellamy" => new DialogueResponseDto
            {
                ReplyText =
                    "If you have a question, just ask it.",
                Emotion = DialogueEmotion.Defensive
            },

            "npc_anna_finch" => new DialogueResponseDto
            {
                ReplyText =
                    "I heard Mr. Bellamy arguing with someone in the Study not long before I found him, but I couldn't tell who the other person was.",
                Emotion = DialogueEmotion.Concerned
            },

            "npc_margaret_vale" => new DialogueResponseDto
            {
                ReplyText =
                    "I'll tell you what I know, but please be precise.",
                Emotion = DialogueEmotion.Calm
            },

            _ => new DialogueResponseDto
            {
                ReplyText =
                    "I'll answer what I can, Detective.",
                Emotion = DialogueEmotion.Neutral
            }
        };

        return response;
    }
}