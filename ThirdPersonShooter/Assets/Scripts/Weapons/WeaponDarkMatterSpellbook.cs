using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponDarkMatterSpellbook : WeaponBase
{
    public override string weaponNameInFile => "DarkMatterSpellbook";
    public override int indexInAnimator => 9;

    public override string weaponName => "Dark Matter Spellbook";


    public override float[] primaryActionDelay => new float[UpgradeTree.maxPointsPerBranch] { 15f, 12f, 10f, 7f };
    public override float[] secondaryActionDelay => new float[UpgradeTree.maxPointsPerBranch] {12f,9f,7.5f,4f};
    public override int[] primaryAmmoPerShot { get; } = Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override int[] secondaryAmmoPerShot { get; } = Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override bool reloadable => false;
    
    public override string primaryProjectilePrefabPath { get; } = "Projectiles/Projectile_GravitonSnare";



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
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)//Called HorizonShift
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        myPc.rb.AddForce(Camera.main.transform.forward * 1500f);
        return true;
    }
}
