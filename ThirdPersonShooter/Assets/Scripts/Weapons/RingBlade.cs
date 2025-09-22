using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBlade : Weapon
{
    public override string weaponName => "RingBlade";

    public override EWeapons weaponType => EWeapons.RingBlade;

    public override float zoomFov => 70f;

    public override Vector2 xRecoilMinMax => new Vector2(-0.2f, 0.2f);

    public override Vector2 yRecoilMinMax => new Vector2(0, 0.2f);

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 2;

    public override float fireDelay => 0.25f;

    public override float yawSpread => 0;

    public override float pitchSpread => 0;

    public override float maxRange => 1000;

    public override bool isFullAuto => true;

    public override float aimingMoveSpeedModifier => 1f;

    public override float aimingSpreadModifier => 1f;

    public override float headshotModifier => 2f;

    public override float reloadDelay => 2f;

    public override float damage => 27f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 1;
    public override bool usesProjectile => true;

    public override string projectilePrefab => "Projectiles/Projectile_RingBlade";
    public override float fireSoundsVolumeModifier => 0.7f;

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
    private void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
