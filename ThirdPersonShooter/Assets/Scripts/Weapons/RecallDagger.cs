using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecallDagger : Weapon
{
    public override string weaponName => "RecallDagger";

    public override EWeapons weaponType => EWeapons.RecallDagger;

    public override float zoomFov => 70f;

    public override Vector2 xRecoilMinMax => Vector2.zero;

    public override Vector2 yRecoilMinMax => Vector2.zero;

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 7;

    public override float fireDelay => 0.135f;

    public override float yawSpread => 0f;

    public override float pitchSpread => 0f;

    public override float maxRange => 1000;

    public override bool isFullAuto => false;

    public override float aimingMoveSpeedModifier => 1f;

    public override float aimingSpreadModifier => 0f;

    public override float headshotModifier => 0f;

    public override float reloadDelay => 1f;

    public override float damage => 0f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 1;
    public override bool usesProjectile => true;

    public override string projectilePrefab => "Projectiles/Projectile_RecallDagger";
    public override float fireSoundsVolumeModifier => 0.45f;

    public override Vector2 fireSoundsPitchRange => new Vector2(0.9f, 1.1f);

    public override float reloadSoundsVolumeModifier => 0.5f;

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
