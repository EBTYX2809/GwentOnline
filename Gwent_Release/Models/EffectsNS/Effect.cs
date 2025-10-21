using System;
using Gwent_Release.Models.CardsNS;

namespace Gwent_Release.Models.EffectsNS
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
