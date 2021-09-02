using System;
using System.Collections.Generic;

namespace SpellChargingPlugin
{
    public class SpellVictimContainer
    {
        public List<IntPtr> Victims { get; set; } = new List<IntPtr>();
        public float Magnitude { get; set; }
        public int Sign { get; set; }
    }
}