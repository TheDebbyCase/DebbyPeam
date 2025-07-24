using DebbyPeam.Misc;
using HarmonyLib;
using PEAKLib.Core;
using Photon.Pun;
using UnityEngine;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(Character))]
    public static class CharacterPatches
    {
        [HarmonyPatch(nameof(Character.Start))]
        [HarmonyPostfix]
        public static void StartAddRope(Character __instance)
        {
            if (PhotonNetwork.IsMasterClient && DebbyPeam.instance.ModConfig.trouserRope.Value)
            {
                Vector3 spawnPos = __instance.GetBodypart(BodypartType.Hip).transform.position;
                NetworkPrefabManager.SpawnNetworkPrefab("Misc/RopeAnchorTrouser", new Vector3(spawnPos.x, spawnPos.y - 0.65f, spawnPos.z), Quaternion.identity).GetComponent<TrouserRope>().Initialize(__instance.photonView.ViewID);
            }
        }
    }
}