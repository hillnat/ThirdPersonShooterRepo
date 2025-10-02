using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.WebRequestMethods;
using static UnityEngine.GraphicsBuffer;

public enum EProjectileMaxBounceBehavior { Destroy, None, Freeze }

public abstract class ProjectileBase : MonoBehaviour, IPunObservable
{
    [HideInInspector] public PhotonView view;
    [HideInInspector]public PlayerControllerBase owningPc;
    [HideInInspector] protected Rigidbody rb;
    [HideInInspector] public Collider mainCollider;
    private float lastImpactTime = float.MinValue;
    public virtual EProjectileMaxBounceBehavior afterMaxBounceBehavior { get; } = EProjectileMaxBounceBehavior.Destroy;
    public virtual EStatusEffects[] onHitStatusEffects { get; } = new EStatusEffects[0];
    public virtual bool hasGravity { get; } = true;
    public virtual bool dealsDamage { get; } = true;
    public virtual float initialForce { get; } = 0f;
    public virtual float persistantForce { get; } = 0f;
    public virtual float lifetime { get; } = 10f;
    public virtual int maxBounces { get; } = 0;
    public abstract float baseDamage { get; }
    public virtual float headshotMultiplier { get; } = 1f;
    public virtual float impactAudioVolumeModifier { get; } = 1f;
    public virtual Vector2 impactAudioPitchRange { get; } = Vector2.one;
    public virtual float colliderRadius { get; } = 0.5f;
    public abstract string projectileNameInFile { get; }

    private float spawnTime = 0f;
    [HideInInspector] public Vector3 targetPosition=Vector3.zero;
    [HideInInspector] public int bounceCounter = 0;
    
    public bool maxBouncesReached => bounceCounter >= maxBounces;
    [HideInInspector] public bool bouncesOffPlayers = true;


    public List<PlayerControllerBase> hitPcs = new List<PlayerControllerBase>();
    [HideInInspector] public bool isFrozen = false;
    private Vector3 lastPosition;
    private Vector3 currentPosition;
    private LayerMask hitLayerMask;
    private AudioClip[] impactAudioClips;

