using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class BackendDialogueService : MonoBehaviour, IDialogueService
{
    [Header("Backend Connection")]

    [SerializeField]
    private string backendBaseUrl = "http://localhost:5031";

    [SerializeField]
    [Min(1)]
    private int requestTimeoutSeconds = 10;

    public async Task<DialogueResponse> GenerateReplyAsync(DialogueRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        string requestUrl = BuildDialogueUrl();

        string requestJson = JsonUtility.ToJson(request);

        byte[] requestBody = Encoding.UTF8.GetBytes(requestJson);

        using (UnityWebRequest webRequest =
               new UnityWebRequest(requestUrl, UnityWebRequest.kHttpVerbPOST))
        {
            webRequest.uploadHandler = new UploadHandlerRaw(requestBody);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            webRequest.SetRequestHeader(
                "Content-Type",
                "application/json");

            webRequest.timeout = Mathf.Max(1, requestTimeoutSeconds);

            await SendWebRequestAsync(webRequest);

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                string errorMessage = string.IsNullOrWhiteSpace(webRequest.error)
                    ? "Unknown backend error."
                    : webRequest.error;

                throw new InvalidOperationException(
                    $"Dialogue backend request failed. " +
                    $"HTTP status: {webRequest.responseCode}. " +
                    $"Error: {errorMessage}");
            }

            string responseJson = webRequest.downloadHandler.text;

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                throw new InvalidOperationException(
                    "Dialogue backend returned an empty response.");
            }

            DialogueResponse response;

            try
            {
                response =
                    JsonUtility.FromJson<DialogueResponse>(responseJson);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Dialogue backend returned malformed JSON.",
                    exception);
            }

            if (response == null)
            {
                throw new InvalidOperationException(
                    "Dialogue backend response could not be read.");
            }

            if (string.IsNullOrWhiteSpace(response.ReplyText))
            {
                throw new InvalidOperationException(
                    "Dialogue backend response contained no ReplyText.");
            }

            return response;
        }
    }

    private string BuildDialogueUrl()
    {
        if (string.IsNullOrWhiteSpace(backendBaseUrl))
        {
            throw new InvalidOperationException(
                "Backend Base URL has not been configured.");
        }

        string cleanBaseUrl = backendBaseUrl.Trim().TrimEnd('/');

        return cleanBaseUrl + "/api/dialogue";
    }

    private static Task<UnityWebRequest> SendWebRequestAsync(
        UnityWebRequest webRequest)
    {
        UnityWebRequestAsyncOperation operation =
            webRequest.SendWebRequest();

        if (operation.isDone)
        {
            return Task.FromResult(webRequest);
        }

        TaskCompletionSource<UnityWebRequest> completionSource =
            new TaskCompletionSource<UnityWebRequest>();

        operation.completed += _ =>
        {
            completionSource.TrySetResult(webRequest);
        };

        return completionSource.Task;
    }
}