using System.Net.Http.Headers;
using System.Text.Json;
using Replicate;
using Telegram.Bot;
using Telegram.Bot.Types;

// List of allowed users by telegram user id
var ALLOWED_USER_IDs = new List<long>{
    
};
var LastUpdateId = 0;
var builder = WebApplication.CreateBuilder(args);
var botUrl = builder.Configuration["BotUrl"]!;   // set your bot webhook public url in appsettings.json
var botToken = builder.Configuration["BotToken"]!;             // set your bot token in appsettings.json
var replicateToken = builder.Configuration["ReplicateToken"]!;   // set your replicate token in appsettings.json
var webhookUrl = botUrl + "/bot";
var replicateWebhook = botUrl + "/replicate-webhook";

var predictionList = new Dictionary<string, (TelegramBotClient bot, Message msg)>();

HttpClient http = new()
{
    BaseAddress = new Uri("https://api.replicate.com/v1/"),
    DefaultRequestHeaders = {
        Authorization = new AuthenticationHeaderValue("Bearer", replicateToken)
    }
};

builder.Services.ConfigureTelegramBot<Microsoft.AspNetCore.Http.Json.JsonOptions>(opt => opt.SerializerOptions);
builder.Services.AddHttpClient("tgwebhook").RemoveAllLoggers().AddTypedClient(httpClient => new TelegramBotClient(botToken, httpClient));
var app = builder.Build();
app.UseHttpsRedirection();

app.MapGet("/bot/setWebhook", async (TelegramBotClient bot) => { await bot.SetWebhookAsync(webhookUrl); return $"Webhook set to {webhookUrl}"; });
app.MapPost("/bot", OnUpdate);
app.MapPost("/replicate-webhook", OnReplicateComplete);
app.Run("http://localhost:5000");

async void OnUpdate(TelegramBotClient bot, Update update)
{
    if (update.Id <= LastUpdateId) return;
    LastUpdateId = update.Id;

    if (update.Message is null) return;			// we want only updates about new Message
    var msg = update.Message;

    if (!ValidateUser(msg.Chat.Id))
    {
        _ = bot.SendTextMessageAsync(msg.Chat, "You're not allowed 🙅🏽‍♂️");
    };

    if (msg.Voice is null)
    {
        _ = bot.SendTextMessageAsync(msg.Chat, "Please, send a Voice message we can process 🗣️");
        return;
    };

    var fileId = msg.Voice?.FileId ?? string.Empty;
    var fileInfo = await bot.GetFileAsync(fileId);
    var filePath = fileInfo.FilePath;
    var fileUri = $"https://api.telegram.org/file/bot{botToken}/{filePath}";
    // _ = bot.SendTextMessageAsync(msg.Chat, $"Processing request... \n\nFile should be available at: {fileUri}");

    //make replicate call
    var reqBody = new
    {
        version = "3ab86df6c8f54c11309d4d1f930ac292bad43ace52d10c80d87eb258b3c9f79c",
        webhook = replicateWebhook,
        webhook_events_filter = new List<string> { "completed" }, // optional
        input = new
        {
            audio = fileUri,
            batch_size = 64
        }
    };
    var req = await http.PostAsJsonAsync("predictions", reqBody);
    req.EnsureSuccessStatusCode();
    var reqContentString = await req.Content.ReadAsStringAsync();
    var predictionObject = JsonSerializer.Deserialize<UnfinishedPredition>(reqContentString);
    predictionList.Add(predictionObject!.id, new(bot, msg));

    //TODO: check if after 8 seconds prediction is not complete, send assurance message
    _ = Task.Delay(8000).ContinueWith((task) =>
    {
        if (predictionList.ContainsKey(predictionObject!.id))
        {
            _ = bot.SendTextMessageAsync(msg.Chat, "Your transcription is on its way...");
        }
    });
}

void OnReplicateComplete(CompletedPredition prediction)
{
    Console.WriteLine("Prediction is complete: {0}", prediction.status);
    if (predictionList.TryGetValue(prediction.id, out var tuple))
    {
        var (bot, msg) = tuple;
        _ = bot.SendTextMessageAsync(
            msg.Chat,
            $"_{prediction.output.text.Trim()}_",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyParameters: msg.MessageId
        );
        predictionList.Remove(prediction.id);
    }
}

bool ValidateUser(long Id)
{
    return ALLOWED_USER_IDs.Contains(Id);
}