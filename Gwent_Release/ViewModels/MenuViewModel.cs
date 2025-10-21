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
                    isNamePlaceholderVisible = string.IsNullOrEmpty(_playerName);
                }
            }
        }

        private bool _isNamePlaceholderVisible;
        public bool isNamePlaceholderVisible
        {
            get => _isNamePlaceholderVisible;
            set
            {
                if (_isNamePlaceholderVisible != value)
                {
                    _isNamePlaceholderVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _IPAddress;
        public string IPAddress
        {
            get => _IPAddress;
            set
            {
                if (_IPAddress != value)
                {
                    _IPAddress = value;
                    OnPropertyChanged();
                    isNamePlaceholderVisible = string.IsNullOrEmpty(_IPAddress);
                }
            }
        }

        private bool _isIPAddressPlaceholderVisible;
        public bool isIPAddressPlaceholderVisible
        {
            get => _isIPAddressPlaceholderVisible;
            set
            {
                if (_isIPAddressPlaceholderVisible != value)
                {
                    _isIPAddressPlaceholderVisible = value;
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

        public List<string> Languages { get; } = new List<string>() { "English", "Russian", "Ukrainian" };

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                    SetLanguage();
                }
            }
        }

        public ICommand PlayCommand { get; }
        public ICommand PickFractionCommand { get; }

        public MenuViewModel()
        {
            PlayCommand = new RelayCommand(Play);
            PickFractionCommand = new RelayCommand(PickFraction);

            CardsList = new List<Card>(CardsStore.NeutralDeck
                            .Concat(CardsStore.NorthKingdomsDeck
                            .Concat(CardsStore.NilfgaardDeck)));

            if (playerName != null) isNamePlaceholderVisible = false;
            else isNamePlaceholderVisible = true;
            isMenuVisible = true;
            isCardsListVisible = false;
        }

        private async void Play(object parameter)
        {
            if (!string.IsNullOrEmpty(_playerName))
            {
                Client client = new Client();

                client.Connect(IPAddress ?? "http://gwent-server.duckdns.org", 10000); // 127.0.0.1

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

        private void SetLanguage()
        {
            switch (SelectedLanguage)
            {
                case "English":
                    LanguageManager.SetLanguage("EN");
                    break;
                case "Russian":
                    LanguageManager.SetLanguage("RU");
                    break;
                case "Ukrainian":
                    LanguageManager.SetLanguage("UA");
                    break;
                default:
                    LanguageManager.SetLanguage("EN");
                    break;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
