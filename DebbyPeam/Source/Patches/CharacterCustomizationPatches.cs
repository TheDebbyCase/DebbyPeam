using DebbyPeam.Misc;
using HarmonyLib;
using UnityEngine;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(CharacterCustomization))]
    public static class CharacterCustomizationPatches
    {
        [HarmonyPatch(nameof(CharacterCustomization.OnPlayerDataChange))]
        [HarmonyPostfix]
        public static void ChangeRopeColour(CharacterCustomization __instance)
        {
            if (DebbyPeam.instance.ModConfig.trouserRope.Value && TrouserRope.trouserRopeDictionary.ContainsKey(__instance._character))
            {
                Material[] materials = new Material[] { TrouserRope.trouserRopeDictionary[__instance._character].rope.ropeBoneVisualizer.meshRenderer.sharedMaterial, TrouserRope.trouserRopeDictionary[__instance._character].ropeAnchorWithRope.anchor.normalPart.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial };
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i].SetTexture("_BaseTexture", null);
                    materials[i].SetColor("_Tint", __instance.PlayerColor / 2f);
                    materials[i].SetFloat("_HueStr1", 0.2f);
                    materials[i].SetColor("_BaseColor", __instance.PlayerColor);
                    materials[i].SetColor("_Color1", __instance.PlayerColor);
                    materials[i].SetColor("_Color21", __instance.PlayerColor);
                    materials[i].SetColor("_Color3", __instance.PlayerColor);
                }
            }
        }
    }
}