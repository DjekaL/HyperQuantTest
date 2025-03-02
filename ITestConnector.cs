using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConnectorTest
{
    interface ITestConnector
    {
        #region Rest

        Task<IEnumerable<TestHQ.Trade>> GetNewTradesAsync(string pair, int maxCount);
        Task<IEnumerable<TestHQ.Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0);

        #endregion

        #region Socket


        event Action<TestHQ.Trade> NewBuyTrade;
        event Action<TestHQ.Trade> NewSellTrade;
        void SubscribeTrades(string pair, int maxCount = 100);
        void UnsubscribeTrades(string pair);

        event Action<TestHQ.Candle> CandleSeriesProcessing;
        void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0);
        void UnsubscribeCandles(string pair);

        #endregion

    }
}
