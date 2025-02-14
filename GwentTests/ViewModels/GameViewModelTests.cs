using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gwent.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gwent.Models;
using System.Collections.ObjectModel;

namespace Gwent.ViewModels.Tests
{
    [TestClass()]
    public class GameViewModelTests
    {
        [TestMethod()]
        public void GameViewModelTest()
        {
            List<Card> cards = new List<Card>
            { 
                CardsStore.NilfgaardDeck.Find(c => c.JsonNameKey == "Fringilla Vigo"),
                CardsStore.NilfgaardDeck.Find(c => c.JsonNameKey == "Heavy Zerrikanian Fire Scorpion"),
                CardsStore.NilfgaardDeck.Find(c => c.JsonNameKey == "Assire var Anahid")
            };

            //var govno = cards.GroupBy(g => g.ActualCardScore);

            

            Console.WriteLine("2");
        }
    }
}