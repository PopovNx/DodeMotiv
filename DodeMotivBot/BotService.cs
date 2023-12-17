using Demotivatogen;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DodeMotivBot;

public class BotService(ILogger<BotService> logger, ITelegramBotClient telegramBot) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        telegramBot.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), new ReceiverOptions
            {
                ThrowPendingUpdates = true,
                AllowedUpdates = new[] { UpdateType.Message },
            },
            cancellationToken);
        logger.LogInformation("Bot started");
        return Task.CompletedTask;
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error while handling update");
        return Task.CompletedTask;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Chat: { Type: ChatType.Private, Id: var chatId }, MessageId: var messageId })
        {
            return;
        }

        if (update.Message is not { Photo: [.., var photo], Caption: var text, Caption.Length: > 0 })
        {
            await bot.SendTextMessageAsync(chatId, "Отправьте одну фотографию с подписью",
                cancellationToken: cancellationToken);
            return;
        }

        var textParts = text.Split('\n');
        if (textParts is not { Length: 1 or 2 })
        {
            await bot.SendTextMessageAsync(chatId, "Напишите текст 1 или 2 строками",
                cancellationToken: cancellationToken);
            return;
        }

        logger.LogInformation("Processing image {FileId}", photo.FileId);
        var mainText = textParts[0];
        var subText = textParts.Length == 2 ? textParts[1] : null;

        using var inputImage = new MemoryStream();
        await using var output = new MemoryStream();

        await bot.GetInfoAndDownloadFileAsync(photo.FileId, inputImage, cancellationToken: cancellationToken);
        inputImage.Seek(0, SeekOrigin.Begin);

        await ImageProcessor.CreateImageAsync(inputImage, output, mainText, subText);
        output.Seek(0, SeekOrigin.Begin);

        var inputMedia = new InputFileStream(output);
        await bot.SendPhotoAsync(chatId, inputMedia, replyToMessageId: messageId,
            cancellationToken: cancellationToken);
        logger.LogInformation("Image {FileId} processed", photo.FileId);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}