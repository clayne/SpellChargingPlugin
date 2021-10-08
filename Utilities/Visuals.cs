using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.Core;
using SpellChargingPlugin.ParticleSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpellChargingPlugin.Utilities
{
    public static class Visuals
    {
        /// <summary>
        /// Note: ArtObjects with the default duration (-1) are PERMANENT and will stick to the character unless they get detached manually
        /// </summary>
        /// <param name="formID"></param>
        /// <param name="target"></param>
        /// <param name="duration">in seconds</param>
        public static void AttachArtObject(uint formID, Character target, float duration = -1)
        {
            var art = TESForm.LookupFormById(formID) as BGSArtObject;
            if (art == null || target == null)
                return;
            Memory.InvokeCdecl(
                Addresses.addr_AttachArtObject,     // int32 __fastcall sub(Character* a1, BGSArtObject* a2, int64 a3, Character* a4, int64 a5, uint8 a6, int64 a7, uint8 a8)
                target.Cast<TESObjectREFR>(),       // target
                art.Address,                        // art object
                duration,                           // duration
                target.Cast<TESObjectREFR>(),       // no clue
                0,                                  // some bool, artobject not visible (near the player, at least) when set to 1
                0,                                  // no clue
                IntPtr.Zero,                        // no clue
                IntPtr.Zero);                       // no clue
        }
        public static void DetachArtObject(uint formID, Character target)
        {
            var art = TESForm.LookupFormById(formID) as BGSArtObject;
            if (art == null || target == null)
                return;
            Memory.InvokeCdecl(
                Addresses.addr_DetachArtObject,                     // int32 __fastcall sub(void* a1, Character*a2, BGSArtObject* a3)
                Memory.ReadPointer(Addresses.addr_gProcessList),    // gProcessLists (static pointer?)
                target.Cast<TESObjectREFR>(),                       // target
                art.Address);                                       // art object
        }
        public static NiAVObject GetParticleSpawnNode(ChargingSpell chargingSpell)
        {
            var plrRootNode = chargingSpell.Owner.Character.Node;
            if (plrRootNode == null)
                return null;
            if (chargingSpell.IsTwoHanded)
                return plrRootNode.LookupNodeByName("NPC Head [Head]");
            switch (chargingSpell.Slot)
            {
                case EquippedSpellSlots.LeftHand:
                    return plrRootNode.LookupNodeByName("NPC L MagicNode [LMag]");
                case EquippedSpellSlots.RightHand:
                    return plrRootNode.LookupNodeByName("NPC R MagicNode [RMag]");
                default:
                    return plrRootNode;
            }
        }

        private static Dictionary<SpellItem, List<Particle>> _spellParticleCache = new Dictionary<SpellItem, List<Particle>>();
        public static List<Particle> GetParticlesFromSpell(SpellItem spell)
        {
            if (!_spellParticleCache.TryGetValue(spell, out var ret))
                _spellParticleCache.Add(spell, ret = spell.Effects.SelectMany(eff =>
                {
                    return new List<string>()
                    {
                        eff.Effect?.MagicProjectile?.ModelName?.Text,  // may look weird with some spells
                        eff.Effect?.CastingArt?.ModelName?.Text,       // this usually works best, but some spells have bad or no (visible) casting art
                        eff.Effect?.HitEffectArt?.ModelName?.Text,     // probably looks dumb
                    }
                    .Where(s => string.IsNullOrEmpty(s) == false);
                }).Distinct().Take((int)Settings.Instance.ParticleLayers).Select(nif => Particle.Create(nif)).ToList());
            return ret;
        }
    }

}
