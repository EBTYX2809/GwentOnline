using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gwent_Release.Models
{
    public class Hand : INotifyPropertyChanged
    {        
        private ObservableCollection<Card> _handCards;
        public ObservableCollection<Card> HandCards
        {
            get => _handCards;
            set
            {
                if (_handCards != value)
                {
                    _handCards = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CardsCount => HandCards.Count;

        private void HandCards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CardsCount));
        }

        public Hand()
        {
            HandCards = new ObservableCollection<Card>();
            HandCards.CollectionChanged += HandCards_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
