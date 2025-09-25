using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(CharacterRopeHandling))]
    public static class CharacterRopeHandlingPatches
    {
        [HarmonyPatch(nameof(CharacterRopeHandling.Update))]
        public static bool UpdateReplaced(CharacterRopeHandling __instance)
        {
            if (!DebbyPeam.instance.ModConfig.trouserRope.Value)
            {
                return true;
            }
            if (!__instance.view.IsMine || !__instance.character.data.isRopeClimbing)
            {
                return false;
            }
            if (!__instance.character.data.heldRope.UnityObjectExists() || (__instance.character.data.heldRope != null && !__instance.character.data.heldRope.IsActive()))
            {
                __instance.view.RPC("StopRopeClimbingRpc", RpcTarget.All);
                return false;
            }
            float num = ((__instance.character.input.movementInput.y < 0f) ? 3f : 1f);
            __instance.character.data.ropePercent += __instance.character.data.heldRope.climbingAPI.GetMove() * __instance.character.input.movementInput.y * num * __instance.climbSpeed * __instance.climbSpeedMod * Time.deltaTime * __instance.character.data.heldRope.climbingAPI.UpMult(__instance.character.data.ropePercent);
            __instance.character.data.ropePercent = Mathf.Clamp01(__instance.character.data.ropePercent);
            float num2 = __instance.staminaUsage;
            if (__instance.character.input.movementInput.y > 0.01f)
            {
                num2 = __instance.staminaUsageUp;
            }
            if (__instance.character.IsLocal && (__instance.character.input.jumpWasPressed || !__instance.character.UseStamina(num2 * Time.deltaTime) || __instance.character.data.currentRagdollControll < 0.3f))
            {
                __instance.view.RPC("StopRopeClimbingRpc", RpcTarget.All);
            }
            return false;
        }
    }
}