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
        public int Duration { get; set; }

        public PowerModifier(float magnitude, int duration)
        {
            Magnitude = magnitude;
            Duration = duration;
        }
    }
}
