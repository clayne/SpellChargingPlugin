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

        [ConfigValue("HotKey", "Hotkey", 
            "Key or key combination to access the various features depending on context.\n" +
            "Pressing the hotkey while idle (not charging or casting) will toggle Overcharge Priority (see above).\n" +
            "Holding the hotkey while charging a spell (but not dual casting) will trigger Spell Sharing and apply the spell to all nearby followers once you release it.\n" +
            "Holding the hotkey while dual casting a spell will trigger Spell Maintenance and semi-permanently apply the spell to you, reducing your maximum Magicka while it is active. Dispel the maintained spell by triggering Spell Maintenance again with the same spell or overwrite it with another.")]
        public string HotKey { get; internal set; }
            = "Shift + G";

        [ConfigValue("PreChargeDelay", "Pre-charge Delay",
            "Time (in seconds) that a non-concentration spell must be held before it begins overcharging.\n" +
            "Set to 0 to begin overcharging immediately.")]
        public float PreChargeDelay { get; internal set; }
            = 0.25f;

        [ConfigValue("PowerPerCharge", "Power Per Charge", 
            "One charge will raise power by this amount in percent.\n" +
            "Note: 'Spell Power' also includes a spell's projectile speed, size, explosion radius, range and impact force (if it has any of those). Those will charge at a slower rate than the two main ones.")]
        public float PowerPerCharge { get; internal set; }
            = 2.5f;

        [ConfigValue("MagickaPerCharge", "Magicka Per Charge", 
            "How much Magicka does one charge cost? This is a flat value, not a percentage!")]
        public float MagickaPerCharge { get; internal set; }
            = 5f;

        [ConfigValue("SkillAffectsPower", "Skill affects Power",
            "Should your skill level in the spell's school further influence the power per charge?")]
        public bool SkillAffectsPower { get; internal set; }
            = true;

        [ConfigValue("ChargesPerSecond", "Charges Per Second", 
            "How many charges are gained per second of charging?\n" +
            "Increasing this setting while decreasing the ones above will give a more granular gain in power at the cost of a tiny hit to performance and stability.")]
        public uint ChargesPerSecond { get; internal set; }
            = 3;

        [ConfigValue("ChargesPerParticle", "Charges Per Particle", 
            "Spawn a charge indicator particle every #N charges. Setting this to 1 will spawn a particle on every new charge.\n" +
            "Particle appearance depends on your mods (spell changes, custom spells, mesh changes, textures etc). Some spells may not spawn any visible particles at all.\n" +
            "Set to 0 to disable the particle system if you think it looks bad or to increase performance a little.")]
        public uint ChargesPerParticle { get; internal set; }
            = 2;

        [ConfigValue("ParticleScale", "Particle Scale", 
            "Make the particles larger (>1.0) or smaller (<1.0). Smaller usually looks better.")]
        public float ParticleScale { get; internal set; }
            = 1.0f;

        [ConfigValue("AllowConcentrationSpells", "Allow Concentration Spells", 
            "Apply Spell Charging to Concentration-Type spells?")]
        public bool AllowConcentrationSpells { get; internal set; }
           = true;

        [ConfigValue("EnableAcceleration", "Enable Charge Acceleration",
            "Speed up charging rate based on how long you've been charging already?\n" +
            "For the impatient wizard.")]
        public bool EnableAcceleration { get; internal set; }
            = true;

        [ConfigValue("AccelerationHalfTime", "Acceleration Halving Time",
            "After charging for this long (in seconds), charging speed will double.\n" +
            "The acceleration is gradual and scales beyond this time, e.g. twice as fast after 5s, three times as fast after 10s, four times after 15s and so on.")]
        public float AccelerationHalfTime { get; internal set; }
            = 3.37f;

        [ConfigValue("UpdatesPerSecond", "Updates Per Second", 
            "Controls how often the plugin updates its state.\n" +
            "Raise it up to your maximum FPS if you want the plugin to be more responsive and the particle effects to look a little smoother at a very small performance cost.\n" +
            "Should also probably increase this when you increase ChargesPerSecond and/or reduce AccelerationHalfTime.")]
        public uint UpdatesPerSecond { get; internal set; }
            = 48;

        [ConfigValue("MaxParticles", "Max Particles", 
            "Maximum number of particles to spawn per hand. Don't go too crazy.\n" +
            "Set to 0 to disable particles.")]
        public uint MaxParticles { get; internal set; }
            = 100;

        [ConfigValue("ParticleLayers", "Particle Layers", 
            "Too few layers cause some spells not to spawn visible particles while charging. Too many will either do nothing or produce ugly results.\n" +
            "Best to leave at 2.")]
        public uint ParticleLayers { get; internal set; }
            = 2;

        [ConfigValue("LogDebugMessages", "Log", 
            "Write debug output to file. Disable unless you experience issues.")]
        public bool LogDebugMessages { get; internal set; }
            = true;
    }
}
