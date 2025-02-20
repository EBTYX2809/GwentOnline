using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.Models
{
    public class BattleRow : INotifyPropertyChanged
    {
        public BattleRows BattleRowType { get; set; }
        private int _batteRowScore;
        public int BatteRowScore
        {
            get => _batteRowScore;
            set
            {
                if (_batteRowScore != value)
                {
                    _batteRowScore = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<Card> _battleRowCards;
        public ObservableCollection<Card> BattleRowCards
        {
            get => _battleRowCards;
            set
            {
                if (_battleRowCards != value)
                {
                    _battleRowCards = value;
                    OnPropertyChanged();
                }
            }
        }

        private Card _hornSlot;
        public Card HornSlot
        {
            get => _hornSlot;
            set
            {
                if (_hornSlot!= value)
                {
                    _hornSlot = value;
                    OnPropertyChanged();
                }
            }
        }
        public int CardsCount => BattleRowCards.Count;
        public List<EffectModifier> EffectModifiers { get; set; }
        public event Action CardAddedToBattleRow;
        public event Action<Card> CardRemovedFromBattleRow;
        public Func<List<Card>, Task<UnitCard>> GiveMedicCardToContext;

        public BattleRow(BattleRows battleRowType)
        {
            BattleRowType = battleRowType;
            BattleRowCards = new ObservableCollection<Card>();
            EffectModifiers = new List<EffectModifier>();
            BatteRowScore = 0;
            HornSlot = null;
            BattleRowCards.CollectionChanged += BattleRowCards_CollectionChanged;
        }

        public async void AddCardToBattleRow(Card card, UnitCard cardForRevive = null)
        {
            if(card.Effect == EffectModifiersStore.Medic)
            {
                if(cardForRevive != null)
                {
                    BattleRowCards.Add(card);

                    ScoringPointsInRow();
                    CardAddedToBattleRow?.Invoke();

                    if (cardForRevive.Effect != EffectModifiersStore.Medic)
                    {
                        card.Effect.ActivateEffect(cardForRevive);
                    }

                    return;
                }
                else if(card == cardForRevive)
                {
                    BattleRowCards.Add(card);

                    ScoringPointsInRow();
                    CardAddedToBattleRow?.Invoke();

                    GameContext.Instance.StartTurn(card);

                    return;
                }
                else 
                {
                    BattleRowCards.Add(card);
                    TurnManager.PlayedCards.Add(card);

                    UnitCard recievedCard = await GiveMedicCardToContext?.Invoke(
                        GameContext.Instance.ActivePlayer.Discard.Where(_card => _card is UnitCard).ToList()); 

                    card.Effect.ActivateEffect(recievedCard);

                    ScoringPointsInRow();
                    CardAddedToBattleRow?.Invoke();

                    if (recievedCard == null) GameContext.Instance.StartTurn(card);

                    return;
                }
            }
            else if (card.Effect == EffectModifiersStore.Bond)
            {
                int index = 0;
                foreach (var dublicateCard in BattleRowCards)
                {
                    if(card.JsonNameKey.TrimEnd('1', '2', '3', '4') == dublicateCard.JsonNameKey.TrimEnd('1', '2', '3', '4'))
                    {
                        BattleRowCards.Insert(index + 1, card);
                        AddEffectModifier(card.Effect as EffectModifier);
                        CardAddedToBattleRow?.Invoke();  
                        
                        TurnManager.PlayedCards.Add(card);                        
                        GameContext.Instance.StartTurn(card);

                        return;
                    }
                    index++;
                }
                BattleRowCards.Add(card);
            }

            else if(card.Effect == EffectModifiersStore.Horn)
            {
                if (card is ActionCard)
                {
                    if (HornSlot == null)
                    {
                        HornSlot = card;
                        AddEffectModifier(card.Effect as EffectModifier);                        
                        TurnManager.HornRow = (int)BattleRowType;
                    }
                }
                else if (card is UnitCard)
                {
                    if (HornSlot == null)
                    {
                        BattleRowCards.Add(card);
                        AddEffectModifier(card.Effect as EffectModifier);
                        HornSlot = CardsStore.NeutralDeck.Find(_card => _card.JsonNameKey == "Commander's Horn");
                    }
                    else if(HornSlot != null)
                    {
                        BattleRowCards.Add(card);
                        ScoringPointsInRow();
                    }
                }
                CardAddedToBattleRow?.Invoke();  
                
                TurnManager.PlayedCards.Add(card);                
                GameContext.Instance.StartTurn(card);               

                return;
            }
            else
            {
                BattleRowCards.Add(card);
            }

            if (card.Effect != null)
            {
                if (card.Effect is EffectModifier effectModifier)
                {
                    AddEffectModifier(effectModifier);
                }
                else if(card.Effect is Effect effect)
                {
                    card.Effect.ActivateEffect(null);
                }
            }
            else if(card is UnitCard unitCard)
            {
                ScoringPointsInCard(unitCard);
            }
            ScoringPointsInRow();
            CardAddedToBattleRow?.Invoke();
            
            TurnManager.PlayedCards.Add(card);            
            GameContext.Instance.StartTurn(card);            
        }

        public void RemoveCardFromBattleRow(Card card)
        {
            if (BattleRowCards.Contains(card))
            {
                BattleRowCards.Remove(card);

                if (card.Effect == EffectModifiersStore.Horn && card is UnitCard)
                {
                    HornSlot = null;
                }

                if (card is UnitCard unitCard)
                {
                    unitCard.ActualCardScore = unitCard.DefaultCardScore;
                }

                if (card.Effect != null)
                {
                    if (card.Effect is EffectModifier effectModifier)
                    {
                        RemoveEffectModifier(effectModifier);
                    }
                }
                ScoringPointsInRow();
                CardRemovedFromBattleRow?.Invoke(card);
            }
            else
            {
                return;
            }
        }

        public void AddEffectModifier(EffectModifier effect)
        {           
            EffectModifiers.Add(effect);

            EffectModifiers = EffectModifiers.OrderBy(effectModifier => effectModifier.Priority).ToList();            

            ScoringPointsInRow();

            if (effect == EffectModifiersStore.Frost
               || effect == EffectModifiersStore.Fog
               || effect == EffectModifiersStore.Rain)
            {
                CardAddedToBattleRow?.Invoke();
            }
        }

        public void RemoveEffectModifier(EffectModifier effect)
        {
            EffectModifiers.Remove(effect);

            EffectModifiers = EffectModifiers.OrderBy(effectModifier => effectModifier.Priority).ToList();

            ScoringPointsInRow();
        }

        private void ScoringPointsInCard(UnitCard unitCard)
        {
            unitCard.ActualCardScore = unitCard.DefaultCardScore;
            foreach (var effect in EffectModifiers)
            {
                effect.ActivateEffect(unitCard);
            }
        }

        private void ScoringPointsInRow()
        {
            BatteRowScore = 0;
            foreach (var card in BattleRowCards)
            {
                if(card is UnitCard unit)
                {
                    ScoringPointsInCard(unit);
                    BatteRowScore += (int)unit.ActualCardScore;
                }
                else if (card is HeroCard hero)
                {
                    BatteRowScore += (int)hero.ActualCardScore;
                }
            }

        }

        public void ClearBattleRow()
        {
            BatteRowScore = 0;
            foreach (var card in BattleRowCards)
            {
                if (card is UnitCard unit) 
                {
                    unit.ActualCardScore = unit.DefaultCardScore;
                }
                CardRemovedFromBattleRow?.Invoke(card);
            }
            HornSlot = null;
            BattleRowCards.Clear();
            EffectModifiers.Clear();
        }        

        private void BattleRowCards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(CardsCount));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
