using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSchimtarBrand : WeaponBase
{
    public override string weaponNameInFile => "SchimtarBrand";

    public override string weaponName => "Shcimtar Brand";
    public override int indexInAnimator => 10;
    public override int primaryAmmoPerShot => 0;
    public override int secondaryAmmoPerShot => 0;
    public override float primaryFireDelay => 0.25f;
    public override float secondaryFireDelay => 1f;
    public override float maxRange => 2f;
    public override string secondaryProjectilePrefabPath => "Projectiles/Projectile_FlameWall";

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
        FireMelee(cameraOrigin, cameraForward, 0.5f);
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        FireProjectile(false, cameraOrigin, myPc.transform.forward, muzzlePosition);
        return true;
    }
}
