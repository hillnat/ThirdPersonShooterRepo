using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : Weapon
{
    public override string weaponName => "Sniper";

    public override EWeapons weaponType => EWeapons.Sniper;

    public override float zoomFov =>25f;

    public override Vector2 xRecoilMinMax => new Vector2(-0.5f, 0.5f);

    public override Vector2 yRecoilMinMax => new Vector2(0, 0.5f);

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 4;

    public override float fireDelay => 0.35f;

    public override float yawSpread => 5;

    public override float pitchSpread => 5;

    public override float maxRange => 1000;

    public override bool isFullAuto => false;

    public override float aimingMoveSpeedModifier => 0.35f;

    public override float aimingSpreadModifier => 0f;

    public override float headshotModifier => 4f;

    public override float reloadDelay => 1.5f;

    public override float damage => 30f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 1;
    public override bool usesProjectile => false;

    public override string projectilePrefab => "";
    public override float fireSoundsVolumeModifier => 0.35f;

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
}
