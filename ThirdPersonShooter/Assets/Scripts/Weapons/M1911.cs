using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M1911 : Weapon
{
    public override string weaponName => "M1911";

    public override EWeapons weaponType => EWeapons.M1911;

    public override float zoomFov => 70f;

    public override Vector2 xRecoilMinMax => new Vector2(-0.2f, 0.2f);

    public override Vector2 yRecoilMinMax => new Vector2(0, 0.2f);

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 7;

    public override float fireDelay => 0.2f;

    public override float yawSpread => 1;

    public override float pitchSpread => 1;

    public override float maxRange => 500;

    public override bool isFullAuto => true;

    public override float aimingMoveSpeedModifier => 0.9f;

    public override float aimingSpreadModifier => 0.2f;

    public override float headshotModifier => 2f;

    public override float reloadDelay => 1f;

    public override float damage => 26f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 1;
    public override bool usesProjectile => false;

    public override string projectilePrefab => "";
    public override float fireSoundsVolumeModifier => 0.4f;

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
