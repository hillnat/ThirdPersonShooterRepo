using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileFlamingAsteroid : ProjectileBase
{
    public override float initialForce => 0f;

    public override float persistantForce => 3750f;

    public override float lifetime => 20f;

    public override int maxBounces => 0;

    public override float baseDamage => 30;

    public override float headshotMultiplier => 0;
    public override bool hasGravity => false;

    public override string projectileNameInFile => "FlamingAsteroid";
    public override float colliderRadius => 3f;

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
