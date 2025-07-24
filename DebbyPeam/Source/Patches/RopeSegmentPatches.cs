using DebbyPeam.Misc;
using HarmonyLib;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(RopeSegment))]
    public static class RopeSegmentPatches
    {
        [HarmonyPatch(nameof(RopeSegment.IsInteractible))]
        [HarmonyPrefix]
        public static bool NoSelfInteraction(ref bool __result, RopeSegment __instance, ref Character interactor)
        {
            if (DebbyPeam.instance.ModConfig.trouserRope.Value && TrouserRope.trouserRopeDictionary.ContainsKey(interactor))
            {
                if (TrouserRope.trouserRopeDictionary[interactor].rope.GetRopeSegments().Contains(__instance.transform))
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}