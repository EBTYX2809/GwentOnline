using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gwent_Release.Models
{
    public static class TurnManager
    {
        public static Client client;
        public static List<Card> PlayedCards { get; set; } = new List<Card>();
        public static int HornRow { get; set; }
        private static string turnInfo { get; set; }
        public static Action ActivateLeader { get; set; }
        public static void GiveTurn()
        {
            turnInfo = string.Join("|", PlayedCards.Select(card => card.JsonNameKey));

            if (HornRow > 0) turnInfo += "|" + HornRow;

            if (turnInfo == "") turnInfo = "pass";

            client.SendInfo(turnInfo);

            PlayedCards.Clear();
            turnInfo = null;
            HornRow = 0;
        }
        
        public static async Task EnemyTurn()
        {
            turnInfo = await client.ReceiveInfo();
            
            if (turnInfo != "pass")
            {
                string[] playedCards = turnInfo.Split('|');

                List<Card> playerCards = new List<Card>
                (
                    GameContext.Instance.ActivePlayer.Hand.HandCards
                    .Concat(GameContext.Instance.ActivePlayer.Discard)
                    .Concat(GameContext.Instance.ActivePlayer.MeleeBattleRow.BattleRowCards)
                    .Concat(GameContext.Instance.ActivePlayer.MiddleBattleRow.BattleRowCards)
                    .Concat(GameContext.Instance.ActivePlayer.SiegeBattleRow.BattleRowCards)
                );                

                if (GameContext.Instance.ActivePlayer.Leader.Effect == EffectModifiersStore.FoltestCardFromDeckPlay)
                {
                    playerCards.AddRange(GameContext.Instance.ActivePlayer.Deck.DeckCards);
                }
                else if (GameContext.Instance.ActivePlayer.Leader.Effect == EffectModifiersStore.EmhyrCardSteal)
                {
                    playerCards.AddRange(GameContext.Instance.PassivePlayer.Discard);
                }

                playerCards.Add(GameContext.Instance.ActivePlayer.Leader);

                foreach (string cardName in playedCards)
                {
                    if (cardName == "1" || cardName == "2" || cardName == "3")
                    {
                        HornRow = int.Parse(cardName);
                    }
                    else
                    {
                        PlayedCards.Add(playerCards.Find(_card => _card.JsonNameKey == cardName));
                    }
                }
            }
            else
            {
                GameContext.Instance.StartTurn(null);
                return;
            }

            var firstCard = PlayedCards.First();
            PlayedCards.Remove(firstCard);
            if (firstCard.Effect != EffectModifiersStore.Decoy)
            {
                GameContext.Instance.ActivePlayer.Hand.HandCards.Remove(firstCard);
            }

            UnitCard secondCard = null;
            if (PlayedCards.Count >= 1)
            {
                secondCard = PlayedCards.First() as UnitCard;                
            }

            if (firstCard is ActionCard horn && horn.Effect == EffectModifiersStore.Horn)
            {
                foreach (var battleRow in GameContext.Instance.ActivePlayer.PlayerBattleRows)
                {
                    if (battleRow.BattleRowType == (BattleRows)HornRow)
                    {
                        battleRow.AddCardToBattleRow(firstCard);
                    }
                }
            }
            else if (firstCard.Effect == EffectModifiersStore.EmhyrCardSteal
                     || firstCard.Effect == EffectModifiersStore.FoltestCardFromDeckPlay)
            {
                if (secondCard.Effect == EffectModifiersStore.Medic)
                {
                    ActivateLeader?.Invoke();
                    PlayedCards.Remove(secondCard);
                    MedicRevive(secondCard, PlayedCards.First() as UnitCard);
                }
                else
                {
                    firstCard.Effect.ActivateEffect(secondCard);
                    ActivateLeader?.Invoke();
                }
            }
            else if (firstCard.Effect == EffectModifiersStore.Decoy)
            {
                firstCard.Effect.ActivateEffect(secondCard);
            }
            else if (firstCard.Effect == EffectModifiersStore.Medic)
            {
                MedicRevive(firstCard, secondCard);
            }
            else if (firstCard.Effect == EffectModifiersStore.Frost
                || firstCard.Effect == EffectModifiersStore.Fog
                || firstCard.Effect == EffectModifiersStore.Rain)
            {
                GameContext.Instance.WeatherCardsBattleRow.Add(firstCard as WeatherCard);
                GameContext.Instance.StartTurn(firstCard);
            }
            else if (firstCard.Effect == EffectModifiersStore.ClearWeather || firstCard.Effect == EffectModifiersStore.Scorch)
            {
                firstCard.Effect.ActivateEffect(null);
                GameContext.Instance.StartTurn(firstCard);
            }
            else
            {
                EffectModifiersStore.SpawnCard(firstCard);
            }
        }

        public static void ClearTurnInfo()
        {
            PlayedCards.Clear();
            turnInfo = null;
            HornRow = 0;
        }

        private static void MedicRevive(Card firstCard = null, UnitCard secondCard = null)
        {            
            if (secondCard != null)
            {
                foreach (var card in PlayedCards)
                {
                    secondCard = card as UnitCard;
                    foreach (var row in GameContext.Instance.ActivePlayer.PlayerBattleRows)
                    {
                        if (firstCard.BattleRow == row.BattleRowType)
                        {
                            row.AddCardToBattleRow(firstCard, secondCard);
                            firstCard = secondCard;
                            secondCard = null;
                            break;
                        }
                    }
                }
            }            
            else
            {
                foreach (var row in GameContext.Instance.ActivePlayer.PlayerBattleRows)
                {
                    if (firstCard.BattleRow == row.BattleRowType)
                    {
                        row.AddCardToBattleRow(firstCard, firstCard as UnitCard);
                    }
                }
            }
        }
    }
}
