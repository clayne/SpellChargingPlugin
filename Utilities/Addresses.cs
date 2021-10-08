using System;

namespace SpellChargingPlugin.Utilities
{
    public static class Addresses
    {
        // ActiveEffect::CalculateDurationAndMagnitude_14053DF40
        // void __fastcall sub(ActiveEffect* a1, Character* a2, MagicTarget* a3)
        public static readonly IntPtr addr_CalculateDurationAndMagnitude = NetScriptFramework.Main.GameInfo.GetAddressOf(33278); // 0x14053DF40
        public static readonly IntPtr addr_ActorIsDualCasting = NetScriptFramework.Main.GameInfo.GetAddressOf(37815); // 0x140632060
        public static readonly IntPtr addr_AttachArtObject = NetScriptFramework.Main.GameInfo.GetAddressOf(22289); // 0x14030F9A0
        public static readonly IntPtr addr_DetachArtObject = NetScriptFramework.Main.GameInfo.GetAddressOf(40382); // 0x14030F9A0
        public static readonly IntPtr addr_gProcessList = NetScriptFramework.Main.GameInfo.GetAddressOf(514167); // 0x141EBEAD0
        public static readonly IntPtr addr_TimeSinceFrame = NetScriptFramework.Main.GameInfo.GetAddressOf(516940);
    }
}
