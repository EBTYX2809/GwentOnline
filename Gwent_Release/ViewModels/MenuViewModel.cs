using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using Gwent_Release.Views;
using Gwent_Release.Models;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.ViewModels
{
    public class MenuViewModel : INotifyPropertyChanged
    {
        private string _playerName => GameContext.Instance.Player1.Name;
        public string playerName
        {
            get => _playerName;
            set
            {
                if (_playerName != value)
                {
                    GameContext.Instance.Player1.Name = value;
                    OnPropertyChanged();
                    isPlaceholderVisible = string.IsNullOrEmpty(_playerName);
                }
            }
        }

        private bool _isPlaceholderVisible;
        public bool isPlaceholderVisible
        {
            get => _isPlaceholderVisible;
            set
            {
                if (_isPlaceholderVisible != value)
                {
                    _isPlaceholderVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _setLanguageToggleButton;
        public bool setLanguageToggleButton
        {
            get => _setLanguageToggleButton;
            set
            {
                if (_setLanguageToggleButton != value)
                {
                    _setLanguageToggleButton = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _pickFractionToggleButton;
        public bool pickFractionToggleButton
        {
            get => _pickFractionToggleButton;
            set
            {
                if (_pickFractionToggleButton != value)
                {
                    _pickFractionToggleButton = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isMenuVisible;
        public bool isMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                if (_isMenuVisible != value)
                {
                    _isMenuVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<Card> _cardsList;
        public List<Card> CardsList
        {
            get => _cardsList;
            set
            {
                if (_cardsList != value)
                {
                    _cardsList = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isCardsListVisible;
        public bool isCardsListVisible
        {
            get => _isCardsListVisible;
            set
            {
                if (_isCardsListVisible != value)
                {
                    _isCardsListVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand PickFractionCommand { get; }
        public ICommand SetLanguageCommand { get; }

        public MenuViewModel()
        {
            PlayCommand = new RelayCommand(Play);
            PickFractionCommand = new RelayCommand(PickFraction);
            SetLanguageCommand = new RelayCommand(SetLanguage);

            CardsList = new List<Card>(CardsStore.NeutralDeck
                            .Concat(CardsStore.NorthKingdomsDeck
                            .Concat(CardsStore.NilfgaardDeck)));

            if (playerName != null) isPlaceholderVisible = false;
            else isPlaceholderVisible = true;
            isMenuVisible = true;
            isCardsListVisible = false;
        }

        private async void Play(object parameter)
        {
            if (!string.IsNullOrEmpty(_playerName))
            {
                Client client = new Client();

                client.Connect("35.229.67.170", 10000); // 127.0.0.1 // 35.229.67.170

                isMenuVisible = false;

                bool isConnected = await client.WaitingSecondPlayer();

                isMenuVisible = true;

                if (isConnected)
                {
                    Window currentWindow = Application.Current.MainWindow;

                    var mainWindow = new MainWindow(client);
                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();

                    currentWindow?.Close();
                }
            }
            else
            {
                MessageBox.Show("Enter name.");
            }
        }

        private void PickFraction(object parameter)
        {
            if (!pickFractionToggleButton) GameContext.Instance.Player1.fraction = Fractions.NorthKingdoms;
            else GameContext.Instance.Player1.fraction = Fractions.Nilfgaard;
        }

        private void SetLanguage(object parameter)
        {
            if (!setLanguageToggleButton) LanguageManager.SetLanguage("EN");
            else LanguageManager.SetLanguage("RU");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
