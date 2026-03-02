using Microsoft.Extensions.Configuration;

using BridgeBot.Infrastructure;
using BridgeBot.Services;
using BridgeBot.Workers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();


string TelegramBotToken = config["Cfg:TelegramToken"];
string WebHookUrl = config["Cfg:DiscordWebhook"];



// 1. Создаем каркас приложения
var builder = Host.CreateApplicationBuilder(args);

// 2. Труба
builder.Services.AddSingleton<MessageBus>();


// 4. Telegram сервис
builder.Services.AddSingleton<TelegramService>(sp =>
    new TelegramService(TelegramBotToken, sp.GetRequiredService<MessageBus>()));

// Discord сервис
builder.Services.AddSingleton<DiscordService>(new DiscordService(WebHookUrl));


// 3. Рабочий...
builder.Services.AddHostedService<BridgeWorker>();

// 5. Собираем всё в объект host
var host = builder.Build();

// 6. Достаем созданный объект TelegramService из контейнера и вызываем метод start();
var tgService = host.Services.GetRequiredService<TelegramService>();
tgService.start();

// 7. Запуск всего приложения
host.Run();