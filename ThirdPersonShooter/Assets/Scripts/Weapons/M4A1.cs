using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M4A1 : Weapon
{

    public override string weaponName => "M4A1";

    public override EWeapons weaponType => EWeapons.M4A1;

    public override float zoomFov => 70f;

    public override Vector2 xRecoilMinMax => new Vector2(-0.1f, 0.1f);

    public override Vector2 yRecoilMinMax => new Vector2(0,0.1f);

    public override float recoilFadeMultiplier => 20f;

    public override int maxAmmo => 30;

    public override float fireDelay => 0.07f;

    public override float yawSpread => 2;

    public override float pitchSpread => 2;

    public override float maxRange => 1000;

    public override bool isFullAuto => true;

    public override float aimingMoveSpeedModifier => 0.7f;

    public override float aimingSpreadModifier => 0.5f;

    public override float headshotModifier => 2f;

    public override float reloadDelay => 1.5f;

    public override float damage => 17f;

    public override string fireSoundsPath => $"{weaponName}/Fire/";

    public override string reloadSoundsPath => $"{weaponName}/Reload/";

    public override int pelletsPerShot => 1;

    public override bool usesProjectile => false;

    public override string projectilePrefab => "";

    public override float fireSoundsVolumeModifier => 0.25f;

    public override Vector2 fireSoundsPitchRange => new Vector2(0.9f,1.1f);

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
