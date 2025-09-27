using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{

    protected PlayerController myPc;
    public Vector2 currentRecoil = Vector2.zero;
    private float lastPrimaryFireTime = 0;
    private float lastSecondaryFireTime = 0;
    public int primaryAmmo=0;
    public int secondaryAmmo=0;
    public float reloadStartTime = float.MinValue;
    private bool wasReloading = false;
    private AudioClip[] primaryFireSounds;
    private AudioClip[] secondaryFireSounds;
    private AudioClip[] reloadSounds;
    private LayerMask hitLayerMask = new LayerMask();

    public virtual int indexInAnimator { get; } = 0;
    public abstract string weaponNameInFile { get; }
    public abstract string weaponName { get; }
    public virtual Vector2 xRecoilMinMax { get; } = Vector2.zero;
    public virtual Vector2 yRecoilMinMax { get; } = Vector2.zero;
    public virtual float zoomFov { get; } = 80f;
    public virtual int primaryMaxAmmo { get; } = 0;
    public virtual int secondaryMaxAmmo { get; } = 0;
    public virtual int primaryAmmoPerShot { get; } = 1;
    public virtual int secondaryAmmoPerShot { get; } = 1;
    public virtual float recoilFadeMultiplier { get; } = 20f;//Recoil takes 1sec/this to return to 0
    public virtual float damage { get; } = 0f;
    public virtual float aimingMoveSpeedModifier { get; } = 1f;
    public virtual float aimingSpreadModifier { get; } = 1f; 
    public virtual int pelletsPerShot { get; } = 1;
    public virtual float headshotModifier { get; } = 1f;
    public virtual float primaryFireDelay { get; } = 0f;
    public virtual float secondaryFireDelay { get; } = 0f;
    public virtual float yawSpread { get; } = 0f;
    public virtual float pitchSpread { get; } = 0f;
    public virtual float maxRange { get; } = 1000f;
    public virtual bool isFullAuto { get; } = false;
    public virtual string primaryProjectilePrefabPath { get; } = "";
    public virtual string secondaryProjectilePrefabPath { get; } = "";
    //Reload
    public abstract float reloadDelay { get; }

    
    private bool GetUsesSpread() { return yawSpread != 0 && pitchSpread != 0; }

    public string GetReloadSoundsPath() { return $"Audio/Weapons/{weaponNameInFile}/Reload"; }
    public string GetPrimaryFireSoundsPath() { return $"Audio/Weapons/{weaponNameInFile}/PrimaryFire"; }
    public string GetSecondaryFireSoundsPath() { return $"Audio/Weapons/{weaponNameInFile}/SecondaryFire"; }

    private AudioClip GetRandomPrimaryFireSound() { return primaryFireSounds.Length!=0 ? primaryFireSounds[Random.Range(0, primaryFireSounds.Length)] : null; }
    private AudioClip GetRandomSecondaryFireSound() { return secondaryFireSounds.Length!=0 ? secondaryFireSounds[Random.Range(0, secondaryFireSounds.Length)] : null; }
    private AudioClip GetRandomReloadSound() { return reloadSounds.Length!=0 ? reloadSounds[Random.Range(0, reloadSounds.Length)] : null; }

    public Vector2 GetRandomRecoil() { 
        return new Vector2(Random.Range(yRecoilMinMax.x, yRecoilMinMax.y), Random.Range(xRecoilMinMax.x, xRecoilMinMax.y)); 
    }
    public bool GetIsReloading() { return (GameManager.instance.time < reloadStartTime + reloadDelay); }
    public float GetReloadTimeLeft() { return GetIsReloading() ? ((int)(((reloadStartTime + reloadDelay) - GameManager.instance.time) * 10f) / 10f) : 0f; }
    public bool GetCanReload() { return !GetIsReloading() && primaryAmmo != primaryMaxAmmo; }
    public bool GetCanPrimaryFire() { return GameManager.instance.time > lastPrimaryFireTime + primaryFireDelay && primaryAmmo>0; }
    public bool GetCanSecondaryFire() { return GameManager.instance.time > lastSecondaryFireTime + primaryFireDelay && secondaryAmmo>0; }
    private float GetValueFromYawSpread()
    {
        float val = yawSpread;
        if (myPc != null && myPc.isAiming)//If aiming, apply modifier to spread
        {
            val *= aimingSpreadModifier;
        }
        return Random.Range(-val, val);
    }
    private float GetValueFromPitchSpread()
    {
        float val = pitchSpread;
        if (myPc != null && myPc.isAiming)//If aiming, apply modifier to spread
        {
            val *= aimingSpreadModifier;
        }
        return Random.Range(-val, val);
    }
    private Vector3 ApplySpreadToVector(Vector3 vector)
    {
        return Quaternion.AngleAxis(GetValueFromYawSpread(), Vector3.up) *
                            Quaternion.AngleAxis(GetValueFromPitchSpread(), myPc.transform.right) *
                            vector;//Calculate spread
    }

    public virtual bool DoPrimaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanPrimaryFire()) { return false; }
        AudioManager.instance.PlaySound(true, GetRandomPrimaryFireSound(), muzzlePosition, 1f, Random.Range(0.9f, 1.1f), myPc.myView.ViewID);

        ChangePrimaryAmmo(-primaryAmmoPerShot);
        lastPrimaryFireTime = GameManager.instance.time;
        return true;
    }
    public virtual bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanSecondaryFire()) { return false; }
        AudioManager.instance.PlaySound(true, GetRandomSecondaryFireSound(), muzzlePosition, 1f, Random.Range(0.9f, 1.1f), myPc.myView.ViewID);

        ChangeSecondaryAmmo(-secondaryAmmoPerShot);

        lastSecondaryFireTime = GameManager.instance.time;
        return true;
    }
    public void FireHitscan(bool isPrimaryFire, Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (GetIsReloading()) { CancelReload(); }

        currentRecoil += GetRandomRecoil();


        for (int i = 0; i < pelletsPerShot; i++)
        {

            Vector3 camRayDirection = GetUsesSpread() ? ApplySpreadToVector(cameraForward) : cameraForward;//Determine which vector to use


            Physics.queriesHitBackfaces = true;
            bool cameraRaySuccess = Physics.Raycast(cameraOrigin, camRayDirection, out RaycastHit hit1, maxRange, hitLayerMask);//Cast main ray from camera

            ParticleManager.instance.PlayDefaultLineFx(true, new Vector3[] { muzzlePosition, !cameraRaySuccess ? cameraOrigin + camRayDirection * maxRange : hit1.point });

            if (!cameraRaySuccess)
            {
                Physics.queriesHitBackfaces = false;
                return;
            }


            Vector3 muzzleDirection = (hit1.point - muzzlePosition).normalized;//Direction from muzzle point to hit of camera ray 
            bool muzzleRaySuccess = Physics.Raycast(muzzlePosition, muzzleDirection, out RaycastHit hit2, maxRange);//Muzzle ray
            Physics.queriesHitBackfaces = false;

            if (!muzzleRaySuccess) { return; }


            if (hit2.transform.root.gameObject.TryGetComponent<PlayerController>(out PlayerController hitPc) && hitPc != myPc && !hitPc.GetIsDead())//Hit player
            {
                float finalDamage = Mathf.Lerp(damage, 0, hit2.distance / maxRange);
                bool isHeadshot = hit2.collider.GetType() == typeof(SphereCollider);
                if (isHeadshot)
                {
                    finalDamage *= headshotModifier;
                    AudioManager.instance.PlayHeadshotSound(true, hit2.point, 1f, 1f, int.MinValue);
                }
                hitPc.myView.RPC(nameof(PlayerController.RPC_SetLastHitBy), RpcTarget.All, myPc.myView.ViewID);
                hitPc.myView.RPC(nameof(PlayerController.RPC_ChangeHealth), RpcTarget.All, -finalDamage);

                ParticleManager.instance.PlayGoreParticles(true, hit2.point, Quaternion.Euler(hit2.normal), int.MinValue);//Spawn blood
            }
            else
            {
                ParticleManager.instance.PlayImpactParticles(true, hit2.point, Quaternion.Euler(hit2.normal), int.MinValue);
            }
        }
    }
    public void FireProjectile(bool isPrimaryFire, Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (GetIsReloading()) { CancelReload(); }

        myPc.SpawnProjectile(isPrimaryFire ? primaryProjectilePrefabPath : secondaryProjectilePrefabPath, muzzlePosition, /*Quaternion.Euler(cameraForward)*/Quaternion.identity, muzzlePosition + (cameraForward * 5));
    }
    public void ChangePrimaryAmmo(int delta)
    {
        primaryAmmo = Mathf.Clamp(primaryAmmo + delta, 0, primaryMaxAmmo);
    }
    public void ChangeSecondaryAmmo(int delta)
    {
        secondaryAmmo = Mathf.Clamp(secondaryAmmo + delta, 0, secondaryMaxAmmo);
    }
    public void StartReloading()
    {
        if (GetIsReloading()) { return; }
        wasReloading = true;
        reloadStartTime = GameManager.instance.time;

        AudioClip reloadAudio = GetRandomReloadSound();
        if (reloadAudio != null) { AudioManager.instance.PlaySound(true, reloadAudio, Vector3.zero, 1f, Random.Range(0.9f,1.1f),myPc.myView.ViewID); }
    }
    public void CancelReload()
    {
        wasReloading = false;
        reloadStartTime = float.MinValue;
    }
    public void ResetWeapon()
    {
        primaryAmmo = primaryMaxAmmo;
        currentRecoil = Vector2.zero;
        if(GetIsReloading() ) {CancelReload();}
    }
    public void HandleRecoil()
    {
        currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, Time.fixedDeltaTime * recoilFadeMultiplier);
        if ((currentRecoil.x > 0 && currentRecoil.x < 0.001f) || (currentRecoil.x < 0 && currentRecoil.x > -0.001f)) { currentRecoil.x = 0; }
        if ((currentRecoil.y > 0 && currentRecoil.y < 0.001f) || (currentRecoil.y < 0 && currentRecoil.y > -0.001f)) { currentRecoil.y = 0; }
    }
    #region Unity Callbacks
    public void Awake()
    {
        reloadSounds = Resources.LoadAll<AudioClip>(GetReloadSoundsPath());
        primaryFireSounds = Resources.LoadAll<AudioClip>(GetPrimaryFireSoundsPath());
        secondaryFireSounds = Resources.LoadAll<AudioClip>(GetSecondaryFireSoundsPath());
        myPc = transform.root.gameObject.GetComponent<PlayerController>();
    }
    public virtual void CompleteReload()
    {
        primaryAmmo = primaryMaxAmmo;
        secondaryAmmo = secondaryMaxAmmo;
        reloadStartTime = float.MinValue;
    }
    public void Start()
    {
        primaryAmmo = primaryMaxAmmo;
        secondaryAmmo = secondaryMaxAmmo;
        string[] masks = new string[2] { "Default", "Player" };
        hitLayerMask = LayerMask.GetMask(masks);
    }
    public void Update()
    {     
        //If we were reloading last frame but arent anymore, the reload has finished
        if (wasReloading && !GetIsReloading()) {
            CompleteReload();
        }
    }
    public void FixedUpdate()
    {
        HandleRecoil();
    }
    public void LateUpdate()
    {
        wasReloading = GetIsReloading();
    }
    #endregion
}
