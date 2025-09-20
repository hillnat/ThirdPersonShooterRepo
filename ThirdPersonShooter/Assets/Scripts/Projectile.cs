using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum EProjectiles { RingBlade, Arrow, RecallDagger }
public class Projectile : MonoBehaviour, IPunObservable
{
    public enum AfterMaxBounceBehavior { Destroy, ReturnToSender, Freeze }
    public AfterMaxBounceBehavior afterMaxBounceBehavior = AfterMaxBounceBehavior.Destroy;
    public PhotonView view;
    public PlayerController owningPc;
    private Rigidbody rb;
    public Collider mainCollider;
    public EProjectiles projectileType=0;
    public float initialForce = 5000f;
    public float persistantForce = 50f;
    public float lifetime = 5f;
    private float spawnTime = 0f;
    public Vector3 targetPosition=Vector3.zero;
    public int bounces = 0;
    public int maxBounces = 2;
    public bool maxBouncesReached => bounces >= maxBounces;
    public bool bouncesOffPlayers = true;
    public AudioClip impactAudio;
    public Vector2 impactAudioPitchRange = new Vector2(0.9f, 1f);
    public float impactAudioVolumeModifier = 0.05f;
    public bool wantsToFlyToPlayer = false;//Used by return to sender and recall dagger
    public float flyToPlayerSpeed = 25f;
    public float baseDamage = 20f;
    public float headshotMultiplier = 1.5f;
    public List<PlayerController> hitPcs = new List<PlayerController>();
    public bool isFrozen = false;
    private Spin spinComponent;
    private Vector3 lastPosition;
    private Vector3 currentPosition;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (view.IsMine)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            if (spinComponent != null)
            {
                stream.SendNext(spinComponent.isOn);
            }
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            if (spinComponent != null)
            {
                spinComponent.isOn = (bool)stream.ReceiveNext();
            }
        }
    }

    private void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        //col = GetComponent<Collider>();
        spinComponent = GetComponentInChildren<Spin>();
        spawnTime = GameManager.instance.time;
        hitPcs.Clear();
    }
    void Start()
    {
        if (view.IsMine)
        {
            FreezeProjectile(false);
            transform.LookAt(targetPosition);
            rb.AddForce(initialForce * transform.forward);
            targetPosition = Vector3.zero;
            lastPosition = transform.position;
        }
        else
        {
            Destroy(rb);
            Destroy(mainCollider);
        }
    }

    void Update()
    {
        if (view.IsMine)
        {
            if (lifetime != 0 && GameManager.instance.time > spawnTime + lifetime)
            {
                DestroyProjectile();
            }
            
            if (!wantsToFlyToPlayer && (maxBouncesReached && afterMaxBounceBehavior == AfterMaxBounceBehavior.ReturnToSender)) {
                wantsToFlyToPlayer = true;
            }
            if (wantsToFlyToPlayer)
            {
                if (rb.velocity.magnitude > 0) { rb.velocity = Vector3.zero;rb.angularVelocity = Vector3.zero; }
                transform.LookAt(owningPc.transform.position);
                transform.position = Vector3.Lerp(transform.position, owningPc.transform.position, Time.deltaTime * flyToPlayerSpeed);
  
                if (Vector3.Distance(transform.position, owningPc.transform.position) < 0.5f)//Despawn if close enough to player when flying back
                {
                    DestroyProjectile();

                    Weapon weapon = owningPc.GetWeapon();
                    if (weapon != null && weapon.weaponType == EWeapons.RingBlade)
                    {
                        owningPc.GetWeapon().IncrementAmmo(1);
                    }
                }
            }
        }
    }
    private void FixedUpdate()
    {
        if (view.IsMine)
        {
            if ((!isFrozen)||(isFrozen&&wantsToFlyToPlayer))
            {
                currentPosition = transform.position;
                Vector3 dir = (currentPosition - lastPosition);
                RaycastHit[] hits = Physics.SphereCastAll(lastPosition, 0.05f, dir.normalized, dir.magnitude);
                Debug.DrawRay(lastPosition, dir, Color.yellow, 5f);
                //Debug.DrawLine(lastPosition, currentPosition, Color.red, 5f);
                for (int i = 0; i < hits.Length; i++)
                {
                    PlayerController hitPc = hits[i].collider.transform.root.GetComponent<PlayerController>();
                    bool didHitPlayer = false;
                    bool isHeadshot = hits[i].collider.GetType() == typeof(SphereCollider);

                    if (hitPc != null && hitPc != owningPc && !hitPcs.Contains(hitPc) && !hitPc.GetIsDead()) { didHitPlayer = TryDealDamageToPc(hitPc, baseDamage * (isHeadshot? headshotMultiplier : 1f), isHeadshot, hits[i].point); }

                    if (didHitPlayer && bouncesOffPlayers)
                    {
                        bounces++;
                        HandleBounceBehavior();
                    }
                }
                lastPosition = currentPosition;




                if (!wantsToFlyToPlayer)
                {
                    rb.AddForce(persistantForce * transform.forward * Time.fixedDeltaTime);
                }

            }
        }
    }
    private void DestroyProjectile()
    {
        if (owningPc.ownedProjectiles.Contains(this)) { owningPc.ownedProjectiles.Remove(this); }
        PhotonNetwork.Destroy(view);
    }
    public void FreezeProjectile(bool state)
    {
        isFrozen = state;
        spinComponent.isOn = !isFrozen;
        ToggleCollider(!isFrozen);
        if (state)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.None;
        }
    }
    public void ToggleCollider(bool state)
    {
        mainCollider.enabled = state;
    }
    public bool TryDealDamageToPc(PlayerController targetPc, float damage, bool isHeadshot, Vector3 impactPos)
    {
        bool didHitPlayer = false;
        
        if (targetPc != null && targetPc != owningPc && !hitPcs.Contains(targetPc) && !targetPc.GetIsDead())
        {
            hitPcs.Add(targetPc);
            ParticleManager.instance.PlayGoreParticles(true, transform.position, transform.rotation);
            targetPc.myView.RPC(nameof(PlayerController.RPC_SetLastHitBy), RpcTarget.All, owningPc.myView.ViewID);
            targetPc.myView.RPC(nameof(PlayerController.RPC_ChangeHealth), RpcTarget.All, -damage);
            if (isHeadshot) { AudioManager.instance.PlayHeadshotSound(true, impactPos, 1f, 1f, int.MinValue); }
            //ParticleManager.instance.SpawnDamageNumber(hitPc.transform.position, finalDamage);
            didHitPlayer = true;
        }
        return didHitPlayer;
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collider hit");

        if (isFrozen || wantsToFlyToPlayer) { return; }
        if (collision.gameObject.transform.root.GetComponent<PlayerController>()) { return; }
        AudioManager.instance.PlaySound(true, impactAudio, transform.position, impactAudioVolumeModifier, Random.Range(impactAudioPitchRange.x,impactAudioPitchRange.y), int.MinValue);
        ParticleManager.instance.PlayImpactParticles(true, transform.position, transform.rotation);
        bounces++;
        HandleBounceBehavior();
    }
  
    public void HandleBounceBehavior()
    {
        if (maxBouncesReached)
        {
            switch (afterMaxBounceBehavior)
            {
                case AfterMaxBounceBehavior.Destroy:
                    DestroyProjectile();
                    break;
                case AfterMaxBounceBehavior.ReturnToSender:
                    FreezeProjectile(true);
                    break;
                case AfterMaxBounceBehavior.Freeze:
                    FreezeProjectile(true);
                    break;
                default: break;
            }
           
            switch (projectileType)
            {
                case EProjectiles.RecallDagger:
                    hitPcs.Clear();//Recall dagger can hit on the way back
                    break;
                default: break;
            }
        }

        switch (projectileType)
        {
            case EProjectiles.RingBlade:
                hitPcs.Clear();
                break;
            default: break;
        }
    }
}
