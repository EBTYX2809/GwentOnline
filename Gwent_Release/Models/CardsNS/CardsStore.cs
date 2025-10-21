using System.Collections.Generic;
using Gwent_Release.Models.EffectsNS;

namespace Gwent_Release.Models.CardsNS
{
    public static class CardsStore
    {
        public static readonly List<Card> NorthKingdomsDeck = new List<Card>();
        public static readonly List<Card> NilfgaardDeck = new List<Card>();
        public static readonly List<Card> NeutralDeck = new List<Card>();

        static CardsStore()
        {
            CreateCards();
            LanguageManager.SetLanguage("EN");
        }

        private static void CreateCards()
        {
            CreateNorthKingdomsDeck();
            CreateNilfgaardDeck();
            CreateNeutralDeck();
        }

        private static void CreateNorthKingdomsDeck()
        {
            // HeroCards ///////////////////////////////////////////////////////////////////////////
            NorthKingdomsDeck.Add(new HeroCard("Esterad Thyssen", 10, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow));
            NorthKingdomsDeck.Add(new HeroCard("John Natalis", 10, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow));
            NorthKingdomsDeck.Add(new HeroCard("Philippa Eilhart", 10, Fractions.NorthKingdoms, BattleRows.MiddleBattleRow));
            NorthKingdomsDeck.Add(new HeroCard("Vernon Roche", 10, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow));

            // UnitCards ///////////////////////////////////////////////////////////////////////////
            NorthKingdomsDeck.Add(new UnitCard("Ballista 1", 6, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Ballista 2", 6, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Blue Stripes Commando 1", 4, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Blue Stripes Commando 2", 4, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Blue Stripes Commando 3", 4, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Catapult 1", 8, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Catapult 2", 8, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Crinfrid Reavers Dragon 1", 5, Fractions.NorthKingdoms, BattleRows.MiddleBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Crinfrid Reavers Dragon 2", 5, Fractions.NorthKingdoms, BattleRows.MiddleBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NorthKingdomsDeck.Add(new UnitCard("Dun Banner Medic", 5, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Medic
            });
            NorthKingdomsDeck.Add(new UnitCard("Kaedweni Siege Expert 1", 1, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Buff
            });
            NorthKingdomsDeck.Add(new UnitCard("Kaedweni Siege Expert 2", 1, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Buff
            });
            NorthKingdomsDeck.Add(new UnitCard("Kaedweni Siege Expert 3", 1, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Buff
            });
            NorthKingdomsDeck.Add(new UnitCard("Keira Metz", 5, Fractions.NorthKingdoms, BattleRows.MiddleBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Prince Stennis", 5, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NorthKingdomsDeck.Add(new UnitCard("Siege Tower", 6, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Siegfried of Denesle", 5, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Sigismund Dijkstra", 5, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NorthKingdomsDeck.Add(new UnitCard("Thaler", 1, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NorthKingdomsDeck.Add(new UnitCard("Trebuchet 1", 6, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Trebuchet 2", 6, Fractions.NorthKingdoms, BattleRows.SiegeBattleRow));
            NorthKingdomsDeck.Add(new UnitCard("Ves", 5, Fractions.NorthKingdoms, BattleRows.MeleeBattleRow));
            // 26

            NorthKingdomsDeck.Add(new ActionCard("Foltest", fraction: Fractions.NorthKingdoms)
            {
                Effect = EffectModifiersStore.FoltestCardFromDeckPlay
            });
        }
        private static void CreateNilfgaardDeck()
        {
            // HeroCards ///////////////////////////////////////////////////////////////////////////
            NilfgaardDeck.Add(new HeroCard("Letho of Gulet", 10, Fractions.Nilfgaard, BattleRows.MeleeBattleRow));
            NilfgaardDeck.Add(new HeroCard("Menno Coehoorn", 10, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Medic
            });
            NilfgaardDeck.Add(new HeroCard("Morvran Voorhis", 10, Fractions.Nilfgaard, BattleRows.SiegeBattleRow));
            NilfgaardDeck.Add(new HeroCard("Tibor Eggebracht", 10, Fractions.Nilfgaard, BattleRows.MiddleBattleRow));

            // UnitCards ///////////////////////////////////////////////////////////////////////////
            NilfgaardDeck.Add(new UnitCard("Assire var Anahid", 6, Fractions.Nilfgaard, BattleRows.MiddleBattleRow));
            NilfgaardDeck.Add(new UnitCard("Black Infantry Archer 1", 10, Fractions.Nilfgaard, BattleRows.MiddleBattleRow));
            NilfgaardDeck.Add(new UnitCard("Black Infantry Archer 2", 10, Fractions.Nilfgaard, BattleRows.MiddleBattleRow));
            NilfgaardDeck.Add(new UnitCard("Cahir Mawr Dyffryn aep Ceallah", 6, Fractions.Nilfgaard, BattleRows.MeleeBattleRow));
            NilfgaardDeck.Add(new UnitCard("Etolian Auxiliary Archers' 1", 1, Fractions.Nilfgaard, BattleRows.MiddleBattleRow)
            {
                Effect = EffectModifiersStore.Medic
            });
            NilfgaardDeck.Add(new UnitCard("Etolian Auxiliary Archers' 2", 1, Fractions.Nilfgaard, BattleRows.MiddleBattleRow)
            {
                Effect = EffectModifiersStore.Medic
            });
            NilfgaardDeck.Add(new UnitCard("Fringilla Vigo", 6, Fractions.Nilfgaard, BattleRows.MiddleBattleRow));
            NilfgaardDeck.Add(new UnitCard("Heavy Zerrikanian Fire Scorpion", 10, Fractions.Nilfgaard, BattleRows.SiegeBattleRow));
            NilfgaardDeck.Add(new UnitCard("Impera Brigade Guard 1", 3, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NilfgaardDeck.Add(new UnitCard("Impera Brigade Guard 2", 3, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NilfgaardDeck.Add(new UnitCard("Impera Brigade Guard 3", 3, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NilfgaardDeck.Add(new UnitCard("Impera Brigade Guard 4", 3, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NilfgaardDeck.Add(new UnitCard("Shilard Fitz-Oesterlen", 7, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NilfgaardDeck.Add(new UnitCard("Siege Engineer", 6, Fractions.Nilfgaard, BattleRows.SiegeBattleRow));
            NilfgaardDeck.Add(new UnitCard("Stefan Skellen", 9, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NilfgaardDeck.Add(new UnitCard("Vattier de Rideaux", 4, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NilfgaardDeck.Add(new UnitCard("Young Emissary 1", 5, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NilfgaardDeck.Add(new UnitCard("Young Emissary 2", 5, Fractions.Nilfgaard, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Bond
            });
            NilfgaardDeck.Add(new UnitCard("Zerrikanian Fire Scorpion", 5, Fractions.Nilfgaard, BattleRows.SiegeBattleRow));
            // 23

            NilfgaardDeck.Add(new ActionCard("Emhyr", fraction: Fractions.Nilfgaard)
            {
                Effect = EffectModifiersStore.EmhyrCardSteal
            });
        }
        private static void CreateNeutralDeck()
        {
            // HeroCards ///////////////////////////////////////////////////////////////////////////
            NeutralDeck.Add(new HeroCard("Avallac'h", 0, Fractions.Neutral, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Spy
            });
            NeutralDeck.Add(new HeroCard("Cirilla Fiona Elen Riannon", 15, Fractions.Neutral, BattleRows.MeleeBattleRow));
            NeutralDeck.Add(new HeroCard("Geralt Of Rivia", 15, Fractions.Neutral, BattleRows.MeleeBattleRow));
            NeutralDeck.Add(new HeroCard("Triss Merigold", 7, Fractions.Neutral, BattleRows.MeleeBattleRow));
            NeutralDeck.Add(new HeroCard("Yennefer Of Vengerberg", 7, Fractions.Neutral, BattleRows.MiddleBattleRow)
            {
                Effect = EffectModifiersStore.Medic
            });

            // UnitCards ///////////////////////////////////////////////////////////////////////////
            NeutralDeck.Add(new UnitCard("Dandelion", 2, Fractions.Neutral, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Horn
            });
            NeutralDeck.Add(new UnitCard("Villentretenmerth", 7, Fractions.Neutral, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.MeleeScorch
            });

            // ActionCards /////////////////////////////////////////////////////////////////////////
            NeutralDeck.Add(new WeatherCard("Biting Frost", BattleRows.WeatherCardsBattleRow, BattleRows.MeleeBattleRow)
            {
                Effect = EffectModifiersStore.Frost
            });
            NeutralDeck.Add(new WeatherCard("Impenetrable Fog", BattleRows.WeatherCardsBattleRow, BattleRows.MiddleBattleRow)
            {
                Effect = EffectModifiersStore.Fog
            });
            NeutralDeck.Add(new WeatherCard("Torrential Rain", BattleRows.WeatherCardsBattleRow, BattleRows.SiegeBattleRow)
            {
                Effect = EffectModifiersStore.Rain
            });
            NeutralDeck.Add(new WeatherCard("Clear Weather", BattleRows.None)
            {
                Effect = EffectModifiersStore.ClearWeather
            });
            NeutralDeck.Add(new ActionCard("Commander's Horn")
            {
                Effect = EffectModifiersStore.Horn
            });
            NeutralDeck.Add(new ActionCard("Decoy")
            {
                Effect = EffectModifiersStore.Decoy
            });
            NeutralDeck.Add(new ActionCard("Scorch")
            {
                Effect = EffectModifiersStore.Scorch
            });
            // 14
        }
    }
}
