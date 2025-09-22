using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Weapon
{
    public override string weaponName => "Shotgun";

    public override EWeapons weaponType => EWeapons.Shotgun;

    public override float zoomFov => 70f;

    public override Vector2 xRecoilMinMax => new Vector2(-0.2f, 0.2f);

    public override Vector2 yRecoilMinMax => new Vector2(0, 0.2f);

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 4;

    public override float fireDelay => 0.4f;

    public override float yawSpread => 4;

    public override float pitchSpread => 4;

    public override float maxRange => 20;

    public override bool isFullAuto => true;

    public override float aimingMoveSpeedModifier => 0.7f;

    public override float aimingSpreadModifier => 0.75f;

    public override float headshotModifier => 2f;

    public override float reloadDelay => 0.25f;

    public override float damage => 8f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 18;

    public bool isShotgunReload = true;

    public override bool usesProjectile => false;

    public override string projectilePrefab => "";
    public override float fireSoundsVolumeModifier => 0.85f;

    public override Vector2 fireSoundsPitchRange => new Vector2(0.9f, 1.1f);

    public override float reloadSoundsVolumeModifier => 0.8f;

    public override Vector2 reloadSoundsPitchRange => new Vector2(0.95f, 1.05f);
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
        IncrementAmmo(1);
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
