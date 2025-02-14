using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Gwent_Release.Views;

namespace Gwent_Release.Models
{
    public static class GameContext
    {
        public static Player Player1 { get; set; }
        public static Player Player2 { get; set; }
        public static Player ActivePlayer { get; set; }
        public static Player PassivePlayer { get; set; }
        public static Player StarterPlayer { get; set; }
        public static bool IsPlayer1Turn { get; set; }
        public static ObservableCollection<WeatherCard> WeatherCardsBattleRow { get; set; } = new ObservableCollection<WeatherCard>(); 
        public static Action NewWeatherAdded { get; set; }       
        public static Func<string, Task> TurnAnnouncement { get; set; }
        public static List<string> RoundWinners = new List<string>();
        static GameContext() 
        {
            Player1 = new Player();                                    
            Player2 = new Player();                        
            ActivePlayer = Player1;
            PassivePlayer = Player2;
            WeatherCardsBattleRow.CollectionChanged += WeatherCardsBattleRow_CollectionChanged;
        }

        private static void WeatherCardsBattleRow_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NewWeatherAdded?.Invoke();
        }

        public static List<BattleRow> GetAllPlayersRows()
        {
            return ActivePlayer?.PlayerBattleRows.Concat(PassivePlayer?.PlayerBattleRows).ToList();
        }        

        public static void SwitchTurn()
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

        private static void EndRoundAnnouncement(string announcement)
        {
            TurnAnnouncement?.Invoke(announcement);
            RoundWinners.Add(announcement);
        }

        public static void StartTurn(Card card)
        {
            if(card != null)
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

        private static void EndTurn()
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

        private static void EndRound()
        {
            // Bullshit
            if (Player1.GeneralScore > Player2.GeneralScore)
            {
                if (!Player2.IsFirstRoundWon) Player2.IsFirstRoundWon = true;
                else Player2.IsSecondRoundWon = true;

                EndRoundAnnouncement($"{Player1.Name} win round. Score {Player1.GeneralScore} - {Player2.GeneralScore}.");

                if (Player1.Leader.Fraction == Fractions.NorthKingdoms) Player1.TakeCard(1);                                                   
            }
            else if (Player2.GeneralScore > Player1.GeneralScore)
            {
                if (!Player1.IsFirstRoundWon) Player1.IsFirstRoundWon = true;
                else Player1.IsSecondRoundWon = true;

                EndRoundAnnouncement($"{Player2.Name} win round. Score {Player2.GeneralScore} - {Player1.GeneralScore}.");

                if (Player2.Leader.Fraction == Fractions.NorthKingdoms) Player2.TakeCard(1);                                                               
            }
            else if (Player1.GeneralScore == Player2.GeneralScore)
            {
                if (Player1.Leader.Fraction == Fractions.Nilfgaard && Player2.Leader.Fraction == Fractions.Nilfgaard
                    || Player1.Leader.Fraction == Fractions.NorthKingdoms && Player2.Leader.Fraction == Fractions.NorthKingdoms)
                {
                    if (!Player1.IsFirstRoundWon) Player1.IsFirstRoundWon = true;
                    else Player1.IsSecondRoundWon = true;
                    if (!Player2.IsFirstRoundWon) Player2.IsFirstRoundWon = true;
                    else Player2.IsSecondRoundWon = true;

                    EndRoundAnnouncement($"Draw in round. Score {Player1.GeneralScore} - {Player2.GeneralScore}.");                    
                }
                else if (Player1.Leader.Fraction == Fractions.Nilfgaard)
                {
                    if (!Player2.IsFirstRoundWon) Player2.IsFirstRoundWon = true;                                                                
                    else Player2.IsSecondRoundWon = true;

                    EndRoundAnnouncement($"{Player1.Name} win round by his fraction ability. " +
                                         $"Score {Player1.GeneralScore} - {Player2.GeneralScore}.");
                }
                else if(Player2.Leader.Fraction == Fractions.Nilfgaard)
                {
                    if (!Player1.IsFirstRoundWon) Player1.IsFirstRoundWon = true;                                       
                    else Player1.IsSecondRoundWon = true;

                    EndRoundAnnouncement($"{Player2.Name} win round by his fraction ability. " +
                                         $"Score {Player2.GeneralScore} - {Player1.GeneralScore}.");
                }
            }

            if (Player1.IsSecondRoundWon || Player2.IsSecondRoundWon)
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
        
        private static void StartNewRound()
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
            string name = Player1.Name;
            Player1 = new Player() { Name = name };
            Player2 = new Player();
            ActivePlayer = Player1;
            PassivePlayer = Player2;
            WeatherCardsBattleRow.Clear();
        }

        public static void ReturnToMenuWindow(Client client)
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
    }
}
