using DebbyPeam.Misc;
using HarmonyLib;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(CharacterAfflictions))]
    public static class CharacterAfflictionsPatches
    {
        [HarmonyPatch(nameof(CharacterAfflictions.UpdateWeight))]
        [HarmonyPostfix]
        public static void RopeWeight(CharacterAfflictions __instance)
        {
            if (DebbyPeam.instance.ModConfig.trouserRope.Value && TrouserRope.trouserRopeDictionary.ContainsKey(__instance.character))
            {
                float toAdd = TrouserRope.trouserRopeDictionary[__instance.character].weightAdded;
                if (toAdd > 0)
                {
                    __instance.SetStatus(CharacterAfflictions.STATUSTYPE.Weight, __instance.character.refs.afflictions.currentStatuses[(int)CharacterAfflictions.STATUSTYPE.Weight] + toAdd);
                }
            }
        }
    }
}