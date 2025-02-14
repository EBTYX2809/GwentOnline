using System;

namespace Gwent_Release.Models
{
    public class Effect
    {
        public Action<UnitCard> Modifier { get; set; }
        public void ActivateEffect(UnitCard context)
        {
            Modifier?.Invoke(context);
        }
    }
}
