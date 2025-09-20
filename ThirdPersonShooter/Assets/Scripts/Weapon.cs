using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum EWeapons { M4A1, M1911, Shotgun, Tec9, Sniper, RingBlade, RecallDagger }

public abstract class Weapon : MonoBehaviour
{

    private PlayerController myPc;
    public Vector2 currentRecoil = Vector2.zero;
    private float lastFiredTime = 0;
    public int ammo=0;
    public float reloadStartTime = float.MinValue;
    private bool wasReloading = false;
    private AudioClip[] fireSounds;
    private AudioClip[] reloadSounds;
   

    public bool pingPongRecoil = false;//Recoil always goes left or right based on if ammo is even or odd. Note you can use yRecoilMax.x as a minimum range if this is true, rather than it being the negative range

    private bool usesSpread => yawSpread != 0 && pitchSpread != 0;


    public abstract string weaponName { get; }
    public abstract EWeapons weaponType { get; }
    //Recoil
    public abstract Vector2 xRecoilMinMax { get; }
    public abstract Vector2 yRecoilMinMax { get; }
    public abstract float recoilFadeMultiplier { get; }//Recoil takes 1sec/this to return to 0

    //Aiming
    public abstract float zoomFov { get; }
    public abstract float aimingMoveSpeedModifier { get; }
    public abstract float aimingSpreadModifier { get; }
    //Shooting
    public abstract float damage { get; }
    public abstract int maxAmmo { get; }
    public abstract int pelletsPerShot { get; }
    public abstract float headshotModifier { get; }
    public abstract float fireDelay { get; }
    public abstract float yawSpread { get; }
    public abstract float pitchSpread { get; }
    public abstract float maxRange { get; }
    public abstract bool isFullAuto { get; }
    public abstract bool usesProjectile { get; }
    public abstract string projectilePrefab { get; }
    //Reload
    public abstract float reloadDelay { get; }

    //Audio
    public abstract string fireSoundsPath { get; }
    public abstract float fireSoundsVolumeModifier{get; }
    public abstract Vector2 fireSoundsPitchRange {get; }
    public abstract string reloadSoundsPath { get; }
    public abstract float reloadSoundsVolumeModifier { get; }
    public abstract Vector2 reloadSoundsPitchRange { get; }

    //public abstract float fireAudioVolumeModifier = 1f;
    //public abstract float reloadAudioVolumeModifier = 1f;
    private AudioClip GetRandomFireSound() { return fireSounds.Length!=0 ? fireSounds[Random.Range(0, fireSounds.Length)] : null; }
    private AudioClip GetRandomReloadSound() { return reloadSounds.Length!=0 ? reloadSounds[Random.Range(0, reloadSounds.Length)] : null; }

    public Vector2 GetRandomRecoil() { 
        return new Vector2(Random.Range(yRecoilMinMax.x, yRecoilMinMax.y), pingPongRecoil ? Mathf.Abs(Random.Range(xRecoilMinMax.x, xRecoilMinMax.y))*(ammo%2==1?1f:-1f) : Random.Range(xRecoilMinMax.x, xRecoilMinMax.y)); 
    }
    public bool GetIsReloading() { return (GameManager.instance.time < reloadStartTime + reloadDelay); }
    public float GetReloadTimeLeft() { return GetIsReloading() ? ((int)(((reloadStartTime + reloadDelay) - GameManager.instance.time) * 10f) / 10f) : 0f; }
    public bool GetCanReload() { return !GetIsReloading() && ammo != maxAmmo; }
    public bool GetCanFire() { return GameManager.instance.time > lastFiredTime + fireDelay && ammo>0; }
    public virtual void Fire(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanFire()) { return; }
        if (GetIsReloading()) { CancelReload(); } 
        ammo--;
        lastFiredTime = GameManager.instance.time;
        //Add recoil
        currentRecoil += GetRandomRecoil();
        //Play sound
        AudioManager.instance.PlaySound(true, GetRandomFireSound(), muzzlePosition, fireSoundsVolumeModifier, Random.Range(fireSoundsPitchRange.x, fireSoundsPitchRange.y),myPc.myView.ViewID);

