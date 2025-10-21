using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows.Resources;
using System.Windows;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.Models
{
    public static class LanguageManager
    {
        private static string Language;
        private static JsonNode jsonNode;
        public static CardInfo GetInfo(string key)
        {
            var info = jsonNode[key];

            return new CardInfo { Name = info["Name"]?.ToString(), Description = info["Description"]?.ToString() };
        }

        public static void SetLanguage(string language) 
        {
            Language = language;
            
            Uri uri = new Uri($"pack://application:,,,/JsonLanguages/CardsInfo{Language}.json");
            StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);

            string jsonData;
            using (StreamReader reader = new StreamReader(resourceInfo.Stream))
            {
                jsonData = reader.ReadToEnd();
            }

            jsonNode = JsonNode.Parse(jsonData);            

            foreach (Card card in CardsStore.NeutralDeck)
            {
                card.CardInfo = GetInfo(card.JsonNameKey);
            }
            foreach (Card card in CardsStore.NilfgaardDeck)
            {
                card.CardInfo = GetInfo(card.JsonNameKey);
            }
            foreach (Card card in CardsStore.NorthKingdomsDeck)
            {
                card.CardInfo = GetInfo(card.JsonNameKey);
            }
        }
    }
}