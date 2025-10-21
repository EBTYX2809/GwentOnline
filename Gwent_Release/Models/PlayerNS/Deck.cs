using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.Models.PlayerNS
{
    public class Deck : INotifyPropertyChanged
    {
        private ObservableCollection<Card> _deckCards;
        public ObservableCollection<Card> DeckCards
        {
            get => _deckCards;
            set
            {
                if (_deckCards != value)
                {
                    _deckCards = value;
                    OnPropertyChanged();
                }
            }
        }

        public int CardsCount => DeckCards.Count;

        private void DeckCards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CardsCount));
        }

        public Deck()
        {
            DeckCards = new ObservableCollection<Card>();
            DeckCards.CollectionChanged += DeckCards_CollectionChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
