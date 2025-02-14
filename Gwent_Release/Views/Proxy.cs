using System.Collections.ObjectModel;
using Gwent_Release.Models;

namespace Gwent_Release.Views
{
    public class Proxy // Have to use because of static
    {
        public Player Player1 => GameContext.Player1;
        public Player Player2 => GameContext.Player2;
        public ObservableCollection<WeatherCard> WeatherCards => GameContext.WeatherCardsBattleRow;
    }
}
