using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Vector2 currentRecoil = Vector2.zero;
    public Vector2 xRecoilMinMax = Vector2.zero;
    public Vector2 yRecoilMinMax = Vector2.zero;
    public Vector2 GetRandomRecoil() { return new Vector2(Random.Range(yRecoilMinMax.x, yRecoilMinMax.y), Random.Range(xRecoilMinMax.x, xRecoilMinMax.y)); }
    public string weaponName = "Weapon";
    public int ammo = 0;
    public int maxAmmo = 30;
    public float lastFiredTime = 0;
    public float fireDelay = 0.05f;
    private int impactParticles =1;
    public int fireSound;
    public float reloadStartTime = float.MinValue;
    public float reloadDelay = 1.5f;
    private bool wasReloading = false;
    public float damage = 50f;
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
        if (fireSound != int.MinValue) { AudioManager.instance.RPC_SpawnSound(true, fireSound, muzzlePosition,1f); }
        if (impactParticles != int.MinValue) { ParticleManager.instance.RPC_SpawnParticle(true,impactParticles, hit2.point, Quaternion.identity); }

        if (muzzleRaySuccess && cameraRaySuccess && (hit1.transform == hit2.transform))
        {
            if (hit2.transform.gameObject.TryGetComponent<PlayerController>(out PlayerController hitPc) && !hitPc.isDead)
            {
                hitPc.myView.RPC("RPC_ChangeHealth", RpcTarget.All,-damage);
                ParticleManager.instance.RPC_SpawnParticle(true, 0, hit2.point, Quaternion.Euler(hit2.normal));
            }
        }
    }
    public void StartReloading()
    {
        if (GetIsReloading()) { return; }
        wasReloading = true;
        reloadStartTime = GameManager.instance.time;
        Debug.Log("reload started");
    }
    public void CancelReload()
    {
        wasReloading = false;
        reloadStartTime = float.MinValue;
        Debug.Log("reload cancelled");
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
}
