using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Buffers.Text;
using System.Text.Json;

namespace Connector_Test
{
    class RestConnector : ConnectorTest.ITestConnector {

        private HttpClient _httpClient;
        private string _exchangeUrl = "https://api-pub.bitfinex.com/v2/";

        public RestConnector() {
            _httpClient = new HttpClient();
        }

       public async Task<IEnumerable<TestHQ.Trade>> GetNewTradesAsync(string pair, int maxCount) { 
            var url = $"{_exchangeUrl}trades/t{pair}/hist?limit={maxCount}";
            var response = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<IEnumerable<TestHQ.Trade>>(response);
       }

        public async Task<IEnumerable<TestHQ.Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0) {
            var url = $"{_exchangeUrl}candles/trade:{periodInSec}:{pair}:a{count}:p{from}:p{to}/hist";
            var response = await _httpClient.GetStringAsync(url);
            return JsonSerializer.Deserialize<IEnumerable<TestHQ.Candle>>(response);
        }

        public event Action<TestHQ.Trade> NewBuyTrade { add { } remove { } }
        public event Action<TestHQ.Trade> NewSellTrade { add { } remove { } }
        public event Action<TestHQ.Candle> CandleSeriesProcessing { add { } remove { } }
        public void SubscribeTrades(string pair, int maxCount = 100) => throw new NotImplementedException();
        public void UnsubscribeTrades(string pair) => throw new NotImplementedException();
        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0) => throw new NotImplementedException();
        public void UnsubscribeCandles(string pair) => throw new NotImplementedException();
    }
    
}
