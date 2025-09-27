using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShotgun : WeaponBase
{
    public override string weaponName => "Shotgun";
    public override string weaponNameInFile => "Shotgun";
    public override int indexInAnimator => 3;
    public override float zoomFov => 70f;
    public override Vector2 xRecoilMinMax => new Vector2(-0.2f, 0.2f);
    public override Vector2 yRecoilMinMax => new Vector2(0, 0.2f);
    public override int primaryMaxAmmo => 4;
    public override float primaryFireDelay => 0.4f;
    public override float yawSpread => 4;
    public override float pitchSpread => 4;
    public override float maxRange => 20;
    public override bool isFullAuto => true;
    public override float aimingMoveSpeedModifier => 0.7f;
    public override float aimingSpreadModifier => 0.75f;
    public override float headshotModifier => 2f;
    public override float reloadDelay => 0.25f;
    public override float damage => 8f;
    public override int pelletsPerShot => 18;

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
