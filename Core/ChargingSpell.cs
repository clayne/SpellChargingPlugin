using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using SpellChargingPlugin.ParticleSystem;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SpellChargingPlugin.Core
{
    public class ChargingSpell : IStateHolder<ChargingSpell>
    {
        public ChargingActor Holder { get; set; }
        public SpellItem Spell { get; set; }
        public EquippedSpellSlots Slot { get; set; }
        public State<ChargingSpell> CurrentState { get; set; }
        public bool IsResetting { get; set; }
        public bool IsTwoHanded => Spell.EquipSlot?.FormId == Settings.Instance.EquipBothFormID;

        private readonly SpellPowerManager _spellPowerManager;
        private NiNode _particleOrbitCenter;
        private float _timeSpentCharging;
        private float _chargeLevel = 0f;
        private bool _isConcentration;

        private ParticleEngine _particleEngine;
        private List<Particle> _spellParticles;

        private float _fChargesPerSecond = 1.0f / Settings.Instance.ChargesPerSecond;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            DebugHelper.Print($"[ChargingSpell] {spell.Name} Initializing");

            Holder = holder;
            Spell = spell;
            Slot = slot;

            _isConcentration = spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration;

            _particleOrbitCenter = GetNode(Slot) as NiNode;

            DebugHelper.Print($"[ChargingSpell] . Spell power manager");
            _spellPowerManager = SpellPowerManager.CreateFor(spell);
            _spellPowerManager.Growth = Settings.Instance.PowerPerCharge / 100f;
            _spellPowerManager.Multiplier = _chargeLevel;

            DebugHelper.Print($"[ChargingSpell] . Base particle load");

            // TODO: this is stupid, cache it or something
            _spellParticles = spell.Effects.SelectMany(eff =>
            {
                return new List<string>()
                {
                    eff.Effect?.MagicProjectile?.ModelName?.Text,  // may look stupid with some spells
                    eff.Effect?.CastingArt?.ModelName?.Text,       // this usually works best, but some spells have bad or no (visible) casting art
                    eff.Effect?.HitEffectArt?.ModelName?.Text,     // probably looks dumb
                }
                .Where(s => string.IsNullOrEmpty(s) == false);
            }).Distinct().Take((int)Settings.Instance.ParticleLayers).Select(nif => Particle.Create(nif)).ToList();

            DebugHelper.Print($"[ChargingSpell] . Particle engine create");
            _particleEngine = ParticleEngine.Create(Settings.Instance.MaxParticles);
            _chargeLevel = 0.0f;

            DebugHelper.Print($"[ChargingSpell] . State init");
            CurrentState = new States.Idle(this);
        }

        /// <summary>
        /// This will only affect newly spawned particles and most likely causes visual glitches
        /// </summary>
        public void UpdateParticleNode()
        {
            _particleOrbitCenter = GetNode(Slot) as NiNode;
        }

        // TODO: Maybe find a better fitting node for the twohanded spells
        private NiAVObject GetNode(EquippedSpellSlots slot)
        {
            var plrRootNode = Holder.Actor.Node;
            // This may be possible?
            if (plrRootNode == null)
                return null;
            if(IsTwoHanded)
                return plrRootNode.LookupNodeByName("NPC Head [Head]");
            switch (slot)
            {
                case EquippedSpellSlots.LeftHand:
                    return plrRootNode.LookupNodeByName("NPC L MagicNode [LMag]");
                case EquippedSpellSlots.RightHand:
                    return plrRootNode.LookupNodeByName("NPC R MagicNode [RMag]");
                default:
                    return plrRootNode;
            }
        }

        internal void Update(float elapsedSeconds)
        {
            CurrentState.Update(elapsedSeconds);
            _particleEngine.Update(elapsedSeconds);
        }

        /// <summary>
        /// Should only be called by state
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        internal void UpdateCharge(float elapsedSeconds)
        {
            _timeSpentCharging += elapsedSeconds;
            if (_timeSpentCharging < _fChargesPerSecond)
                return;
            _timeSpentCharging = 0f;

            
            if (!TryDrainMagicka(Settings.Instance.MagickaPerCharge * (_isConcentration ? 0.1f : 1f)))
                return;

            _chargeLevel += 1.0001f;
            AddPowerForCharge(_chargeLevel);
            if ((int)_chargeLevel % Settings.Instance.ChargesPerParticle == 0)
                AddParticleForCharge(_chargeLevel);
        }

        private void AddParticleForCharge(float chargeLevel)
        {
            if (ParticleEngine.GlobalParticleCount >= Settings.Instance.MaxParticles)
                return;

            int localParticleCount = (int)(chargeLevel / Settings.Instance.ChargesPerParticle);
            float fadeFactor = 1f - 100f / (100f + localParticleCount);
            float speedFactor = 1f + fadeFactor;
            int distanceFactor = (int)Math.Sqrt(localParticleCount * (1f - fadeFactor));

            if (IsTwoHanded)
                distanceFactor *= 4;

            float r1 = (5f + Randomizer.NextInt(distanceFactor, (distanceFactor * 3) / 2)) * (Randomizer.Roll(0.5) ? -1f : 1f);
            float r2 = (5f + Randomizer.NextInt(distanceFactor, (distanceFactor * 3) / 2)) * (Randomizer.Roll(0.5) ? -1f : 1f);
            float r3 = (5f + Randomizer.NextInt(distanceFactor, (distanceFactor * 3) / 2)) * (Randomizer.Roll(0.5) ? -1f : 1f);

            int a1 = Randomizer.NextInt(-1, 1);
            int a2 = Randomizer.NextInt(-1, 1);
            int a3 = a1 == 0 && a2 == 0 ? 1 : Randomizer.NextInt(-1, 1);

            var scale = Randomizer.NextInt(166, 333) * 0.001f;
            var fade = 0.5f + 0.5f * fadeFactor;

            var translate = new Vector3D(r1, r2, r3 * 0.8f);

            foreach (var p in _spellParticles)
            {
                if (ParticleEngine.GlobalParticleCount >= Settings.Instance.MaxParticles)
                    return;

                Particle newParticle = p
                    .Clone()
                    .SetScale(scale)
                    .SetFade(fade)
                    .Translate(translate)
                    .AttachToNode(_particleOrbitCenter);

                newParticle.AddBehavior(new OrbitBehavior(newParticle, new Vector3D(0, 0, 0), new Vector3D(a1, a2, a3), speedFactor));
                newParticle.AddBehavior(new AimForwardBehavior(newParticle));
                newParticle.AddBehavior(new BreatheBehavior(newParticle, 0.1f, 1f, 5f)
                { Active = () => CurrentState is States.Charging });
                newParticle.AddBehavior(new FadeBehavior(newParticle, 0.5f)
                { Active = () => !(CurrentState is States.Charging) });

                _particleEngine.Add(newParticle);
            }
        }

        private void AddPowerForCharge(float chargeLevel)
        {
            _spellPowerManager.Multiplier = chargeLevel;
        }

        private bool TryDrainMagicka(float magCost)
        {
            if (Holder.Actor.GetActorValue(ActorValueIndices.Magicka) < magCost)
                return false;
            Holder.Actor.DamageActorValue(ActorValueIndices.Magicka, -magCost);
            return true;
        }

        // TODO: reset & clean with a little more grace
        public void Reset()
        {
            IsResetting = true;
            _chargeLevel = 0.0f;
            _spellPowerManager.Multiplier = _chargeLevel;
            _particleEngine.Clear();
        }
    }
}
