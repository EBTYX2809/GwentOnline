using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gwent_Release.Models
{
    public class Card : INotifyPropertyChanged
    {        
        public object ObjectProperty => this; // Need to give Card object in converter binding                                              
        private int? _actualCardScore;
        public int? ActualCardScore
        {
            get => _actualCardScore;
            set
            {
                if (_actualCardScore != value)
                {
                    _actualCardScore = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ObjectProperty));
                }
            }
        }
        private CardInfo _cardInfo;
        public CardInfo CardInfo 
        {
            get => _cardInfo;
            set
            {
                if (_cardInfo != value) 
                {
                    _cardInfo = value;
                    OnPropertyChanged();

                }
            }
        }

        private string path = "pack://application:,,,/Images/";
        private string _image;
        public string Image
        {
            get => _image;
            set
            {
                if (_image != value) 
                {
                    _image = value;
                }
            }
        }
        public string JsonNameKey { get; private set; }
        public Fractions Fraction {  get; private set; }
        public BattleRows BattleRow { get; private set; }
        public Effect Effect { get; set; }
        public Card(string jsonNameKey, int? actualCardScore = null, Fractions fraction = 0, BattleRows battleRow = 0)
        {
            JsonNameKey = jsonNameKey;
            Image = path + JsonNameKey + ".jpg";
            Fraction = fraction;
            BattleRow = battleRow;
            ActualCardScore = actualCardScore;
        }

        public virtual Card CopyCard()
        {
            return new Card(JsonNameKey, ActualCardScore, Fraction, BattleRow) 
            {
                Effect = Effect, 
                CardInfo = CardInfo 
            };
        }

        public void CardUsed(Action switchTurn)
        {
            switchTurn?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
