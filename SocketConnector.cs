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
        private ClientWebSocket _socket = new ClientWebSocket();
        private Uri _exchangeUrl = new("wss://api-pub.bitfinex.com/ws/2");

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public SocketConnector() {
            _socket = new ClientWebSocket();
            _ = Task.Run(ReceiveMessagesAsync);
        }

        public async Task ConnectAsync() {
            await _socket.ConnectAsync(_exchangeUrl, CancellationToken.None);
            _ = Task.Run(ReceiveMessagesAsync);
        }

        public async Task DisconnectAsync() {
            if (_socket != null && (_socket.State == WebSocketState.Open || _socket.State == WebSocketState.CloseReceived)) {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            _socket?.Dispose();
        }

        public void SubscribeTrades(string pair, int maxCount = 100) {
           SendMessageAsync($"{{\"event\":\"subscribe\", \"channel\":\"trades\", \"symbol\":\"{pair}\"}}");
        }

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0) {
            //SendMessageAsync($"{{\"event\":\"subscribe\", \"channel\":\"candles\", \"key\":\"trade:{periodInSec}:{pair}\"}}");
            SendMessageAsync($"{{\"event\":\"subscribe\", \"channel\":\"candles\", \"key\":\"trade:1m:{pair}\"}}");
        }

        public async void UnsubscribeTrades(string pair) { 
            SendMessageAsync($"{{\"event\":\"unsubscribe\", \"channel\":\"trades\", \"symbol\":\"{pair}\"}}");
        }

        public async void UnsubscribeCandles(string pair) {
            SendMessageAsync($"{{\"event\":\"unsubscribe\", \"channel\":\"candles\", \"symbol\":\"{pair}\"}}");
        }

        private async Task SendMessageAsync(string message) {
            if (_socket.State == WebSocketState.Open) {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, new CancellationTokenSource().Token);
            }
        }

        private async Task ReceiveMessagesAsync() {
            while (_socket.State == WebSocketState.Open) {
                var data = new byte[90000];
                var response = await _socket.ReceiveAsync(new ArraySegment<byte>(data), new CancellationTokenSource().Token);
                var msg = Encoding.UTF8.GetString(data, 0, response.Count);
                HandleMessage(msg);
            }
        }

        private string tradePair;
        private string candlePair;

        private void HandleMessage(string data) {
            var doc = JsonDocument.Parse(data);
            string key = string.Empty;
            try {
                 key = doc.RootElement.GetProperty("key").GetString();
                string pair = key.Split(':')[2].TrimStart('t').TrimEnd(':');
                candlePair = pair;
            }
            catch { }
            try {
                key = doc.RootElement.GetProperty("pair").GetString();
                tradePair = key;
            }
            catch { }
            try {
                var elements = JsonSerializer.Deserialize<object[]>(data);
                var chanelId = elements[0].ToString();
                var json = JsonSerializer.Deserialize<object[]>(elements[1].ToString());
                foreach (var arr in json) {
                    var elm = JsonSerializer.Deserialize<decimal[]>(arr.ToString());
                    if (elm.Count() == 4) {
                        var trade = new Trade {
                            Id = elm[0].ToString(),
                            Pair = tradePair,
                            Price = elm[3],
                            Amount = Math.Abs(elm[2]),
                            Side = elm[2] > 0 ? "buy" : "sell",
                            Time = DateTimeOffset.FromUnixTimeMilliseconds((long)elm[1])
                        };
                        if (trade.Side == "buy") {
                            NewBuyTrade?.Invoke(trade);
                        } else {
                            NewSellTrade?.Invoke(trade);
                        }
                    }
                    if (elm.Count() == 6) {
                        var candle = new Candle {
                            Pair = candlePair,
                            OpenTime = DateTimeOffset.FromUnixTimeMilliseconds((long)elm[0]),
                            OpenPrice = elm[1],
                            ClosePrice = elm[2],
                            HighPrice = elm[3],
                            LowPrice = elm[4],
                            TotalVolume = elm[5],
                            TotalPrice = elm[5] * (elm[3] * elm[4] / 2)
                        };
                        CandleSeriesProcessing?.Invoke(candle);
                    }
                }
            }
            catch { }
        } 

        public Task<IEnumerable<TestHQ.Trade>> GetNewTradesAsync(string pair, int maxCount) => throw new NotImplementedException();
        public Task<IEnumerable<TestHQ.Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0) => throw new NotImplementedException();
    }
}