    private string impactSoundsPath => $"Audio/Projectiles/{projectileNameInFile}/Impact";
    private AudioClip GetRandomImpactAudio()
    {
        if (impactAudioClips.Length == 0) { return null; }
        else { return impactAudioClips[Random.Range(0, impactAudioClips.Length)]; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
    #region Unity Callbacks
    public void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = hasGravity;
        mainCollider = GetComponent<Collider>();
        spawnTime = GameManager.instance.localTime;
        hitPcs.Clear();
        if (mainCollider.GetType() == typeof(SphereCollider))
        {
            ((SphereCollider)mainCollider).radius = colliderRadius;
        }
        hitLayerMask = LayerMask.GetMask(new string[2] { "Player" , "ProjectileBlocker"});
    }
    public void Start()
    {
        if (view.IsMine)
        {
            impactAudioClips = Resources.LoadAll<AudioClip>(impactSoundsPath);
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

    public void Update()
    {
        if (view.IsMine)
        {
            if (lifetime != 0 && GameManager.instance.localTime > spawnTime + lifetime)
            {
                DestroyProjectile();
            }
        }
    }
    public void FixedUpdate()
    {
        if (view.IsMine)
        {
            if (!isFrozen)
            {
                currentPosition = transform.position;
                Vector3 dir = (currentPosition - lastPosition);
                RaycastHit[] hits = Physics.SphereCastAll(lastPosition, 0.05f, dir.normalized, dir.magnitude, hitLayerMask);
                Debug.DrawRay(lastPosition, dir, Color.yellow, 5f);
                for (int i = 0; i < hits.Length; i++)
                {
                    PlayerControllerBase hitPc = hits[i].collider.transform.root.GetComponent<PlayerControllerBase>();
                    bool didHitPlayer = false;
                    bool isHeadshot = hits[i].collider.GetType() == typeof(SphereCollider);

                    if (hitPc != null && hitPc != owningPc && !hitPcs.Contains(hitPc) && !hitPc.GetIsDead()) { ProcessHit(hitPc, baseDamage*(isHeadshot ? headshotMultiplier : 1f), isHeadshot, hits[i].point); }

                    if (didHitPlayer && bouncesOffPlayers)
                    {
                        bounceCounter++;
                        HandleBounceBehavior();
                    }
                }
                lastPosition = currentPosition;

                rb.AddForce(persistantForce * transform.forward * Time.fixedDeltaTime);
            }
        }
    }
    public void LateUpdate()
    {
        
    }
    #endregion
    public virtual bool ProcessHit(PlayerControllerBase hitPc, float damage, bool isHeadshot, Vector3 impactPoint)
    {
        if (!dealsDamage) { return false; }
        return DealDamageToPc(hitPc, damage, isHeadshot, impactPoint);
    }
    private void DestroyProjectile()
    {
        if (owningPc.ownedProjectiles.Contains(this)) { owningPc.ownedProjectiles.Remove(this); }
        PhotonNetwork.Destroy(view);
    }
    private void FreezeProjectile(bool state)
    {
        isFrozen = state;
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
    private void ToggleCollider(bool state)
    {
        mainCollider.enabled = state;
    }
    protected bool DealDamageToPc(PlayerControllerBase targetPc, float damage, bool isHeadshot, Vector3 impactPos)
    {
        bool didHitPlayer = false;
        
        if (targetPc != null && targetPc != owningPc && !hitPcs.Contains(targetPc) && !targetPc.GetIsDead())
        {
            hitPcs.Add(targetPc);
            ParticleManager.instance.PlayGoreParticles(true, transform.position, transform.rotation, int.MinValue);
            for (int i = 0; i < onHitStatusEffects.Length; i++)
            {
                targetPc.myView.RPC(nameof(PlayerControllerBase.RPC_AddStatusEffect), RpcTarget.All, onHitStatusEffects[i], StatusEffectBase.GetRandomUniqueId());
            }
            targetPc.myView.RPC(nameof(PlayerControllerBase.RPC_SetLastHitBy), RpcTarget.All, owningPc.myView.ViewID);
            targetPc.myView.RPC(nameof(PlayerControllerBase.RPC_ChangeHealth), RpcTarget.All, -damage);
            if (isHeadshot) { AudioManager.instance.PlayHeadshotSound(true, impactPos, 1f, 1f, int.MinValue); }
            //ParticleManager.instance.SpawnDamageNumber(hitPc.transform.position, finalDamage);
            didHitPlayer = true;
        }
        return didHitPlayer;
    }
    public void ProcessCollision(Collision collision)//Oncollisionenter
    {
        if (isFrozen) { return; }
        if (collision.gameObject.transform.root.GetComponent<PlayerControllerBase>()) { return; }
        if (GameManager.instance.localTime > lastImpactTime + 0.25f)
        {
            AudioManager.instance.PlaySound(true, GetRandomImpactAudio(), transform.position, impactAudioVolumeModifier, Random.Range(impactAudioPitchRange.x, impactAudioPitchRange.y), int.MinValue);
            ParticleManager.instance.PlayImpactParticles(true, transform.position, transform.rotation, int.MinValue);
            lastImpactTime=GameManager.instance.localTime;
        }
        HandleBounceBehavior();
    }
  
    public virtual void HandleBounceBehavior()
    {
        bounceCounter++;

        if (maxBouncesReached)
        {
            switch (afterMaxBounceBehavior)
            {
                case EProjectileMaxBounceBehavior.Destroy:
                    DestroyProjectile();
                    break;
                case EProjectileMaxBounceBehavior.None:
                    break;
                case EProjectileMaxBounceBehavior.Freeze:
                    FreezeProjectile(true);
                    break;
                default: break;
            }
        }
    }
}
