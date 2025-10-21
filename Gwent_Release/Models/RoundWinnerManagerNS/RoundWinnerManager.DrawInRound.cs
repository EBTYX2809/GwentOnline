namespace Gwent_Release.Models.RoundWinnerManagerNS
{
    public partial class RoundWinnerManager
    {
        protected class DrawInRound : IWinRound
        {
            public void WinRound()
            {
                if (GameContext.Instance.Player1.Leader.Fraction == Fractions.Nilfgaard && GameContext.Instance.Player2.Leader.Fraction == Fractions.Nilfgaard
                    || GameContext.Instance.Player1.Leader.Fraction == Fractions.NorthKingdoms && GameContext.Instance.Player2.Leader.Fraction == Fractions.NorthKingdoms)
                {
                    if (!GameContext.Instance.Player1.IsFirstRoundLoose) GameContext.Instance.Player1.IsFirstRoundLoose = true;
                    else GameContext.Instance.Player1.IsSecondRoundLoose = true;
                    if (!GameContext.Instance.Player2.IsFirstRoundLoose) GameContext.Instance.Player2.IsFirstRoundLoose = true;
                    else GameContext.Instance.Player2.IsSecondRoundLoose = true;

                    EndRoundAnnouncement($"Draw in round. Score {GameContext.Instance.Player1.GeneralScore} - {GameContext.Instance.Player2.GeneralScore}.");
                }
                else if (GameContext.Instance.Player1.Leader.Fraction == Fractions.Nilfgaard)
                {
                    if (!GameContext.Instance.Player2.IsFirstRoundLoose) GameContext.Instance.Player2.IsFirstRoundLoose = true;
                    else GameContext.Instance.Player2.IsSecondRoundLoose = true;

                    EndRoundAnnouncement($"{GameContext.Instance.Player1.Name} win round by his fraction ability. " +
                                         $"Score {GameContext.Instance.Player1.GeneralScore} - {GameContext.Instance.Player2.GeneralScore}.");
                }
                else if (GameContext.Instance.Player2.Leader.Fraction == Fractions.Nilfgaard)
                {
                    if (!GameContext.Instance.Player1.IsFirstRoundLoose) GameContext.Instance.Player1.IsFirstRoundLoose = true;
                    else GameContext.Instance.Player1.IsSecondRoundLoose = true;

                    EndRoundAnnouncement($"{GameContext.Instance.Player2.Name} win round by his fraction ability. " +
                                         $"Score {GameContext.Instance.Player2.GeneralScore} - {GameContext.Instance.Player1.GeneralScore}.");
                }
            }
        }
    }
}
