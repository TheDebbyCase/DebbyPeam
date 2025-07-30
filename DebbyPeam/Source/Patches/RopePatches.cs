using DebbyPeam.Misc;
using HarmonyLib;
using Photon.Pun;
using System.Collections.Generic;
namespace DebbyPeam.Patches
{
    [HarmonyPatch(typeof(Rope))]
    public static class RopePatches
    {
        [HarmonyPatch(nameof(Rope.Awake))]
        [HarmonyPostfix]
        public static void FixRopeShaders(Rope __instance)
        {
            DebbyPeam.instance.utils.FixShaders(__instance.ropeSegmentPrefab);
            DebbyPeam.instance.utils.FixShaders(__instance.remoteSegmentPrefab);
        }
        [HarmonyPatch(nameof(Rope.OnPlayerEnteredRoom))]
        [HarmonyPostfix]
        public static void PlayerJoinSync(ref Photon.Realtime.Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient && TrouserRope.trouserRopeDictionary.Count > 0)
            {
                List<Character> players = Character.AllCharacters;
                for (int i = 0; i < players.Count; i++)
                {
                    if (TrouserRope.trouserRopeDictionary.ContainsKey(players[i]))
                    {
                        TrouserRope trouserRope = TrouserRope.trouserRopeDictionary[players[i]];
                        trouserRope.photonView.RPC("InitializeRPC", newPlayer, players[i].photonView.ViewID);
                        trouserRope.photonView.RPC("GetRopeRPC", newPlayer, trouserRope.rope.photonView.ViewID);
                    }
                }
            }
        }
    }
}