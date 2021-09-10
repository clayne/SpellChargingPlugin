﻿using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using NetScriptFramework.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpellChargingPlugin
{
    public static class Util
    {
        // ActiveEffect::CalculateDurationAndMagnitude_14053DF40
        // void __fastcall sub(ActiveEffect* a1, Character* a2, MagicTarget* a3)
        public static IntPtr addr_CalculateDurationAndMagnitude = new IntPtr(0x14053DF40).FromBase();

        private static Dictionary<string, NiAVObject> _nifCache = new Dictionary<string, NiAVObject>();

        public static NiAVObject LoadNif(string nifPath)
        {
            if (!_nifCache.TryGetValue(nifPath, out NiAVObject toLoad))
            {
                NiAVObject.LoadFromFile(
                    new NiObjectLoadParameters()
                    {
                        FileName = nifPath,
                        Callback = p =>
                        {
                            if (p.Success)
                            {
                                if (p.Result[0] is NiAVObject obj)
                                {
                                    DebugHelper.Print($"[Util] NIF: {nifPath} loaded");
                                    toLoad = obj;
                                    toLoad.IncRef();
                                    _nifCache.Add(nifPath, toLoad);
                                    DebugHelper.Print($"[Util] NIF: {nifPath} cached");
                                }
                            }
                        }
                    });
            }
            DebugHelper.Print($"[Util] NIF: Returning {toLoad} ({toLoad?.Name})");
            return toLoad;
        }

        public sealed class SimpleTimer
        {
            private float _elapsedSeconds = 0f;

            public bool Enabled { get; set; } = true;

            public void Update(float elapsedSeconds)
            {
                if (!Enabled)
                    return;
                _elapsedSeconds += elapsedSeconds;
            }

            /// <summary>
            /// Check if a certain amount of time has passed since this method has been called and reset the timer
            /// </summary>
            /// <param name="seconds"></param>
            /// <param name="elapsed">The current value of the internal timer</param>
            /// <returns></returns>
            public bool HasElapsed(float seconds, out float elapsed, bool reset = true)
            {
                elapsed = _elapsedSeconds;
                if (!Enabled || _elapsedSeconds < seconds)
                    return false;
                if (reset)
                    _elapsedSeconds = 0.0f;
                return true;
            }
        }

        public sealed class SimpleDeferredExecutor
        {
            private static ConcurrentDictionary<uint, SimpleDeferredExecutor> _executors = new ConcurrentDictionary<uint, SimpleDeferredExecutor>();

            private SimpleTimer _timer = new SimpleTimer();
            private Action _action;
            private float _deferTime;

            public bool Completed { get; private set; }

            private SimpleDeferredExecutor(Action action, float initialDefer = 0f)
            {
                _action = action;
                _deferTime = initialDefer;
            }

            public static void Defer(Action action, uint ident, float time)
            {
                if (!_executors.TryGetValue(ident, out var exec))
                    _executors.TryAdd(ident, new SimpleDeferredExecutor(action, time));
                else
                    Interlocked.Exchange(ref exec._deferTime, exec._deferTime + time);
            }

            public static void Update(float elapsedSeconds)
            {
                if (_executors.IsEmpty)
                    return;
                for (int i = 0; i < _executors.Count; i++)
                {
                    var exec = _executors.ElementAt(i);
                    if (exec.Value.Completed)
                    {
                        _executors.TryRemove(exec.Key, out _);
                        --i;
                    }
                    else
                    {
                        exec.Value._timer.Update(elapsedSeconds);
                        if (!exec.Value._timer.HasElapsed(exec.Value._deferTime, out _))
                            continue;
                        exec.Value._action();
                        exec.Value.Completed = true;
                    }
                }
            }
        }

        public static class Visuals
        {
            public static void AttachArtObject(uint formID, Character target, float duration = -1)
            {
                var art = TESForm.LookupFormById(formID) as BGSArtObject;
                if (art == null || target == null)
                    return;
                Memory.InvokeCdecl(
                    new IntPtr(0x14030F9A0).FromBase(), // int32 __fastcall sub(Character* a1, BGSArtObject* a2, int64 a3, Character* a4, int64 a5, uint8 a6, int64 a7, uint8 a8)
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
                var func = new IntPtr(0x1406DDA30).FromBase();
                Memory.InvokeCdecl(func,                                    // int32 __fastcall sub(void* a1, Character*a2, BGSArtObject* a3)
                    Memory.ReadPointer(new IntPtr(0x141EBEAD0).FromBase()), // gProcessLists (static pointer?)
                    target.Cast<TESObjectREFR>(),                           // target
                    art.Address);                                           // art object
            }
        }
    }
}
