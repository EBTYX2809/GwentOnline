namespace Gwent_Release.Models.CardsNS
{
    public class WeatherCard : Card
    {
        public BattleRows DropBattleRow { get; set; }
        public BattleRows ActionBattleRow { get; set; }

        public WeatherCard(string jsonNameKey, BattleRows dropBattleRow, BattleRows actionBattleRow = 0,
            int? actualCardScore = null, Fractions fraction = 0, BattleRows battleRow = 0)
            : base(jsonNameKey, actualCardScore, fraction, battleRow)
        {
            DropBattleRow = dropBattleRow;
            ActionBattleRow = actionBattleRow;
        }
        public override WeatherCard CopyCard()
        {
            return new WeatherCard(JsonNameKey, DropBattleRow, ActionBattleRow, ActualCardScore, Fraction, BattleRow)
            {
                Effect = Effect,
                CardInfo = CardInfo
            };
        }
    }
}
