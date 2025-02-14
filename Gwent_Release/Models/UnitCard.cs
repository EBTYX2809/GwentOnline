namespace Gwent_Release.Models
{
    public class UnitCard : Card
    {
        public int DefaultCardScore;
        public UnitCard(string jsonNameKey, int? actualCardScore = null, Fractions fraction = 0, BattleRows battleRow = 0) : base(jsonNameKey, actualCardScore, fraction, battleRow)
        {
            DefaultCardScore = (int)actualCardScore;
        }
        public override UnitCard CopyCard()
        {
            return new UnitCard(JsonNameKey, ActualCardScore, Fraction, BattleRow)
            {
                Effect = Effect,
                CardInfo = CardInfo
            };
        }
    }
}
