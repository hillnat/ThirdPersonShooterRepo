using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum EWeapons { M4A1, M1911, Shotgun, Tec9, Sniper }

public class Weapon : MonoBehaviour
{

    private PlayerController myPc;
    public string weaponName = "Weapon";
    public EWeapons thisWeapon;
    public float zoomFov = 60f;
    [Header("Recoil")]
    public Vector2 currentRecoil = Vector2.zero;
    public Vector2 xRecoilMinMax = Vector2.zero;
    public Vector2 yRecoilMinMax = Vector2.zero;
    public float recoilFadeMultiplier = 20f;//Recoil takes 1sec/this to return to 0
    public bool pingPongRecoil = false;//Recoil always goes left or right based on if ammo is even or odd. Note you can use yRecoilMax.x as a minimum range if this is true, rather than it being the negative range
    [Header("Ammo")]
    public int ammo = 0;
    public int maxAmmo = 30;
    [Header("Shooting")]
    private float lastFiredTime = 0;
    public float fireDelay = 0.05f;
    public int pelletsPerShot = 1;
    public float yawSpread = 0;
    public float pitchSpread = 0;
    public float maxRange = 1000;
    public bool isFullAuto = false;
    public float aimingMoveSpeedModifier = 1f;
    public float aimingSpreadModifier = 1f;
    public float headshotModifier = 2f;
    private bool hasSpread => yawSpread!=0 && pitchSpread!=0;
    [Header("Reloading")]
    public float reloadStartTime = float.MinValue;
    public float reloadDelay = 1.5f;
    private bool wasReloading = false;
    public bool isShotgunReload = false;
    [Header("Damage")]
    public float damage = 50f;
    //[Header("Sound")]
    public string fireSoundsPath => $"{weaponName}/Fire/";
    public string reloadSoundsPath => $"{weaponName}/Reload/";
    private AudioClip[] fireSounds;
    private AudioClip[] reloadSounds;
    public float fireAudioVolumeModifier = 1f;
    public float reloadAudioVolumeModifier = 1f;
    [Header("Particles")]
    public GameObject impactParticles;
    public GameObject goreParticles;
    public GameObject bulletTrailFx;
    private AudioClip GetRandomFireSound() { return fireSounds.Length!=0 ? fireSounds[Random.Range(0, fireSounds.Length)] : null; }
    private AudioClip GetRandomReloadSound() { return reloadSounds.Length!=0 ? reloadSounds[Random.Range(0, reloadSounds.Length)] : null; }

    public Vector2 GetRandomRecoil() { 
        return new Vector2(Random.Range(yRecoilMinMax.x, yRecoilMinMax.y), pingPongRecoil ? Mathf.Abs(Random.Range(xRecoilMinMax.x, xRecoilMinMax.y))*(ammo%2==1?1f:-1f) : Random.Range(xRecoilMinMax.x, xRecoilMinMax.y)); 
    }
    public bool GetIsReloading() { return (GameManager.instance.time < reloadStartTime + reloadDelay); }
    public float GetReloadTimeLeft() { return GetIsReloading() ? ((int)(((reloadStartTime + reloadDelay) - GameManager.instance.time) * 10f) / 10f) : 0f; }
    public bool GetCanReload() { return !GetIsReloading() && ammo != maxAmmo; }
    public bool GetCanFire() { return GameManager.instance.time > lastFiredTime + fireDelay && ammo>0; }
    public void Fire(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanFire()) { return; }
        if (GetIsReloading()) { CancelReload(); }
        
