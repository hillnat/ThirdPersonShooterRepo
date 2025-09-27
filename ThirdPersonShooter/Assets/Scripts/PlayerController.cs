using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static StatusEffectBase;
public enum ETeams { None, Red, Blue, Any}
public abstract class PlayerController : MonoBehaviour, IPunObservable
{
    //[Header("Components")]
    public Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;
    [HideInInspector]public PhotonView myView;
    private Camera myCamera;
    [HideInInspector]public List<ProjectileBase> ownedProjectiles;
    private Transform muzzlePoint;
    private Transform headPoint;
    private Transform cameraParent;
    private Transform playerHudUi;
    public WeaponBase[] allItems;
    public ItemMesh[] allItemMeshes;
    private ETeams myTeam = ETeams.Any;
    private int heldItem = 0;

    private Vector2 mouseLookXY = Vector2.zero;

    public List<StatusEffectBase> currentStatusEffects = new List<StatusEffectBase>();

    public abstract float moveSpeed {get;}
    public abstract float jumpForce { get;}
    public abstract int maxItems { get;}
    public abstract string characterNameInFile { get;}

    //Header("Footsteps")]
    private const float footstepDelay = 0.31f;
    private Vector3 cameraOffset = new Vector3(0.5f, 0.5f, -1f);
    private const float baseFov = 90f;
    

    [HideInInspector]public float mouseSensitivty = 25f;
    private int jumps = 1;


    private float lastFootstepTime = 0f;
    private float lastJumpTime = 0f;
    private float lastDeathTime = 0f;

    private const float jumpDelay = 0.5f;
    private const float fakeGravity = 3000f;
    private const float groundedCheckDistance = 0.05f;
    private const float groundedCheckOriginOffset = -0.99f;


    [HideInInspector]public bool isAiming = false;
    private bool isGrounded = false;

    //[Header("Ground Checks")]

    private Vector3 currentGroundNormal = Vector3.zero;
    /*//[Header("Wall Running")]
    private bool isTouchingWallRight = false;
    private bool isTouchingWallLeft = false;
    private bool isWallRunningRight = false;
    private bool isWallRunningLeft = false;
    //[Header("Wall Jumping")]
    /*private const float wallJumpCheckDistance = 1f;
    private const float wallJumpCheckOriginOffset = 0.1f;
    private const float wallJumpForce = 1200f;
    private float lastWallJumpTime = float.MinValue;
    private const float wallJumpDelay = 1f;*/

    //[Header("Inventory")]

    private TMP_Text primaryAmmoText;
    private TMP_Text secondaryAmmoText;
    private Image reloadIndicator;
    private TMP_Text healthText;

    private LayerMask cameraCollisionLayerMask = new LayerMask();


