using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShotgun : WeaponBase
{
    public override string weaponName => "Shotgun";
    public override string weaponNameInFile => "Shotgun";
    public override int indexInAnimator => 3;
    public override Vector2[] xRecoilMinMax => new Vector2[UpgradeTree.maxPointsPerBranch] { new Vector2(0.4f, 0.4f), new Vector2(0.3f, 0.35f), new Vector2(0.2f, 0.25f), new Vector2(0.05f, 0.07f) };
    public override Vector2[] yRecoilMinMax => new Vector2[UpgradeTree.maxPointsPerBranch] { new Vector2(0.4f, 0.4f), new Vector2(0.3f, 0.35f), new Vector2(0.2f, 0.25f), new Vector2(0.05f, 0.07f) };
    public override int[] primaryMaxAmmo => new int[UpgradeTree.maxPointsPerBranch] { 3, 4, 5, 6 };
    public override float[] primaryFireDelay => new float[UpgradeTree.maxPointsPerBranch] { 1f, 0.8f, 0.6f, 0.5f, };
    public override float[] yawSpread => new float[UpgradeTree.maxPointsPerBranch] { 4f, 3.5f, 3f, 2.5f };
    public override float[] pitchSpread => new float[UpgradeTree.maxPointsPerBranch] { 4f, 3.5f, 3f, 2.5f };
    public override float[] maxRange => new float[UpgradeTree.maxPointsPerBranch] { 15, 17, 20, 25 };
    public override float[] headshotModifier => new float[UpgradeTree.maxPointsPerBranch] { 1f, 1.25f, 1.5f, 1.75f };
    public override float[] reloadDelay => new float[UpgradeTree.maxPointsPerBranch] { 0.3f, 0.25f, 0.2f, 0.17f };
    public override float[] damage => new float[UpgradeTree.maxPointsPerBranch] { 4, 5, 6, 7 };
    public override int[] pelletsPerShot => new int[UpgradeTree.maxPointsPerBranch] { 12, 14, 16, 18 }; 

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
    public override void CompleteReload()
    {
        //base.CompleteReload();
        ChangePrimaryAmmo(1);
        reloadStartTime = float.MinValue;

        if (GetCanReload())
        {
            StartReloading();
        }
    }
    private void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
