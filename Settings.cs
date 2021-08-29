using NetScriptFramework.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class Settings
    {
        public const float UpdateRate = 1.0f / 60.0f;

        [ConfigValue("ChargeInterval", "Charge Speed", "How much time has to pass before gaining a charge? In seconds.")]
        public static float ChargeInterval { get; internal set; }
            = 1f / 2f;

        [ConfigValue("LogDebugMessages", "Log", "Write debug output to file.")]
        public static bool LogDebugMessages { get; internal set; }
            = true;

        [ConfigValue("CostPerCharge", "Charge Cost", "How much mana is needed for one charge? In percent, based on the setting below.")]
        public static float CostPerCharge { get; internal set; }
            = 1f / 10f;

        [ConfigValue("UseSpellBaseMagicka", "Use spell base magicka", "Use the spell's cost as a base for mana cost? Otherwise, use max mana.")]
        public static bool UseSpellBaseMagicka { get; internal set; }
            = true;

        [ConfigValue("PowerPerCharge", "Charge Power", "How much power does one charge add? In percent.")]
        public static float PowerPerCharge { get; internal set; }
            = 1f / 5f;

        [ConfigValue("MaxChargeForParticles", "Max Charge Particles", "At what charge level do the swirly bits stop appearing?")]
        public static float MaxChargeForParticles { get; internal set; }
            = 100f;

        
    }
}
