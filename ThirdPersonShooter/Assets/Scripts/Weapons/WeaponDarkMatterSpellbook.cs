using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDarkMatterSpellbook : WeaponBase
{
    public override string weaponNameInFile => "DarkMatterSpellbook";
    public override int indexInAnimator => 9;

    public override string weaponName => "Dark Matter Spellbook";


    public override int secondaryAmmoPerShot => 0;
    public override int primaryAmmoPerShot => 0;
    public override float primaryFireDelay => 5f;
    public override float secondaryFireDelay => 3f;

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
        FireProjectile(true, cameraOrigin, cameraForward, muzzlePosition);
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)//Called HorizonShift
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        myPc.rb.AddForce(Camera.main.transform.forward * 1500f);
        return true;
    }
}
