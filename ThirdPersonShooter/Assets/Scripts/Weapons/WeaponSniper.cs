using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSniper : WeaponBase
{
    public override string weaponNameInFile => "Sniper";

    public override string weaponName => "Sniper";
    public override int indexInAnimator => 5;

    public override Vector2[] xRecoilMinMax => new Vector2[UpgradeTree.maxPointsPerBranch] { new Vector2(0.4f, 0.4f), new Vector2(0.3f, 0.35f), new Vector2(0.2f, 0.25f), new Vector2(0.05f, 0.07f) };

    public override Vector2[] yRecoilMinMax => new Vector2[UpgradeTree.maxPointsPerBranch] { new Vector2(0.4f, 0.4f), new Vector2(0.3f, 0.35f), new Vector2(0.2f, 0.25f), new Vector2(0.05f, 0.07f) };

    public override float[] damage => new float[UpgradeTree.maxPointsPerBranch] { 33,44,55,77 };
    public override float[] headshotModifier => new float[UpgradeTree.maxPointsPerBranch] { 2f,2.5f,3f,4f };

    public override int[] primaryMaxAmmo => new int[UpgradeTree.maxPointsPerBranch] { 3,4,5,6 };

    public override float[] primaryFireDelay => new float[UpgradeTree.maxPointsPerBranch] { 1f,0.8f,0.6f,0.5f,};

    public override float[] reloadDelay => new float[UpgradeTree.maxPointsPerBranch] { 5f,4.5f,4f,3.5f };
    public override float[] maxRange => new float[UpgradeTree.maxPointsPerBranch] { 100, 300, 500, 1000 };
    public override float[] yawSpread => new float[UpgradeTree.maxPointsPerBranch] { 0.5f, 0.1f, 0f, 0f };
    public override float[] pitchSpread => new float[UpgradeTree.maxPointsPerBranch] { 0.5f,0.1f,0f,0f};

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
        FireHitscan(cameraOrigin, cameraForward, muzzlePosition, true);
        return true;
    }
}
