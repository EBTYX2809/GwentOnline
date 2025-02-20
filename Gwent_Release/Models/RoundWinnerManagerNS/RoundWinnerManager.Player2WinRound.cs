namespace Gwent_Release.Models.RoundWinnerManagerNS
{
    public partial class RoundWinnerManager
    {
        protected class Player2WinRound : IWinRound
        {
            public void WinRound()
            {
                if (!GameContext.Instance.Player1.IsFirstRoundLoose) GameContext.Instance.Player1.IsFirstRoundLoose = true;
                else GameContext.Instance.Player1.IsSecondRoundLoose = true;

                EndRoundAnnouncement($"{GameContext.Instance.Player2.Name} win round. " +
                    $"Score {GameContext.Instance.Player2.GeneralScore} - {GameContext.Instance.Player1.GeneralScore}.");

                if (GameContext.Instance.Player2.Leader.Fraction == Fractions.NorthKingdoms) GameContext.Instance.Player2.TakeCard(1);
            }
        }
    }
}
