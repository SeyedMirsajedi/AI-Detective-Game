using AIDetectiveGame.Backend.Contracts;

namespace AIDetectiveGame.Backend.Services;

public interface IDialogueProvider
{
    Task<DialogueResponseDto> GenerateReplyAsync(
        DialogueRequestDto request,
        CancellationToken cancellationToken);
}