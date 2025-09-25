using DebbyPeam.Misc;
using HarmonyLib;
using PEAKLib.Core;
using System;
using UnityEngine;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(LoadingScreenHandler))]
    public static class LoadingScreenHandlerPatches
    {
        [HarmonyPatch(nameof(LoadingScreenHandler.Load))]
        [HarmonyPrefix]
        public static bool LoadWithRope(ref LoadingScreen.LoadingScreenType type, ref Action runAfter)
        {
            if (!DebbyPeam.instance.ModConfig.trouserRope.Value)
            {
                return true;
            }
            if (type == LoadingScreen.LoadingScreenType.Plane)
            {
                runAfter += () =>
                {
                    Vector3 spawnPos = Character.localCharacter.GetBodypart(BodypartType.Hip).transform.position;
                    NetworkPrefabManager.SpawnNetworkPrefab("Misc/RopeAnchorTrouser", new Vector3(spawnPos.x, spawnPos.y - 0.65f, spawnPos.z), Quaternion.identity).GetComponent<TrouserRope>().Initialize(Character.localCharacter.photonView.ViewID);
                };
            }
            return true;
        }
    }
}