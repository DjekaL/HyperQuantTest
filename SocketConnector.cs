using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text.Json;
using TestHQ;

namespace Connector_Test
{
    class SocketConnector : ConnectorTest.ITestConnector
    {
        private ClientWebSocket _socket;
        private Uri _exchangeUrl = new ("ws://api-pub.bitfinex.com/v2/");

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public SocketConnector() {
            _socket = new ClientWebSocket();
            _ = Task.Run(ReceiveMessagesAsync);
        }

        public async Task ConnectAsync() {
            await _socket.ConnectAsync(_exchangeUrl, default);
        }

        public void SubscribeTrades(string pair, int maxCount = 100) {
           SendMessageAsync($"{{\"event\":\"subscribe\", \"channel\":\"trades\", \"symbol\":\"t{pair}\"}}");
        }

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0) {
            SendMessageAsync($"{{\"event\":\"subscribe\", \"channel\":\"candles\", \"key\":\"trade:{periodInSec}:t{pair}\"}}");
        }

        public async void UnsubscribeTrades(string pair) { 
            SendMessageAsync($"{{\"event\":\"unsubscribe\", \"channel\":\"trades\", \"symbol\":\"t{pair}\"}}");
        }

        public async void UnsubscribeCandles(string pair) {
            SendMessageAsync($"{{\"event\":\"unsubscribe\", \"channel\":\"candles\", \"symbol\":\"t{pair}\"}}");
        }

        private async Task SendMessageAsync(string message) {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, new CancellationTokenSource().Token);
        }

        private async Task ReceiveMessagesAsync() {
            var data = new byte[4000];
            var response = await _socket.ReceiveAsync(new ArraySegment<byte>(data), new CancellationTokenSource().Token);
            var msg = Encoding.UTF8.GetString(data, 0, response.Count);
        }

        public Task<IEnumerable<TestHQ.Trade>> GetNewTradesAsync(string pair, int maxCount) => throw new NotImplementedException();
        public Task<IEnumerable<TestHQ.Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0) => throw new NotImplementedException();
    }
}
