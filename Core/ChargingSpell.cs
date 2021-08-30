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

        public bool IsTwoHanded => Slot == EquippedSpellSlots.Other;
        public ParticleEngine Particle => _particleEngine;

        private readonly SpellPowerManager _spellPowerManager;
        private NiNode _particleOrbitCenter;
        private float _timeSpentCharging;
        private float _chargeLevel = 0f;

        private ParticleEngine _particleEngine = new ParticleEngine();
        private Particle _myEffectParticle;

        private TESCameraStates _currentCamState;

        public ChargingSpell(ChargingActor holder, SpellItem spell, EquippedSpellSlots slot)
        {
            Holder = holder;
            Spell = spell;
            Slot = slot;

            _spellPowerManager = SpellPowerManager.CreateFor(spell);
            _spellPowerManager.Growth = Settings.Instance.PowerPerCharge;
            _spellPowerManager.Multiplier = _chargeLevel;

            _particleEngine = ParticleEngine.Create(Settings.Instance.MaxParticles);
            _chargeLevel = 0.0f;

            CurrentState = new States.Idle(this);

            // TODO: this probably shouldn't be handled by this object
            Events.OnUpdateCamera.Register(e =>
            {
                var id = e.Camera.State.Id;
                if (id == _currentCamState)
                    return;
                _currentCamState = id;
                if (id == TESCameraStates.FirstPerson || id == TESCameraStates.ThirdPerson1 || id == TESCameraStates.ThirdPerson2)
                    RelocateParticles(_currentCamState);
            });
        }

        /// <summary>
        /// This will only affect newly spawned particles and most likely causes visual glitches
        /// </summary>
        private void RelocateParticles(TESCameraStates _currentCamState)
        {
            _particleOrbitCenter = GetNode(Slot) as NiNode;
            DebugHelper.Print($"Camera switch to {_currentCamState} Change particle node and expect funny visuals!");
        }

        // TODO: Maybe find a better fitting node for the twohanded spells
        private NiAVObject GetNode(EquippedSpellSlots slot)
        {
            var plrRootNode = Holder.Actor.Node;
            // This may be possible?
            if (plrRootNode == null)
                return null;
            switch (slot)
            {
                case EquippedSpellSlots.LeftHand:
                    return plrRootNode.LookupNodeByName("NPC L MagicNode [LMag]");
                case EquippedSpellSlots.RightHand:
                    return plrRootNode.LookupNodeByName("NPC R MagicNode [RMag]");
                case EquippedSpellSlots.Other:
                    return plrRootNode.LookupNodeByName("NPC Head [Head]");
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
        /// called by state
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        internal void UpdateCharge(float elapsedSeconds)
        {
            _timeSpentCharging += elapsedSeconds;
            if (_timeSpentCharging < Settings.Instance.ChargeInterval)
                return;
            _timeSpentCharging = 0f;

            if (!TryDrainMagicka(Holder.Actor.GetActorValueMax(ActorValueIndices.Magicka) * 0.01f))
                return;

            _chargeLevel += 1.0001f;
            AddPowerForCharge(_chargeLevel);
            if ((int)_chargeLevel % Settings.Instance.ChargesPerParticle == 0)
                AddParticleForCharge(_chargeLevel);
        }

        private void AddParticleForCharge(float chargeLevel)
        {
            var particleNum = chargeLevel / (float)Settings.Instance.ChargesPerParticle;
            if (particleNum > Settings.Instance.MaxChargeForParticles)
                return;

            float visualFactor = 1.0f - (100 / (100 + particleNum));
            int distanceFactor = (int)(10f + (float)Math.Log(20f * particleNum * (1.0f - visualFactor)));

            if (IsTwoHanded)
                distanceFactor *= 2;

            float r1 = Randomizer.NextInt(-distanceFactor, distanceFactor);
            float r2 = Randomizer.NextInt(-distanceFactor, distanceFactor);
            float r3 = Randomizer.NextInt(-distanceFactor, distanceFactor);

            var scale = Randomizer.NextInt(250, 500) * 0.001f;
            var fade = 0.5f;
            var translate = new Vector3D(r1, r2, r3 * 0.5f);

            Particle newParticle = _myEffectParticle
                .Clone()
                .SetScale(scale)
                .SetFade(fade)
                .Translate(translate)
                .AttachToNode(_particleOrbitCenter as NiNode);

            newParticle.AddBehavior(new OrbitBehavior(newParticle, new Vector3D(0, 0, 0) , new Vector3D(1, 0, 0)));
            newParticle.AddBehavior(new BreatheBehavior(newParticle, 0.1f, 1f, 10f));
            newParticle.AddBehavior(new LookForwardBehavior(newParticle));

            _particleEngine.Add(newParticle);
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

        public bool Equals(SpellItem other)
            => other?.FormId == Spell?.FormId;
    }
}
