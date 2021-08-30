using NetScriptFramework.SkyrimSE;
using System;
using System.Collections.Generic;
using System.Linq;
using static NetScriptFramework.SkyrimSE.ActiveEffect;

namespace SpellChargingPlugin.Core
{
    /// <summary>
    /// Handles Effect Magnitude and Duration adjustments for a single Spell
    /// </summary>
    public class SpellPowerManager
    {
        /// <summary>
        /// Percentage gain of power ("power per charge")
        /// </summary>
        public float Growth { get => _growth; set { _growth = value; Update(0.0f); } }

        /// <summary>
        /// Growth multiplier ("current charges")
        /// </summary>
        public float Multiplier { get => _multiplier; set { _multiplier = value; Update(0.0f); } }

        private float _growth;
        private float _multiplier;
        private SpellItem _managedSpell;

        private SpellPowerManager() { }
        public static SpellPowerManager CreateFor(SpellItem spell)
        {
            if (spell == null)
                throw new ArgumentException("Can't assign NULL spell!");

            var ret = new SpellPowerManager()
            {
                _managedSpell = spell,
                _growth = 0.0f,
                _multiplier = 0.0f,
            };
            return ret;
        }

        /// <summary>
        /// Refresh Spell magnitudes. Normally only necessary after updating Growth or Power, but those Properties already call this.
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        private void Update(float elapsedSeconds)
        {
            foreach (var eff in _managedSpell.Effects)
            {
                var basePower = SpellHelper.GetBasePower(eff);
                var modifier = _growth * _multiplier;

                if (modifier > 0.0f && Settings.Instance.HalfPowerWhenMagAndDur)
                {
                    bool hasMag = eff.Magnitude > 0f;
                    bool hasDur = eff.Duration > 0;
                    modifier *= hasMag && hasDur ? 0.5f : 1f;
                }

                eff.Magnitude = basePower.Magnitude * (1.0f + modifier);
                eff.Duration = (int)(basePower.Duration * (1.0f + modifier));
            }
        }
    }
}