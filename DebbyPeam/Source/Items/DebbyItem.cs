using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
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
        public Animator animator;
        public List<RandomAnimation> randomTimeTriggers = new List<RandomAnimation>();
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
            if (randomTimeTriggers.Count == 0)
            {
                GenerateEvents();
            }
            IterateEvents(false);
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
        public void IterateEvents(bool invoke = true)
        {
            for (int i = 0; i < randomTimeTriggers.Count; i++)
            {
                RandomAnimation trigger = randomTimeTriggers[i];
                if (!trigger.triggerableStates.Contains(item.itemState))
                {
                    continue;
                }
                trigger.timer -= Time.deltaTime;
                if (trigger.timer <= 0f)
                {
                    if (invoke)
                    {
                        log.LogDebug($"\"{ItemName}\" triggered timed effect \"{i}\" during state: \"{item.itemState}\"");
                        AnimSync(trigger);
                    }
                    trigger.timer = UnityEngine.Random.Range(trigger.minimumTime, trigger.maximumTime);
                }
            }
        }
        public void AnimSync(RandomAnimation trigger)
        {
            object obj = string.Empty;
            if (animator != null)
            {
                switch (trigger.animationValueType)
                {
                    case ValueType.Integer:
                        {
                            if (trigger.animationValue == string.Empty)
                            {
                                trigger.animationValue = "0";
                            }
                            obj = int.Parse(trigger.animationValue);
                            if (trigger.randomValue)
                            {
                                obj = UnityEngine.Random.Range(Mathf.FloorToInt(trigger.randomMin), Mathf.CeilToInt(trigger.randomMax));
                            }
                            break;
                        }
                    case ValueType.Float:
                        {
                            if (trigger.animationValue == string.Empty)
                            {
                                trigger.animationValue = "0";
                            }
                            obj = float.Parse(trigger.animationValue);
                            if (trigger.randomValue)
                            {
                                obj = UnityEngine.Random.Range(trigger.randomMin, trigger.randomMax);
                            }
                            break;
                        }
                    case ValueType.Boolean:
                        {
                            if (trigger.animationValue == string.Empty)
                            {
                                trigger.animationValue = "false";
                            }
                            obj = bool.Parse(trigger.animationValue);
                            if (trigger.randomValue)
                            {
                                obj = UnityEngine.Random.Range(0, 2) == 1;
                            }
                            break;
                        }
                }
                item.photonView.RPC("AnimSyncRPC", RpcTarget.All, trigger.animationName, obj);
                return;
            }
            log.LogWarning($"\"{ItemName}\" tried to use an animation but did not have an Animator!");
        }
        [PunRPC]
        public void AnimSyncRPC(string animName, object value = null)
        {
            switch (value)
            {
                case bool booleanValue:
                    {
                        animator.SetBool(animName, booleanValue);
                        break;
                    }
                case int integerValue:
                    {
                        animator.SetInteger(animName, integerValue);
                        break;
                    }
                case float floatValue:
                    {
                        animator.SetFloat(animName, floatValue);
                        break;
                    }
                default:
                    {
                        animator.SetTrigger(animName);
                        break;
                    }
            }
            log.LogDebug($"\"{ItemName}\" syncing animation \"{animName}\" with value \"{value}\"");
        }
        public virtual void PlaySFX()
        {
            customSFX.Play(transform.position);
        }
    }
    [Serializable]
    public class RandomAnimation
    {
        public float timer = 0f;
        public float minimumTime = 1f;
        public float maximumTime = 2f;
        public List<ItemState> triggerableStates = new List<ItemState>();
        public ValueType animationValueType;
        public string animationName = "";
        public string animationValue = string.Empty;
        public bool randomValue = false;
        public float randomMin = 0f;
        public float randomMax = 1f;
    }
    public enum ValueType
    {
        None,
        Integer,
        Float,
        Boolean
    }
}