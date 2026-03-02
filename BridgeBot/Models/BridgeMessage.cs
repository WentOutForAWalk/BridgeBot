namespace BridgeBot.Models
{

    // Структура для передачи сообщения.
    public record BridgeMessage(
        long ChatId,      
        string UserName,  
        string? Text,      
        string? AvatarUrl ,
        string? FileURL
    );
}
