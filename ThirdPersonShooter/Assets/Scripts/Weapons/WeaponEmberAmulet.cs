using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponEmberAmulet : WeaponBase
{
    public override string weaponNameInFile => "EmberAmulet";

    public override string weaponName => "Ember Amulet";

    public override float[] primaryFireDelay => new float[UpgradeTree.maxPointsPerBranch] { 15f, 12f, 10f, 7f };
    public override float[] secondaryFireDelay => new float[UpgradeTree.maxPointsPerBranch] { 12f, 9f, 7.5f, 4f };
    public override int[] primaryAmmoPerShot { get; } = Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override int[] secondaryAmmoPerShot { get; } = Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override bool reloadable => false;
    public override string secondaryProjectilePrefabPath => "Projectiles/Projectile_FlamingAsteroid";

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
        myPc.myView.RPC(nameof(myPc.RPC_AddStatusEffect), Photon.Pun.RpcTarget.AllBufferedViaServer, EStatusEffects.FlamingArmor, StatusEffectBase.GetRandomUniqueId());
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)//Called HorizonShift
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        FireProjectile(myPc.transform.position+ (myPc.transform.forward*5f), Vector3.zero, myPc.transform.position+(Vector3.up*200f), false);
        return true;
    }
}
