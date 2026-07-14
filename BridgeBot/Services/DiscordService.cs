using BridgeBot.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
            var payload = new { username = message.UserName, content = message.Text, avatar_url = message.AvatarUrl };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"[Discord]: Сообщение от {message.UserName} отправлено.");
            Console.WriteLine("--------------------------------------------");
        }

        public async Task SendBridgeFileAsync(BridgeMessage message)
        {
            byte[] fileBytes = await _httpClient.GetByteArrayAsync(message.FileURL);
            using var form = new MultipartFormDataContent();
            
            var json = JsonSerializer.Serialize(new { username = message.UserName, avatar_url = message.AvatarUrl });
            form.Add(new StringContent(json, Encoding.UTF8, "application/json"), "payload_json");

            var fileContent = new ByteArrayContent(fileBytes);
            form.Add(fileContent, "file", Path.GetFileName(message.FileURL));

            var response = await _httpClient.PostAsync(_webhookUrl, form);
            response.EnsureSuccessStatusCode();
        }
    }
}
