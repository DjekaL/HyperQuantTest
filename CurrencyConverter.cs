using System;
using System.Net.Http;
using System.Text.Json;

namespace Connector_Test {
    public class CurrencyConverter {
        private HttpClient _httpClient;
        private string _exchangeUrl = "https://api-pub.bitfinex.com/v2/tickers?symbols=";
        private Dictionary<string, decimal> _balance;
        private Dictionary<string, decimal> _price;
        public CurrencyConverter() {
            _httpClient = new HttpClient();
            _balance = new Dictionary<string, decimal> {
                {"BTC", 1},
                {"XRP", 15000},
                {"XMR", 50},
                {"DSH", 30}
            };
        }
        public string GetCurrancies() {
            return string.Join(",", _balance.Keys.ToList().Select(x => $"t{x}USD"));
        }
        public async Task<Dictionary<string, decimal>> GetCurrencyPriceAsync(string cur) {
            var response = await _httpClient.GetStringAsync($"{_exchangeUrl}{cur}");
            var doc = JsonSerializer.Deserialize<object[]>(response);
            _price = new Dictionary<string, decimal>();
            foreach (var arr in doc) {
                var elm = JsonSerializer.Deserialize<object[]>(arr.ToString());
                _price.Add(elm[0].ToString().Replace("t", "").Replace("USD", ""), decimal.Parse(elm[1].ToString().Replace(".", ",")));
            }
            return Convert();
        }

        private Dictionary<string, decimal> Convert() {
            var totalBalance = _balance.Where(x =>  _price.ContainsKey(x.Key)).Sum(x => x.Value * _price[x.Key]);
            return new Dictionary<string, decimal>( _price.ToDictionary(x => x.Key, x => totalBalance / x.Value));
        }
    }
}
