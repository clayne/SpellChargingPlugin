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
        public bool IsTwoHanded => Spell.EquipSlot?.FormId == 0x00013F45;
        public int ChargeLevel { get; private set; } = 0;
        public bool CanCharge { get; }

        private NiNode _particleParentNode;

        private readonly ParticleEngine _particleEngine = new ParticleEngine();
        private List<Particle> _spellParticles;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            Holder = holder;
            Spell = spell;
            Slot = slot;

            // if the spell cannot (or should not) be charged, don't bother with setup
            CanCharge = SpellHelper.CanSpellBeCharged(spell);
            DebugHelper.Print($"[ChargingSpell:{spell.Name}] {(!CanCharge ? "Can't" : "Can")} be charged.");
            if (!CanCharge)
                return;

            // dirty way to pre-cache base power
            foreach (var eff in spell.Effects)
            {
                SpellHelper.GetBasePower(eff);
            }

            RefreshParticleNode();

            // TODO: this is stupid, cache it or something
            DebugHelper.Print($"[ChargingSpell:{spell.Name}] Load Base Particle(s)");
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
            _particleEngine.Update(elapsedSeconds);
            CurrentState.Update(elapsedSeconds);
        }

        /// <summary>
        /// Should only be called by Charging State
        /// </summary>
        public void AddCharge()
        {
            ++ChargeLevel;
            if (Settings.Instance.ChargesPerParticle > 0 && ChargeLevel > 0 && ChargeLevel % Settings.Instance.ChargesPerParticle == 0)
                AddParticleForCharge();
            SpellPowerManager.Instance.IncreasePower(this, ChargeLevel);
        }

        private void AddParticleForCharge()
        {
            if (_particleEngine.ParticleCount >= Settings.Instance.MaxParticles)
                return;

            int localParticleCount = ChargeLevel / (int)Settings.Instance.ChargesPerParticle;
            int distanceFactor = (int)Math.Sqrt(localParticleCount);

            float r1 = (8f + 1f * distanceFactor) * (Randomizer.Roll(0.5) ? -1f : 1f);
            float r2 = (8f + 1f * distanceFactor) * (Randomizer.Roll(0.5) ? -1f : 1f);
            float r3 = (8f + 1f * distanceFactor) * (Randomizer.Roll(0.5) ? -1f : 1f);

            if (IsTwoHanded)
            {
                r1 *= 4f;
                r2 *= 4f;
                r3 *= 8f;
            }
                
            int a1 = Randomizer.NextInt(-1, 1);
            int a2 = Randomizer.NextInt(-1, 1);
            int a3 = a1 == 0 && a2 == 0 ? 1 : Randomizer.NextInt(-1, 1);

            var scale = Randomizer.NextInt(333, 666) * 0.001f * Settings.Instance.ParticleScale;
            var fade = 0.8f;

            var translate = new Vector3D(r1, r2, r3 * 0.8f);

            foreach (var p in _spellParticles)
            {
                if (_particleEngine.ParticleCount >= Settings.Instance.MaxParticles)
                    return;

                Particle newParticle = p
                    .Clone()
                    .SetScale(scale)
                    .SetFade(fade)
                    .Translate(translate)
                    .AttachToNode(_particleParentNode);

                newParticle.AddBehavior(new OrbitBehavior(newParticle, new Vector3D(), new Vector3D(a1, a2, a3), 1f));
                newParticle.AddBehavior(new AimForwardBehavior(newParticle));
                newParticle.AddBehavior(new BreatheBehavior(newParticle, 0.1f, 1f, 16f)
                { Active = () => CurrentState is StateMachine.States.OverchargingBase });
                newParticle.AddBehavior(new FadeBehavior(newParticle, 1.0f)
                { Active = () => !(CurrentState is StateMachine.States.OverchargingBase) });

                _particleEngine.Add(newParticle);
            }
        }

        public void Refund()
        {
            if (Spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration)
                return;
            float toRefund = ChargeLevel * Settings.Instance.MagickaPerCharge;
            Holder.Actor.RestoreActorValue(ActorValueIndices.Magicka, toRefund);
        }

        public void Clean()
        {
            Reset();
            _particleEngine.Clear();
            SpellPowerManager.Instance.ResetSpellModifiers(Spell);
            SpellPowerManager.Instance.ResetSpellPower(Spell);
        }

        public void Reset()
        {
            ChargeLevel = 0;
        }
    }
}
