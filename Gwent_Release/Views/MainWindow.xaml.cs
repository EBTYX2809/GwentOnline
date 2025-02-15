using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Gwent_Release.Models;

namespace Gwent_Release.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private Client client;

        private List<ItemsControl> ItemsControlBattleRows = new List<ItemsControl>();
        private List<ItemsControl> OponentItemsControlBattleRows = new List<ItemsControl>();
        private List<Border> BattleRowHornSlots = new List<Border>();

        private TaskCompletionSource<UnitCard> _cardSelectionTaskSource;
        private TaskCompletionSource<Card> _muliganCard;
        private CancellationTokenSource cancellationToken;

        private UIElement draggedCardVisual;
        private Point lastMousePosition;
        public MainWindow(Client _client)
        {
            client = _client;
            TurnManager.client = client;

            InitializeComponent();
            InitializeRows();
            OponentInitializeRows();
            InitializeHornSlots();
            MusicBG.Initialize(bgMusic);

            foreach (var row in GameContext.Player1.PlayerBattleRows)
            {
                row.GiveMedicCardToContext += ReturnRevivedCard;
            }
            GameContext.NewWeatherAdded += PaintRowsByWeather;
            GameContext.TurnAnnouncement += ShowAnnouncement;
            TurnManager.ActivateLeader += DeactivatePlayer2Leader;
            
            Loaded += MainWindow_Loaded;                     
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {                       
            MusicBG.PlayNextTrack();

            GameContext.Player1.CreateDeck(GameContext.Player1.fraction);

            cancellationToken = new CancellationTokenSource();
            try
            {
                await StartMuligan(cancellationToken.Token);
            }
            catch (OperationCanceledException) { }

            GameContext.ActivePlayer = GameContext.Player2;            
            await ShowAnnouncement("Awaiting oponent and preparing decks.");

            await client.SwapDeck(GameContext.Player1.Hand.HandCards.Concat(GameContext.Player1.Deck.DeckCards).ToList());
            GameContext.ActivePlayer = GameContext.Player1;

            await client.TossCoin();
            await ShowAnnouncement($"{GameContext.ActivePlayer.Name} start game.");
            if (GameContext.ActivePlayer != GameContext.Player1)
            {
                TurnManager.EnemyTurn();
            }
        }

        private async Task ShowAnnouncement(string text)
        {
            AnnouncementBox.Text = text;
            AnnouncementBox.Visibility = Visibility.Visible;
            await Task.Delay(1000);
            AnnouncementBox.Text = null;
            AnnouncementBox.Visibility = Visibility.Collapsed;
            PlayersTurnMarksVisibilitySwitcher();

        }

        private void PlayersTurnMarksVisibilitySwitcher()
        {
            if (GameContext.IsPlayer1Turn)
            {
                Player1TurnMark.Visibility = Visibility.Visible;
                Player2TurnMark.Visibility = Visibility.Collapsed;
            }
            else
            {
                Player2TurnMark.Visibility = Visibility.Visible;
                Player1TurnMark.Visibility = Visibility.Collapsed;
            }
        }

        private async Task StartMuligan(CancellationToken token)
        {
            ShowDiscard(null);
            HideDiscard.Content = "Accept";

            DiscardMenu.ItemsSource = GameContext.Player1.Hand.HandCards;

            for (int i = 0; i < 2; i++)
            {
                token.ThrowIfCancellationRequested();
                _muliganCard = new TaskCompletionSource<Card>();

                using (token.Register(() => _muliganCard.TrySetCanceled()))
                {
                    Card selectedCard = await _muliganCard.Task;
                    GameContext.Player1.Hand.HandCards.Remove(selectedCard);
                    GameContext.Player1.Deck.DeckCards.Add(selectedCard);
                    var cardForReplace = GameContext.Player1.Deck.DeckCards.First();
                    GameContext.Player1.Hand.HandCards.Add(cardForReplace);
                    GameContext.Player1.Deck.DeckCards.Remove(cardForReplace);
                }
            }

            DiscardMenu.ItemsSource = GameContext.Player1.Discard;
            HideDiscard.Content = "Hide discard";
            ShowDiscardMenu.Visibility = Visibility.Collapsed;
            GameDesk.Effect = null;
        }

        private void InitializeRows()
        {
            ItemsControlBattleRows.Add(Player1MeleeBattleRow);
            ItemsControlBattleRows.Add(Player1MiddleBattleRow);
            ItemsControlBattleRows.Add(Player1SiegeBattleRow);           
            ItemsControlBattleRows[0].Tag = BattleRows.MeleeBattleRow;
            ItemsControlBattleRows[1].Tag = BattleRows.MiddleBattleRow;
            ItemsControlBattleRows[2].Tag = BattleRows.SiegeBattleRow;

            ItemsControlBattleRows.Add(WeatherCardsBattleRow);
            ItemsControlBattleRows.ElementAt(3).Tag = BattleRows.WeatherCardsBattleRow;
        }
        private void OponentInitializeRows()
        {
            OponentItemsControlBattleRows.Add(Player2MeleeBattleRow);
            OponentItemsControlBattleRows.Add(Player2MiddleBattleRow);
            OponentItemsControlBattleRows.Add(Player2SiegeBattleRow);
            OponentItemsControlBattleRows[0].Tag = BattleRows.MeleeBattleRow;
            OponentItemsControlBattleRows[1].Tag = BattleRows.MiddleBattleRow;
            OponentItemsControlBattleRows[2].Tag = BattleRows.SiegeBattleRow;
        }
        private void InitializeHornSlots()
        {
            BattleRowHornSlots.Add(Player1MeleeBattleRowHornSlot);
            BattleRowHornSlots.Add(Player1MiddleBattleRowHornSlot);
            BattleRowHornSlots.Add(Player1SiegeBattleRowHornSlot);
        }

        // Colors
        private SolidColorBrush DefaultColor = Brushes.Transparent;
        private SolidColorBrush CardPickColor = new SolidColorBrush(Color.FromArgb(64, 179, 255, 0));
        private SolidColorBrush FrostColor = new SolidColorBrush(Color.FromArgb(64, 0, 255, 255));
        private SolidColorBrush FogColor = new SolidColorBrush(Color.FromArgb(64, 177, 189, 189));
        private SolidColorBrush RainColor = new SolidColorBrush(Color.FromArgb(64, 0, 0, 255));
        private SolidColorBrush CardPickPlusWeatherColor = new SolidColorBrush(Color.FromArgb(64, 217, 255, 128));

        private Card GetCard(DragEventArgs e)
        {
            var type = e.Data.GetFormats();
            return e.Data.GetData(type.First()) as Card;
        }        

        private void HighlightBattleRow(Card card)
        {
            if (card.Effect == EffectModifiersStore.Spy)
            {
                var oponentRow = OponentItemsControlBattleRows
                    .FirstOrDefault(row => row.Tag is BattleRows battleRowType && battleRowType == card.BattleRow);

                if (oponentRow != null && oponentRow.Background == DefaultColor)
                {
                    oponentRow.Background = CardPickColor;
                    return;
                }
                else if (oponentRow?.Background == FrostColor 
                      || oponentRow?.Background == FogColor 
                      || oponentRow?.Background == RainColor)
                {
                    oponentRow.Background = CardPickPlusWeatherColor;
                    return;
                }
            }

            if (card is WeatherCard weatherCard)
            {
                var weatherRow = ItemsControlBattleRows
                    .FirstOrDefault(row => row.Tag is BattleRows battleRowType && battleRowType == weatherCard.DropBattleRow);
                if (weatherRow != null)
                {
                    weatherRow.Background = CardPickColor;
                    return;
                }
            }

            if (card is ActionCard horn && horn.Effect == EffectModifiersStore.Horn)
            {
                foreach (var hornSlot in BattleRowHornSlots)
                {
                    if (hornSlot.Child is ContentControl contentControl && contentControl.Content == null)
                    {                        
                        hornSlot.Background = CardPickColor;                        
                    }                    
                }
                return;
            }
                        
            var targetRow = ItemsControlBattleRows
                .FirstOrDefault(row => row.Tag is BattleRows battleRowType && battleRowType == card.BattleRow);

            if (targetRow != null && targetRow.Background == DefaultColor)
            {
                targetRow.Background = CardPickColor;
            }
            else if (targetRow?.Background == FrostColor || targetRow?.Background == FogColor || targetRow?.Background == RainColor)
            {
                targetRow.Background = CardPickPlusWeatherColor;
            }
        }

        private void UnHighlightBattleRow()
        {
            var allRows = ItemsControlBattleRows.Concat(OponentItemsControlBattleRows).ToList();

            foreach (var row in allRows)
            {
                if (row.Background == CardPickColor)
                {
                    row.Background = DefaultColor;                                        
                }
                else if(row.Background == CardPickPlusWeatherColor)
                {
                    var battleRow = GameContext.ActivePlayer.PlayerBattleRows
                        .Find(_battleRow => row.Tag is BattleRows battleRowType && _battleRow.BattleRowType == battleRowType);

                    if (battleRow.EffectModifiers.Contains(EffectModifiersStore.Frost))
                    {
                        row.Background = FrostColor;
                    }
                    else if (battleRow.EffectModifiers.Contains(EffectModifiersStore.Fog))
                    {
                        row.Background = FogColor;
                    }
                    else if (battleRow.EffectModifiers.Contains(EffectModifiersStore.Rain))
                    {
                        row.Background = RainColor;
                    }
                }
            }
            foreach (var hornSlot in BattleRowHornSlots)
            {
                if (hornSlot.Background == CardPickColor)
                {
                    hornSlot.Background = DefaultColor;
                }
            }
        }

        private void PaintRowsByWeather()
        {
            if (GameContext.WeatherCardsBattleRow.Count() > 0)
            {
                foreach (WeatherCard weatherCard in GameContext.WeatherCardsBattleRow)
                {
                    foreach (var row in GameContext.GetAllPlayersRows())
                    {
                        if (row.BattleRowType == weatherCard.ActionBattleRow)
                        {
                            if (!row.EffectModifiers.Contains(weatherCard.Effect as EffectModifier))
                            {
                                row.AddEffectModifier(weatherCard.Effect as EffectModifier);
                            }
                            else
                            {
                                continue;
                            }

                            var visualRow1 = ItemsControlBattleRows
                                .FirstOrDefault(_row => _row.Tag is BattleRows battleRowType 
                                && battleRowType == weatherCard.ActionBattleRow);

                            var visualRow2 = OponentItemsControlBattleRows
                                .FirstOrDefault(_row => _row.Tag is BattleRows battleRowType 
                                && battleRowType == weatherCard.ActionBattleRow);

                            if (weatherCard.Effect == EffectModifiersStore.Frost)
                            {
                                visualRow1.Background = FrostColor;
                                visualRow2.Background = FrostColor;
                            }
                            else if (weatherCard.Effect == EffectModifiersStore.Fog)
                            {
                                visualRow1.Background = FogColor;
                                visualRow2.Background = FogColor;
                            }
                            else if (weatherCard.Effect == EffectModifiersStore.Rain)
                            {
                                visualRow1.Background = RainColor;
                                visualRow2.Background = RainColor;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var row in ItemsControlBattleRows)
                {
                    row.Background = DefaultColor;
                }

                foreach (var row in OponentItemsControlBattleRows)
                {
                    row.Background = DefaultColor;
                }
            }
        }



        ////////////////////////////////////////////////////////////////////////////////////////////

        private void ShowDiscard(List<Card> cardsForShow)
        {
            DiscardMenu.ItemsSource = cardsForShow;
            var blur = new BlurEffect();
            blur.Radius = 10;
            GameDesk.Effect = blur;
            ShowDiscardMenu.Visibility = Visibility.Visible;            
        }

        public async Task<UnitCard> ReturnRevivedCard(List<Card> cardsForRevive)
        {          
            if (cardsForRevive.Count() > 0)
            {
                ShowDiscard(cardsForRevive);
                HideDiscard.Visibility = Visibility.Collapsed;

                _cardSelectionTaskSource = new TaskCompletionSource<UnitCard>();

                UnitCard selectedCard = await _cardSelectionTaskSource.Task;

                DiscardMenu.ItemsSource = GameContext.Player1.Discard;
                HideDiscard.Visibility = Visibility.Visible;
                ShowDiscardMenu.Visibility = Visibility.Collapsed;
                GameDesk.Effect = null;

                return selectedCard;
            }
            else return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GameContext.ActivePlayer == GameContext.Player1)
            {
                var card = sender as Grid;

                var itemsControl = FindParentItemsControl(card);

                if (itemsControl == Hand)
                {
                    HighlightBattleRow(((Grid)sender).DataContext as Card);

                    draggedCardVisual = CreateCardVisual(card);
                    lastMousePosition = e.GetPosition(GameDesk);

                    GameDesk.Children.Add(draggedCardVisual);

                    Canvas.SetLeft(draggedCardVisual, lastMousePosition.X);
                    Canvas.SetTop(draggedCardVisual, lastMousePosition.Y);

                    DragDrop.DoDragDrop(card, card.DataContext, DragDropEffects.Move);

                    GameDesk.Children.Remove(draggedCardVisual);
                    draggedCardVisual = null;
                    UnHighlightBattleRow();
                }
                else if (itemsControl == DiscardMenu && HideDiscard.Visibility == Visibility.Collapsed) // For Medic
                {
                    _cardSelectionTaskSource.SetResult(((Grid)sender).DataContext as UnitCard);

                    _cardSelectionTaskSource = null;

                    return;
                }
                else if (itemsControl == DiscardMenu && HideDiscard.Content == "Accept") // For Mulligan
                {
                    _muliganCard.SetResult(((Grid)sender).DataContext as Card); 
                    
                    _muliganCard = null;

                    return;
                }
                else return;
            }
            else return;
        }

        private void CardGrid_Drop(object sender, DragEventArgs e)
        {
            var sourceCard = ((Grid)sender).DataContext as Card;

            if (FindParentItemsControl(sender as Grid) != Hand)
            {
                var card = GetCard(e);

                if (card.Effect == EffectModifiersStore.Decoy)
                {
                    TurnManager.PlayedCards.Add(card);
                    TurnManager.PlayedCards.Add(sourceCard);

                    card.Effect.ActivateEffect(sourceCard as UnitCard);
                }
            }
            else return;
        }

        private void CardGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var card = sender as Border;
            card.BorderBrush = CardPickColor;
        }

        private void CardGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            var card = sender as Border;
            card.BorderBrush = DefaultColor;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        
        private ItemsControl FindParentItemsControl(DependencyObject element)
        {
            while (element != null)
            {
                if (element is ItemsControl itemsControl)
                {
                    return itemsControl;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }               

        private UIElement CreateCardVisual(Grid originalCard)
        {
            var newCard = new Grid
            {
                Width = originalCard.Width,
                Height = originalCard.Height,
                Background = originalCard.Background,
                Opacity = 0.8,
                IsHitTestVisible = false
            };
            
            foreach (var rowDef in originalCard.RowDefinitions)
            {
                newCard.RowDefinitions.Add(new RowDefinition { Height = rowDef.Height });
            }
            
            foreach (var colDef in originalCard.ColumnDefinitions)
            {
                newCard.ColumnDefinitions.Add(new ColumnDefinition { Width = colDef.Width });
            }
            
            foreach (var child in originalCard.Children)
            {
                if (child is TextBlock tb)
                {
                    var newText = new TextBlock
                    {
                        Text = tb.Text,
                        FontSize = tb.FontSize,
                        Foreground = tb.Foreground,
                        HorizontalAlignment = tb.HorizontalAlignment,
                        VerticalAlignment = tb.VerticalAlignment,
                        Margin = tb.Margin
                    };

                    Grid.SetRow(newText, Grid.GetRow(tb));
                    Grid.SetColumn(newText, Grid.GetColumn(tb));
                    newCard.Children.Add(newText);
                }
            }

            if (originalCard.ToolTip is ToolTip originalToolTip &&
                originalToolTip.Content is StackPanel originalStackPanel)
            {
                var newStackPanel = new StackPanel { Orientation = originalStackPanel.Orientation };
                foreach (var child in originalStackPanel.Children)
                {
                    if (child is TextBlock tb)
                    {
                        newStackPanel.Children.Add(new TextBlock
                        {
                            Text = tb.Text,
                            Foreground = tb.Foreground,
                            HorizontalAlignment = tb.HorizontalAlignment
                        });
                    }
                }

                newCard.ToolTip = new ToolTip
                {
                    Background = originalToolTip.Background,
                    Content = newStackPanel
                };
            }

            if (originalCard.Background is ImageBrush originalBrush)
            {
                newCard.Background = new ImageBrush
                {
                    ImageSource = originalBrush.ImageSource,
                    Stretch = originalBrush.Stretch
                };
            }

            return newCard;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////

        private void Player1MeleeBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Spy)
            {
                return;
            }

            if (card.BattleRow == BattleRows.MeleeBattleRow)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);
                GameContext.Player1.MeleeBattleRow.AddCardToBattleRow(card);          

                e.Handled = true;
            }
            else return;
        }

        private void Player1MeleeBattleRowHornSlot_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Horn && card is ActionCard)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);                
                GameContext.Player1.MeleeBattleRow.AddCardToBattleRow(card);                
            }
        }

        private void Player1MiddleBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Spy)
            {
                return;
            }

            if (card.BattleRow == BattleRows.MiddleBattleRow)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);
                GameContext.Player1.MiddleBattleRow.AddCardToBattleRow(card);           

                e.Handled = true;
            }
            else return;
        }

        private void Player1MiddleBattleRowHornSlot_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Horn && card is ActionCard)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);                
                GameContext.Player1.MiddleBattleRow.AddCardToBattleRow(card);                
            }
        }

        private void Player1SiegeBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Spy)
            {
                return;
            }

            if (card.BattleRow == BattleRows.SiegeBattleRow)
            {
                GameContext.Player1.Hand.HandCards.Remove(card); 
                GameContext.Player1.SiegeBattleRow.AddCardToBattleRow(card);            

                e.Handled = true;
            }
            else return;
        }

        private void Player1SiegeBattleRowHornSlot_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Horn && card is ActionCard)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);                
                GameContext.Player1.SiegeBattleRow.AddCardToBattleRow(card);                
            }
        }

        private void GameDesk_Drop(object sender, DragEventArgs e)
        {            
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Scorch || card.Effect == EffectModifiersStore.ClearWeather)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);
                card.Effect.ActivateEffect(null);

                TurnManager.PlayedCards.Add(card);
                GameContext.StartTurn(card);

                e.Handled = true;
            }
            else return;
        }

        private void GameDesk_DragOver(object sender, DragEventArgs e)
        {
            if (draggedCardVisual != null)
            {
                var position = e.GetPosition(GameDesk);
                Canvas.SetLeft(draggedCardVisual, position.X - draggedCardVisual.RenderSize.Width / 2);
                Canvas.SetTop(draggedCardVisual, position.Y - draggedCardVisual.RenderSize.Height / 2);
            }
        }

        private void WeatherCardsBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card is WeatherCard weatherCard)
            {
                GameContext.Player1.Hand.HandCards.Remove(card);
                GameContext.WeatherCardsBattleRow.Add(weatherCard);

                TurnManager.PlayedCards.Add(card);
                GameContext.StartTurn(card);
                
                PaintRowsByWeather();

                e.Handled = true;
            }
            else return;
        }

        private void PassButton_Click(object sender, RoutedEventArgs e)
        {
            if (GameContext.ActivePlayer == GameContext.Player1)
            {
                GameContext.Player1.HasPassed = true;
                GameContext.StartTurn(null);
            }
            else return;            
        }

        private void Player2SiegeBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Spy && GameContext.ActivePlayer != GameContext.Player2)
            {
                if (card.BattleRow == BattleRows.SiegeBattleRow)
                {
                    GameContext.Player1.Hand.HandCards.Remove(card); 
                    GameContext.Player2.SiegeBattleRow.AddCardToBattleRow(card);          

                    e.Handled = true;
                }
                else return;
            }
            else return;
        }

        private void Player2MiddleBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Spy && GameContext.ActivePlayer != GameContext.Player2)
            {
                if (card.BattleRow == BattleRows.MiddleBattleRow)
                {
                    GameContext.Player1.Hand.HandCards.Remove(card);
                    GameContext.Player2.MiddleBattleRow.AddCardToBattleRow(card);           

                    e.Handled = true;
                }
                else return;
            }
            else return;
        }

        private void Player2MeleeBattleRow_Drop(object sender, DragEventArgs e)
        {
            var card = GetCard(e);

            if (card.Effect == EffectModifiersStore.Spy && GameContext.ActivePlayer != GameContext.Player2)
            {
                if (card.BattleRow == BattleRows.MeleeBattleRow)
                {
                    GameContext.Player1.Hand.HandCards.Remove(card);
                    GameContext.Player2.MeleeBattleRow.AddCardToBattleRow(card);           

                    e.Handled = true;
                }
                else return;
            }
            else return;
        }

        private void Player1Discard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowDiscard(GameContext.Player1.Discard.ToList());            
        }
        private void Player2Discard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ShowDiscard(GameContext.Player2.Discard.ToList());
        }

        private void HideDiscard_Click(object sender, RoutedEventArgs e)
        {
            if (HideDiscard.Content == "Accept")
            {
                DiscardMenu.ItemsSource = GameContext.Player1.Discard;
                HideDiscard.Content = "Hide discard";
                ShowDiscardMenu.Visibility = Visibility.Collapsed;
                GameDesk.Effect = null;
                cancellationToken.Cancel();
            }
            else
            {
                GameDesk.Effect = null;
                ShowDiscardMenu.Visibility = Visibility.Collapsed;
            }
        }

        private async void Player1Leader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (GameContext.ActivePlayer == GameContext.Player1 && GameContext.Player1.Leader.Effect != null)
            {
                TurnManager.PlayedCards.Add(GameContext.Player1.Leader);

                var leaderEffect = GameContext.Player1.Leader.Effect;

                if (leaderEffect == EffectModifiersStore.EmhyrCardSteal)
                {
                    leaderEffect.ActivateEffect(
                        await ReturnRevivedCard(GameContext.Player2.Discard
                        .Where(_card => _card is UnitCard).ToList()));
                }
                else if (leaderEffect == EffectModifiersStore.FoltestCardFromDeckPlay)
                {
                    leaderEffect.ActivateEffect(
                        await ReturnRevivedCard(GameContext.Player1.Deck.DeckCards
                        .Where(_card => _card is UnitCard).ToList()));
                }                                                
                Player1Leader.Opacity = 0.5;
                Player1Leader.IsHitTestVisible = false;                
            }
            else
            {
                return;
            }
        }    
        
        private void DeactivatePlayer2Leader()
        {
            Player2Leader.Opacity = 0.5;
            Player2Leader.IsHitTestVisible = false;
        }

        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MusicBG.SetVolume(e.NewValue);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GameContext.ReturnToMenuWindow(client);
        }
    }
}
