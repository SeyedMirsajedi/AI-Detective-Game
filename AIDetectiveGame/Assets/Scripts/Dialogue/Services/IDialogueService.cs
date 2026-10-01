using System.Threading.Tasks;

public interface IDialogueService
{
    Task<DialogueResponse> GenerateReplyAsync(
        DialogueRequest request);
}