using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.ParticleSystem;
using SpellChargingPlugin.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public ParticleEngine ParticleEngine => _particleEngine;

        private PowerModifier[] _powerModifier;
        private SpellPower[] _spellBase;
        private NiAVObject _magicNode;
        private float _timeSpentCharging;

        private float _chargeLevel = 0f;

        private ParticleEngine _particleEngine = new ParticleEngine();
        private Particle _myEffectParticle;
        private List<Particle> _additionalParticles = new List<Particle>();

        private readonly float _constantMagickaCost;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            Holder = holder;
            Spell = spell;
            Slot = slot;

            _magicNode = FindMyMagicNode();
            InitializeParticleEngine(spell.Effects.FirstOrDefault());

            foreach (var item in spell.Effects.Where(e => e.Effect?.MagicProjectile != null))
            {
                var p = CreateParticleFromNIF(item.Effect.MagicProjectile.ModelName.Text);
                if (p != null)
                    _additionalParticles.Add(p);
            }

            _constantMagickaCost = spell.SpellData.CostOverride * Settings.CostPerCharge;
            _spellBase = SpellHelper.DefineBaseSpellPower(Spell);
            _powerModifier = _spellBase.Select(_ => new PowerModifier(0f, 0)).ToArray();
            _chargeLevel = 0.0f;

            var factory = new StateFactory<ChargingSpell>();
            var idleState = new States.Idle(factory, this);
            CurrentState = factory.GetOrCreate(() => idleState);
        }

        private NiAVObject FindMyMagicNode()
        {
            // The character root node.
            var plrRootNode = Holder.Character.Node;

            // This maybe possible?
            if (plrRootNode == null)
                return null;
            return Slot == EquippedSpellSlots.LeftHand
                    ? plrRootNode.LookupNodeByName("NPC L MagicNode [LMag]")
                    : plrRootNode.LookupNodeByName("NPC R MagicNode [RMag]");
        }

        private Particle CreateParticleFromNIF(string fname)
        {
            Particle ret = null;
            NiAVObject.LoadFromFile(
                new NiObjectLoadParameters()
                {
                    FileName = fname,
                    Callback = p =>
                    {
                        if (p.Success)
                        {
                            if (p.Result[0] is NiAVObject obj)
                            {
                                Particle particle = _particleEngine.CreateFromNiAVObject(obj);
                                particle.SetVelocity(new Vector3D(0, 0, 0));
                                particle.SetScale(1f);
                                ret = particle;
                            }
                        }
                    }
                });
            return ret;
        }

        private void InitializeParticleEngine(ActiveEffect.EffectItem effectItem)
        {
            if (effectItem == null)
                return;
            var fname = effectItem.Effect.CastingArt.ModelName.Text;
            // no casting art? maybe
            if (string.IsNullOrEmpty(fname))
                return;

            DebugHelper.Print($"Precaching particle from NIF {fname}...");
            NiAVObject.LoadFromFileAsync(
                new NiObjectLoadParameters()
                {
                    FileName = fname,
                    Callback = p =>
                    {
                        if (p.Success)
                        {
                            if (p.Result[0] is NiAVObject obj)
                            {
                                Particle particle = _particleEngine.CreateFromNiAVObject(obj);
                                particle.SetVelocity(new Vector3D(0, 0, 0));
                                particle.SetScale(1f);
                                _myEffectParticle = particle;
                                DebugHelper.Print($"Caching sucessful!");

                                _particleEngine.AddBehavior(
                                    new ParticleSystem.Behaviors.OrbitBehavior(
                                        _myEffectParticle.Object.LocalTransform.Position));
                                _particleEngine.AddBehavior(
                                    new ParticleSystem.Behaviors.BreatheBehavior(0.25f, 1f, 3.14f));
                                _particleEngine.AddBehavior(
                                    new ParticleSystem.Behaviors.LookAtBehavior());
                                _particleEngine.Initialized = true;
                            }
                        }
                    }
                });
        }

        internal void Update(float elapsedSeconds)
        {
            CurrentState.Update(elapsedSeconds);
            _particleEngine.Update(elapsedSeconds);
        }

        /// <summary>
        /// called by state
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        internal void UpdateCharge(float elapsedSeconds)
        {
            _timeSpentCharging += elapsedSeconds;
            if (_timeSpentCharging < Settings.ChargeInterval)
                return;
            _timeSpentCharging = 0f;

            if (!TryDrainMagicka(_constantMagickaCost))
                return;

            _chargeLevel += 1.0f;
            AddPowerForCharge(_chargeLevel);
            AddParticleForCharge(_chargeLevel);
            ScaleHandSpellForCharge(_chargeLevel);
            UpdateStats();
        }

        private void ScaleHandSpellForCharge(float chargeLevel)
        {
            if (_magicNode.LocalTransform.Scale > 5f)
                return;
            _magicNode.LocalTransform.Scale = 1f + 5f * (1f - Settings.MaxParticles / (Settings.MaxParticles + chargeLevel));
            _magicNode.Update(0.5f);
        }

        private void AddParticleForCharge(float chargeLevel)
        {
            if (chargeLevel > Settings.MaxParticles)
                return;

            float visualFactor = 1.0f - (25f / (25f + chargeLevel));
            int distanceFactor = (int)(2f * chargeLevel * (1-visualFactor));

            float r1 = NetScriptFramework.Tools.Randomizer.NextInt(-distanceFactor, distanceFactor);
            float r2 = NetScriptFramework.Tools.Randomizer.NextInt(-distanceFactor, distanceFactor);
            float r3 = NetScriptFramework.Tools.Randomizer.NextInt(-distanceFactor, distanceFactor);

            int a1 = NetScriptFramework.Tools.Randomizer.NextInt(-1, 1);
            int a2 = NetScriptFramework.Tools.Randomizer.NextInt(-1, 1);
            int a3 = a1 == 0 && a2 == 0 ? 1 : NetScriptFramework.Tools.Randomizer.NextInt(-1, 1);

            var scale = (float)NetScriptFramework.Tools.Randomizer.NextInt(500, 1500) * 0.001f;
            var fade = 0.5f;
            var translate = new Vector3D(r1, r2, r3 * 0.1f);
            var velocity = new Vector3D(a1, a3, a2);

            Particle newParticle = _myEffectParticle.Clone();
            newParticle.SetScale(scale);
            newParticle.SetFade(fade);
            newParticle.Translate(translate);
            newParticle.SetVelocity(velocity);
            newParticle.AttachToNode(_magicNode.Parent);
            _particleEngine.Add(newParticle);

            foreach (var p in _additionalParticles)
            {
                Particle np = p.Clone();
                np.SetScale(scale);
                np.SetFade(fade);
                np.Translate(translate);
                np.SetVelocity(velocity);
                np.AttachToNode(_magicNode.Parent);
                _particleEngine.Add(np);
            }
        }

        private void AddPowerForCharge(float chargeLevel)
        {
            for (int i = 0; i < _spellBase.Length; i++)
            {
                var eff = Spell.Effects[i];

                bool hasMag = eff.Magnitude > 0f;
                bool hasDur = eff.Duration > 0;
                float realPercantage = chargeLevel * Settings.PowerPerCharge * (hasMag && hasDur ? 0.5f : 1f);

                _powerModifier[i].Magnitude = _spellBase[i].Magnitude * realPercantage;
                _powerModifier[i].Duration = _spellBase[i].Duration * realPercantage;

                DebugHelper.Print($"Charge {Spell.Name}.{eff.Effect.Name} bonusMAG: {_powerModifier[i].Magnitude}, bonusDur: {_powerModifier[i].Duration}");
            }
        }

        public void UpdateStats()
        {
            for (int i = 0; i < _spellBase.Length; i++)
            {
                var eff = Spell.Effects[i];

                eff.Magnitude = _spellBase[i].Magnitude + _powerModifier[i].Magnitude;
                eff.Duration = (int)(_spellBase[i].Duration + _powerModifier[i].Duration);
            }
        }

        private bool TryDrainMagicka(float magCost)
        {
            DebugHelper.Print($"Trying to drain {magCost} magicka");
            if (Holder.Character.GetActorValue(ActorValueIndices.Magicka) < magCost)
                return false;
            Holder.Character.DamageActorValue(ActorValueIndices.Magicka, -magCost);
            return true;
        }

        public void Reset()
        {
            IsResetting = true;
            for (int i = 0; i < Spell.Effects.Count; i++)
            {
                _powerModifier[i].Magnitude = 0f;
                _powerModifier[i].Duration = 0f;

                var eff = Spell.Effects[i];
                DebugHelper.Print($"Reset {Spell.Name}.{eff.Effect.Name}");
            }
            _chargeLevel = 0.0f;
            _magicNode.LocalTransform.Scale = 1f;
            UpdateStats();
            _particleEngine.ClearParticles();
            _particleEngine.ResetBehaviors();
        }
    }
}