        /*
         */
        ammo--;
        lastFiredTime = GameManager.instance.time;
        //Add recoil
        currentRecoil += GetRandomRecoil();
        //Play sound
        AudioClip fireAudio = GetRandomFireSound();
        if (fireAudio != null) { AudioManager.instance.PlaySound(true, fireAudio, muzzlePosition, fireAudioVolumeModifier, myPc.myView.ViewID); }
        for (int i = 0; i < pelletsPerShot; i++)
        {
            float tempYawSpread = yawSpread;
            float tempPitchSpread = pitchSpread;
            if (myPc.isAiming)
            {
                tempPitchSpread *= aimingSpreadModifier;
                tempYawSpread *= aimingSpreadModifier;
            }
            Vector3 rotatedCamForward = 
                    Quaternion.AngleAxis(Random.Range(-tempYawSpread, tempYawSpread), Vector3.up) * 
                    Quaternion.AngleAxis(Random.Range(-tempPitchSpread, tempPitchSpread), myPc.transform.right) *
                    cameraForward;//Calculate spread
            Vector3 camRayDirection = hasSpread ? rotatedCamForward : cameraForward;//If we have spread use it
            bool cameraRaySuccess = Physics.Raycast(cameraOrigin, camRayDirection, out RaycastHit hit1, maxRange);//Camera ray

            //Muzzle direction is from the muzzle to the hit point, unless it didnt hit, then to the camera ray end point (so particles still spawn at the end of the ray)
            Vector3 muzzleDirection = 
                (hit1.point - 
                muzzlePosition).normalized;

            bool muzzleRaySuccess = Physics.Raycast(muzzlePosition, muzzleDirection, out RaycastHit hit2, maxRange);//Muzzle ray

            ParticleManager.instance.PlayLineFx(true, bulletTrailFx, new Vector3[] { muzzlePosition, !cameraRaySuccess ? cameraOrigin + camRayDirection * maxRange : hit2.point });


            if (cameraRaySuccess)
            {

                Debug.DrawRay(cameraOrigin, (hasSpread ? rotatedCamForward : cameraForward) * 999f, Color.green, 1f);
                Debug.DrawLine(muzzlePosition, hit1.point, Color.yellow, 1f);



                float t = hit2.distance / maxRange;
                float finalDamage = Mathf.Lerp(damage, 0, t);
                
                bool hitPlayer = false;//Not known yet
                bool hitAnything = false;//Not known yet
                if (muzzleRaySuccess  /*&& cameraRaySuccess &&(hit1.transform == hit2.transform)*/)
                {
                    if (hit2.transform.root.gameObject.TryGetComponent<PlayerController>(out PlayerController hitPc) && hitPc != myPc)//Hit player
                    {

                        bool isHeadshot = hit2.collider.GetType() == typeof(SphereCollider);
                        if (isHeadshot) { finalDamage *= headshotModifier; }
                        string hitData = "";
                        hitData += $"time : {GameManager.instance.time}\n";
                        hitData += $"headshot : {isHeadshot}\n";
                        hitData += $"collider : {hit2.collider}\n";
                        hitData += $"final damage : {finalDamage}\n";
                        Debug.Log(hitData);
                        
                        
                        hitPlayer = true;
                        if (!hitPc.isDead)
                        {
                            hitPc.myView.RPC(nameof(PlayerController.RPC_SetLastHitBy), RpcTarget.All, myPc.myView.ViewID);
                            hitPc.myView.RPC(nameof(PlayerController.RPC_ChangeHealth), RpcTarget.All, -finalDamage);
                            //ParticleManager.instance.SpawnDamageNumber(hitPc.transform.position, finalDamage);
                        }
                    }
                    hitAnything = true;
                }
                if (hitPlayer)
                {
                    ParticleManager.instance.PlayParticle(true, goreParticles, hit2.point, Quaternion.Euler(hit2.normal));//Spawn blood
                }
                else if (hitAnything) { ParticleManager.instance.PlayParticle(true, impactParticles, hit2.point, Quaternion.Euler(hit2.normal)); }//Spawn dust
            }        
        }    
    }
    public void StartReloading()
    {
        if (GetIsReloading()) { return; }
        wasReloading = true;
        reloadStartTime = GameManager.instance.time;

        AudioClip reloadAudio = GetRandomReloadSound();
        if (reloadAudio != null) { AudioManager.instance.PlaySound(true, reloadAudio, Vector3.zero, reloadAudioVolumeModifier, myPc.myView.ViewID); }
        Debug.Log("reload started");
    }
    public void CancelReload()
    {
        wasReloading = false;
        reloadStartTime = float.MinValue;
        Debug.Log("reload cancelled");
    }
    public void ResetWeapon()
    {
        ammo = maxAmmo;
        currentRecoil = Vector2.zero;
        if(GetIsReloading() ) {CancelReload();}
    }
    #region Unity Callbacks
    private void Awake()
    {
        reloadSounds = Resources.LoadAll<AudioClip>("Audio/"+reloadSoundsPath);
        fireSounds = Resources.LoadAll<AudioClip>("Audio/" + fireSoundsPath);
        Debug.Log($"{gameObject.name} found {reloadSounds.Length} reload sounds and {fireSounds.Length} fire sounds");
        myPc = transform.root.gameObject.GetComponent<PlayerController>();
    }
    private void Start()
    {
        ammo = maxAmmo;
    }
    private void Update()
    {
        currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, Time.deltaTime*recoilFadeMultiplier);
        if ((currentRecoil.x > 0 && currentRecoil.x < 0.01f) || (currentRecoil.x < 0 && currentRecoil.x > -0.01f)) { currentRecoil.x = 0; }
        if ((currentRecoil.y > 0 && currentRecoil.y < 0.01f) || (currentRecoil.y < 0 && currentRecoil.y > -0.01f)) { currentRecoil.y = 0; }
        
        //If we were reloading last frame but arent anymore, the reload has finished
        if (wasReloading && !GetIsReloading()) {
            if (isShotgunReload)
            {
                ammo++;
            }
            else
            {
                ammo = maxAmmo;
            }
            
            reloadStartTime = float.MinValue;
            Debug.Log("reload complete");
            if (isShotgunReload && GetCanReload())
            {
                StartReloading();
                Debug.Log("Shotgun reload");
            }
        }
    }
    private void LateUpdate()
    {
        wasReloading = GetIsReloading();
    }
    #endregion
}
