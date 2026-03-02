using BridgeBot.Infrastructure;
using BridgeBot.Models;
using BridgeBot.Services;
using Microsoft.Extensions.Hosting;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    if (message.Text != null)
                    {
                        _discord.SendBridgeMessageAsync(message);
                    }
                    else if (message.FileURL != null) { 
                        _discord.SendBridgeFileAsync(message);
                    }
                                        


                    Console.WriteLine($"[WORKER LOG]: Получено сообщение!");
                    Console.WriteLine($"   От кого: {message.UserName} (ID: {message.ChatId})");
                    Console.WriteLine($"   Текст: {message.Text}");
                    if (message.AvatarUrl != null){ Console.WriteLine($"   AvatarUrl : {message.AvatarUrl}"); }


                }
            }
            catch (Exception ex)
            {
               
                Console.WriteLine($"[CRITICAL ERROR]: В работе воркера произошел сбой: {ex.Message}");
            }
        }
    }

}

