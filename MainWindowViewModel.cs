using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using TestHQ;

namespace Connector_Test {
    public class MainWindowViewModel : INotifyPropertyChanged {
        ObservableCollection<Trade> _tradesData = new();
        public ObservableCollection<Trade> TradesData {
            get { return _tradesData; }
            set {
                _tradesData = value;
                OnPropertyChanged();
            }
        }

        ObservableCollection<Candle> _candelsData = new();
        public ObservableCollection<Candle> CandelsData {
            get { return _candelsData; }
            set {
                _candelsData = value;
                OnPropertyChanged();
            }
        }
        Dictionary<string, decimal> _currancy = new();
        public Dictionary<string, decimal> Currancy {
            get { return _currancy; }
            set {
                _currancy = value;
                OnPropertyChanged();
            }
        }

        SocketConnector _tradeCon = new SocketConnector();
        SocketConnector _candelCon = new SocketConnector();
        CurrencyConverter _curConverter; 

        public MainWindowViewModel() {
            //Trades
            _tradeCon = new SocketConnector();
            BindingOperations.EnableCollectionSynchronization(TradesData, this);
            _tradeCon.NewBuyTrade += trade => TradesData.Add(trade);
            _tradeCon.NewSellTrade += trade => TradesData.Add(trade);

            //Candels
            _candelCon = new SocketConnector();
            BindingOperations.EnableCollectionSynchronization(CandelsData, this);
            _candelCon.CandleSeriesProcessing += candel => CandelsData.Add(candel);

            _curConverter = new CurrencyConverter();
            

            _ = Run();
            
        }

        public async Task Run() {
            await _tradeCon.ConnectAsync();
            await _candelCon.ConnectAsync();
            _tradeCon.SubscribeTrades("tBTCUSD");
            _candelCon.SubscribeCandles("tBTCUSD", 60, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, 5);

            Currancy = await _curConverter.GetCurrencyPriceAsync(_curConverter.GetCurrancies());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
