
using BridgeBot.Models;
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BridgeBot.Services
{
    public class DiscordService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _webhookUrl;

        public DiscordService(string webhookUrl)
        {
            _webhookUrl = webhookUrl;
        }

        public async Task SendBridgeMessageAsync(BridgeMessage message)
        {
            // Формируем объект для Discord
            var payload = new
            {
                username = message.UserName, 
                content = message.Text,      
                avatar_url = message.AvatarUrl 
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync(_webhookUrl, content);
                response.EnsureSuccessStatusCode();
                Console.WriteLine($"[Discord]: Сообщение от {message.UserName} отправлено.");
                Console.WriteLine("--------------------------------------------");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discord Error]: {ex.Message}");
            }
        }

        public async Task SendBridgeFileAsync(BridgeMessage message)
        {
            // Скачиваем файл по url...
            byte[] fileBytes = await _httpClient.GetByteArrayAsync(message.FileURL);
            
            using var form = new MultipartFormDataContent();

            // добавляем json
            var json = JsonSerializer.Serialize(new { username = message.UserName, avatar_url = message.AvatarUrl });
            form.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");

            var fileContent = new ByteArrayContent(fileBytes);
            
            // if else добавить если понадобиться ...
            form.Add(fileContent, "file", Path.GetFileName(message.FileURL));
            await _httpClient.PostAsync(_webhookUrl, form);
        }
    }
}
