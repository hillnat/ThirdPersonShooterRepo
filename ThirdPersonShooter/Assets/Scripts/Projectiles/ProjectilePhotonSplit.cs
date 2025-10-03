using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectilePhotonSplit : ProjectileBase
{
    public override float initialForce => 3500f;

    public override float persistantForce => 4000f;

    public override float lifetime => 7f;

    public override int maxBounces => 0;

    public override float baseDamage => 17;

    public override float headshotMultiplier => 3;
    public override bool hasGravity => false;
    public override float colliderRadius => 0.2f;
    public override EProjectileMaxBounceBehavior afterMaxBounceBehavior => EProjectileMaxBounceBehavior.Destroy;

    public override float impactAudioVolumeModifier => 0.2f;

    public override Vector2 impactAudioPitchRange => new Vector2(0.9f,1.1f);

    public override string projectileNameInFile => "PhotonSplit";

    private void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();
    }
    private void LateUpdate()
    {
        base.LateUpdate();
    }
    private void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcessCollision(collision);
    }
    public override bool ProcessHit(PlayerControllerBase hitPc, float damage, bool isHeadshot, Vector3 impactPoint)
    {
        bool enemyEffectedByPhotonDecay = hitPc.currentStatusEffects.Any(item => item is StatusEffectPhotonDecay);
        float statusEffectMultiplier = (enemyEffectedByPhotonDecay ? 2f : 1f);
        if (enemyEffectedByPhotonDecay) { 
            hitPc.myView.RPC(nameof(PlayerControllerBase.RPC_RemoveStatusEffect), Photon.Pun.RpcTarget.All, EStatusEffects.PhotonDecay);
            WeaponBase spellbook = owningPc.GetWeaponOfType(typeof(WeaponPhotonSpellbook));
            if (spellbook != null)
            {
                spellbook.lastPrimaryActionTime -= 1.25f;
            }
        }
        return DealDamageToPc(hitPc, damage*statusEffectMultiplier, isHeadshot, impactPoint);
    }
}
