using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static NetScriptFramework.SkyrimSE.ActiveEffect;

namespace SpellChargingPlugin.Core
{
    /// <summary>
    /// Singleton that is supposed to track actors actors with ActiveEffects that have been inflicted by another (or the same) character.
    /// Needs to be cleared regularly to avoid wasting resources on effects that have expired.
    /// </summary>
    public sealed class ActiveEffectTracker
    {
        private static ActiveEffectTracker _instance;
        public static ActiveEffectTracker Instance => _instance ?? (_instance = new ActiveEffectTracker());

        private ConcurrentDictionary<IntPtr, ConcurrentSet<ActiveEffectHolder>> _trackedEffects = new ConcurrentDictionary<IntPtr, ConcurrentSet<ActiveEffectHolder>>();
        private ActiveEffectTracker() { }

        public TrackingResult Tracked(IntPtr activeEffectPtr)
        {
            if (_trackedEffects.TryGetValue(activeEffectPtr, out var victims))
                return new TrackingResult(victims);
            else
                return new TrackingResult(Enumerable.Empty<ActiveEffectHolder>());
        }
        public TrackingResult Tracked()
        {
            return new TrackingResult(_trackedEffects.SelectMany(kv => kv.Value));
        }
        public TrackingSetup Track(IntPtr activeEffectPtr)
        {
            if (!_trackedEffects.TryGetValue(activeEffectPtr, out var ret))
                if (!_trackedEffects.TryAdd(activeEffectPtr, ret = new ConcurrentSet<ActiveEffectHolder>()))
                    throw new Exception($"[ActiveEffectTracker] Failed to track entry! {activeEffectPtr.ToHexString()}");
            return new TrackingSetup(ret);
        }
        public void Clear()
        {
            DebugHelper.Print($"[ActiveEffectTracker] Clearing");
            _trackedEffects.Clear();
        }

        public void PurgeInvalids()
        {
            var iv = _trackedEffects.SelectMany(kv => kv.Value).Where(v => v.Invalid).ToArray();
            foreach (var item in iv)
            {
                DebugHelper.Print($"[ActiveEffectTracker] Purging invalid entry {item.Effect.ToHexString()}");
                _trackedEffects.TryRemove(item.Effect,out var _);
            }
        }

        public sealed class TrackingResult : IEnumerable<ActiveEffectHolder>
        {
            private IEnumerable<ActiveEffectHolder> _victims;
            public TrackingResult(IEnumerable<ActiveEffectHolder> victims)
            {
                this._victims = victims;
            }

            public TrackingResult FromOffender(IntPtr offenderPtr)
            {
                _victims = _victims.Where(e => e.Offender == offenderPtr);
                return this;
            }

            public TrackingResult ForVictim(IntPtr victimPtr)
            {
                _victims = _victims.Where(e => e.Me == victimPtr);
                return this;
            }

            public TrackingResult ForBaseEffect(uint formID)
            {
                _victims = _victims.Where(e => e.BaseEffectID == formID);
                return this;
            }

            public IEnumerator<ActiveEffectHolder> GetEnumerator()
            {
                return _victims.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _victims.GetEnumerator();
            }
        }
        public sealed class TrackingSetup
        {
            ActiveEffectHolder _entry;
            public TrackingSetup(ICollection<ActiveEffectHolder> currentVictims)
            {
                _entry = new ActiveEffectHolder();
                currentVictims.Add(_entry);
            }

            public TrackingSetup FromOffender(IntPtr offenderPtr)
            {
                _entry.Offender = offenderPtr;
                return this;
            }

            public TrackingSetup ForVictim(IntPtr victimPtr)
            {
                _entry.Me = victimPtr;
                return this;
            }

            public TrackingSetup WithBaseEffectID(uint formId)
            {
                _entry.BaseEffectID = formId;
                return this;
            }

            public TrackingSetup WithActiveEffect(IntPtr activeEffect)
            {
                _entry.Effect = activeEffect;
                return this;
            }
        }
        public sealed class ActiveEffectHolder
        {
            public IntPtr Effect { get; set; }
            public IntPtr Me { get; set; }
            public IntPtr Offender { get; set; }
            public float Magnitude { get; set; }
            public int Sign { get; set; } = 1;
            public float Duration { get; set; }
            public uint BaseEffectID { get; set; }
            public bool Invalid { get; internal set; }
        }
    }
}