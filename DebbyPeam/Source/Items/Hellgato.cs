using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace DebbyPeam.Items
{
    public class Hellgato : DebbyItem
    {
        public Character closestCharacter;
        public float forceTowards = 20f;
        public override void GenerateEvents()
        {
            base.GenerateEvents();
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 3f, maximumTime = 7f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.None, animationName = "Ear Flick", animationValue = string.Empty, randomValue = false, randomMin = 0f, randomMax = 0f });
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 2f, maximumTime = 5f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.None, animationName = "Blink", animationValue = string.Empty, randomValue = false, randomMin = 0f, randomMax = 0f });
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 3f, maximumTime = 5f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.None, animationName = "Paw Twitch", animationValue = string.Empty, randomValue = false, randomMin = 0f, randomMax = 0f });
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 0.5f, maximumTime = 1f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.Integer, animationName = "Ear Side", animationValue = string.Empty, randomValue = true, randomMin = 0f, randomMax = 1.5f });
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 0.5f, maximumTime = 1f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.Integer, animationName = "Paw Side", animationValue = string.Empty, randomValue = true, randomMin = 0f, randomMax = 3.5f });
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 7.5f, maximumTime = 15f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.None, animationName = "Meow", animationValue = string.Empty, randomValue = false, randomMin = 0f, randomMax = 0f });
            randomTimeTriggers.Add(new RandomAnimation() { timer = 0f, minimumTime = 5f, maximumTime = 10f, triggerableStates = new List<ItemState>() { ItemState.Ground, ItemState.Held, ItemState.InBackpack }, animationValueType = ValueType.None, animationName = "Closest Player Check", animationValue = string.Empty, randomValue = false, randomMin = 0f, randomMax = 0f });
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (PhotonNetwork.IsMasterClient && item.rig != null && closestCharacter != null)
            {
                Vector3 direction = (closestCharacter.Center - transform.position).normalized;
                Quaternion rotator = Quaternion.FromToRotation(transform.forward, -direction);
                item.rig.AddTorque(new Vector3(rotator.x, rotator.y, rotator.z) * balanceForce);
                Vector3 flattenedDir = new Vector3(direction.x, 0f, direction.z).normalized;
                item.rig.AddForce(flattenedDir * forceTowards);
            }
        }
        public void ClosestPlayerCheck()
        {
            closestCharacter = DebbyPeam.instance.utils.ClosestCharacter(transform.position);
        }
    }
}