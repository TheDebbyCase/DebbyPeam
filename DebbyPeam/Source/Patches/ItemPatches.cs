using HarmonyLib;
using Peak.Afflictions;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
namespace DebbyPeam.Patches
{
    public class SkullThrowInstantiater : MonoBehaviour
    {
        public void Start()
        {
            DebbyPeam.instance.log.LogDebug("SkullThrowInstantiater Start");
            PhotonNetwork.Instantiate("Misc/SkullThrowLogic", transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
    public class SkullThrowLogic : MonoBehaviour
    {
        public PhotonView photonView;
        public List<Affliction> afflictions = new List<Affliction>();
        public void Start()
        {
            DebbyPeam.instance.log.LogDebug("SkullThrowLogic Start");
            ItemDatabase.TryGetItem(25, out Item skullRef);
            Action_ApplyMassAffliction skullMassAffliction = skullRef.GetComponent<Action_ApplyMassAffliction>();
            photonView = GetComponent<PhotonView>();
            afflictions = new List<Affliction>(skullMassAffliction.extraAfflictions);
            if (skullMassAffliction.affliction != null)
            {
                DebbyPeam.instance.log.LogDebug($"Adding {skullMassAffliction.affliction.GetAfflictionType()} to SkullThrowLogic");
                afflictions.Add(skullMassAffliction.affliction);
            }
            if (PhotonNetwork.IsMasterClient)
            {
                DebbyPeam.instance.log.LogDebug($"SkullThrowLogic finding closest player");
                float closestPlayerDist = float.MaxValue;
                int playerIndex = -1;
                DebbyPeam.instance.log.LogDebug($"Character Count: \"{Character.AllCharacters.Count}\"");
                for (int i = 0; i < Character.AllCharacters.Count; i++)
                {
                    float newDist = Vector3.Distance(transform.position, Character.AllCharacters[i].HipPos());
                    DebbyPeam.instance.log.LogDebug($"Distance from character {i}: \"{newDist}\"");
                    if (newDist < closestPlayerDist && newDist < 5f)
                    {
                        closestPlayerDist = newDist;
                        playerIndex = i;
                        DebbyPeam.instance.log.LogDebug($"SkullThrowLogic player \"{Character.AllCharacters[i].characterName}\" ({playerIndex}) is now the closest character with distance: \"{closestPlayerDist}\"");
                    }
                }
                if (playerIndex != -1)
                {
                    DebbyPeam.instance.log.LogDebug($"Player selected");
                    photonView.RPC("CurseLocalPlayerRPC", Character.AllCharacters[playerIndex].view.Owner);
                }
                StartCoroutine(DestroyTimerCoroutine());
            }
            DebbyPeam.instance.log.LogDebug("SkullThrowLogic Start Finished");
        }
        public IEnumerator DestroyTimerCoroutine()
        {
            DebbyPeam.instance.log.LogDebug($"SkullThrowLogic Destroy Coroutine Start");
            yield return new WaitForSeconds(5f);
            DebbyPeam.instance.log.LogDebug($"SkullThrowLogic Destroy Coroutine End");
            PhotonNetwork.Destroy(gameObject);
        }
        [PunRPC]
        public void CurseLocalPlayerRPC()
        {
            DebbyPeam.instance.log.LogDebug($"Killing you!");
            Character.localCharacter.Invoke("DieInstantly", 0.02f);
            photonView.RPC("AddAfflictionsRPC", RpcTarget.Others);
        }
        [PunRPC]
        public void AddAfflictionsRPC()
        {
            for (int i = 0; i < afflictions.Count; i++)
            {
                DebbyPeam.instance.log.LogDebug($"Adding affliction: {afflictions[i].GetAfflictionType()}");
                Character.localCharacter.refs.afflictions.AddAffliction(afflictions[i]);
            }
        }
    }
    [HarmonyPatch(typeof(Item))]
    public static class ItemPatches
    {
        [HarmonyPatch(nameof(Item.Awake))]
        [HarmonyPrefix]
        public static bool MakeSkullBreakable(Item __instance)
        {
            if (DebbyPeam.instance.ModConfig.throwableSkull.Value && __instance.UIData.itemName == "Cursed Skull")
            {
                DebbyPeam.instance.log.LogDebug("Cursed Skull Spawning");
                ItemDatabase.TryGetItem(36, out Item coconutRef);
                DebbyPeam.instance.log.LogDebug($"\"{coconutRef.UIData.itemName}\" reference found");
                GameObject skullObject = __instance.gameObject;
                Bonkable bonkable = skullObject.AddComponent<Bonkable>();
                bonkable.minBonkVelocity = 10f;
                bonkable.ragdollTime = 1f;
                bonkable.bonkForce = 400f;
                bonkable.bonkRange = 1f;
                bonkable.bonk = coconutRef.GetComponent<Bonkable>().bonk;
                DebbyPeam.instance.log.LogDebug("Skull Bonkable added");
                Breakable breakable = skullObject.AddComponent<Breakable>();
                breakable.breakOnCollision = true;
                breakable.minBreakVelocity = 15f;
                breakable.instantiateOnBreak = new List<GameObject>();
                breakable.breakSFX = __instance.GetComponent<CursedSkullVFX>().doneSFX.ToList();
                if (PhotonNetwork.IsMasterClient)
                {
                    breakable.instantiateNonItemOnBreak = new List<GameObject>() { DebbyPeam.instance.miscPrefabsList["SkullThrowInstantiater"] };
                }
                breakable.ragdollCharacterOnBreak = false;
                breakable.pushForce = 3f;
                breakable.wholeBodyPushForce = 2f;
                DebbyPeam.instance.log.LogDebug("Skull Breakable added");
                EventOnItemCollision eventOnItemCollision = skullObject.AddComponent<EventOnItemCollision>();
                eventOnItemCollision.onlyWhenImKinematic = true;
                eventOnItemCollision.eventOnCollided = new UnityEvent();
                eventOnItemCollision.eventOnCollided.AddListener(() => { __instance.SetKinematicNetworked(false); });
                eventOnItemCollision.minCollisionVelocity = 8f;
                eventOnItemCollision.onlyOnce = true;
                DebbyPeam.instance.log.LogDebug("Skull EventOnItemCollision added");
            }
            return true;
        }
    }
}