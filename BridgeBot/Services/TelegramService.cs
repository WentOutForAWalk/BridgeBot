using BridgeBot.Infrastructure;
using BridgeBot.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BridgeBot.Services
{
    public class TelegramService
    {
        private readonly TelegramBotClient _client;
        private readonly MessageBus _messageBus;
        private string _token;
        public TelegramService(string token, MessageBus messageBus)
        {
            _client = new TelegramBotClient(token);
            _messageBus = messageBus;
            _token = token;
        }

        // Старт Бота
        public void start()
        {
            _client.StartReceiving(UpdateHandler, ErrorHandler);
        }

        // Метод который вызывается при ошибке
        private async Task ErrorHandler(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine("[TG]: Ошибка: " + exception.Message);
            await Task.CompletedTask;
        }


        // Метод который вызывается при сообщении боту ||||||||||||||||||||||||||||||||||||||||||||||||||||
        private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
        {

            if (update.Message == null)
            {
                return;
            }

            string? avatarUrl = null;
            
            var userId = update.Message.From!.Id;

            if (update.Message is not { } message)
                return;


            //2.Запрашиваем фото профиля
            var userPhotos = await client.GetUserProfilePhotos(userId, limit: 1, cancellationToken: token);

            if (userPhotos.TotalCount > 0)
            {
                var _userPhotos = userPhotos.Photos[0].Last().FileId;

                var _userPhotosFile = await client.GetFile(_userPhotos, cancellationToken: token);

                avatarUrl = $"https://api.telegram.org/file/bot{_token}/{_userPhotosFile.FilePath}";
            }


            var username = update.Message.From?.Username ?? "User";
            





            if (update.Message is { Text: { } text } msg)
            {   
                try
                {

                    if (userPhotos.TotalCount > 0)
                    {
                        var _userPhotos = userPhotos.Photos[0].Last().FileId;

                        var _userPhotosFile = await client.GetFile(_userPhotos, cancellationToken: token);

                        avatarUrl = $"https://api.telegram.org/file/bot{_token}/{_userPhotosFile.FilePath}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при получении фото: {ex.Message}");
                }


                
                _messageBus.Push(new BridgeMessage(msg.Chat.Id, username, text, avatarUrl, null));

                Console.WriteLine($"[TG]: Сообщение от {username} отправлено в воркер.");
            }
            
            string? fileId = null;

            if (update.Message.Photo != null)
            {
                    fileId = update.Message.Photo.Last().FileId;
            }
            else if (update.Message.Video != null)
            {
                    fileId = update.Message.Video.FileId;
            }
            else if (update.Message.Animation != null)
            {
                    fileId = update.Message.Animation.FileId;
            }
            else if (update.Message.Document != null)
            {
                    fileId = update.Message.Document.FileId;
            }


            if (fileId != null)
            {
                var file = await client.GetFile(fileId);

                string fileUrl = $"https://api.telegram.org/file/bot{_token}/{file.FilePath}";


                if (update.Message.Caption != null) {
                    _messageBus.Push(new BridgeMessage(update.Message.Chat.Id, username, update.Message.Caption, avatarUrl, null)); 
                }

                _messageBus.Push(new BridgeMessage(update.Message.Chat.Id, username, null, avatarUrl, fileUrl));



                await _client.SendChatAction(
                    chatId: update.Message.Chat.Id,
                    action: ChatAction.Typing,
                    cancellationToken: token
                );

            }



            
            


            await Task.CompletedTask;
        }
    }
}
