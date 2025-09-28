using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFlameWall : ProjectileBase
{
    public override float initialForce => 50f;

    public override float lifetime => 10f;

    public override int maxBounces => 0;

    public override float baseDamage => 0;

    public override bool hasGravity => true;
    public override EProjectileMaxBounceBehavior afterMaxBounceBehavior => EProjectileMaxBounceBehavior.Freeze;

    public override float impactAudioVolumeModifier => 0.2f;

    public override Vector2 impactAudioPitchRange => new Vector2(0.9f, 1.1f);

    public override string projectileNameInFile => "PhotonBind";
    public override bool dealsDamage => false;

    private void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
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
