namespace Gwent_Release.Models.CardsNS
{
    public class HeroCard : Card
    {
        public HeroCard(string jsonNameKey, int? actualCardScore = null, Fractions fraction = 0, BattleRows battleRow = 0)
            : base(jsonNameKey, actualCardScore, fraction, battleRow)
        { }
        public override HeroCard CopyCard()
        {
            return new HeroCard(JsonNameKey, ActualCardScore, Fraction, BattleRow)
            {
                Effect = Effect,
                CardInfo = CardInfo
            };
        }
    }
}
