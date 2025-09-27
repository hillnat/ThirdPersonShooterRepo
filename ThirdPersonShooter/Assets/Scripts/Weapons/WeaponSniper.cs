using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSniper : WeaponBase
{
    public override string weaponNameInFile => "Sniper";

    public override string weaponName => "Sniper";
    public override int indexInAnimator => 5;

    public override Vector2 xRecoilMinMax => new Vector2(-0.1f, 0.1f);

    public override Vector2 yRecoilMinMax => new Vector2(0.1f,0.11f);

    public override float zoomFov => 35f;

    public override float aimingMoveSpeedModifier => 0.5f;

    public override float aimingSpreadModifier => 0f;

    public override float damage => 33;

    public override int primaryMaxAmmo => 4;

    public override float primaryFireDelay => 1f;

    public override float reloadDelay => 2f;


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
        FireHitscan(true, cameraOrigin, cameraForward, muzzlePosition);
        return true;
    }
}
