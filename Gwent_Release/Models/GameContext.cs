using Gwent_Release.Views;
using Gwent_Release.Models.RoundWinnerManagerNS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.Models
{
    public class GameContext : RoundWinnerManager, INotifyPropertyChanged
    {
        private static GameContext _instance = new GameContext();

        public static GameContext Instance => _instance;

        private Player _player1;
        public Player Player1
        {
            get => _player1;
            set
            {
                if (_player1 != value)
                {
                    _player1 = value;
                    OnPropertyChanged();
                }
            }
        }
        private Player _player2;
        public Player Player2
        {
            get => _player2;
            set
            {
                if (_player2 != value)
                {
                    _player2 = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public Player ActivePlayer { get; set; }
        public Player PassivePlayer { get; set; }
        public Player StarterPlayer { get; set; }
        private bool _isPlayer1Turn;
        public bool IsPlayer1Turn
        {
            get => _isPlayer1Turn;
            set
            {
                if(_isPlayer1Turn != value)
                {
                    _isPlayer1Turn = value;
                    OnPropertyChanged();
                }
            }
        }
        
        private ObservableCollection<WeatherCard> _weatherCardsBattleRow;
        public ObservableCollection<WeatherCard> WeatherCardsBattleRow
        {
            get => _weatherCardsBattleRow;
            set
            {
                if (_weatherCardsBattleRow != value) 
                {
                    _weatherCardsBattleRow = value;
                    OnPropertyChanged();
                }
            }
        }

        private RoundWinner roundWinner = new RoundWinner();

        public Action NewWeatherAdded { get; set; }
        public Func<string, Task> TurnAnnouncement { get; set; }        
        private GameContext()
        {
            Player1 = new Player();
            Player2 = new Player();
            ActivePlayer = Player1;
            PassivePlayer = Player2;
            WeatherCardsBattleRow = new ObservableCollection<WeatherCard>();
            WeatherCardsBattleRow.CollectionChanged += WeatherCardsBattleRow_CollectionChanged;
        }

        private void WeatherCardsBattleRow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NewWeatherAdded?.Invoke();
        }

        public List<BattleRow> GetAllPlayersRows()
        {
            return ActivePlayer?.PlayerBattleRows.Concat(PassivePlayer?.PlayerBattleRows).ToList();
        }

        public void SwitchTurn()
        {
            IsPlayer1Turn = !IsPlayer1Turn;

            if (IsPlayer1Turn)
            {
                ActivePlayer = Player1;
                PassivePlayer = Player2;
            }
            else
            {
                ActivePlayer = Player2;
                PassivePlayer = Player1;
            }
        }

        public void StartTurn(Card card)
        {
            if (card != null)
            {
                if (ActivePlayer == Player1)
                {
                    TurnManager.GiveTurn();
                }
                else
                {
                    TurnManager.ClearTurnInfo();
                }
                TurnAnnouncement?.Invoke($"{ActivePlayer.Name} finished turn.");
                card.CardUsed(EndTurn);
            }
            else
            {
                ActivePlayer.HasPassed = true;
                if (ActivePlayer == Player1)
                {
                    TurnManager.GiveTurn();
                }
                else
                {
                    TurnManager.ClearTurnInfo();
                }
                SwitchTurn();
                TurnAnnouncement?.Invoke($"{PassivePlayer.Name} passed.");
                EndTurn();
            }
        }

        private void EndTurn()
        {
            if (ActivePlayer.HasPassed && PassivePlayer.HasPassed)
            {
                EndRound();
            }
            else
            {
                if (!ActivePlayer.HasPassed && !PassivePlayer.HasPassed)
                {
                    SwitchTurn();
                }

                if (ActivePlayer != Player1)
                {
                    TurnManager.EnemyTurn();
                }
            }
        }

        private void EndRound()
        {
            if (Player1.GeneralScore > Player2.GeneralScore)
            {
                roundWinner.SetWinner(new Player1WinRound());
                roundWinner.WinRound();
            }
            else if (Player2.GeneralScore > Player1.GeneralScore)
            {
                roundWinner.SetWinner(new Player2WinRound());
                roundWinner.WinRound();
            }
            else if (Player1.GeneralScore == Player2.GeneralScore)
            {
                roundWinner.SetWinner(new DrawInRound());
                roundWinner.WinRound();
            }

            if (Player1.IsSecondRoundLoose || Player2.IsSecondRoundLoose)
            {
                string gameInfo = "GG\n";
                foreach (var round in RoundWinners)
                {
                    gameInfo += round + "\n";
                }
                MessageBox.Show(gameInfo);
                ReturnToMenuWindow(TurnManager.client);
                return;
            }
            else
            {
                StartNewRound();
            }
        }

        private void StartNewRound()
        {
            foreach (var row in Player1.PlayerBattleRows)
            {
                row.ClearBattleRow();
            }

            foreach (var row in Player2.PlayerBattleRows)
            {
                row.ClearBattleRow();
            }

            Player1.HasPassed = false;
            Player2.HasPassed = false;
            WeatherCardsBattleRow.Clear();

            if (StarterPlayer == Player1)
            {
                ActivePlayer = Player2;
                PassivePlayer = Player1;
                IsPlayer1Turn = false;

                TurnManager.EnemyTurn();
            }
            else if (StarterPlayer == Player2)
            {
                ActivePlayer = Player1;
                PassivePlayer = Player2;
                IsPlayer1Turn = true;
            }
        }

        private static void RestartGameContext()
        {
            string name = _instance.Player1.Name;
            _instance = new GameContext();
            _instance.Player1.Name = name;
        }

        public void ReturnToMenuWindow(Client client)
        {
            Window currentWindow = Application.Current.MainWindow;

            var menuWindow = new Menu();
            Application.Current.MainWindow = menuWindow;
            menuWindow.Show();

            RestartGameContext();

            MessageBox.Show("Returning to the menu.");

            client.SendInfo("Disconnect");

            try
            {
                currentWindow?.Close();
            }
            catch (Exception ex) { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
