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
        public sealed class ChargingSpellCreationArgs
        {
            public SpellItem Spell { get; set; }
            public ChargingActor Owner { get; set; }
            public EquippedSpellSlots Slot { get; set; }
        }

        public ChargingActor Owner { get; private set; }
        public SpellItem Spell { get; private set; }
        public EquippedSpellSlots Slot { get; private set; }
        public State<ChargingSpell> CurrentState { get; set; }
        public bool IsTwoHanded => Spell.EquipSlot?.FormId == 0x00013F45;

        private ParticleEngine _particleEngine;
        private int _chargeLevel;
        private bool _canCharge;

        private ChargingSpell(){}

        public static ChargingSpell Create(ChargingSpellCreationArgs args)
        {
            var ret = new ChargingSpell()
            {
                Owner = args.Owner,
                Spell = args.Spell,
                Slot = args.Slot,
                _canCharge = SpellHelper.CanSpellBeCharged(args.Spell),
                _chargeLevel = 0,
            };
            DebugHelper.Print($"[ChargingSpell:{args.Spell.Name}] {(!ret._canCharge ? "Can't" : "Can")} be charged.");
            if (!ret._canCharge)
                return ret;

            // cache base power
            foreach (var eff in args.Spell.Effects)
                SpellHelper.GetBasePower(eff);

            ret._particleEngine =
                ParticleEngine.Create(
                    new ParticleEngine.ParticleEngineCreationArgs()
                    {
                        Limit = (int)Settings.Instance.MaxParticles,
                        ParticleBatchFactory = ret.CreateParticles,
                    });
            ret.CurrentState = new StateMachine.States.Idle(ret);
            return ret;
        }

        internal void Update(float elapsedSeconds)
        {
            if (!_canCharge)
                return;
            _particleEngine.Update(elapsedSeconds);
            CurrentState.Update(elapsedSeconds);
        }

        /// <summary>
        /// Should only be called by Charging State
        /// </summary>
        public void AddCharge()
        {
            ++_chargeLevel;
            _particleEngine.SpawnParticle();
            SpellPowerManager.Instance.IncreasePower(this, _chargeLevel);
        }

        public void Refund()
        {
            if (Spell.SpellData.CastingType == EffectSettingCastingTypes.Concentration)
                return;
            float toRefund = _chargeLevel * Settings.Instance.MagickaPerCharge;
            Owner.Character.RestoreActorValue(ActorValueIndices.Magicka, toRefund);
        }

        public void Reset()
        {
            _chargeLevel = 0;
            _particleEngine.Clear();
            SpellPowerManager.Instance.ResetSpellModifiers(Spell);
            SpellPowerManager.Instance.ResetSpellPower(Spell);
        }

        private IEnumerable<Particle> CreateParticles()
        {
            int localParticleCount = _chargeLevel / (int)Settings.Instance.ChargesPerParticle;
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

            return Utilities.Visuals.GetParticlesFromSpell(Spell).Select(p =>
            {
                Particle newParticle = p
                    .Clone()
                    .SetScale(scale)
                    .SetFade(fade)
                    .Translate(translate)
                    .AttachToNode(Utilities.Visuals.GetParticleSpawnNode(this) as NiNode);

                newParticle.AddBehavior(new OrbitBehavior(newParticle, new Vector3D(), new Vector3D(a1, a2, a3), 1f));
                newParticle.AddBehavior(new AimForwardBehavior(newParticle));
                newParticle.AddBehavior(new BreatheBehavior(newParticle, 0.1f, 1f, 16f)
                { Active = () => CurrentState is StateMachine.States.OverchargingBase });
                newParticle.AddBehavior(new FadeBehavior(newParticle, 1.0f)
                { Active = () => !(CurrentState is StateMachine.States.OverchargingBase) });

                return newParticle;
            });
        }
    }
}
