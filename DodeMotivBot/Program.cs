using DodeMotivBot;
using Telegram.Bot;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<BotService>();

if (builder.Configuration.GetValue<string>("BotToken") is not ( { } token))
{
    throw new InvalidOperationException("Bot token is not specified");
}

builder.Services.AddTransient<ITelegramBotClient, TelegramBotClient>(_ =>
    new TelegramBotClient(token));

var host = builder.Build();
host.Run();