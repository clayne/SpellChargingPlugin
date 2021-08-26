﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public struct SpellPower
    {
        public readonly float Magnitude;
        public readonly int Duration;

        public SpellPower(float magnitude, int duration)
        {
            Magnitude = magnitude;
            Duration = duration;
        }
    }
}