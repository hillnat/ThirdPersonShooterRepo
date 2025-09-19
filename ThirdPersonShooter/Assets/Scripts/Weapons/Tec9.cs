using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tec9 : Weapon
{
    public override string weaponName => "Tec9";

    public override EWeapons weaponType => EWeapons.Tec9;

    public override float zoomFov => 80f;

    public override Vector2 xRecoilMinMax => new Vector2(-0.05f, 0.05f);

    public override Vector2 yRecoilMinMax => new Vector2(0, 0.1f);

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 22;

    public override float fireDelay => 0.065f;

    public override float yawSpread => 3;

    public override float pitchSpread => 3;

    public override float maxRange => 75;

    public override bool isFullAuto => true;

    public override float aimingMoveSpeedModifier => 0.8f;

    public override float aimingSpreadModifier => 0.75f;

    public override float headshotModifier => 1.5f;

    public override float reloadDelay => 1f;

    public override float damage => 12f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 1;
    public override bool usesProjectile => false;

    public override string projectilePrefab => "";
    public override float fireSoundsVolumeModifier => 0.3f;

    public override Vector2 fireSoundsPitchRange => new Vector2(0.9f, 1.1f);

    public override float reloadSoundsVolumeModifier => 0.3f;

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
