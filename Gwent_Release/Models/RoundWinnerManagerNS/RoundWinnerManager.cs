using System.Collections.Generic;

namespace Gwent_Release.Models.RoundWinnerManagerNS
{
    public partial class RoundWinnerManager
    {
        protected List<string> RoundWinners = new List<string>();

        // Should be private non static, or public in GameContext
        public static void EndRoundAnnouncement(string announcement)
        {
            GameContext.Instance.TurnAnnouncement?.Invoke(announcement);
            GameContext.Instance.RoundWinners.Add(announcement);
        }

        protected interface IWinRound
        {
            public void WinRound();
        }

        protected class RoundWinner
        {
            private IWinRound roundWinner;
            public RoundWinner() { }

            public void SetWinner(IWinRound _roundWinner)
            {
                roundWinner = _roundWinner;
            }

            public void WinRound()
            {
                roundWinner.WinRound();
            }
        }                        
    }
}
