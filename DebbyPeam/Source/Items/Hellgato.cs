using Photon.Pun;
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
            CreateRepeatAnim("Ear Flick", min: 3f, max: 7f);
            CreateRepeatAnim("Blink", min: 2f, max: 5f);
            CreateRepeatAnim("Paw Twitch", min: 3f, max: 5f);
            CreateRepeatAnim("Ear Side", min: 0.5f, max: 1f, randMax: 1f);
            CreateRepeatAnim("Paw Side", min: 0.5f, max: 1f, randMax: 3f);
            CreateRepeatAnim("Meow", min: 7.5f, max: 15f);
            CreateRepeatAnim("Closest Player Check", min: 5f, max: 10f);
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