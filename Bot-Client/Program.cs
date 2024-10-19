using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Replicate;

var token = Environment.GetEnvironmentVariable("TOKEN");
token ??= "";
var replicateToken = "";

HttpClient http = new()
{
    BaseAddress = new Uri("https://api.replicate.com/v1/"),
    DefaultRequestHeaders = {
        Authorization = new AuthenticationHeaderValue("Bearer", replicateToken)
    }
};

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var me = await bot.GetMeAsync();
var allowedUserIds = new List<long>{
};
await bot.DropPendingUpdatesAsync();
bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;


Console.WriteLine($"@{me.Username} is running... Press Escape to terminate");
while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
cts.Cancel(); // stop the bot


async Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine(exception);
    await Task.Delay(2000, cts.Token);
}

async Task OnMessage(Message msg, UpdateType type)
{
    if (!allowedUserIds.Contains(msg.Chat.Id))
    {
        _ = bot.SendTextMessageAsync(msg.Chat, "Not in the list... no vas para el getto");
        return;
    }

    string fileId = string.Empty;

    if (msg.Text is not { } text)
    {
        fileId = msg.Voice?.FileId ?? string.Empty;
        var fileInfo = await bot.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;
        var fileUri = $"https://api.telegram.org/file/bot{token}/{filePath}";
        _ = bot.SendTextMessageAsync(msg.Chat, $"Processing request... \n\nFile should be available at: {fileUri}");
        //make replicate call
        var reqBody = /* JsonSerializer.Serialize( */new
        {
            version = "3ab86df6c8f54c11309d4d1f930ac292bad43ace52d10c80d87eb258b3c9f79c",
            input = new
            {
                audio = fileUri,
                batch_size = 64
            }
        };//);
        // var reqContent = new StringContent(reqBody, Encoding.UTF8, "application/json");
        var req = await http.PostAsJsonAsync("predictions", reqBody);
        req.EnsureSuccessStatusCode();
        var reqContentString = await req.Content.ReadAsStringAsync();
        var predictionObject = JsonSerializer.Deserialize<Prediction>(reqContentString);
        
        // _ = bot.SendTextMessageAsync(msg.Chat, $"Here's your transcribed message:\n\n{textResult.output.text}");
        // Console.WriteLine($"File available at https://api.telegram.org/file/bot{token}/{filePath}");
    }
}

async Task OnTextMessage(Message msg) // received a text message that is not a command
{
    Console.WriteLine($"Received text '{msg.Text}' in {msg.Chat}");
    await OnCommand("/start", "", msg); // for now we redirect to command /start
}

async Task OnCommand(string command, string args, Message msg)
{
    Console.WriteLine($"Received command: {command} {args}");
    switch (command)
    {
        case "/start":
            await bot.SendTextMessageAsync(msg.Chat, """
                <b><u>Bot menu</u></b>:
                /photo [url]    - send a photo <i>(optionally from an <a href="https://picsum.photos/310/200.jpg">url</a>)</i>
                /inline_buttons - send inline buttons
                /keyboard       - send keyboard buttons
                /remove         - remove keyboard buttons
                /poll           - send a poll
                """, parseMode: ParseMode.Html, linkPreviewOptions: true,
                replyMarkup: new ReplyKeyboardRemove()); // also remove keyboard to clean-up things
            break;
        case "/photo":
            if (args.StartsWith("http"))
                await bot.SendPhotoAsync(msg.Chat, args, caption: "Source: " + args);
            else
            {
                await bot.SendChatActionAsync(msg.Chat, ChatAction.UploadPhoto);
                await Task.Delay(2000); // simulate a long task
                await using var fileStream = new FileStream("bot.gif", FileMode.Open, FileAccess.Read);
                await bot.SendPhotoAsync(msg.Chat, fileStream, caption: "Read https://telegrambots.github.io/book/");
            }
            break;
        case "/inline_buttons":
            var inlineMarkup = new InlineKeyboardMarkup()
                .AddNewRow("1.1", "1.2", "1.3")
                .AddNewRow()
                    .AddButton("WithCallbackData", "CallbackData")
                    .AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://github.com/TelegramBots/Telegram.Bot"));
            await bot.SendTextMessageAsync(msg.Chat, "Inline buttons:", replyMarkup: inlineMarkup);
            break;
        case "/keyboard":
            var replyMarkup = new ReplyKeyboardMarkup(true)
                .AddNewRow("1.1", "1.2", "1.3")
                .AddNewRow().AddButton("2.1").AddButton("2.2");
            await bot.SendTextMessageAsync(msg.Chat, "Keyboard buttons:", replyMarkup: replyMarkup);
            break;
        case "/remove":
            await bot.SendTextMessageAsync(msg.Chat, "Removing keyboard", replyMarkup: new ReplyKeyboardRemove());
            break;
        case "/poll":
            await bot.SendPollAsync(msg.Chat, "Question", ["Option 0", "Option 1", "Option 2"], isAnonymous: false, allowsMultipleAnswers: true);
            break;
    }
}

