using NetScriptFramework.SkyrimSE;
using System.Collections.Generic;

namespace SpellChargingPlugin.Utilities
{
    public static class Nif
    {
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
                                }
                            }
                        }
                    });
            }
            return toLoad;
        }
    }
}
