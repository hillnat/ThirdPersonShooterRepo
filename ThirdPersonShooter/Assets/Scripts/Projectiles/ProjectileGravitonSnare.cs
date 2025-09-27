using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectileGravitonSnare : ProjectileBase
{
    public override float initialForce => 0f;

    public override float persistantForce => 2750f;

    public override float lifetime => 7f;

    public override int maxBounces => 0;

    public override float baseDamage => 0;

    public override float headshotMultiplier => 0;
    public override bool hasGravity => false;

    public override EProjectileMaxBounceBehavior afterMaxBounceBehavior => EProjectileMaxBounceBehavior.Destroy;
    public override StatusEffectBase.EStatusEffects[] onHitStatusEffects => new StatusEffectBase.EStatusEffects[1] { StatusEffectBase.EStatusEffects.GravitonGrasp };

    public override float impactAudioVolumeModifier => 0.2f;

    public override Vector2 impactAudioPitchRange => new Vector2(0.9f,1.1f);

    public override string projectileNameInFile => "GravitonSnare";

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
    public override bool ProcessHit(PlayerController hitPc, float damage, bool isHeadshot, Vector3 impactPoint)
    {
        bool enemyEffectedByPhotonBind = hitPc.currentStatusEffects.Any(item => item is StatusEffectPhotonDecay);
        float statusEffectMultiplier = (enemyEffectedByPhotonBind ? 2f : 1f);
        if (enemyEffectedByPhotonBind) { hitPc.myView.RPC(nameof(PlayerController.RPC_RemoveStatusEffect), Photon.Pun.RpcTarget.All, StatusEffectBase.EStatusEffects.PhotonDecay); }
        return DealDamageToPc(hitPc, damage*statusEffectMultiplier, isHeadshot, impactPoint);
    }
}
