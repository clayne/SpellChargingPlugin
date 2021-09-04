using NetScriptFramework.Tools;
using SpellChargingPlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public sealed class Settings
    {
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = Create());

        private static Settings Create()
        {
            var ret = new Settings();
            ConfigFile.LoadFrom<Settings>(ret, "SpellChargingPlugin", true);
            return ret;
        }
        private Settings(){ }

        [ConfigValue("OperationMode", "Default Mode", "By default, only a spell's magnitude (damage/healing/armor..) will be affedted.\nAllowed: Magnitude, Duration, Both, Disabled\nYou can also switch between them ingame by pressing the hotkey.")]
        public string OperationMode { get; internal set; }
            = "Magnitude";

        [ConfigValue("HotKey", "HotKey", "Key or key combination to toggle between modes or disable the mod altogether.")]
        public string HotKey { get; internal set; }
            = "Shift + G";

        [ConfigValue("PowerPerCharge", "Power Per Charge", "One charge will raise power by this amount in percent. Recommended to set this and MagickaPerCharge to the same value in order to have spells gain 100% power for every 100 points of Magicka spent on charges. ")]
        public float PowerPerCharge { get; internal set; }
            = 10.0f;

        [ConfigValue("MagickaPerCharge", "Magicka Per Charge", "How much Magicka does one charge cost? This is a flat value, not a percentage!")]
        public float MagickaPerCharge { get; internal set; }
            = 10f;

        [ConfigValue("ChargesPerSecond", "Charges Per Second", "How many charges are gained per second of charging?")]
        public uint ChargesPerSecond { get; internal set; }
            = 2;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", "Spawn a charge indicator particle every N charges. Setting this to 1 would spawn a particle on every new charge.\nWith the default settings, every particle would indicate that your spell has become 50% stronger.")]
        public uint ChargesPerParticle { get; internal set; }
            = 5;

        [ConfigValue("AllowConcentrationSpells", "Allow Concentration Spells", "Apply Spell Charging to Concentration-Type spells?")]
        public bool AllowConcentrationSpells { get; internal set; }
           = true;

        [ConfigValue("UpdatesPerSecond", "Updates Per Second", "Performance setting. Controls how often the plugin updates its state. Leave at 30 UPS or raise it up to your maximum FPS if you want the charge particle effects to look a little smoother.")]
        public uint UpdatesPerSecond { get; internal set; }
            = 30;

        [ConfigValue("MaxParticles", "Max Particles", "Performance setting. Maximum number of particles to spawn in TOTAL (counting spells in both hands). Don't go too crazy.")]
        public uint MaxParticles { get; internal set; }
            = 100;

        [ConfigValue("ParticleLayers", "Particle Layers", "Performance setting. Too few layers cause some spells not to spawn visible particles while charging.")]
        public uint ParticleLayers { get; internal set; }
            = 2;

        [ConfigValue("LogDebugMessages", "Log", "Write debug output to file. If usng MO, look inside your Overwrite folder. Leave disabled unless you experience issues.")]
        public bool LogDebugMessages { get; internal set; }
            = true;

        [ConfigValue("EquipBothFormID", "EquipBoth FormID", "Don't touch this.", ConfigEntryFlags.PreferHex | ConfigEntryFlags.Hidden)]
        public uint EquipBothFormID { get; internal set; }
            = 0x00013F45;
    }
}
