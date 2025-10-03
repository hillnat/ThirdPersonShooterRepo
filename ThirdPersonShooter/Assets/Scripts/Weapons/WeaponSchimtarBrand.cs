using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponSchimtarBrand : WeaponBase
{
    public override string weaponNameInFile => "SchimtarBrand";

    public override string weaponName => "Shcimtar Brand";
    public override int indexInAnimator => 10;
    public override int[] primaryAmmoPerShot => Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override int[] secondaryAmmoPerShot => Enumerable.Repeat(0, UpgradeTree.maxPointsPerBranch).ToArray();
    public override float[] primaryActionDelay => new float[UpgradeTree.maxPointsPerBranch] { 1.5f, 1.2f, 0.8f, 0.5f };
    public override float[] secondaryActionDelay => new float[UpgradeTree.maxPointsPerBranch] { 13f, 11f, 9f, 6f };
    public override float[] maxRange => new float[UpgradeTree.maxPointsPerBranch] { 1.5f,1.75f,2f,2.3f };
    public override bool reloadable => false;

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
        FireMelee(cameraOrigin, cameraForward, 0.5f, true);
        return true;
    }
    public override bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!base.DoSecondaryAction(cameraOrigin, cameraForward, muzzlePosition)) { return false; }
        myPc.myView.RPC(nameof(myPc.RPC_AddStatusEffect), Photon.Pun.RpcTarget.AllBufferedViaServer, EStatusEffects.FlamingArmor, StatusEffectBase.GetRandomUniqueId());
        return true;
    }
}
