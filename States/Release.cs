﻿using SpellChargingPlugin.Core;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin.States
{
    public class Release : State<ChargingSpell>
    {
        public Release(StateFactory<ChargingSpell> factory, ChargingSpell context) : base(factory, context)
        {
        }

        public override void Update(float diff)
        {
            TransitionTo(() => new Idle(_factory, _context));
        }
    }
}