    private string painSoundsPath => $"Audio/Characters/{characterNameInFile}/Pain";
    private string jumpSoundsPath => $"Audio/Characters/{characterNameInFile}/Jump";
    private string footstepSoundsPath => $"Audio/Characters/Player/Footsteps";
    private float lastPainSoundTime = 0f;
    private AudioClip[] footstepSounds;
    private AudioClip[] jumpSounds;
    private AudioClip[] painSounds;
    //[Header("Networking")]
    private bool isMine = false;
    private int lastHitByViewId = int.MinValue;
    //[Header("Stats")]
    private float health = 100;
    private int kills = 0;
    private int deaths = 0;
    //[Header("Collider")]
    private float baseColliderHeight = 0f;
    private float lastChangeWeaponTime = float.MinValue;
    private float moveSpeedModifierFromStatusEffects = 1f;
    //[Header("Shooting")]
    private Transform GetChildWithTag(Transform t, string tag)
    {
        if (t.gameObject.tag == tag) { return t; }
        else
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Transform result = GetChildWithTag(t.GetChild(i), tag);
                if (result != null)//Only teminate from base branch
                {
                    return result;
                }
            }
        }
        return null;//Base branch termination
    }
    private Transform[] GetAllChildrenWithTag(Transform t, string tag)
    {
        List<Transform> found = new List<Transform>();
        if (t.gameObject.tag == tag) { found.Add(t); }
        for (int i = 0; i < t.childCount; i++)
        {
            Transform[] result = GetAllChildrenWithTag(t.GetChild(i), tag);
            found.AddRange(result);
        }
        return found.ToArray();//Base branch termination
    }

    #region Username
    public string username
    {
        get { return _username; }
        set { _username = value; if (myView != null) { myView.RPC(nameof(SyncUsername), RpcTarget.AllBufferedViaServer, value); } }
    }
    private string _username="";
    [PunRPC]
    public void SyncUsername(string newUsername)
    {
        _username = newUsername;//Make sure to use _username to avoid RPC loop
    }
    #endregion
    #region Getters
    public bool GetShouldPlayFootstep() { return (isGrounded) && ((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z)) / 2) > 0.5f; }
    public bool GetJumpDelayElapsed() { return GameManager.instance.time > lastJumpTime + jumpDelay; }
    public bool GetCanJump() { return GetJumpDelayElapsed() && jumps > 0; }
    
    private bool GetIsChangingWeapon() { return GameManager.instance.time < lastChangeWeaponTime + 0.2f; }
    public bool GetIsDead() { return health <= 0; }
    #endregion
    #region Weapon
    public WeaponBase? GetWeapon()
    {
        if(heldItem>=allItems.Length){return null;}
        return allItems[heldItem];
    }
    /*public void AddWeapon(EWeapons weapon)
    {
        Weapon newWeapon = allItems[(int)weapon];
        if (!currentItems.Contains(newWeapon))
        {
            currentItems.Add(newWeapon);
            Debug.Log($"Added weapon {newWeapon.name} / {weapon}");
            newWeapon.ResetWeapon();
        }
        RefreshWeaponMeshes();
    }
    public void RemoveWeapon(EWeapons weapon)
    {
        Weapon newWeapon = allItems[(int)weapon];
        if (currentItems.Contains(newWeapon))
        {
            currentItems.Remove(newWeapon);
            Debug.Log($"Removed weapon {newWeapon.name} / {weapon}");
        }
        RefreshWeaponMeshes();
    }
    public bool HasWeapon(EWeapons weapon)
    {
        return currentItems.Contains(allItems[(int)weapon]);
    }*/
    private void ChangeWeapon(int newHeldItem)
    {
        lastChangeWeaponTime = GameManager.instance.time;
        WeaponBase currentWeapon = GetWeapon();
        if (currentWeapon != null && currentWeapon.GetIsReloading()) { currentWeapon.CancelReload(); }

        heldItem = Mathf.Clamp(newHeldItem, 0, allItems.Length - 1);

        //heldItemEnum = GetWeapon().weaponType;
        
        RefreshWeaponMeshes();
    }
    public void RefreshWeaponMeshes()
    {        
        ToggleAllWeaponMeshes(false);
        if (GetWeapon() != null) {
            for (int i = 0; i < allItemMeshes.Length; i++)
            {
                if (GetWeapon().indexInAnimator == allItemMeshes[i].indexInAnimator) { allItemMeshes[i].gameObject.SetActive(true); }
            }
        }      
    }
    public void ToggleAllWeaponMeshes(bool state)
    {
        for (int i = 0; i < allItemMeshes.Length; i++)
        {
            allItemMeshes[i].gameObject.SetActive(state);
        }
    }
    private void ResetAllWeapons()
    {
        for (int i = 0; i < allItems.Length; i++)
        {
            allItems[i].ResetWeapon();
        }
    }

    #endregion
    #region Networking
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (isMine)
        {
            stream.SendNext(cameraParent.transform.eulerAngles);
            stream.SendNext(health);
        }
        else
        {
            cameraParent.transform.eulerAngles = (Vector3)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
        }
    }
    #endregion   
    #region Unity Callbacks
    public void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        myView = GetComponent<PhotonView>();
        col = GetComponent<CapsuleCollider>();


        muzzlePoint = GetChildWithTag(transform, "MuzzlePoint");
        cameraParent = GetChildWithTag(transform, "CameraParent");
        headPoint = GetChildWithTag(transform, "HeadPoint");
        playerHudUi = GetChildWithTag(transform, "PlayerHudUi");
        primaryAmmoText = GetChildWithTag(transform, "PlayerHud_PrimaryAmmoText").GetComponent<TMP_Text>();
        secondaryAmmoText = GetChildWithTag(transform, "PlayerHud_SecondaryAmmoText").GetComponent<TMP_Text>();
        healthText = GetChildWithTag(transform, "PlayerHud_HealthText").GetComponent<TMP_Text>();
        reloadIndicator = GetChildWithTag(transform, "PlayerHud_ReloadIndicator").GetComponent<Image>();
        baseColliderHeight = col.height;
        isMine = myView.IsMine;
        if (isMine)
        {
            myCamera = Camera.main;
            myCamera.transform.parent = cameraParent;
            myCamera.transform.localPosition = cameraOffset;
            myCamera.fieldOfView = baseFov;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.drag = 4f;
            rb.angularDrag = 4f;
            rb.mass = 1f;
            footstepSounds = Resources.LoadAll<AudioClip>(footstepSoundsPath);
            painSounds = Resources.LoadAll<AudioClip>(painSoundsPath);
            jumpSounds = Resources.LoadAll<AudioClip>(jumpSoundsPath);
            gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
            headPoint.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
        }
        else
        {
            Destroy(rb);
            Destroy(playerHudUi.gameObject);
        }
        
    }
    public void Start()
    {
        allItemMeshes = GetComponentsInChildren<ItemMesh>();

        if (isMine) {
            health = 100;
            
            ResetAllWeapons();
            mouseSensitivty = SettingsManager.instance.settingsFile.sensitivity;
            GameManager.instance.view.RPC(nameof(GameManager.RPC_RefreshPlayerList), RpcTarget.All);//Tell GM to update player list once we have spawned
            
            username = SettingsManager.instance.settingsFile.username;

            string[] masks = new string[1] { "Default" };
            cameraCollisionLayerMask = LayerMask.GetMask(masks);
        }
        RefreshWeaponMeshes();
    }

    public void Update()
    {
        if (isMine)
        {
            HandleUI();
            HandleCamera();
            HandleAiming();
            HandleStatusEffects();
            if (!GetIsDead())
            {
                if (!SettingsManager.instance.settingsOpen)
                {
                    HandleInputs();
                }
                
                HandleFootsteps();
            }
            else
            {
                if (GameManager.instance.time > lastDeathTime + 3f)
                {
                    Respawn();
                }
            }
            HandleGroundAndWallChecks();
        }
    }
    public void FixedUpdate()
    {
        if (isMine)
        {
            if (!GetIsDead())
            {
                HandleMovement();
            }
            HandleFakeGravity();
            HandleAnimatorStates();
        }
    }
    #endregion
    #region UI
    private void HandleUI()
    {
        if (GetWeapon() != null)
        {
            WeaponBase weapon = GetWeapon();
            primaryAmmoText.text = $"{weapon.primaryAmmo}";
            secondaryAmmoText.text = $"{weapon.secondaryAmmo}";

            if (weapon.GetIsReloading())
            {
                if(!reloadIndicator.enabled){reloadIndicator.enabled = true;}
                reloadIndicator.fillAmount = Mathf.Clamp01(1f-Mathf.InverseLerp(weapon.reloadStartTime, weapon.reloadStartTime + weapon.reloadDelay, GameManager.instance.time));
            }
            else
            {
                reloadIndicator.enabled = false;
            }
            if (weapon.currentRecoil != Vector2.zero) { AddMouseLook(GetWeapon().currentRecoil); }
        }
        else { primaryAmmoText.text = ""; secondaryAmmoText.text = ""; }


        healthText.text = Mathf.CeilToInt(health) + "";
        healthText.color = Color.Lerp(Color.red, Color.green, health / 100f);
    }

    #endregion
    #region Movement & Physics
    private void HandleFakeGravity()
    {
        if (!isGrounded) { rb.AddForce(Vector3.down * fakeGravity * Time.fixedDeltaTime); }
    }
    private void HandleMovement()
    {
        if (InputManager.instance.wasdInputs != Vector2.zero)
        {
            float aimMoveSpeedMod = (isAiming ? GetWeapon().aimingMoveSpeedModifier : 1f);
            Vector3 forwardVector = transform.forward * InputManager.instance.wasdInputs.y;
            Vector3 rightVector = transform.right * InputManager.instance.wasdInputs.x;
            Vector3 movementVector = Vector3.ProjectOnPlane((forwardVector + rightVector), currentGroundNormal);
            rb.AddForce(movementVector*Time.fixedDeltaTime* moveSpeedModifierFromStatusEffects* aimMoveSpeedMod * moveSpeed);
        }
    }
    private void HandleGroundAndWallChecks()
    {
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, groundedCheckOriginOffset, 0), Vector3.down, out RaycastHit groundHit, groundedCheckDistance);
        
        if (isGrounded) { currentGroundNormal = groundHit.normal; }
        if (isGrounded && GetJumpDelayElapsed())
        {
            jumps = 1;
        }
        else { currentGroundNormal = Vector3.zero; }

        Debug.DrawRay(transform.position + new Vector3(0, groundedCheckOriginOffset, 0), Vector3.down * groundedCheckDistance, isGrounded ? Color.green : Color.red);
    }

    #endregion
    #region Input
    private void HandleInputs()
    {
        if (InputManager.instance.jump)
        {
            if (GetCanJump())
            {
                jumps=0;
                rb.AddForce(Vector3.up * jumpForce);
                AudioManager.instance.PlaySound(true, jumpSounds[UnityEngine.Random.Range(0,jumpSounds.Length)], Vector3.zero, 0.15f, 0.9f,myView.ViewID);
                anim.SetTrigger("Jump");
            }
        }
        if (InputManager.instance.scrollDelta != Vector2.zero)//change  item
        {
            ChangeWeapon(heldItem + (int)InputManager.instance.scrollDelta.y);
        }
        if(GetWeapon() && !GetIsChangingWeapon())
        {
            WeaponBase weapon = GetWeapon();
            if (((!GetWeapon().isFullAuto && InputManager.instance.mouse1) || (GetWeapon().isFullAuto && InputManager.instance.mouse1Hold)) && GetWeapon().GetCanPrimaryFire())
            {
                if (weapon.DoPrimaryAction(myCamera.transform.position, myCamera.transform.forward, muzzlePoint.transform.position))
                {
                    anim.SetTrigger("Fire");
                }
            }
            if ((!GetWeapon().isFullAuto && InputManager.instance.mouse2) || (GetWeapon().isFullAuto && InputManager.instance.mouse2Hold) && GetWeapon().GetCanSecondaryFire())
            {
                if (weapon.DoSecondaryAction(myCamera.transform.position, myCamera.transform.forward, muzzlePoint.transform.position))
                {
                    anim.SetTrigger("Fire");
                }
            }
            if (InputManager.instance.reload && weapon.GetCanReload())
            {
                weapon.StartReloading();
            }
        }
        
        if (InputManager.instance.alpha1) { ChangeWeapon(0); }
        if (InputManager.instance.alpha2) { ChangeWeapon(1); }
        if (InputManager.instance.alpha3) { ChangeWeapon(2); }
        if (InputManager.instance.alpha4) { ChangeWeapon(3); }
        if (InputManager.instance.alpha5) { ChangeWeapon(4); }
        if (InputManager.instance.alpha6) { ChangeWeapon(5); }
        if (InputManager.instance.alpha7) { ChangeWeapon(6); }
        if (InputManager.instance.alpha8) { ChangeWeapon(7); }
        if (InputManager.instance.alpha9) { ChangeWeapon(8); }
        if (InputManager.instance.alpha0) { ChangeWeapon(9); }
    }
    #endregion
    #region Animation
    private void HandleAnimatorStates()
    {
        anim.SetInteger("MovementX", Mathf.RoundToInt(InputManager.instance.wasdInputs.x));
        anim.SetInteger("MovementZ", Mathf.RoundToInt(InputManager.instance.wasdInputs.y));
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("Aiming", isAiming);
        anim.SetBool("Dead", GetIsDead());
        if (GetWeapon() != null) { 
            anim.SetBool("Reloading", GetWeapon().GetIsReloading());
            anim.SetInteger("HeldItem", (int)GetWeapon().indexInAnimator);
        }
        else { 
            anim.SetBool("Reloading", false);
            anim.SetInteger("HeldItem", 0);
        }
    }
    #endregion
    #region Camera & Aiming
    private void HandleCamera()
    {
        cameraParent.transform.localEulerAngles = new Vector3(mouseLookXY.x, 0, 0);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, mouseLookXY.y, transform.eulerAngles.z);

        if (!SettingsManager.instance.settingsOpen && InputManager.instance.mouseDelta != Vector2.zero)//Apply mouse rotations
        {
            AddMouseLook(new Vector2(InputManager.instance.mouseDelta.y * Time.deltaTime * mouseSensitivty, InputManager.instance.mouseDelta.x * Time.deltaTime * mouseSensitivty));
        }  
        if (Physics.Raycast(cameraParent.transform.position, cameraParent.transform.TransformVector(cameraOffset), out RaycastHit camHit, cameraOffset.magnitude, cameraCollisionLayerMask))//Wall check for camera
        {
            myCamera.transform.position = camHit.point;
        }
        else
        {
            myCamera.transform.localPosition = cameraOffset;
        }
        Debug.DrawRay(cameraParent.transform.position, cameraParent.transform.TransformVector(cameraOffset) * cameraOffset.magnitude, Color.white);
    }
    public void AddMouseLook(Vector2 xy)
    {
        mouseLookXY.x -= xy.x;
        mouseLookXY.y += xy.y;

        mouseLookXY.x = Mathf.Clamp(mouseLookXY.x, -90f, 90f);
        if (mouseLookXY.y > 360f) { mouseLookXY.y -= 360f; }
        if (mouseLookXY.y < -360f) { mouseLookXY.y += 360f; }
    }
    
    private void HandleAiming()
    {
        /*Weapon weapon = GetWeapon();
        isAiming = weapon!=null&&InputManager.instance.mouse2Hold;
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, isAiming ? weapon.zoomFov : baseFov, Time.deltaTime * 30f);*/
    }
    #endregion
    #region RPCs
    [PunRPC]
    public void RPC_ChangeHealth(float delta)
    {
        if (!isMine || GetIsDead()) { return; }
        health += delta;
        if (delta < 0 && GameManager.instance.time>lastPainSoundTime+0.1f)
        {
            lastPainSoundTime = GameManager.instance.time;
            AudioManager.instance.PlaySound(true, painSounds[UnityEngine.Random.Range(0, painSounds.Length)], Vector3.zero, 0.7f, UnityEngine.Random.Range(0.9f,1.05f), myView.ViewID);
        }
        if(health<=0) { Die(); }
    }
    [PunRPC]
    public void RPC_AddKills(int delta)
    {
        kills += delta;
    }
    [PunRPC]
    public void RPC_AddDeaths(int delta)
    {
        deaths += delta;
    }
    [PunRPC]
    public void RPC_SetLastHitBy(int viewId)
    {
        if(!isMine) { return; }
        lastHitByViewId = viewId;
    }
    #endregion
    #region Footsteps
    private void HandleFootsteps()
    {
        if (GetShouldPlayFootstep() && GameManager.instance.time > lastFootstepTime + footstepDelay)
        {
            lastFootstepTime = GameManager.instance.time;
            AudioManager.instance.PlaySound(true, footstepSounds[UnityEngine.Random.Range(0,footstepSounds.Length)], transform.position-Vector3.up, 0.075f, 1f, int.MinValue);
        }
    }
    #endregion
    #region Projectiles
    public void SpawnProjectile(string prefabPath, Vector3 position, Quaternion rotation, Vector3 targetPosition)
    {
        GameObject go = PhotonNetwork.Instantiate(prefabPath, position, rotation);
        ProjectileBase newProjectile = go.GetComponent<ProjectileBase>();
        newProjectile.owningPc = this;
        newProjectile.targetPosition = targetPosition;
        ownedProjectiles.Add(newProjectile);
    }
    #endregion
    #region Death & Respawn
    public void Die()
    {
        myView.RPC(nameof(RPC_AddDeaths), RpcTarget.AllBuffered, 1);//Add death
        if (lastHitByViewId != int.MinValue)
        {
            PhotonView.Find(lastHitByViewId).RPC(nameof(RPC_AddKills), RpcTarget.AllBuffered, 1);//Add kill to killer
        }
        KillfeedManager.instance.view.RPC(nameof(KillfeedManager.AddKillfeedElement), RpcTarget.All, lastHitByViewId, myView.ViewID);
        health = 0;
        lastDeathTime = GameManager.instance.time;
        ResetAllWeapons();
        ClearStatusEffects();
        col.height = 0.1f;
    }
    public void Respawn()
    {
        ClearStatusEffects();
        ResetAllWeapons();
        health = 100;
        col.height = baseColliderHeight;
        transform.position = GameManager.instance.GetRandomSpawn(myTeam);
        lastHitByViewId = int.MinValue;
    }
    #endregion
    #region Status Effects
    [PunRPC]
    public void RPC_AddStatusEffect(StatusEffectBase.EStatusEffects eStatusEffect, int uniqueID)
    {
        StatusEffectBase newEffect = StatusEffectBase.GetStatusEffectTypeFromEnum(eStatusEffect);
        newEffect.displayParticlesUniqueID = uniqueID;
        newEffect.startTime = GameManager.instance.time;
        currentStatusEffects.Add(newEffect);
        if (myView.IsMine)
        {
            ParticleManager.instance.PlayParticle(true, newEffect.GetRandomDisplayParticle(), transform.position, Quaternion.identity, myView.ViewID, uniqueID);
        }
    }
    public void HandleStatusEffects()
    {
        moveSpeedModifierFromStatusEffects = 1f;
        for (int i = 0; i < currentStatusEffects.Count; i++)
        {
            float time = GameManager.instance.time;
            StatusEffectBase effect = currentStatusEffects[i];
            if (time > effect.lifeTime + effect.startTime)
            {
                currentStatusEffects.RemoveAt(i);
                i--;
            }

            if (effect.GetType() == typeof(StatusEffectGravitonGrasp))
            {
                moveSpeedModifierFromStatusEffects = 0.1f;
            }
        }
    }
    private void ClearStatusEffects()
    {
        for (int i = 0; i < currentStatusEffects.Count; i++)
        {
            if (currentStatusEffects[i].displayParticlesUniqueID != int.MinValue)
            {
                ParticleManager.instance.DestroyParticleWithUniqueID(currentStatusEffects[i].displayParticlesUniqueID);
            }
        }
        currentStatusEffects.Clear();
    }
    [PunRPC]
    public void RPC_RemoveStatusEffect(StatusEffectBase.EStatusEffects eStatusEffect)
    {
        StatusEffectBase toRemove = StatusEffectBase.GetStatusEffectTypeFromEnum(eStatusEffect);

        for (int i = 0; i < currentStatusEffects.Count; i++)
        {
            StatusEffectBase currentEffect = currentStatusEffects[i];
            if (currentEffect.GetType() == toRemove.GetType())
            {
                if (currentStatusEffects[i].displayParticlesUniqueID != int.MinValue)
                {
                    ParticleManager.instance.DestroyParticleWithUniqueID(currentStatusEffects[i].displayParticlesUniqueID);
                }
                currentStatusEffects.RemoveAt(i);
                i--;
            }
        }
    }
    #endregion
}
