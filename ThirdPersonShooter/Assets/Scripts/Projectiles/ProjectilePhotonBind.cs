using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePhotonBind : ProjectileBase
{
    public override float initialForce => 750f;

    public override float persistantForce => 2250f;

    public override float lifetime => 7f;

    public override int maxBounces => 0;

    public override float baseDamage => 3;

    public override float headshotMultiplier => 3;
    public override bool hasGravity => false;
    public override EStatusEffects[] onHitStatusEffects => new EStatusEffects[1] { EStatusEffects.PhotonDecay };
    public override EProjectileMaxBounceBehavior afterMaxBounceBehavior => EProjectileMaxBounceBehavior.Destroy;

    public override float impactAudioVolumeModifier => 0.2f;

    public override Vector2 impactAudioPitchRange => new Vector2(0.9f,1.1f);

    public override string projectileNameInFile => "PhotonBind";

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
}
