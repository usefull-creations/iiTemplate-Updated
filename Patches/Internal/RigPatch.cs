using HarmonyLib;

namespace StupidTemplate.Patches.Internal
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
    public class RigDisablePatch
    {
        public static bool Prefix(VRRig __instance) =>
            __instance != VRRig.LocalRig;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public class RigPostTickPatch
    {
        public static bool Prefix(VRRig __instance) =>
            !__instance.isLocal || __instance.enabled;
    }
}
