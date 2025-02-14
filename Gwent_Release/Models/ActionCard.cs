namespace Gwent_Release.Models
{
    public class ActionCard : Card
    {
        public ActionCard(string jsonNameKey, int? actualCardScore = null, Fractions fraction = 0, BattleRows battleRow = 0)
            : base(jsonNameKey, actualCardScore, fraction, battleRow)
        { }
        public override ActionCard CopyCard()
        {
            return new ActionCard(JsonNameKey, ActualCardScore, Fraction, BattleRow)
            {
                Effect = Effect,
                CardInfo = CardInfo
            };
        }
    }
}
