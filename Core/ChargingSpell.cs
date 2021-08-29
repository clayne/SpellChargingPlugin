using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.ParticleSystem;
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
        public bool IsTwoHanded { get; set; }
        public ParticleEngine ParticleEngine => _particleEngine;

        private PowerModifier[] _powerModifier;
        private SpellPower[] _spellBase;

        private NiAVObject _particleOrbitCenter;

        private float _timeSpentCharging;

        private float _chargeLevel = 0f;

        private ParticleEngine _particleEngine = new ParticleEngine();
        private Particle _myEffectParticle;
        private List<Particle> _additionalParticles = new List<Particle>();

        private TESCameraStates _currentCamID;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot, bool isTwoHand = false)
        {
            Holder = holder;
            Spell = spell;
            Slot = slot;
            IsTwoHanded = isTwoHand;

            _particleOrbitCenter = GetNode(slot, isTwoHand);

            // TODO: find a way to intelligently find the best mesh to use as a particle instead of loading and using ALL of them
            InitializeParticleEngine(spell.Effects.FirstOrDefault());
            var tmpNifs = new List<string>();
            foreach (var adeff in spell.Effects.Select(e => e.Effect?.HitEffectArt).Where(e => e != null))
            {
                var nif = adeff.ModelName.Text;
                tmpNifs.Add(nif);
            }
            foreach (var adeff in spell.Effects.Select(e => e.Effect?.CastingArt).Where(e => e != null))
            {
                var nif = adeff.ModelName.Text;
                tmpNifs.Add(nif);
            }
            foreach (var adeff in spell.Effects.Select(e => e.Effect?.MagicProjectile).Where(e => e != null))
            {
                var nif = adeff.ModelName.Text;
                tmpNifs.Add(nif);
            }
            foreach (var toLoad in tmpNifs.Distinct())
            {
                _additionalParticles.Add(CreateParticleFromNIF(toLoad));
            }
            
            _spellBase = SpellHelper.DefineBaseSpellPower(Spell);
            _powerModifier = _spellBase.Select(_ => new PowerModifier(0f, 0)).ToArray();
            _chargeLevel = 0.0f;

            var factory = new StateFactory<ChargingSpell>();
            var idleState = new States.Idle(factory, this);
            CurrentState = factory.GetOrCreate(() => idleState);

            _currentCamID = PlayerCamera.Instance.State.Id;

            // TODO: this probably shouldn't be handled by this object
            Events.OnUpdateCamera.Register(e =>
            {
                var id = e.Camera.State.Id;
                if (id == _currentCamID)
                    return;
                _currentCamID = id;
                if (id == TESCameraStates.FirstPerson || id == TESCameraStates.ThirdPerson1 || id == TESCameraStates.ThirdPerson2)
                    RelocateParticles();
            });

        }

        private void RelocateParticles()
        {
            _particleOrbitCenter = GetNode(Slot, IsTwoHanded);
            DebugHelper.Print($"Change particle node to {_particleOrbitCenter.Name}");
        }

        private NiAVObject GetNode(EquippedSpellSlots slot, bool both = false)
        {
            var plrRootNode = Holder.Character.Node;
            // This may be possible?
            if (plrRootNode == null)
                return null;
            if (both)
                return plrRootNode.LookupNodeByName("NPC Head [Head]");
            return slot == EquippedSpellSlots.LeftHand
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
                                DebugHelper.Print($"NIF: {fname} Loaded");
                                Particle particle = _particleEngine.CreateFromNiAVObject(obj);
                                particle.SetVelocity(new Vector3D(0, 0, 0));
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
                                particle.SetFade(1.0f);
                                particle.SetScale(0.5f);
                                _myEffectParticle = particle;

                                _particleEngine.AddBehavior(
                                    new ParticleSystem.Behaviors.OrbitBehavior(
                                        _myEffectParticle.Object.LocalTransform.Position));
                                _particleEngine.AddBehavior(
                                    new ParticleSystem.Behaviors.BreatheBehavior(0.1f, 1f, 10f));
                                _particleEngine.AddBehavior(
                                    new ParticleSystem.Behaviors.LookAtBehavior(Holder.Character));
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

            if (!TryDrainMagicka(Holder.Character.GetActorValueMax(ActorValueIndices.Magicka) * 0.01f))
                return;

            _chargeLevel += 1.01f;
            AddPowerForCharge(_chargeLevel);
            if((int)_chargeLevel % Settings.ChargesPerParticle == 0)
                AddParticleForCharge(_chargeLevel);
            UpdateStats();
        }

        int __pn = 0;
        private void AddParticleForCharge(float chargeLevel)
        {
            var particleNum = chargeLevel / (float)Settings.ChargesPerParticle;
            if (particleNum > Settings.MaxChargeForParticles)
                return;
            
            float visualFactor = 1.0f - (100 / (100 + particleNum));
            int distanceFactor = (int)(10f + (float)Math.Log( 20f * particleNum * (1.0f - visualFactor)) );

            if (IsTwoHanded)
                distanceFactor *= 2;

            ++__pn;
            DebugHelper.Print($"{Spell.Name} Particles: {__pn}, DistanceFac: {distanceFactor}, VisualFac: {visualFactor}");

            float r1 = NetScriptFramework.Tools.Randomizer.NextInt(-distanceFactor, distanceFactor);
            float r2 = NetScriptFramework.Tools.Randomizer.NextInt(-distanceFactor, distanceFactor);
            float r3 = NetScriptFramework.Tools.Randomizer.NextInt(-distanceFactor, distanceFactor);

            int a1 = NetScriptFramework.Tools.Randomizer.NextInt(-1, 1);
            int a2 = NetScriptFramework.Tools.Randomizer.NextInt(-1, 1);
            int a3 = a1 == 0 && a2 == 0 ? 1 : NetScriptFramework.Tools.Randomizer.NextInt(-1, 1);

            var scale = (float)NetScriptFramework.Tools.Randomizer.NextInt(250, 500) * 0.001f;
            var fade = 0.5f;
            var translate = new Vector3D(r1, r2, r3 * 0.5f);
            var velocity = new Vector3D(a1, a3, a2);

            Particle newParticle = _myEffectParticle.Clone();
            newParticle.SetScale(scale);
            newParticle.SetFade(fade);
            newParticle.Translate(translate);
            newParticle.SetVelocity(velocity);
            newParticle.AttachToNode(_particleOrbitCenter.Parent);
            _particleEngine.Add(newParticle);

            // TODO: get rid of this particle spam
            foreach (var p in _additionalParticles)
            {
                Particle np = p.Clone();
                np.SetScale(scale);
                np.SetFade(fade);
                np.Translate(translate);
                np.SetVelocity(velocity);
                np.AttachToNode(_particleOrbitCenter.Parent);
                _particleEngine.Add(np);
            }
        }

        private void AddPowerForCharge(float chargeLevel)
        {
            for (int i = 0; i < _spellBase.Length; i++)
            {
                var eff = Spell.Effects[i];
                var powerBonus = 0.01f * Settings.PowerPerHundredCharges * chargeLevel;

                if (Settings.HalfPowerWhenMagAndDur)
                {
                    bool hasMag = eff.Magnitude > 0f;
                    bool hasDur = eff.Duration > 0;
                    powerBonus = powerBonus * (hasMag && hasDur ? 0.5f : 1f);
                }

                _powerModifier[i].Magnitude = _spellBase[i].Magnitude * powerBonus;
                _powerModifier[i].Duration = _spellBase[i].Duration * powerBonus;

                //DebugHelper.Print($"Charge {Spell.Name}.{eff.Effect.Name} bonusMAG: {_powerModifier[i].Magnitude}, bonusDur: {_powerModifier[i].Duration}");
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
            if (Holder.Character.GetActorValue(ActorValueIndices.Magicka) < magCost)
                return false;
            Holder.Character.DamageActorValue(ActorValueIndices.Magicka, -magCost);
            return true;
        }

        // TODO: reset & clean with a little more grace
        public void Reset()
        {
            __pn = 0;

            IsResetting = true;
            for (int i = 0; i < Spell.Effects.Count; i++)
            {
                _powerModifier[i].Magnitude = 0f;
                _powerModifier[i].Duration = 0f;

                var eff = Spell.Effects[i];
                DebugHelper.Print($"Reset {Spell.Name}.{eff.Effect.Name}");
            }
            _chargeLevel = 0.0f;
            UpdateStats();
            _particleEngine.ClearParticles();
            _particleEngine.ResetBehaviors();
        }
    }
}
