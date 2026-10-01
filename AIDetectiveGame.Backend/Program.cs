using System.Text.Json;
using AIDetectiveGame.Backend.Contracts;
using AIDetectiveGame.Backend.Services;

var builder = WebApplication.CreateBuilder(args);


// Keep JSON property names exactly as our C# DTO property names.
// Example: "ReplyText" instead of "replyText".
// Unity's JsonUtility already expects this existing contract.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});


// ------------------------------------------------------------
// Hosted LLM HttpClient
// ------------------------------------------------------------
//
// ASP.NET manages this HttpClient for us using IHttpClientFactory
// internally.
//
// We do not manually create a new HttpClient for every request.
builder.Services.AddHttpClient<OpenAICompatibleDialogueProvider>(
    (serviceProvider, client) =>
    {
        IConfiguration configuration =
            serviceProvider.GetRequiredService<IConfiguration>();


        string baseUrl =
            configuration["HostedLlm:BaseUrl"]
            ?? throw new InvalidOperationException(
                "HostedLlm:BaseUrl is missing.");


        int timeoutSeconds =
            configuration.GetValue<int?>(
                "HostedLlm:RequestTimeoutSeconds")
            ?? 10;


        if (timeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "HostedLlm:RequestTimeoutSeconds must be greater than zero.");
        }


        client.BaseAddress =
            new Uri(
                baseUrl.TrimEnd('/') + "/");


        client.Timeout =
            TimeSpan.FromSeconds(
                timeoutSeconds);
    });


// ------------------------------------------------------------
// Dialogue provider selection
// ------------------------------------------------------------

var dialogueProviderMode =
    builder.Configuration["DialogueProvider:Mode"]
    ?? "Mock";


if (dialogueProviderMode.Equals(
        "Mock",
        StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddSingleton<
        IDialogueProvider,
        MockDialogueProvider>();
}
else if (dialogueProviderMode.Equals(
             "Hosted",
             StringComparison.OrdinalIgnoreCase))
{
    // OpenAICompatibleDialogueProvider is a typed HttpClient.
    //
    // IDialogueProvider will resolve to that provider whenever
    // /api/dialogue needs it.
    builder.Services.AddTransient<IDialogueProvider>(
        serviceProvider =>
            serviceProvider.GetRequiredService<
                OpenAICompatibleDialogueProvider>());
}
else
{
    throw new InvalidOperationException(
        $"Unsupported DialogueProvider:Mode '{dialogueProviderMode}'. " +
        "Valid values are 'Mock' or 'Hosted'.");
}


var app = builder.Build();


// ------------------------------------------------------------
// Health endpoint
// ------------------------------------------------------------

app.MapGet("/health", () =>
{
    return Results.Text(
        "OK",
        "text/plain");
});


// ------------------------------------------------------------
// Dialogue endpoint
// ------------------------------------------------------------

app.MapPost(
    "/api/dialogue",
    async (
        DialogueRequestDto request,
        IDialogueProvider dialogueProvider,
        ILogger<Program> logger,
        CancellationToken cancellationToken) =>
    {
        // Basic request validation.
        if (string.IsNullOrWhiteSpace(
                request.ScenarioId))
        {
            return Results.BadRequest(
                new
                {
                    Error =
                        "ScenarioId is required."
                });
        }


        if (string.IsNullOrWhiteSpace(
                request.NpcId))
        {
            return Results.BadRequest(
                new
                {
                    Error =
                        "NpcId is required."
                });
        }


        if (string.IsNullOrWhiteSpace(
                request.PlayerMessage))
        {
            return Results.BadRequest(
                new
                {
                    Error =
                        "PlayerMessage is required."
                });
        }


        logger.LogInformation(
            "Received dialogue request. " +
            "ScenarioId: {ScenarioId}, NpcId: {NpcId}",
            request.ScenarioId,
            request.NpcId);


        try
        {
            DialogueResponseDto response =
                await dialogueProvider.GenerateReplyAsync(
                    request,
                    cancellationToken);


            return Results.Ok(response);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            // This covers provider/HttpClient timeout.
            logger.LogWarning(
                "Dialogue provider timed out. NpcId: {NpcId}",
                request.NpcId);


            return Results.Problem(
                statusCode: StatusCodes.Status504GatewayTimeout,
                title: "Dialogue provider timeout.",
                detail:
                    "The dialogue provider did not respond in time.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "Hosted dialogue HTTP failure. NpcId: {NpcId}",
                request.NpcId);


            return Results.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Dialogue provider unavailable.",
                detail:
                    "The dialogue provider could not return a response.");
        }
        catch (InvalidOperationException exception)
        {
            logger.LogError(
                exception,
                "Hosted dialogue response/configuration failure. " +
                "NpcId: {NpcId}",
                request.NpcId);


            return Results.Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Dialogue provider unavailable.",
                detail:
                    "The dialogue provider returned an invalid response.");
        }
    });


app.Run();