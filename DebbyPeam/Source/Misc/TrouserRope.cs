using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Zorro.ControllerSupport.Rumble.RumbleClip;
namespace DebbyPeam.Misc
{
    public class TrouserRope : MonoBehaviour
    {
        readonly BepInEx.Logging.ManualLogSource log = DebbyPeam.instance.log;
        public static Dictionary<Character, TrouserRope> trouserRopeDictionary = new Dictionary<Character, TrouserRope>();
        public PhotonView photonView;
        public Character playerOwner;
        public RopeAnchorWithRope ropeAnchorWithRope;
        public Rope rope;
        public float weightAdded = 0f;
        public float playerDragForce = 100f;
        public void Initialize(int id)
        {
            if (Character.GetCharacterWithPhotonID(id, out playerOwner))
            {
                PrepareRope();
                photonView.RPC("InitializeRPC", RpcTarget.Others, id);
            }
            else
            {
                log.LogWarning("Trouser Rope failed to spawn!");
                PhotonNetwork.Destroy(gameObject);
            }
        }
        [PunRPC]
        public void InitializeRPC(int id)
        {
            if (playerOwner == null)
            {
                Character.GetCharacterWithPhotonID(id, out playerOwner);
                PrepareRope();
            }
        }
        public void PrepareRope()
        {
            if (trouserRopeDictionary.ContainsKey(playerOwner))
            {
                trouserRopeDictionary.Remove(playerOwner);
            }
            trouserRopeDictionary.Add(playerOwner, this);
            ropeAnchorWithRope = GetComponent<RopeAnchorWithRope>();
            DebbyPeam.instance.utils.FixShaders(gameObject);
            DebbyPeam.instance.utils.FixShaders(ropeAnchorWithRope.ropePrefab);
            if (PhotonNetwork.IsMasterClient)
            {
                ropeAnchorWithRope.ropeSegmentLength = Random.Range(2f, 10f);
                ropeAnchorWithRope.spoolOutTime = 1.5f * Mathf.Clamp(ropeAnchorWithRope.ropeSegmentLength / 5f, 1f, 2f);
                log.LogDebug($"Master Client spawning rope with length: \"{ropeAnchorWithRope.ropeSegmentLength}\" and spool time of: \"{ropeAnchorWithRope.spoolOutTime}\"");
                ropeAnchorWithRope.SpawnRope();
            }
            transform.parent = playerOwner.GetBodypart(BodypartType.Hip).transform;
            transform.localPosition = new Vector3(0f, -2f, 0.5f);
            log.LogDebug($"Setting Rope parent to player: \"{playerOwner.characterName}\"");
            StartCoroutine(WaitForRopeSegments());
        }
        public IEnumerator WaitForRopeSegments()
        {
            rope = null;
            while (rope == null)
            {
                rope = ropeAnchorWithRope.ropeInstance.GetComponent<Rope>();
                yield return null;
            }
            yield return new WaitUntil(() => rope.GetRopeSegments().Count > 0);
            List<Transform> segments = rope.GetRopeSegments();
            ConfigurableJoint joint = segments[0].GetComponent<ConfigurableJoint>();
            joint.connectedBody = ropeAnchorWithRope.anchor.anchorPoint.gameObject.AddComponent<Rigidbody>();
            joint.connectedBody.constraints = RigidbodyConstraints.FreezePosition;
            joint.connectedBody.angularDamping = 0.1f;
            joint.connectedBody.linearDamping = 0.1f;
            Material[] materials = new Material[] { trouserRopeDictionary[playerOwner].rope.ropeBoneVisualizer.meshRenderer.sharedMaterial, trouserRopeDictionary[playerOwner].ropeAnchorWithRope.anchor.normalPart.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial };
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i].SetTexture("_BaseTexture", null);
                materials[i].SetColor("_Tint", playerOwner.refs.customization.PlayerColor / 2f);
                materials[i].SetFloat("_HueStr1", 0.2f);
                materials[i].SetColor("_BaseColor", playerOwner.refs.customization.PlayerColor);
                materials[i].SetColor("_Color1", playerOwner.refs.customization.PlayerColor);
                materials[i].SetColor("_Color21", playerOwner.refs.customization.PlayerColor);
                materials[i].SetColor("_Color3", playerOwner.refs.customization.PlayerColor);
            }
        }
        public void Update()
        {
            float newWeightAdded = 0f;
            for (int i = 0; i < ropeAnchorWithRope.rope.charactersClimbing.Count; i++)
            {
                newWeightAdded += 0.2f + ropeAnchorWithRope.rope.charactersClimbing[i].refs.afflictions.currentStatuses[(int)CharacterAfflictions.STATUSTYPE.Weight];
            }
            if (newWeightAdded != weightAdded)
            {
                weightAdded = newWeightAdded;
            }
        }
        public void FixedUpdate()
        {
            if (PhotonNetwork.IsMasterClient && ropeAnchorWithRope.rope.charactersClimbing.Count > 0)
            {
                Vector3 ropeNormal = ropeAnchorWithRope.rope.GetRopeSegments()[0].position - playerOwner.GetBodypart(BodypartType.Hip).transform.position;
                ropeNormal *= playerDragForce;
                playerOwner.AddForceToBodyPart(playerOwner.GetBodypartRig(BodypartType.Hip), ropeNormal * 0.2f, ropeNormal);
            }
        }
        public void OnDestroy()
        {
            if (rope != null)
            {
                rope.Clear();
            }
            if (playerOwner != null)
            {
                trouserRopeDictionary.Remove(playerOwner);
            }
        }
    }
}