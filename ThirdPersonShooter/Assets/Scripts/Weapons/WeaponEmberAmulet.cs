using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponEmberAmulet : WeaponBase
{
    public override string weaponNameInFile => "EmberAmulet";

    public override string weaponName => "Ember Amulet";

    public override float[] primaryActionDelay => new float[UpgradeTree.maxPointsPerBranch] { 20f, 15f, 10f, 8f };
    public override float[] secondaryActionDelay => new float[UpgradeTree.maxPointsPerBranch] { 30f, 23f, 15f, 10f };
    public override int[] primaryAmmoPerShot { get; } = Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override int[] secondaryAmmoPerShot { get; } = Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override bool reloadable => false;
    public override string secondaryProjectilePrefabPath => "Projectiles/Projectile_FlamingAsteroid";
    public override string primaryProjectilePrefabPath => "Projectiles/Projectile_FlameWall";

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
        FireProjectile(cameraOrigin, myPc.transform.forward, muzzlePosition, true);
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)//Called HorizonShift
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        FireProjectile(myPc.transform.position+ (myPc.transform.forward*10f), Vector3.zero, myPc.transform.position+(Vector3.up*200f), false);
        return true;
    }
}
