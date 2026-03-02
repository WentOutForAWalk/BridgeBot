using System.Threading.Channels;
using BridgeBot.Models;




namespace BridgeBot.Infrastructure
{
    public class MessageBus
    {
        private readonly Channel<BridgeMessage> _channel = Channel.CreateUnbounded<BridgeMessage>();

        public void Push(BridgeMessage message)
        {
            _channel.Writer.TryWrite(message);
        }

        public IAsyncEnumerable<BridgeMessage> ReadAllAsync(CancellationToken ct)
        {
            return _channel.Reader.ReadAllAsync(ct);
        }

    }
}

