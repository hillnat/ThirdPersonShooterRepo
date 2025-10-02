using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponPhotonSpellbook : WeaponBase
{
    public override string weaponNameInFile => "PhotonSpellbook";
    public override int indexInAnimator => 8;

    public override string weaponName => "Photon Spellbook";


    public override int[] primaryAmmoPerShot => Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override int[] secondaryAmmoPerShot => Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();

    public override float[] primaryFireDelay => new float[UpgradeTree.maxPointsPerBranch] { 3f, 2f, 1f, 0.7f };
    public override float[] secondaryFireDelay => new float[UpgradeTree.maxPointsPerBranch] { 8f, 6f, 4, 2f };

    public override bool reloadable => false;
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
        FireProjectile(cameraOrigin, cameraForward, muzzlePosition, true);
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        FireProjectile(cameraOrigin, cameraForward, muzzlePosition, false);
        return true;
    }
}
