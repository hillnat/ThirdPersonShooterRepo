using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private PlayerController myPc;
    public string weaponName = "Weapon";
    [Header("Recoil")]
    public Vector2 currentRecoil = Vector2.zero;
    public Vector2 xRecoilMinMax = Vector2.zero;
    public Vector2 yRecoilMinMax = Vector2.zero;
    public bool pingPongRecoil = false;//Recoil always goes left or right based on if ammo is even or odd. Note you can use yRecoilMax.x as a minimum range if this is true, rather than it being the negative range
    [Header("Ammo")]
    public int ammo = 0;
    public int maxAmmo = 30;
    [Header("Shooting")]
    private float lastFiredTime = 0;
    public float fireDelay = 0.05f;
    [Header("Reloading")]
    public float reloadStartTime = float.MinValue;
    public float reloadDelay = 1.5f;
    private bool wasReloading = false;
    [Header("Damage")]
    public float damage = 50f;
    [Header("Sound")]
    public string fireSoundsPath = "Weapon/Fire/";
    public string reloadSoundsPath = "Weapon/Reload/";
    private AudioClip[] fireSounds;
    private AudioClip[] reloadSounds;
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
    public bool GetCanFire() { return GameManager.instance.time > lastFiredTime + fireDelay && !GetIsReloading() && ammo>0; }
    public void Fire(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanFire()) { return; }


        ammo--;
        lastFiredTime = GameManager.instance.time;

        currentRecoil += GetRandomRecoil();

        bool cameraRaySuccess = Physics.Raycast(cameraOrigin, cameraForward, out RaycastHit hit1, 999);//Camera ray
        Vector3 muzzleDirection = (hit1.point - muzzlePosition).normalized;
        bool muzzleRaySuccess = Physics.Raycast(muzzlePosition, muzzleDirection, out RaycastHit hit2, 999);//Muzzle ray
        
        Debug.DrawRay(cameraOrigin, cameraForward * 999f, Color.green, 1f);
        Debug.DrawLine(muzzlePosition, hit1.point, Color.yellow, 1f);
        AudioClip fireAudio = GetRandomFireSound();
        if (fireAudio!=null) { AudioManager.instance.PlaySound(true, fireAudio, Vector3.zero, 1f, myPc.myView.ViewID); }
        ParticleManager.instance.PlayLineFx(true, bulletTrailFx, new Vector3[] { muzzlePosition, hit2.point });

        bool hitPlayer = false;//Not known yet
        bool hitAnything = false;//Not known yet
        if (muzzleRaySuccess && cameraRaySuccess && (hit1.transform == hit2.transform))
        {
            if (hit2.transform.gameObject.TryGetComponent<PlayerController>(out PlayerController hitPc) && hitPc!=myPc)//Hit player
            {
                hitPlayer = true;
                if (!hitPc.isDead)
                {
                    hitPc.myView.RPC("RPC_ChangeHealth", RpcTarget.All, -damage);
                }
            }
            hitAnything = true;
        }
        if (hitPlayer)
        {
            ParticleManager.instance.PlayParticle(true, goreParticles, hit2.point, Quaternion.Euler(hit2.normal));
        }
        else if (hitAnything) { ParticleManager.instance.PlayParticle(true, impactParticles, hit2.point, Quaternion.Euler(hit2.normal)); }
        
    }
    public void StartReloading()
    {
        if (GetIsReloading()) { return; }
        wasReloading = true;
        reloadStartTime = GameManager.instance.time;

        AudioClip reloadAudio = GetRandomReloadSound();
        if (reloadAudio != null) { AudioManager.instance.PlaySound(true, reloadAudio, Vector3.zero, 1f, myPc.myView.ViewID); }
        Debug.Log("reload started");
    }
    public void CancelReload()
    {
        wasReloading = false;
        reloadStartTime = float.MinValue;
        Debug.Log("reload cancelled");
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
        currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, Time.deltaTime*20f);
        if ((currentRecoil.x > 0 && currentRecoil.x < 0.01f) || (currentRecoil.x < 0 && currentRecoil.x > -0.01f)) { currentRecoil.x = 0; }
        if ((currentRecoil.y > 0 && currentRecoil.y < 0.01f) || (currentRecoil.y < 0 && currentRecoil.y > -0.01f)) { currentRecoil.y = 0; }
        
        //If we were reloading last frame but arent anymore, the reload has finished
        if (wasReloading && !GetIsReloading()) {
            ammo = maxAmmo;
            reloadStartTime = float.MinValue;
            Debug.Log("reload complete");
        }
    }
    private void LateUpdate()
    {
        wasReloading = GetIsReloading();
    }
    #endregion
}
