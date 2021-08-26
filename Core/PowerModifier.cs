using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.Core
{
    public class PowerModifier
    {
        public float Magnitude { get; set; }
        public float Duration { get; set; }

        public PowerModifier(float magnitude, float duration)
        {
            Magnitude = magnitude;
            Duration = duration;
        }
    }
}
