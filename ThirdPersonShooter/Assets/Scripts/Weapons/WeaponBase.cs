using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{

    protected PlayerControllerBase myPc;
    public Vector2 currentRecoil = Vector2.zero;
    public float lastPrimaryFireTime = 0;
    public float lastSecondaryFireTime = 0;
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
    //Each value that can be changed via upgrades is stored in an array with the length of the max upgrade count.
    public virtual Vector2[] xRecoilMinMax { get; } = Enumerable.Repeat(Vector2.zero, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual Vector2[] yRecoilMinMax { get; } = Enumerable.Repeat(Vector2.zero, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual int[] primaryMaxAmmo { get; } = Enumerable.Repeat(1, UpgradeTree.maxPointsPerBranch).ToArray();//Max ammo doesnt matter if ammo per shot is 0
    public virtual int[] secondaryMaxAmmo { get; } = Enumerable.Repeat(1, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual int[] primaryAmmoPerShot { get; } = Enumerable.Repeat(1, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual int[] secondaryAmmoPerShot { get; } = Enumerable.Repeat(1, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] recoilFadeMultiplier { get; } = Enumerable.Repeat(20f, UpgradeTree.maxPointsPerBranch).ToArray();//Recoil takes 1sec/this to return to 0
    public virtual float[] damage { get; } = Enumerable.Repeat(0f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual int[] pelletsPerShot { get; } = Enumerable.Repeat(1, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] headshotModifier { get; } = Enumerable.Repeat(1f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] primaryFireDelay { get; } = Enumerable.Repeat(1f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] secondaryFireDelay { get; } = Enumerable.Repeat(1f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] yawSpread { get; } = Enumerable.Repeat(0f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] pitchSpread { get; } = Enumerable.Repeat(0f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] maxRange { get; } = Enumerable.Repeat(1000f, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual float[] reloadDelay { get; } = Enumerable.Repeat(5f, UpgradeTree.maxPointsPerBranch).ToArray();

    public virtual bool[] isFullAuto { get; } = Enumerable.Repeat(false, UpgradeTree.maxPointsPerBranch).ToArray();
    public virtual bool reloadable { get; } = true;//false if weapon uses cooldowns
    public virtual string primaryProjectilePrefabPath { get; } = "";
    public virtual string secondaryProjectilePrefabPath { get; } = "";
    //Reload
    public int primaryBranchNumInUpgradeTree=0;
    public int secondayBranchNumInUpgradeTree=0;
    public int GetPrimaryAbilityLevel()
    {
        return myPc.myUpgradeTree.GetLevelOfBranch(primaryBranchNumInUpgradeTree) - 1;
    }
    public int GetSecondaryAbilityLevel()
    {
        return myPc.myUpgradeTree.GetLevelOfBranch(secondayBranchNumInUpgradeTree) - 1;
    }

    private bool GetUsesSpread() {
        int level = GetPrimaryAbilityLevel();
        return yawSpread[level] != 0 && pitchSpread[level] != 0; 
    }

    public string GetReloadSoundsPath() { return $"Audio/Weapons/{weaponNameInFile}/Reload"; }
    public string GetPrimaryFireSoundsPath() { return $"Audio/Weapons/{weaponNameInFile}/PrimaryFire"; }
    public string GetSecondaryFireSoundsPath() { return $"Audio/Weapons/{weaponNameInFile}/SecondaryFire"; }

    private AudioClip GetRandomPrimaryFireSound() { return primaryFireSounds.Length!=0 ? primaryFireSounds[Random.Range(0, primaryFireSounds.Length)] : null; }
    private AudioClip GetRandomSecondaryFireSound() { return secondaryFireSounds.Length!=0 ? secondaryFireSounds[Random.Range(0, secondaryFireSounds.Length)] : null; }
    private AudioClip GetRandomReloadSound() { return reloadSounds.Length!=0 ? reloadSounds[Random.Range(0, reloadSounds.Length)] : null; }

    public Vector2 GetRandomRecoil() {
        int level = GetPrimaryAbilityLevel();
        return new Vector2(Random.Range(yRecoilMinMax[level].x, yRecoilMinMax[level].y), Random.Range(xRecoilMinMax[level].x, xRecoilMinMax[level].y)); 
    }
    public bool GetIsReloading() {
        int level = GetPrimaryAbilityLevel();
        return level!=-1 && reloadable && (GameManager.instance.localTime < reloadStartTime + reloadDelay[level]); 
    }
    public float GetReloadTimeLeft() {
        if (!reloadable || !GetIsReloading()){ return 0f; }
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return 0f; }
        return ((int)(((reloadStartTime + reloadDelay[level]) - GameManager.instance.localTime) * 10f) / 10f); 
    }
    public float GetReloadTimeElapsed01()
    {
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return 0; }
        return Mathf.Clamp01(Mathf.InverseLerp(0f, reloadDelay[level], GameManager.instance.localTime - reloadStartTime));
    }
    public bool GetCanReload() { return reloadable && !GetIsReloading() && GetPrimaryAbilityLevel() != -1; }
    public bool GetCanDoPrimaryAction() { 
        int level = GetPrimaryAbilityLevel();
        if(level==-1) {return false; }
        return GameManager.instance.localTime > lastPrimaryFireTime + primaryFireDelay[level] &&
            (primaryAmmo > 0 || primaryAmmoPerShot[level] == 0);
    }
    public bool GetCanDoSecondaryAction() {
        int level = GetSecondaryAbilityLevel();
        if (level == -1) { return false; }
        return GameManager.instance.localTime > lastSecondaryFireTime + secondaryFireDelay[level] &&
            (secondaryAmmo > 0 || secondaryAmmoPerShot[level] == 0);
    }
    public float GetPrimaryFireDelayElapsedAmount01()
    {
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return 0f; }
        return Mathf.Clamp01(1f-Mathf.InverseLerp(0f, primaryFireDelay[level], GameManager.instance.localTime - lastPrimaryFireTime));
    }
    public float GetSecondaryFireDelayElapsedAmount01()
    {
        int level = GetSecondaryAbilityLevel();
        if (level == -1) { return 0f; }
        return Mathf.Clamp01(1f-Mathf.InverseLerp(0f, secondaryFireDelay[level], GameManager.instance.localTime - lastSecondaryFireTime));
    }
    public float GetPrimaryFireDelayTimeLeft()
    {
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return 0f; }
        float elapsed = (primaryFireDelay[level] + lastPrimaryFireTime) - (GameManager.instance.localTime);
        return ((int)(elapsed * 10)) / 10f;
    }
    public float GetSecondaryFireDelayTimeLeft()
    {
        int level = GetSecondaryAbilityLevel();
        if (level == -1) { return 0f; }
        float elapsed = (secondaryFireDelay[level] + lastSecondaryFireTime) - (GameManager.instance.localTime);
        return ((int)(elapsed * 10)) / 10f;
    }
    private float GetValueFromYawSpread()
    {
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return 0f; }
        float val = yawSpread[level];
        return Random.Range(-val, val);
    }
    private float GetValueFromPitchSpread()
    {
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return 0f; }
        float val = pitchSpread[level];
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
        if (!GetCanDoPrimaryAction()) { return false; }
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return false; }
        AudioManager.instance.PlaySound(true, GetRandomPrimaryFireSound(), muzzlePosition, 1f, Random.Range(0.9f, 1.1f), myPc.myView.ViewID);
        ChangePrimaryAmmo(-primaryAmmoPerShot[level]);
        lastPrimaryFireTime = GameManager.instance.localTime;
        return true;
    }
    public virtual bool DoSecondaryAction(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition)
    {
        if (!GetCanDoSecondaryAction()) { return false; }
        int level = GetSecondaryAbilityLevel();
        if (level == -1) { return false; }
        AudioManager.instance.PlaySound(true, GetRandomSecondaryFireSound(), muzzlePosition, 1f, Random.Range(0.9f, 1.1f), myPc.myView.ViewID);
        ChangeSecondaryAmmo(-secondaryAmmoPerShot[level]);
        lastSecondaryFireTime = GameManager.instance.localTime;
        return true;
    }
    public void FireMelee(Vector3 cameraOrigin, Vector3 cameraForward, float radius, bool isPrimary)
    {
        if (GetIsReloading()) { CancelReload(); }
        int level = isPrimary ? GetPrimaryAbilityLevel() : GetSecondaryAbilityLevel();
        if (level == -1) { return; }

        RaycastHit[] hits = Physics.SphereCastAll(cameraOrigin+cameraForward, radius, cameraForward, maxRange[level], hitLayerMask);//Cast main ray from camera
        Debug.DrawRay(cameraOrigin + cameraForward, cameraForward * maxRange[level], Color.yellow,5);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.transform.root.gameObject.TryGetComponent<PlayerControllerBase>(out PlayerControllerBase hitPc) && hitPc != myPc && !hitPc.GetIsDead())//Hit player
            {
                float finalDamage = damage[level];
                bool isHeadshot = hit.collider.GetType() == typeof(SphereCollider);
                if (isHeadshot)
                {
                    finalDamage *= headshotModifier[level];
                    AudioManager.instance.PlayHeadshotSound(true, hit.point, 1f, 1f, int.MinValue);
                }
                hitPc.myView.RPC(nameof(PlayerControllerBase.RPC_SetLastHitBy), RpcTarget.All, myPc.myView.ViewID);
                hitPc.myView.RPC(nameof(PlayerControllerBase.RPC_ChangeHealth), RpcTarget.All, -finalDamage);

                ParticleManager.instance.PlayGoreParticles(true, hit.point, Quaternion.Euler(hit.normal), int.MinValue);//Spawn blood
            }
            else
            {
                ParticleManager.instance.PlayImpactParticles(true, hit.point, Quaternion.Euler(hit.normal), int.MinValue);
            }
        }   
    }
    public void FireHitscan(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition, bool isPrimary)
    {
        if (GetIsReloading()) { CancelReload(); }

        currentRecoil += GetRandomRecoil();
        int level = isPrimary ? GetPrimaryAbilityLevel() : GetSecondaryAbilityLevel();
        if (level == -1) { return; }

        for (int i = 0; i < pelletsPerShot[level]; i++)
        {

            Vector3 camRayDirection = GetUsesSpread() ? ApplySpreadToVector(cameraForward) : cameraForward;//Determine which vector to use


            Physics.queriesHitBackfaces = true;
            bool cameraRaySuccess = Physics.Raycast(cameraOrigin, camRayDirection, out RaycastHit hit1, maxRange[level], hitLayerMask);//Cast main ray from camera

            ParticleManager.instance.PlayDefaultLineFx(true, new Vector3[] { muzzlePosition, !cameraRaySuccess ? cameraOrigin + camRayDirection * maxRange[level] : hit1.point });

            if (!cameraRaySuccess)
            {
                Physics.queriesHitBackfaces = false;
                return;
            }


            Vector3 muzzleDirection = (hit1.point - muzzlePosition).normalized;//Direction from muzzle point to hit of camera ray 
            bool muzzleRaySuccess = Physics.Raycast(muzzlePosition, muzzleDirection, out RaycastHit hit2, maxRange[level]);//Muzzle ray
            Physics.queriesHitBackfaces = false;

            if (!muzzleRaySuccess) { return; }


            if (hit2.transform.root.gameObject.TryGetComponent<PlayerControllerBase>(out PlayerControllerBase hitPc) && hitPc != myPc && !hitPc.GetIsDead())//Hit player
            {
                float finalDamage = Mathf.Lerp(damage[level], 0, hit2.distance / maxRange[level]);
                bool isHeadshot = hit2.collider.GetType() == typeof(SphereCollider);
                if (isHeadshot)
                {
                    finalDamage *= headshotModifier[level];
                    AudioManager.instance.PlayHeadshotSound(true, hit2.point, 1f, 1f, int.MinValue);
                }
                hitPc.myView.RPC(nameof(PlayerControllerBase.RPC_SetLastHitBy), RpcTarget.All, myPc.myView.ViewID);
                hitPc.myView.RPC(nameof(PlayerControllerBase.RPC_ChangeHealth), RpcTarget.All, -finalDamage);

                ParticleManager.instance.PlayGoreParticles(true, hit2.point, Quaternion.Euler(hit2.normal), int.MinValue);//Spawn blood
            }
            else
            {
                ParticleManager.instance.PlayImpactParticles(true, hit2.point, Quaternion.Euler(hit2.normal), int.MinValue);
            }
        }
    }
    public void FireProjectile(Vector3 cameraOrigin, Vector3 cameraForward, Vector3 muzzlePosition, bool isPrimary)
    {
        if (GetIsReloading()) { CancelReload(); }
        int level = isPrimary ? GetPrimaryAbilityLevel() : GetSecondaryAbilityLevel();
        if (level == -1) { return; }
        string path = isPrimary ? primaryProjectilePrefabPath : secondaryProjectilePrefabPath;
        myPc.SpawnProjectile(path, muzzlePosition, Quaternion.identity, cameraOrigin + (cameraForward * 100));
    }
    public void ChangePrimaryAmmo(int delta)
    {
        int level = GetPrimaryAbilityLevel();
        if(level == -1) { return; }
        primaryAmmo = Mathf.Clamp(primaryAmmo + delta, 0, primaryMaxAmmo[level]);
    }
    public void ChangeSecondaryAmmo(int delta)
    {
        int level = GetSecondaryAbilityLevel();
        if (level == -1) { return; }
        secondaryAmmo = Mathf.Clamp(secondaryAmmo + delta, 0, secondaryMaxAmmo[level]);
    }
    public void StartReloading()
    {
        if (!reloadable || GetIsReloading()) { return; }
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return; }
        wasReloading = true;
        reloadStartTime = GameManager.instance.localTime;

        AudioClip reloadAudio = GetRandomReloadSound();
        if (reloadAudio != null) { AudioManager.instance.PlaySound(true, reloadAudio, Vector3.zero, 1f, Random.Range(0.9f,1.1f),myPc.myView.ViewID); }
    }
    public void CancelReload()
    {
        if (!reloadable) { return; }
        wasReloading = false;
        reloadStartTime = float.MinValue;
    }
    public void ResetWeapon()
    {
        if(GetIsReloading()) {CancelReload();}
        int primaryLevel = GetPrimaryAbilityLevel();
        int secondaryLevel = GetSecondaryAbilityLevel();
        if (primaryLevel != -1) { primaryAmmo = primaryMaxAmmo[primaryLevel]; }
        if (secondaryLevel != -1) { secondaryAmmo = secondaryMaxAmmo[secondaryLevel]; }
        lastPrimaryFireTime = float.MinValue;
        lastSecondaryFireTime=float.MinValue;
        reloadStartTime= float.MinValue;
        currentRecoil = Vector2.zero;
        if(GetIsReloading() ) {CancelReload();}
    }
    public void HandleRecoil()
    {
        int level = GetPrimaryAbilityLevel();
        if (level == -1) { return; }
        currentRecoil = Vector2.Lerp(currentRecoil, Vector2.zero, Time.fixedDeltaTime * recoilFadeMultiplier[GetPrimaryAbilityLevel()]);
        if ((currentRecoil.x > 0 && currentRecoil.x < 0.001f) || (currentRecoil.x < 0 && currentRecoil.x > -0.001f)) { currentRecoil.x = 0; }
        if ((currentRecoil.y > 0 && currentRecoil.y < 0.001f) || (currentRecoil.y < 0 && currentRecoil.y > -0.001f)) { currentRecoil.y = 0; }
    }
    #region Unity Callbacks
    public void Awake()
    {
        reloadSounds = Resources.LoadAll<AudioClip>(GetReloadSoundsPath());
        primaryFireSounds = Resources.LoadAll<AudioClip>(GetPrimaryFireSoundsPath());
        secondaryFireSounds = Resources.LoadAll<AudioClip>(GetSecondaryFireSoundsPath());
        myPc = transform.root.gameObject.GetComponent<PlayerControllerBase>();
    }
    public virtual void CompleteReload()
    {
        if (!reloadable) { return; }
        primaryAmmo = primaryMaxAmmo[GetPrimaryAbilityLevel()];
        secondaryAmmo = secondaryMaxAmmo[GetSecondaryAbilityLevel()];
        reloadStartTime = float.MinValue;
    }
    public void Start()
    {
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
