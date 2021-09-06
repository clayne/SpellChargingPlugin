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
        public ChargingActor Holder { get; }
        public SpellItem Spell { get; }
        public EquippedSpellSlots Slot { get; }

        public State<ChargingSpell> CurrentState { get; set; }
        public bool IsTwoHanded => Spell.EquipSlot?.FormId == Settings.Instance.EquipBothFormID;
        public bool CanCharge { get; }

        private readonly SpellPowerManager _spellPowerManager;
        private NiNode _particleParentNode;
        private int _chargeLevel = 0;

        private ParticleEngine _particleEngine;
        private List<Particle> _spellParticles;
        private Util.SimpleTimer _chargingTimer = new Util.SimpleTimer();
        private readonly float _fChargesPerSecond = 1.0f / Settings.Instance.ChargesPerSecond;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            DebugHelper.Print($"[ChargingSpell] {spell.Name} Initializing");

            Holder = holder;
            Spell = spell;
            Slot = slot;

            // if the spell cannot (or should not) be charged, don't bother with setup
            CanCharge = SpellHelper.CanSpellBeCharged(spell);
            if (!CanCharge)
                return;

            RefreshParticleNode();

            DebugHelper.Print($"[ChargingSpell] Create Spell Power Manager");
            _spellPowerManager = SpellPowerManager.Create(this);
            _spellPowerManager.Growth = Settings.Instance.PowerPerCharge / 100f;

            if (Settings.Instance.SkillAffectsPower)
            {
                var school = spell.Effects.FirstOrDefault()?.Effect?.AssociatedSkill;
                if(school != null)
                    _spellPowerManager.Growth *= (1f + Holder.Actor.GetActorValue(school.Value) / 100f);
            }

            // TODO: this is stupid, cache it or something
            DebugHelper.Print($"[ChargingSpell] Load Base Particle(s)");
            _spellParticles = spell.Effects.SelectMany(eff =>
            {
                return new List<string>()
                {
                    eff.Effect?.CastingArt?.ModelName?.Text,       // this usually works best, but some spells have bad or no (visible) casting art
                    eff.Effect?.MagicProjectile?.ModelName?.Text,  // may look stupid with some spells
                    eff.Effect?.HitEffectArt?.ModelName?.Text,     // probably looks dumb
                }
                .Where(s => string.IsNullOrEmpty(s) == false);
            }).Distinct().Take((int)Settings.Instance.ParticleLayers).Select(nif => Particle.Create(nif)).ToList();

            DebugHelper.Print($"[ChargingSpell] Create Particle Engine");
            _particleEngine = ParticleEngine.Create(Settings.Instance.MaxParticles);

            DebugHelper.Print($"[ChargingSpell] Enter Idle State");
            CurrentState = new StateMachine.States.Idle(this);
        }

        /// <summary>
        /// This will only affect newly spawned particles and most likely causes visual glitches.
        /// </summary>
        public void RefreshParticleNode()
        {
            _particleParentNode = GetNode(Slot) as NiNode;
        }

        // TODO: Maybe find a better fitting node for the twohanded spells
        private NiAVObject GetNode(EquippedSpellSlots slot)
        {
            var plrRootNode = Holder.Actor.Node;
            // This may be possible?
            if (plrRootNode == null)
                return null;
            if (IsTwoHanded)
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
            if (!CanCharge)
                return;

            CurrentState.Update(elapsedSeconds);
            _particleEngine.Update(elapsedSeconds);
        }

        /// <summary>
        /// Should only be called by Charging State
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        public void UpdateCharge(float elapsedSeconds)
        {
            if (!CanCharge)
                return;

            _chargingTimer.Update(elapsedSeconds);
            if (!_chargingTimer.HasElapsed(_fChargesPerSecond, out _))
                return;

            // Don't waste mana when you can't charge the chosen property
            if (Holder.Mode == ChargingActor.OperationMode.Duration && !SpellHelper.HasDuration(Spell))
                return;
            if (Holder.Mode == ChargingActor.OperationMode.Magnitude && !SpellHelper.HasMagnitude(Spell))
                return;

            if (!TryDrainMagicka(Settings.Instance.MagickaPerCharge))
                return;

            ++_chargeLevel;
            if (Settings.Instance.ChargesPerParticle > 0 && _chargeLevel > 0f && _chargeLevel % Settings.Instance.ChargesPerParticle == 0)
                AddParticleForCharge(_chargeLevel);
            _spellPowerManager.IncreasePower();
        }

        private void AddParticleForCharge(float chargeLevel)
        {
            if (ParticleEngine.GlobalParticleCount >= Settings.Instance.MaxParticles)
                return;

            int localParticleCount = (int)(chargeLevel / Settings.Instance.ChargesPerParticle);
            int distanceFactor = (int)Math.Sqrt(localParticleCount);

            float r1 = (10f + Randomizer.NextInt(distanceFactor, (distanceFactor * 4) / 3)) * (Randomizer.Roll(0.5) ? -1f : 1f);
            float r2 = (10f + Randomizer.NextInt(distanceFactor, (distanceFactor * 4) / 3)) * (Randomizer.Roll(0.5) ? -1f : 1f);
            float r3 = (10f + Randomizer.NextInt(distanceFactor, (distanceFactor * 4) / 3)) * (Randomizer.Roll(0.5) ? -1f : 1f);

            if (IsTwoHanded)
            {
                r1 *= 4f;
                r2 *= 4f;
                r3 *= 8f;
            }
                
            int a1 = Randomizer.NextInt(-1, 1);
            int a2 = Randomizer.NextInt(-1, 1);
            int a3 = a1 == 0 && a2 == 0 ? 1 : Randomizer.NextInt(-1, 1);

            var scale = Randomizer.NextInt(200, 500) * 0.001f;
            var fade = 0.5f;

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
                    .AttachToNode(_particleParentNode);

                newParticle.AddBehavior(new OrbitBehavior(newParticle, new Vector3D(), new Vector3D(a1, a2, a3), 2f));
                newParticle.AddBehavior(new AimForwardBehavior(newParticle));
                newParticle.AddBehavior(new BreatheBehavior(newParticle, 0.1f, 1f, 8f)
                { Active = () => CurrentState is StateMachine.States.Charging });
                newParticle.AddBehavior(new FadeBehavior(newParticle, 0.75f)
                { Active = () => !(CurrentState is StateMachine.States.Charging) });

                _particleEngine.Add(newParticle);
            }
        }

        private bool TryDrainMagicka(float magCost)
        {
            if (Holder.Actor.GetActorValue(ActorValueIndices.Magicka) < magCost)
                return false;
            Holder.Actor.DamageActorValue(ActorValueIndices.Magicka, -magCost);
            return true;
        }

        public void Refund()
        {
            if (Spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration)
                return;
            float toRefund = _chargeLevel * Settings.Instance.MagickaPerCharge;
            Holder.Actor.RestoreActorValue(ActorValueIndices.Magicka, toRefund);
        }

        public void ResetAndClean()
        {
            _chargeLevel = 0;
            _spellPowerManager?.ResetPower();
            _particleEngine?.Clear();
        }
    }
}
