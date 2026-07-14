using BridgeBot.Infrastructure;
using BridgeBot.Models;
using BridgeBot.Services;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BridgeBot.Workers
{
    public class BridgeWorker : BackgroundService
    {
        private readonly MessageBus _messageBus;
        private readonly DiscordService _discord;

        public BridgeWorker(MessageBus messageBus, DiscordService discord)
        {
            _messageBus = messageBus;
            _discord = discord;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[SYSTEM]: BridgeWorker успешно запущен и слушает очередь...");

            try
            {
                await foreach (var message in _messageBus.ReadAllAsync(stoppingToken))
                {
                    Console.WriteLine($"[WORKER LOG]: Получено сообщение от {message.UserName} (ID: {message.ChatId}) для обработки.");

                    try
                    {
                        // 1. Если это текстовое сообщение
                        if (message.Text != null)
                        {
                            await _discord.SendBridgeMessageAsync(message);
                        }
                        // 2. Если это медиафайл (картинка, гифка, видео, аудио)
                        else if (message.FileURL != null)
                        {
                            await _discord.SendBridgeFileAsync(message);
                        }

                        // Если всё ушло успешно, выводим логи в консоль
                        Console.WriteLine($"[WORKER LOG]: Сообщение успешно обработано и отправлено в Discord!");
                        Console.WriteLine($" Текст: {message.Text}");
                        if (message.AvatarUrl != null)
                        {
                            Console.WriteLine($" AvatarUrl : {message.AvatarUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Если ByeDPI отвалился и выдал ошибку туннеля — ловим её здесь
                        Console.WriteLine($"[Discord Error]: {ex.Message}. Ошибка отправки через ByeDPI. Возвращаю сообщение в шину...");

                        // Возвращаем сообщение обратно в твою очередь (в трубу)
                        _messageBus.Push(message);

                        // Асинхронно ждем 2 секунды, чтобы локальный ByeDPI оклемался и пробил туннель
                        await Task.Delay(2000, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL ERROR]: В работе воркера произошел сбой: {ex.Message}");
            }
        }
    }
}
