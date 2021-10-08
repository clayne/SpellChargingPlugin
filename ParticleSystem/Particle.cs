using NetScriptFramework;
using NetScriptFramework.SkyrimSE;
using SpellChargingPlugin.ParticleSystem.Behaviors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace SpellChargingPlugin.ParticleSystem
{
    public class Particle : IDisposable
    {
        public bool Delete { get; set; }
        public NiAVObject Object => _niAvObject;

        private NiAVObject _niAvObject;
        private List<IParticleBehavior> _behaviors = new List<IParticleBehavior>();
        private bool _disposing = false;
        public static Particle Create(string nifPath)
        {
            if (string.IsNullOrEmpty(nifPath))
                return null;
            var obj = Utilities.Nif.LoadNif(nifPath).Clone() as NiAVObject;
            //DebugHelper.Print($"Particle obj: {obj}");
            Particle ret = new Particle()
            {
                _niAvObject = obj,
                Delete = false,
            };
            return ret;
        }

        private Particle() {}

        public void Dispose()
        {
            if (_disposing)
                return;
            _disposing = true;
            _niAvObject.Detach();
            _niAvObject.DecRef();
            _niAvObject = null;
            _behaviors.Clear();
        }

        public void Update(float elapsedSeconds)
        {
            foreach (var behavior in _behaviors)
            {
                behavior.Update(elapsedSeconds);
            }
        }

        /// <summary>
        /// Attach this particle to a parent (makes it actually appear in the game).
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>this</returns>
        public Particle AttachToNode(NiNode parent)
        {
            parent.AttachObject(_niAvObject);
            return this;
        }

        /// <summary>
        /// Size
        /// </summary>
        /// <param name="scale"></param>
        /// <returns>this</returns>
        public Particle SetScale(float scale)
        {
            _niAvObject.LocalTransform.Scale = scale;
            return this;
        }

        /// <summary>
        /// Creates a copy of this particle WITHOUT any behaviors
        /// </summary>
        /// <returns>cloned object</returns>
        public Particle Clone()
        {
            var ret = new Particle()
            {
                _niAvObject = _niAvObject.Clone() as NiAVObject,
                Delete = false,
            };
            //DebugHelper.Print($"Cloned: {_niAvObject.Name} ({_niAvObject.Address} -> {ret._niAvObject.Address})");
            return ret;
        }

        /// <summary>
        /// Transparency (?)
        /// </summary>
        /// <param name="fadeValue"></param>
        /// <returns>this</returns>
        public Particle SetFade(float fadeValue)
        {
            var mptr = _niAvObject.Cast<BSFadeNode>();
            if (mptr != IntPtr.Zero)
            {
                Memory.WriteFloat(mptr + 0x130, fadeValue);
                Memory.WriteFloat(mptr + 0x140, fadeValue);
            }
            //DebugHelper.Print($"Fade: {_niAvObject.Name} set to {fadeValue}");
            return this;
        }

        /// <summary>
        /// Move in LOCAL space
        /// </summary>
        /// <param name="offset"></param>
        /// <returns>this</returns>
        public Particle Translate(Vector3D offset)
        {
            using (var alloc = Memory.Allocate(0x10))
            {
                var pt = MemoryObject.FromAddress<NiPoint3>(alloc.Address);
                pt.X = (float)offset.X;
                pt.Y = (float)offset.Y;
                pt.Z = (float)offset.Z;
                _niAvObject.LocalTransform.Translate(pt, _niAvObject.LocalTransform.Position);
            }
            //DebugHelper.Print($"Translation: {_niAvObject.Name} set to {_niAvObject.LocalTransform.Position}");
            return this;
        }

        public Particle AddBehavior(IParticleBehavior behavior)
        {
            _behaviors.Add(behavior);
            //DebugHelper.Print($"Behavior: {_niAvObject.Name} added {behavior}");
            return this;
        }
    }

}
