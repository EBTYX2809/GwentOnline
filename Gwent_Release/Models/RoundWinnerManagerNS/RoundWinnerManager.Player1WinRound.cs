namespace Gwent_Release.Models.RoundWinnerManagerNS
{
    public partial class RoundWinnerManager
    {
        protected class Player1WinRound : IWinRound
        {
            public void WinRound()
            {
                if (!GameContext.Instance.Player2.IsFirstRoundLoose) GameContext.Instance.Player2.IsFirstRoundLoose = true;
                else GameContext.Instance.Player2.IsSecondRoundLoose = true;

                EndRoundAnnouncement($"{GameContext.Instance.Player1.Name} win round. " +
                    $"Score {GameContext.Instance.Player1.GeneralScore} - {GameContext.Instance.Player2.GeneralScore}.");

                if (GameContext.Instance.Player1.Leader.Fraction == Fractions.NorthKingdoms) GameContext.Instance.Player1.TakeCard(1);
            }
        }
    }
}
