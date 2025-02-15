using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Gwent_Release.Models
{
    public class Player : INotifyPropertyChanged
    {
        private string path = "pack://application:,,,/Images/";
        public Fractions fraction { get; set; }
        private string _name;
        public string Name
        {
            get => _name;
            set 
            {
                if (_name != value) 
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }            
        public string _deckName;
        public string DeckName
        {
            get => _deckName;
            set
            {
                if (_deckName != value)
                {
                    _deckName = value;
                    OnPropertyChanged();
                }
            }
        }
        private int _generalScore;
        public int GeneralScore
        {
            get => _generalScore;
            set 
            {
                if(_generalScore != value)
                {
                    _generalScore = value;
                    OnPropertyChanged();
                }
            }
        }
        private Deck _deck;
        public Deck Deck
        {
            get => _deck;
            set
            {
                if (_deck != value)
                {
                    _deck = value;
                    OnPropertyChanged();
                }
            }
        }
        private Hand _hand;
        public Hand Hand
        {
            get => _hand;
            set
            {
                if (_hand != value) 
                {
                    _hand = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<Card> _discard;
        public ObservableCollection<Card> Discard
        {
            get => _discard;
            set
            {
                if (_discard != value)
                {
                    _discard = value;
                    OnPropertyChanged();
                }
            }
        }
        public Card _higherDiscardCard;
        public Card HigherDiscardCard
        {
            get => _higherDiscardCard;
            private set
            {
                if (_higherDiscardCard != value)
                {
                    _higherDiscardCard = value;
                    OnPropertyChanged();
                }
            }
        }
        private void OnDiscardChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateHigherDiscardCard();
        }
        private void UpdateHigherDiscardCard()
        {
            HigherDiscardCard = Discard.Count > 0 ? Discard.Last() : null;
        }
        public List<BattleRow> PlayerBattleRows { get; set; }

        private BattleRow _meleeBattleRow;
        public BattleRow MeleeBattleRow
        {
            get => _meleeBattleRow;
            set 
            {
                if (_meleeBattleRow != value) 
                {
                    _meleeBattleRow = value;
                    OnPropertyChanged();
                }
            }
        }
        private BattleRow _middleBattleRow;
        public BattleRow MiddleBattleRow
        {
            get => _middleBattleRow;
            set
            {
                if (_middleBattleRow != value)
                {
                    _middleBattleRow = value;
                    OnPropertyChanged();
                }
            }
        }
        private BattleRow _siegeBattleRow;
        public BattleRow SiegeBattleRow
        {
            get => _siegeBattleRow;
            set
            {
                if (_siegeBattleRow != value)
                {
                    _siegeBattleRow = value;
                    OnPropertyChanged();
                }
            }
        }
        public ActionCard _leader;
        public ActionCard Leader
        {
            get => _leader;
            set 
            {
                if(_leader != value)
                {
                    _leader = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isFirstRoundWon;
        public bool IsFirstRoundWon
        {
            get => _isFirstRoundWon;
            set
            {
                if (value != _isFirstRoundWon)
                {
                    _isFirstRoundWon = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _isSecondRoundWon;
        public bool IsSecondRoundWon
        {
            get => _isSecondRoundWon;
            set
            {
                if (value != _isSecondRoundWon)
                {
                    _isSecondRoundWon = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _hasPassed;
        public bool HasPassed
        {
            get => _hasPassed;
            set
            {
                if (_hasPassed != value)
                {
                    _hasPassed = value;
                    OnPropertyChanged();
                }
            }
        }
        public Player() 
        {
            IsFirstRoundWon = false;
            IsSecondRoundWon = false;
            HasPassed = false;
            Deck = new Deck ();
            Hand = new Hand ();
            fraction = Fractions.NorthKingdoms;
            Discard = new ObservableCollection<Card> ();
            MeleeBattleRow = new BattleRow(BattleRows.MeleeBattleRow);
            MiddleBattleRow = new BattleRow(BattleRows.MiddleBattleRow);
            SiegeBattleRow = new BattleRow(BattleRows.SiegeBattleRow);
            PlayerBattleRows = new List<BattleRow> { MeleeBattleRow, MiddleBattleRow, SiegeBattleRow };

            _discard.CollectionChanged += OnDiscardChanged;

            foreach (var row in PlayerBattleRows)
            {
                row.CardAddedToBattleRow += ScoringGeneralScore;
            }
            foreach (var row in PlayerBattleRows)
            {
                row.CardRemovedFromBattleRow += MoveCardToDiscard;
            }
        }        

        public void ScoringGeneralScore()
        {
            GeneralScore = 0;
            foreach (var battleRow in PlayerBattleRows)
            {
                GeneralScore += battleRow.BatteRowScore;
            }
        }

        public void MoveCardToDiscard(Card card)
        {
            ScoringGeneralScore();
            Discard.Add(card);
        }

        public void CreateDeck(Fractions fraction)
        {
            var copiedCards = CardsStore.NeutralDeck.Select(c => c.CopyCard()).ToList();

            foreach (var card in copiedCards)
            {               
                Deck.DeckCards.Add(card);
            }

            if (fraction == Fractions.NorthKingdoms)
            {
                copiedCards = CardsStore.NorthKingdomsDeck.Select(c => c.CopyCard()).ToList();
                foreach (var card in copiedCards)
                {
                    Deck.DeckCards.Add(card); 
                }
            }
            else if(fraction == Fractions.Nilfgaard)
            {
                copiedCards = CardsStore.NilfgaardDeck.Select(c => c.CopyCard()).ToList();
                foreach (var card in copiedCards)
                {
                    Deck.DeckCards.Add(card);
                }
            }

            Leader = Deck.DeckCards.Last() as ActionCard;
            Deck.DeckCards.Remove(Leader);

            var random = new Random();

            var shuffledCards = Deck.DeckCards.OrderBy(x => random.Next()).ToList();

            Deck.DeckCards.Clear();
            foreach (var card in shuffledCards)
            {
                Deck.DeckCards.Add(card);
            }

            DeckName = path + fraction.ToString() + "Cover.jpg";
            TakeCard(10);
        }

        public void CreateDeck(string cards)
        {
            string[] recievedDeck = cards.Split('|');

            List<Card> deck = new List<Card>();

            foreach (string cardName in recievedDeck)
            {
                deck.Add(
                    CardsStore.NeutralDeck.Concat(CardsStore.NorthKingdomsDeck.Concat(CardsStore.NilfgaardDeck)).ToList().
                    Find(_card => _card.JsonNameKey == cardName).CopyCard());
            }

            Leader = deck.First() as ActionCard;
            deck.Remove(Leader);

            for (int i = 0; i < deck.Count; i++)
            {
                if (i < 10)
                {
                    Hand.HandCards.Add(deck[i]);
                }
                else
                {
                    Deck.DeckCards.Add(deck[i]);
                }
            }

            DeckName = path + Leader.Fraction.ToString() + "Cover.jpg";
        }

        public void TakeCard(int count)
        {
            try
            {
                for (int i = 0; i < count; i++)
                {
                    Hand.HandCards.Add(Deck.DeckCards.First());
                    Deck.DeckCards.Remove(Deck.DeckCards.First());
                }
            }
            catch (Exception InvalidOperationException)
            {
                MessageBox.Show("Deck is empty.");
                return;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