async Task OnUpdate(Update update)
{
    await (update switch
    {
        { CallbackQuery: { } callbackQuery } => OnCallbackQuery(callbackQuery),
        { PollAnswer: { } pollAnswer } => OnPollAnswer(pollAnswer),
        _ => OnUnhandledUpdate(update)
    });
}

async Task OnCallbackQuery(CallbackQuery callbackQuery)
{
    await bot.AnswerCallbackQueryAsync(callbackQuery.Id, $"You selected {callbackQuery.Data}");
    await bot.SendTextMessageAsync(callbackQuery.Message!.Chat.Id, $"Received callback from inline button {callbackQuery.Data}");
}

async Task OnPollAnswer(PollAnswer pollAnswer)
{
    if (pollAnswer.User != null)
        await bot.SendTextMessageAsync(pollAnswer.User.Id, $"You voted for option(s) id [{string.Join(',', pollAnswer.OptionIds)}]");
}

async Task OnUnhandledUpdate(Update update) => Console.WriteLine($"Received unhandled update {update.Type}");

/*

REPLICATE SUCCESSFUL TRASNCRIPTION RESPONSE
*Processing:
{
  "completed_at": null,
  "created_at": "2024-07-17T20:23:35.038000Z",
  "data_removed": false,
  "error": null,
  "id": "eza20ejnfsrgm0cgr9esbb0bc0",
  "input": {
    "task": "transcribe",
    "audio": "https://api.telegram.org/file/bot___:AAHTJ3x9zgk_6yPTohn1KAp2LQClfh6lkDs/voice/file_0.oga",
    "language": "None",
    "timestamp": "chunk",
    "batch_size": 64,
    "diarise_audio": false
  },
  "logs": null,
  "metrics": {},
  "output": null,
  "started_at": null,
  "status": "starting",
  "urls": {
    "get": "https://api.replicate.com/v1/predictions/eza20ejnfsrgm0cgr9esbb0bc0",
    "cancel": "https://api.replicate.com/v1/predictions/eza20ejnfsrgm0cgr9esbb0bc0/cancel"
  },
  "version": "3ab86df6c8f54c11309d4d1f930ac292bad43ace52d10c80d87eb258b3c9f79c"
}

*Completed:
{
  "completed_at": "2024-07-17T20:23:42.238002Z",
  "created_at": "2024-07-17T20:23:35.038000Z",
  "data_removed": false,
  "error": null,
  "id": "eza20ejnfsrgm0cgr9esbb0bc0",
  "input": {
    "task": "transcribe",
    "audio": "https://api.telegram.org/file/bot___:AAHTJ3x9zgk_6yPTohn1KAp2LQClfh6lkDs/voice/file_0.oga",
    "language": "None",
    "timestamp": "chunk",
    "batch_size": 64,
    "diarise_audio": false
  },
  "logs": "Voila!✨ Your file has been transcribed!",
  "metrics": {
    "predict_time": 1.413151469,
    "total_time": 7.200002
  },
  "output": {
    "text": " Chamo, mira, esas dos medias son de las que tú pusiste ahí para dar también.",
    "chunks": [
      {
        "text": " Chamo, mira, esas dos medias son de las que tú pusiste ahí para dar también.",
        "timestamp": [
          0,
          4.66
        ]
      }
    ]
  },
  "started_at": "2024-07-17T20:23:40.824850Z",
  "status": "succeeded",
  "urls": {
    "get": "https://api.replicate.com/v1/predictions/eza20ejnfsrgm0cgr9esbb0bc0",
    "cancel": "https://api.replicate.com/v1/predictions/eza20ejnfsrgm0cgr9esbb0bc0/cancel"
  },
  "version": "3ab86df6c8f54c11309d4d1f930ac292bad43ace52d10c80d87eb258b3c9f79c"
}
*/

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);