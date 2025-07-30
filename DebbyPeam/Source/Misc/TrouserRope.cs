using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public bool prepared = false;
        public void Start()
        {
            GlobalEvents.OnCharacterOwnerDisconnected = (Action<Character>)Delegate.Combine(GlobalEvents.OnCharacterOwnerDisconnected, new Action<Character>(RopeOwnerDisconnect));
        }
        public void RopeOwnerDisconnect(Character character)
        {
            if (PhotonNetwork.IsMasterClient && trouserRopeDictionary[character] == this)
            {
                log.LogDebug($"Destroying \"{character.characterName}\"'s trouser rope!");
                PhotonNetwork.Destroy(gameObject);
            }
        }
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
            Character.GetCharacterWithPhotonID(id, out playerOwner);
            PrepareRope();
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
                ropeAnchorWithRope.ropeSegmentLength = UnityEngine.Random.Range(2f, 10f);
                ropeAnchorWithRope.spoolOutTime = 1.5f * Mathf.Clamp(ropeAnchorWithRope.ropeSegmentLength / 5f, 1f, 2f);
                log.LogDebug($"Master Client spawning rope with length: \"{ropeAnchorWithRope.ropeSegmentLength}\" and spool time of: \"{ropeAnchorWithRope.spoolOutTime}\"");
                rope = ropeAnchorWithRope.SpawnRope();
                photonView.RPC("GetRopeRPC", RpcTarget.Others, rope.photonView.ViewID);
            }
            transform.parent = playerOwner.GetBodypart(BodypartType.Hip).transform;
            transform.localPosition = new Vector3(0f, -2f, 0.5f);
            log.LogDebug($"Setting Rope parent to player: \"{playerOwner.characterName}\"");
            StartCoroutine(WaitForRopeSegments());
        }
        [PunRPC]
        public void GetRopeRPC(int viewID)
        {
            rope = PhotonView.Find(viewID).GetComponent<Rope>();
        }
        public IEnumerator WaitForRopeSegments()
        {
            yield return new WaitUntil(() => rope != null);
            if (PhotonNetwork.IsMasterClient)
            {
                yield return new WaitUntil(() => rope.GetRopeSegments().Count > 0);
                List<Transform> segments = rope.GetRopeSegments();
                ConfigurableJoint joint = segments[0].GetComponent<ConfigurableJoint>();
                joint.connectedBody = ropeAnchorWithRope.anchor.anchorPoint.gameObject.AddComponent<Rigidbody>();
                joint.connectedBody.constraints = RigidbodyConstraints.FreezePosition;
                joint.connectedBody.angularDamping = 0.1f;
                joint.connectedBody.linearDamping = 0.1f;
            }
            Material[] materials = new Material[]
            {
                rope.ropeBoneVisualizer.meshRenderer.sharedMaterial,
                ropeAnchorWithRope.anchor.normalPart.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial
            };
            CharacterCustomization charCustomization = playerOwner.refs.customization;
            for (int i = 0; i < materials.Length; i++)
            {
                var material = materials[i];
                material.SetTexture("_BaseTexture", null);
                material.SetColor("_Tint", charCustomization.PlayerColor / 2f);
                material.SetFloat("_HueStr1", 0.2f);
                material.SetColor("_BaseColor", charCustomization.PlayerColor);
                material.SetColor("_Color1", charCustomization.PlayerColor);
                material.SetColor("_Color21", charCustomization.PlayerColor);
                material.SetColor("_Color3", charCustomization.PlayerColor);
            }
            prepared = true;
        }
        public void Update()
        {
            if (prepared)
            {
                float newWeightAdded = 0f;
                var charactersClimbing = rope.charactersClimbing;
                for (int i = 0; i < charactersClimbing.Count; i++)
                {
                    newWeightAdded += 0.2f + charactersClimbing[i].refs.afflictions.currentStatuses[(int)CharacterAfflictions.STATUSTYPE.Weight];
                }
                if (newWeightAdded != weightAdded)
                {
                    weightAdded = newWeightAdded;
                }
                if (PhotonNetwork.IsMasterClient && playerOwner == null)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
            }
        }
        public void FixedUpdate()
        {
            if (PhotonNetwork.IsMasterClient && prepared)
            {
                if (rope.charactersClimbing.Count > 0)
                {
                    Vector3 ropeNormal = rope.GetRopeSegments()[0].position - playerOwner.GetBodypart(BodypartType.Hip).transform.position;
                    ropeNormal *= playerDragForce;
                    playerOwner.AddForceToBodyPart(playerOwner.GetBodypartRig(BodypartType.Hip), ropeNormal * 0.2f, ropeNormal);
                }
            }
        }
        public void OnDestroy()
        {
            if (rope != null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(rope.gameObject);
            }
            if (trouserRopeDictionary.ContainsKey(playerOwner))
            {
                trouserRopeDictionary.Remove(playerOwner);
            }
        }
    }
}