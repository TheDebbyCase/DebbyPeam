using DebbyPeam.Misc;
using HarmonyLib;
using PEAKLib.Core;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(Character))]
    public static class CharacterPatches
    {
        public static Dictionary<Character, bool>  characterRopesAdded = new Dictionary<Character, bool>();
        [HarmonyPatch(nameof(Character.Start))]
        [HarmonyPostfix]
        public static void StartAddRope(Character __instance)
        {
            if (!DebbyPeam.instance.ModConfig.trouserRope.Value)
            {
                return;
            }
            if (SceneManagerHelper.ActiveSceneName == "Airport")
            {
                SpawnRope(__instance);
                return;
            }
            if (!characterRopesAdded.TryAdd(__instance, false))
            {
                characterRopesAdded[__instance] = false;
            }
        }
        [HarmonyPatch(nameof(Character.UpdateVariablesFixed))]
        [HarmonyPostfix]
        public static void MorningWood(Character __instance)
        {
            if (!DebbyPeam.instance.ModConfig.trouserRope.Value)
            {
                return;
            }
            if (__instance.data.passedOutOnTheBeach <= 0f && !characterRopesAdded[__instance])
            {
                SpawnRope(__instance);
                characterRopesAdded[__instance] = true;
            }
        }
        public static void SpawnRope(Character character)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 spawnPos = character.GetBodypart(BodypartType.Hip).transform.position;
                NetworkPrefabManager.SpawnNetworkPrefab("Misc/RopeAnchorTrouser", new Vector3(spawnPos.x, spawnPos.y - 0.65f, spawnPos.z), Quaternion.identity).GetComponent<TrouserRope>().Initialize(character.photonView.ViewID);
            }
        }
    }
}