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
    public class SkullThrowLogic : MonoBehaviour
    {
        public PhotonView photonView;
        public List<Affliction> afflictions = new List<Affliction>();
        public void Awake()
        {
            ItemDatabase.TryGetItem(25, out Item skullRef);
            Action_ApplyMassAffliction skullMassAffliction = skullRef.GetComponent<Action_ApplyMassAffliction>();
            photonView = GetComponent<PhotonView>();
            afflictions = new List<Affliction>(skullMassAffliction.extraAfflictions);
            if (skullMassAffliction.affliction != null)
            {
                afflictions.Add(skullMassAffliction.affliction);
            }
        }
        public void Start()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                float closestPlayerDist = float.MaxValue;
                int playerIndex = -1;
                for (int i = 0; i < Character.AllCharacters.Count; i++)
                {
                    float newDist = Vector3.Distance(transform.position, Character.AllCharacters[i].HipPos());
                    if (newDist < closestPlayerDist && newDist < 5f)
                    {
                        closestPlayerDist = newDist;
                        playerIndex = i;
                    }
                }
                if (playerIndex != -1)
                {
                    photonView.RPC("CurseLocalPlayerRPC", Character.AllCharacters[playerIndex].view.Owner);
                }
                StartCoroutine(DestroyTimerCoroutine());
            }
        }
        public IEnumerator DestroyTimerCoroutine()
        {
            yield return new WaitForSeconds(5f);
            PhotonNetwork.Destroy(gameObject);
        }
        [PunRPC]
        public void CurseLocalPlayerRPC()
        {
            Character.localCharacter.Invoke("DieInstantly", 0.02f);
            photonView.RPC("AddAfflictionsRPC", RpcTarget.Others);
        }
        [PunRPC]
        public void AddAfflictionsRPC()
        {
            for (int i = 0; i < afflictions.Count; i++)
            {
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
                ItemDatabase.TryGetItem(36, out Item coconutRef);
                GameObject skullObject = __instance.gameObject;
                GameObject skullThrowObject = new GameObject("SkullThrowLogic", typeof(PhotonView), typeof(SkullThrowLogic), typeof(Action_ApplyMassAffliction));
                Bonkable bonkable = skullObject.AddComponent<Bonkable>();
                bonkable.minBonkVelocity = 10f;
                bonkable.ragdollTime = 1f;
                bonkable.bonkForce = 400f;
                bonkable.bonkRange = 1f;
                bonkable.bonk = coconutRef.GetComponent<Bonkable>().bonk;
                Breakable breakable = skullObject.AddComponent<Breakable>();
                breakable.breakOnCollision = true;
                breakable.minBreakVelocity = 15f;
                breakable.breakSFX = __instance.GetComponent<CursedSkullVFX>().doneSFX.ToList();
                breakable.instantiateNonItemOnBreak.Add(skullThrowObject);
                breakable.ragdollCharacterOnBreak = true;
                breakable.pushForce = 3f;
                breakable.wholeBodyPushForce = 2f;
                EventOnItemCollision eventOnItemCollision = skullObject.AddComponent<EventOnItemCollision>();
                eventOnItemCollision.onlyWhenImKinematic = true;
                eventOnItemCollision.eventOnCollided = new UnityEvent();
                eventOnItemCollision.eventOnCollided.AddListener(() => { __instance.SetKinematicNetworked(false); });
                eventOnItemCollision.minCollisionVelocity = 8f;
                eventOnItemCollision.onlyOnce = true;
            }
            return true;
        }
    }
}