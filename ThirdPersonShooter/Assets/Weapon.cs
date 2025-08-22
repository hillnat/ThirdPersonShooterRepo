using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int meshIndex;
    public string weaponName = "Weapon";
    public int ammo = 0;
    public int maxAmmo = 30;
    public float lastFiredTime = 0;
    public float fireDelay = 0.05f;
    public GameObject impactParticles;
    public bool GetCanFire() { return GameManager.instance.time > lastFiredTime + fireDelay; }
    public void Fire(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanFire()) { return; }
        lastFiredTime = GameManager.instance.time;


        bool cameraRaySuccess = Physics.Raycast(cameraOrigin, cameraForward, out RaycastHit hit1, 999);//Camera ray
        bool muzzleRaySuccess = Physics.Linecast(muzzlePosition, hit1.point, out RaycastHit hit2, 999);//Muzzle ray
        

        Debug.DrawRay(cameraOrigin, cameraForward * 999f, Color.green, 1f);
        Debug.DrawLine(muzzlePosition, hit1.point, Color.yellow, 1f);
        if (impactParticles != null) { ParticleManager.instance.SpawnParticle(impactParticles, hit2.point, Quaternion.identity); }
    }
}
