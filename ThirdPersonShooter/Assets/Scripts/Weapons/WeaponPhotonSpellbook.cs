using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPhotonSpellbook : WeaponBase
{
    public override string weaponNameInFile => "PhotonSpellbook";
    public override int indexInAnimator => 8;

    public override string weaponName => "Photon Spellbook";

    public override float zoomFov => 75f;

    public override int primaryMaxAmmo => 7;
    public override int secondaryMaxAmmo => 2;

    public override float primaryFireDelay => 0.3f;
    public override float secondaryFireDelay => 1f;
    public override float reloadDelay => 2f;
    public override string primaryProjectilePrefabPath { get; } = "Projectiles/Projectile_PhotonSplit";
    public override string secondaryProjectilePrefabPath { get; } = "Projectiles/Projectile_PhotonBind";

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
    public override bool DoPrimaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!base.DoPrimaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        FireProjectile(true, cameraOrigin, cameraForward, muzzlePosition);
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        FireProjectile(false, cameraOrigin, cameraForward, muzzlePosition);
        return true;
    }
}