        if (!usesProjectile)
        {
            bool hitPlayer = false;//Not known yet
            bool hitAnything = false;//Not known yet
            for (int i = 0; i < pelletsPerShot; i++)
            {
                //Cache spread
                float currentYawSpread = yawSpread;
                float currentPitchSpread = pitchSpread;

                if (myPc.isAiming)//If aiming, apply modifier to spread
                {
                    currentPitchSpread *= aimingSpreadModifier;
                    currentYawSpread *= aimingSpreadModifier;
                }
                //Calculate vector from camera using spread
                Vector3 rotatedCamForward =
                        Quaternion.AngleAxis(Random.Range(-currentYawSpread, currentYawSpread), Vector3.up) *
                        Quaternion.AngleAxis(Random.Range(-currentPitchSpread, currentPitchSpread), myPc.transform.right) *
                        cameraForward;//Calculate spread

                Vector3 camRayDirection = usesSpread ? rotatedCamForward : cameraForward;//Determine which vector to use

                bool cameraRaySuccess = Physics.Raycast(cameraOrigin, camRayDirection, out RaycastHit hit1, maxRange);//Cast main ray from camera

                //Muzzle direction is from the muzzle to the hit point, unless it didnt hit, then to the camera ray end point (so particles still spawn at the end of the ray)
                Vector3 muzzleDirection =
                    (hit1.point -
                    muzzlePosition).normalized;

                bool muzzleRaySuccess = Physics.Raycast(muzzlePosition, muzzleDirection, out RaycastHit hit2, maxRange);//Muzzle ray

                ParticleManager.instance.PlayDefaultLineFx(true, new Vector3[] { muzzlePosition, !cameraRaySuccess ? cameraOrigin + camRayDirection * maxRange : hit2.point });

                Debug.DrawRay(cameraOrigin, (usesSpread ? rotatedCamForward : cameraForward) * 999f, Color.green, 5f);
                Debug.DrawLine(muzzlePosition, hit1.point, Color.yellow, 5f);
                if (cameraRaySuccess)
                {
                    if (muzzleRaySuccess  /*&& cameraRaySuccess &&(hit1.transform == hit2.transform)*/)
                    {
                        hitAnything = true;

                        if (hit2.transform.root.gameObject.TryGetComponent<PlayerController>(out PlayerController hitPc) && hitPc != myPc && !hitPc.GetIsDead())//Hit player
                        {
                            float finalDamage = Mathf.Lerp(damage, 0, hit2.distance / maxRange);
                            bool isHeadshot = hit2.collider.GetType() == typeof(SphereCollider);
                            if (isHeadshot) { finalDamage *= headshotModifier; }
                            string hitData = "";
                            hitData += $"time : {GameManager.instance.time}\n";
                            hitData += $"headshot : {isHeadshot}\n";
                            hitData += $"collider : {hit2.collider}\n";
                            hitData += $"final damage : {finalDamage}\n";
                            Debug.Log(hitData);


                            hitPlayer = true;
                            if (!hitPc.GetIsDead())
                            {
                                hitPc.myView.RPC(nameof(PlayerController.RPC_SetLastHitBy), RpcTarget.All, myPc.myView.ViewID);
                                hitPc.myView.RPC(nameof(PlayerController.RPC_ChangeHealth), RpcTarget.All, -finalDamage);
                                //ParticleManager.instance.SpawnDamageNumber(hitPc.transform.position, finalDamage);
                            }

                            if (isHeadshot)
                            {
                                AudioManager.instance.PlayHeadshotSound(true, hit2.point, 1f, 1f, int.MinValue);
                            }
                        }
                    }       
                }
                if (hitPlayer)
                {
                    ParticleManager.instance.PlayGoreParticles(true, hit2.point, Quaternion.Euler(hit2.normal));//Spawn blood
                }
                else if (hitAnything)
                {
                    ParticleManager.instance.PlayImpactParticles(true, hit2.point, Quaternion.Euler(hit2.normal));
                }//Spawn dust
            }
            
        }
        else
        {
            myPc.SpawnProjectile(projectilePrefab, muzzlePosition, Quaternion.Euler(cameraForward), muzzlePosition+(cameraForward*10));
        }
    }
    public void IncrementAmmo(int delta)
    {
        ammo = Mathf.Clamp(ammo + delta, 0, maxAmmo);
    }
    public void StartReloading()
    {
        if (GetIsReloading()) { return; }
        wasReloading = true;
        reloadStartTime = GameManager.instance.time;

        AudioClip reloadAudio = GetRandomReloadSound();
        if (reloadAudio != null) { AudioManager.instance.PlaySound(true, reloadAudio, Vector3.zero, reloadSoundsVolumeModifier, Random.Range(reloadSoundsPitchRange.x,reloadSoundsPitchRange.y),myPc.myView.ViewID); }

        if (weaponType == EWeapons.RecallDagger)
        {
            for (int i = 0; i < myPc.ownedProjectiles.Count; i++)
            {
                if (myPc.ownedProjectiles[i].projectileType == EProjectiles.RecallDagger)
                {
                    Projectile myProjectile = myPc.ownedProjectiles[i];
                    myProjectile.wantsToFlyToPlayer = true;
                    //myProjectile.ToggleCollider(true);
                }
            }
        }
        Debug.Log("reload started");
    }
    public void CancelReload()
    {
        wasReloading = false;
        reloadStartTime = float.MinValue;
    }
    public void ResetWeapon()
    {
        ammo = maxAmmo;
        currentRecoil = Vector2.zero;
        if(GetIsReloading() ) {CancelReload();}
    }
    public void HandleRecoil()
    {
        currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, Time.deltaTime * recoilFadeMultiplier);
        if ((currentRecoil.x > 0 && currentRecoil.x < 0.01f) || (currentRecoil.x < 0 && currentRecoil.x > -0.01f)) { currentRecoil.x = 0; }
        if ((currentRecoil.y > 0 && currentRecoil.y < 0.01f) || (currentRecoil.y < 0 && currentRecoil.y > -0.01f)) { currentRecoil.y = 0; }
    }
    #region Unity Callbacks
    public void Awake()
    {
        reloadSounds = Resources.LoadAll<AudioClip>("Audio/"+reloadSoundsPath);
        fireSounds = Resources.LoadAll<AudioClip>("Audio/" + fireSoundsPath);
        myPc = transform.root.gameObject.GetComponent<PlayerController>();
    }
    public virtual void CompleteReload()
    {
        ammo = maxAmmo;
        reloadStartTime = float.MinValue;
    }
    public void Start()
    {
        ammo = maxAmmo;
    }
    public void Update()
    {
        HandleRecoil();
        
        //If we were reloading last frame but arent anymore, the reload has finished
        if (wasReloading && !GetIsReloading()) {
            CompleteReload();
        }
    }
    public void LateUpdate()
    {
        wasReloading = GetIsReloading();
    }
    #endregion
}
