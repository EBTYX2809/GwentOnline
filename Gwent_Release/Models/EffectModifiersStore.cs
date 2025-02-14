using System;
using System.Collections.Generic;
using System.Linq;

namespace Gwent_Release.Models
{
    public static class EffectModifiersStore
    {
        public static EffectModifier Horn = new EffectModifier();
        public static EffectModifier Frost = new EffectModifier();
        public static EffectModifier Fog = new EffectModifier();
        public static EffectModifier Rain = new EffectModifier();
        public static EffectModifier Buff = new EffectModifier();
        public static EffectModifier Bond = new EffectModifier();

        public static Effect ClearWeather = new Effect();
        public static Effect Medic = new Effect();
        public static Effect Spy = new Effect();
        public static Effect Scorch = new Effect();
        public static Effect MeleeScorch = new Effect();
        public static Effect Decoy = new Effect();

        public static Effect EmhyrCardSteal = new Effect();
        public static Effect FoltestCardFromDeckPlay = new Effect();

        static EffectModifiersStore()
        {
            Bond.Priority = 1;
            Bond.Modifier = context =>
            {
               foreach(var battleRow in GameContext.ActivePlayer.PlayerBattleRows)
               {
                    if(context.Effect == Bond && context.BattleRow == battleRow.BattleRowType)
                    {
                        if (battleRow.BattleRowCards.Count(
                                dublicateCard => dublicateCard.JsonNameKey.TrimEnd('1', '2', '3', '4')
                                    == context.JsonNameKey.TrimEnd('1', '2', '3', '4'))
                            is int dublicatesCount && dublicatesCount > 0)
                        {
                            if (battleRow.EffectModifiers.Any(effect => effect == Frost || effect == Fog || effect == Rain))
                            {
                                context.ActualCardScore = 1;
                            }
                            else
                            {
                                context.ActualCardScore = context.DefaultCardScore;
                            }                            

                            context.ActualCardScore *= dublicatesCount;
                        }
                    }
               }
            };

            Horn.Priority = 3;
            Horn.Modifier = context =>
            {                                
                context.ActualCardScore *= 2;                
            };

            Frost.Priority = 0;
            Frost.Modifier = context =>
            {                                
                context.ActualCardScore = 1;                
            };

            Fog.Priority = 0;
            Fog.Modifier = context =>
            {                                
                context.ActualCardScore = 1;                
            };

            Rain.Priority = 0;
            Rain.Modifier = context =>
            {                                
                context.ActualCardScore = 1;                
            };

            Buff.Priority = 2;
            Buff.Modifier = context =>
            {                                
                context.ActualCardScore++;                
            };

            ClearWeather.Modifier = context =>
            {
                foreach (var weatherCard in GameContext.WeatherCardsBattleRow)
                {
                    foreach (var battleRow in GameContext.GetAllPlayersRows())
                    {
                        battleRow.RemoveEffectModifier(weatherCard.Effect as EffectModifier);
                    }
                }

                GameContext.WeatherCardsBattleRow.Clear();
            };

            Medic.Modifier = context =>
            {
                if (context != null)
                {
                    GameContext.ActivePlayer.Discard.Remove(context);
                    SpawnCard(context);
                }
                else return;
            };

            Spy.Modifier = context =>
            {
                GameContext.ActivePlayer.TakeCard(2);
            };

            Scorch.Modifier = context =>
            {
                int? generalMaxCardScore = 0;
                int? tempMaxCardScore;
                List<UnitCard> MaxCards = new List<UnitCard>();                

                foreach (var battleRow in GameContext.GetAllPlayersRows())
                {
                    tempMaxCardScore = battleRow.BattleRowCards?.OfType<UnitCard>().Max(card => card.ActualCardScore);
                    if (tempMaxCardScore > generalMaxCardScore)
                    {
                        generalMaxCardScore = tempMaxCardScore;
                        MaxCards.Clear();
                        MaxCards.AddRange(battleRow.BattleRowCards?.OfType<UnitCard>().Where(card => card.ActualCardScore == generalMaxCardScore));
                    }
                    else if (tempMaxCardScore == generalMaxCardScore)
                    {
                        MaxCards.AddRange(battleRow.BattleRowCards?.OfType<UnitCard>().Where(card => card.ActualCardScore == generalMaxCardScore));
                    }
                }

                foreach (var battleRow in GameContext.GetAllPlayersRows())
                {
                    foreach (var card in MaxCards)
                    {
                        battleRow.RemoveCardFromBattleRow(card);
                    }
                }
            };

            MeleeScorch.Modifier = context =>
            {
                int? ActivePlayerMaxCardScore = 0;
                int? PassivePlayerMaxCardScore = 0;
                List<UnitCard> MaxCards = new List<UnitCard>();                

                ActivePlayerMaxCardScore = GameContext.ActivePlayer.MeleeBattleRow.BattleRowCards?
                .OfType<UnitCard>()
                .Where(card => card.JsonNameKey != "Villentretenmerth")
                .Max(card => card.ActualCardScore) ?? 0;

                PassivePlayerMaxCardScore = GameContext.PassivePlayer.MeleeBattleRow.BattleRowCards?
                .OfType<UnitCard>()
                .Max(card => card.ActualCardScore) ?? 0;

                if (ActivePlayerMaxCardScore > PassivePlayerMaxCardScore)
                {                    
                    MaxCards.AddRange(GameContext.ActivePlayer.MeleeBattleRow.BattleRowCards?.OfType<UnitCard>().Where(card => card.ActualCardScore == ActivePlayerMaxCardScore));
                }
                else if(ActivePlayerMaxCardScore < PassivePlayerMaxCardScore)
                {                    
                    MaxCards.AddRange(GameContext.PassivePlayer.MeleeBattleRow.BattleRowCards?.OfType<UnitCard>().Where(card => card.ActualCardScore == PassivePlayerMaxCardScore));
                }
                else if(ActivePlayerMaxCardScore == PassivePlayerMaxCardScore)
                {                    
                    MaxCards.AddRange(GameContext.ActivePlayer.MeleeBattleRow.BattleRowCards?.OfType<UnitCard>().Where(card => card.ActualCardScore == ActivePlayerMaxCardScore));
                    MaxCards.AddRange(GameContext.PassivePlayer.MeleeBattleRow.BattleRowCards?.OfType<UnitCard>().Where(card => card.ActualCardScore == PassivePlayerMaxCardScore));
                }
                                
                foreach (var card in MaxCards)
                {
                    GameContext.ActivePlayer.MeleeBattleRow.RemoveCardFromBattleRow(card);
                    GameContext.PassivePlayer.MeleeBattleRow.RemoveCardFromBattleRow(card);
                }                
            };

            Decoy.Modifier = context => 
            {
                foreach (var battleRow in GameContext.ActivePlayer.PlayerBattleRows)
                {
                    if(context?.BattleRow == battleRow.BattleRowType)
                    {
                        for (int i = 0; i < battleRow.CardsCount; i++)
                        {
                            var card = battleRow.BattleRowCards[i];
                            if (context == card)
                            {
                                //Card Decoy = CardsStore.NeutralDeck.Find(decoy => decoy.JsonNameKey == "Decoy");
                                Card Decoy = GameContext.ActivePlayer.Hand.HandCards.First(_card => _card.Effect == EffectModifiersStore.Decoy);

                                GameContext.ActivePlayer.Hand.HandCards.Remove(Decoy);
                                battleRow.BattleRowCards.Insert(i, Decoy); // Вставляем карту
                                battleRow.RemoveCardFromBattleRow(card); // Удаляем оригинальную карту
                                // Ебейший костыль, потому что после прошлого ремува карта автоматичеки попадает в отбой,
                                // а при использовании чучела такого быть не должно, потому просто вручную убираю карту из отбоя))
                                GameContext.ActivePlayer.Discard.Remove(card);                                
                                GameContext.ActivePlayer.Hand.HandCards.Add(card);

                                //i++; // Пропускаем только что добавленную карту
                                break;
                            }
                        }
                        break;
                    }
                }
                GameContext.StartTurn(context);
            };

            EmhyrCardSteal.Modifier = context =>
            {
                if (context != null)
                {
                    GameContext.PassivePlayer.Discard.Remove(context);
                    SpawnCard(context);
                }
                else return;
            };

            FoltestCardFromDeckPlay.Modifier = context =>
            {
                if (context != null)
                {
                    GameContext.ActivePlayer.Deck.DeckCards.Remove(context);
                    SpawnCard(context);
                }
                else return;
            };
            
        }     
        
        public static void SpawnCard(Card card)
        {
            if (card.Effect == Spy)
            {
                foreach (var row in GameContext.PassivePlayer.PlayerBattleRows) // PassivePlayer
                {
                    if (card.BattleRow == row.BattleRowType)
                    {
                        row.AddCardToBattleRow(card);
                    }
                }
            }
            else
            {
                foreach (var row in GameContext.ActivePlayer.PlayerBattleRows)
                {
                    if (card.BattleRow == row.BattleRowType)
                    {
                        row.AddCardToBattleRow(card);
                    }
                }
            }
        }
    }
}
