using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using DebbyPeam.Utils;
using System.Linq;
namespace DebbyPeam.Items
{
    public class DebbyItem : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = DebbyPeam.instance.log;
        public Item item;
        public SFX_Instance customSFX;
        public string ItemName
        {
            get
            {
                if (item == null)
                {
                    string goName = gameObject.name;
                    return goName.Replace("(Clone)", "");
                }
                return item.GetItemName();
            }
        }
        public AnimationHandler anim = null;
        public List<ItemRepeatAnim> repeatAnims = new List<ItemRepeatAnim>();
        public List<Collider> colliders = new List<Collider>();
        public Transform floatPoint;
        public float floatForce;
        public float floatHeight;
        public float balanceForce;
        public float glideForce;
        public virtual void Awake()
        {
            List<string> interactsToLocalize = new List<string>() { item.UIData.mainInteractPrompt, item.UIData.secondaryInteractPrompt, item.UIData.scrollInteractPrompt };
            interactsToLocalize.RemoveAll(x => x == "");
            AddLocalizedStrings(item.UIData.itemName, interactsToLocalize);
            if (TryGetComponent(out Animator animator))
            {
                anim = new AnimationHandler(animator);
                GenerateEvents();
            }
        }
        public void CreateRepeatAnim(string animation, IEnumerable<ItemState> states = null, float min = 1f, float max = 2f, float randMin = 0f, float randMax = 0f)
        {
            ItemRepeatAnim result = new ItemRepeatAnim(anim, animation, states, min, max, randMin, randMax);
            if (result.anim != null)
            {
                repeatAnims.Add(result);
            }
        }
        public virtual void AddLocalizedStrings(string nameToLocalize, List<string> interactStrings = null)
        {
            string toLocalize;
            string finalString;
            for (int i = 0; i < interactStrings.Count + 1; i++)
            {
                if (i == interactStrings.Count)
                {
                    toLocalize = LocalizedText.GetNameIndex(nameToLocalize).ToUpperInvariant();
                    finalString = nameToLocalize;
                }
                else
                {
                    toLocalize = interactStrings[i].ToUpperInvariant();
                    finalString = interactStrings[i];
                }
                if (!LocalizedText.MAIN_TABLE.ContainsKey(toLocalize))
                {
                    List<string> languageStrings = new List<string>();
                    Array langEnumArray = Enum.GetValues(typeof(LocalizedText.Language));
                    for (int j = 0; j < langEnumArray.Length; j++)
                    {
                        languageStrings.Add(finalString);
                    }
                    LocalizedText.MAIN_TABLE.Add(toLocalize, languageStrings);
                }
            }
        }
        public virtual void GenerateEvents()
        {

        }
        public virtual void FixedUpdate()
        {
            if (PhotonNetwork.IsMasterClient && item.rig != null && floatPoint != null)
            {
                Quaternion rotator = Quaternion.FromToRotation(transform.up, Vector3.up);
                if (balanceForce != 0f)
                {
                    item.rig.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                }
                if (floatForce != 0f && floatHeight > 0f)
                {
                    if (Physics.Raycast(floatPoint.position, -Vector3.up, out RaycastHit hit, floatHeight, LayerMask.GetMask("Terrain"), QueryTriggerInteraction.Ignore) && !colliders.Contains(hit.collider))
                    {
                        item.rig.AddForce(Vector3.up * (floatForce / hit.distance) * (1.1f - (Quaternion.Angle(Quaternion.identity, rotator) / 360f)));
                    }
                    else if (glideForce != 0f)
                    {
                        item.rig.AddForce(transform.up * glideForce * (1.1f - (Quaternion.Angle(Quaternion.identity, rotator) / 360f)));
                    }
                }
            }
        }
        public virtual void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                IterateEvents();
            }
        }
        public void IterateEvents()
        {
            for (int i = 0; i < repeatAnims.Count; i++)
            {
                ItemRepeatAnim trigger = repeatAnims[i];
                if (!trigger.triggerableStates.Contains(item.itemState))
                {
                    continue;
                }
                if (trigger.Run(out object value))
                {
                    item.photonView.RPC("AnimSyncRPC", RpcTarget.All, trigger.animation, value);
                }
            }
        }
        [PunRPC]
        public void AnimSyncRPC(string animName, object value)
        {
            anim.SetValue(animName, value);
            string logString = $"\"{ItemName}\" syncing animation \"{animName}\"";
            if (value != null)
            {
                logString += $"with value \"{value}\"";
            }
            log.LogDebug(logString);
        }
        public virtual void PlaySFX()
        {
            customSFX.Play(transform.position);
        }
    }
}