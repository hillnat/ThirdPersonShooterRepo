using Photon.Pun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPunObservable
{
    //[Header("Components")]
    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;
    [Header("Movement")]
    private const float moveSpeed = 2250f;
    //Header("Footsteps")]
    public bool GetShouldPlayFootstep() { return (isGrounded || GetIsWallRunning()) && ((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z)) / 2) > 0.5f; }
    private float lastFootstepTime = 0f;
    private float footstepDelay = 0.31f;
    [Header("Camera")]
    public Vector3 cameraOffset = new Vector3(1f, 0.5f, -2f);
    private Camera myCamera;
    public Transform cameraParent;
    [SerializeField] private float baseFov = 90f;
    public bool isAiming = false;
    //[Header("Mouse Look")]
    private Vector2 mouseLookXY = Vector2.zero;
    public float mouseSensitivty = 25f;
    //[Header("Jumping")]
    private const float jumpForce = 1550f;
    public int jumps = 1;
    private float lastJumpTime = 0f;
    private const float jumpDelay = 0.5f;
    private const float fakeGravity = 3000f;
    public bool GetJumpDelayElapsed() { return GameManager.instance.time > lastJumpTime + jumpDelay; }
    public bool GetCanJump() { return GetJumpDelayElapsed() && jumps>0; }
    //[Header("Ground Checks")]
    private bool isGrounded = false;
    private const float groundedCheckDistance = 0.05f;
    private const float groundedCheckOriginOffset = -0.99f;
    private Vector3 currentGroundNormal = Vector3.zero;
    //[Header("Wall Running")]
    private bool isTouchingWallRight = false;
    private bool isTouchingWallLeft = false;
    private bool GetIsTouchingWall() { return isTouchingWallLeft || isTouchingWallRight; }
    private bool isWallRunningRight = false;
    private bool isWallRunningLeft = false;
    private bool GetIsWallRunning() { return isWallRunningLeft || isWallRunningRight; }
    //[Header("Wall Jumping")]
    private const float wallJumpCheckDistance = 1f;
    private const float wallJumpCheckOriginOffset = 0.1f;
    private bool GetCanWallJump() { return GetIsTouchingWall() && GetWallJumpDelayElapsed() && ((InputManager.instance.wasdInputs.x > 0 && isTouchingWallLeft) || (InputManager.instance.wasdInputs.x < 0 && isTouchingWallRight)); }
    private const float wallJumpForce = 1200f;
    private float lastWallJumpTime = float.MinValue;
    private const float wallJumpDelay = 1f;
    private bool GetWallJumpDelayElapsed() { return GameManager.instance.time > lastWallJumpTime + wallJumpDelay; }
    //[Header("Mantling")]
    private bool hasMantlePoint = false;
    private bool canMantle = true;
    private const float mantleForce = 1450f;
    //[Header("Inventory")]
    public List<Weapon> allWeapons = new List<Weapon>();
    [SerializeField] private int heldItem = -1;
    [SerializeField] private EWeapons heldItemEnum = (EWeapons)int.MaxValue;
    private bool GetIsChangingWeapon() { return GameManager.instance.time < lastChangeWeaponTime + 0.2f; }
    public List<Weapon> currentWeapons = new List<Weapon>();
    public int maxWeapons = 2;
    public bool GetIsInventoryFull() {return currentWeapons.Count >= maxWeapons; }
    [Header("UI")]
    public TMP_Text ammoText;
    public Image reloadIndicator;
    public TMP_Text healthText;
    public TMP_Text kdaText;
    public TMP_Text speedText;
    [Header("Audio")]
    public AudioClip jumpAudio;
    public AudioClip jump2Audio;
    public AudioClip painSounds;
    public string footstepSoundsPath = "Player/Footsteps/";
    private float lastPainSoundTime = 0f;
    [SerializeField]private AudioClip[] footsteps;
    //[Header("Networking")]
    private bool isMine = false;
    public PhotonView myView;
    public List<GameObject> destroyIfNotLocal = new List<GameObject>();
    private int lastHitByViewId = int.MinValue;    
    //[Header("Stats")]
    public float health = 100;
    public int kills = 0;
    public int deaths = 0;
    public bool GetIsDead() { return health <= 0; }
    public float lastDeathTime = 0f;
    //[Header("Collider")]
    private float baseColliderHeight = 0f;
    private float lastChangeWeaponTime = float.MinValue;
    public Transform headPoint;
    //[Header("Shooting")]
    public List<Projectile> ownedProjectiles;
    public Transform muzzlePoint;

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

    #region Weapon
    public Weapon? GetWeapon()
    {
        if (isMine)
        {
            if (currentWeapons.Count == 0) { return null; }
            if (heldItem >= currentWeapons.Count) { return null; }
            if (heldItem < 0) { return null; }
        }
        else if ((int)heldItemEnum == int.MaxValue)
        {
            return null;
        }

        if (isMine)
        {
            return currentWeapons[heldItem];
        }
        else
        {
            return allWeapons[(int)heldItemEnum];
        }
    }
    public void AddWeapon(EWeapons weapon)
    {
        Weapon newWeapon = allWeapons[(int)weapon];
        if (!currentWeapons.Contains(newWeapon))
        {
            currentWeapons.Add(newWeapon);
            Debug.Log($"Added weapon {newWeapon.name} / {weapon}");
            newWeapon.ResetWeapon();
        }
        RefreshWeaponMeshes();
    }
    public void RemoveWeapon(EWeapons weapon)
    {
        Weapon newWeapon = allWeapons[(int)weapon];
        if (currentWeapons.Contains(newWeapon))
        {
            currentWeapons.Remove(newWeapon);
            Debug.Log($"Removed weapon {newWeapon.name} / {weapon}");
        }
        RefreshWeaponMeshes();
    }
    public bool HasWeapon(EWeapons weapon)
    {
        return currentWeapons.Contains(allWeapons[(int)weapon]);
    }
    private void ChangeWeapon(int newHeldItem)
    {
        if(currentWeapons.Count == 0){
            heldItem = -1;
            heldItemEnum = (EWeapons)int.MaxValue;
            return; 
        }
        lastChangeWeaponTime = GameManager.instance.time;
        Weapon currentWeapon = GetWeapon();
        if (currentWeapon != null && currentWeapon.GetIsReloading()) { currentWeapon.CancelReload(); }

        heldItem = Mathf.Clamp(newHeldItem, 0, currentWeapons.Count - 1);

        heldItemEnum = GetWeapon().weaponType;
        
        RefreshWeaponMeshes();
    }
    public EWeapons GetHeldItemIndex() { Weapon weapon = GetWeapon(); if (weapon != null) { return weapon.weaponType; } else { return (EWeapons)int.MaxValue; } }
    public void RefreshWeaponMeshes()
    {
        if (isMine)
        {
            if (currentWeapons.Count == 0)
            {
                heldItem = -1;
                heldItemEnum = (EWeapons)int.MaxValue;
                return;
            }
        }
        
        ToggleAllWeaponMeshes(false);
        if (GetWeapon() != null) { GetWeapon().gameObject.SetActive(true); }
    }
    public void ToggleAllWeaponMeshes(bool state)
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            allWeapons[i].gameObject.SetActive(state);
        }
    }
    private void ResetAllWeapons()
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            allWeapons[i].ResetWeapon();
        }
    }

    #endregion
    #region Networking
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (isMine)
        {
            stream.SendNext((int)heldItemEnum);
            stream.SendNext(cameraParent.transform.eulerAngles);
            stream.SendNext(health);
        }
        else
        {
            int temp = (int)heldItemEnum;
            heldItemEnum = (EWeapons)((int)stream.ReceiveNext());
            if (temp != (int)heldItemEnum)
            {
                RefreshWeaponMeshes();
            }
            cameraParent.transform.eulerAngles = (Vector3)stream.ReceiveNext();
            health = (float)stream.ReceiveNext();
        }
    }
    #endregion
    #region Unity Callbacks
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        myView = GetComponent<PhotonView>();
        col = GetComponent<CapsuleCollider>();
        baseColliderHeight=col.height;
        isMine = myView.IsMine;
        currentWeapons.Clear();
        if (isMine)
        {
            myCamera = Camera.main;
            myCamera.transform.parent = cameraParent;
            myCamera.transform.localPosition = cameraOffset;
            myCamera.fieldOfView = baseFov;
        }
        else
        {
            for (int i = 0; i < destroyIfNotLocal.Count; i++)
            {
                Destroy(destroyIfNotLocal[i].gameObject);
            }
        }
    }
    void Start()
    {
        if (isMine) {
            health = 100;
            
            RefreshKdaText();
            ResetAllWeapons();
            mouseSensitivty = SettingsManager.instance.settingsFile.sensitivity;
            GameManager.instance.view.RPC(nameof(GameManager.RPC_RefreshPlayerList), RpcTarget.All);//Tell GM to update player list once we have spawned
            footsteps = Resources.LoadAll<AudioClip>("Audio/" + footstepSoundsPath);
            username = SettingsManager.instance.settingsFile.username;

            gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
            headPoint.gameObject.layer = LayerMask.NameToLayer("LocalPlayer");
        }
        RefreshWeaponMeshes();
        ToggleAllWeaponMeshes(false);
    }

    void Update()
    {
        if (isMine)
        {
            HandleUI();
            HandleCamera();
            HandleAiming();
            
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
    private void FixedUpdate()
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
            Weapon weapon = GetWeapon();
            ammoText.text = $"{weapon.ammo}";

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
        else { ammoText.text = ""; }


        healthText.text = Mathf.CeilToInt(health) + "";
        healthText.color = Color.Lerp(Color.red, Color.green, health / 100f);

        speedText.text = $"{((Mathf.Abs(rb.velocity.x)+Mathf.Abs(rb.velocity.z))/2f):F1}mps";
    }
    public void RefreshKdaText()
    {
        kdaText.text = $"K:{kills} D:{deaths}";
    }
    #endregion
    #region Movement & Physics
    private void HandleFakeGravity()
    {
        if (!isGrounded & !GetIsWallRunning()) { rb.AddForce(Vector3.down * fakeGravity * Time.fixedDeltaTime); }
    }
    private void HandleMovement()
    {
        if (InputManager.instance.wasdInputs != Vector2.zero && !BuyMenu.instance.isMenuOpen)
        {
            float aimMoveSpeedMod = (isAiming ? GetWeapon().aimingMoveSpeedModifier : 1f);
            Vector3 forwardVector = transform.forward * InputManager.instance.wasdInputs.y * moveSpeed * (GetIsWallRunning() ? 1.5f : 1f) * aimMoveSpeedMod;
            Vector3 rightVector = transform.right * InputManager.instance.wasdInputs.x * moveSpeed * (GetIsWallRunning() ? 0f : 1f) * aimMoveSpeedMod;
            Vector3 movementVector = Vector3.ProjectOnPlane((forwardVector + rightVector), currentGroundNormal);
            rb.AddForce(movementVector*Time.fixedDeltaTime);
            //Directional inputs are already normalized
        }
    }
    private void HandleGroundAndWallChecks()
    {
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, groundedCheckOriginOffset, 0), Vector3.down, out RaycastHit groundHit, groundedCheckDistance);
        isTouchingWallRight = Physics.Raycast(transform.position + transform.right * wallJumpCheckOriginOffset, transform.right, wallJumpCheckDistance);
        isTouchingWallLeft = Physics.Raycast(transform.position + transform.right * -wallJumpCheckOriginOffset, -transform.right, wallJumpCheckDistance);
        hasMantlePoint = Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit mantleHit, 1f);

        isWallRunningRight = isTouchingWallRight && !isGrounded && InputManager.instance.wasdInputs.x > 0;//We are wall running if we arent grounded, are touching wall, and trying to move into the wall
        isWallRunningLeft = isTouchingWallLeft && !isGrounded && InputManager.instance.wasdInputs.x < 0;

        //if (!GetCanWallJump() && isGrounded) { GetCanWallJump() = true; }
        //if (!GetCanWallJump() && GetIsTouchingWall()) { GetCanWallJump() = true; }
        if (!canMantle && isGrounded) { canMantle = true; }
        if (isGrounded) { currentGroundNormal = groundHit.normal; }
        if (isGrounded && GetJumpDelayElapsed())
        {
            jumps = 1;
        }
        else { currentGroundNormal = Vector3.zero; }

        Debug.DrawRay(transform.position + new Vector3(0, groundedCheckOriginOffset, 0), Vector3.down * groundedCheckDistance, isGrounded ? Color.green : Color.red);
        Debug.DrawRay(transform.position + transform.right * wallJumpCheckOriginOffset, transform.right * wallJumpCheckDistance, GetIsTouchingWall() ? Color.green : Color.red);
        Debug.DrawRay(transform.position + transform.right * -wallJumpCheckOriginOffset, transform.right * -wallJumpCheckDistance, GetIsTouchingWall() ? Color.green : Color.red);
        Debug.DrawRay(transform.position, transform.forward, hasMantlePoint ? Color.green : Color.red);
    }

    #endregion
    #region Input
    private void HandleInputs()
    {
        if (InputManager.instance.jump && !BuyMenu.instance.isMenuOpen)
        {
            /*if (hasMantlePoint && canMantle)
            {
                AudioManager.instance.PlaySound(true, jump2Audio, Vector3.zero, 0.15f, 0.9f,myView.ViewID);
                canMantle = false;
                rb.AddForce(Vector3.up * mantleForce);
                anim.SetTrigger("Mantle");
                //Debug.Log("mantle");
            }
            else */if (GetIsTouchingWall() && GetCanWallJump())
            {
                lastWallJumpTime = GameManager.instance.time;
                //rb.AddForce(transform.forward * InputManager.instance.wasdInputs.y * wallJumpForce * 0.2f);
                rb.AddForce(transform.right * InputManager.instance.wasdInputs.x * wallJumpForce);

                rb.AddForce(Vector3.up * jumpForce);

                //Debug.Log("walljump");
                anim.SetTrigger("WallJump");
                AudioManager.instance.PlaySound(true, jump2Audio, Vector3.zero, 0.15f,0.9f, myView.ViewID);

            }
            else if (GetCanJump())
            {
                jumps--;
                rb.AddForce(Vector3.up * jumpForce);
                AudioManager.instance.PlaySound(true, jumpAudio, Vector3.zero, 0.15f, 0.9f,myView.ViewID);
                anim.SetTrigger("Jump");
            }
        }
        if (InputManager.instance.scrollDelta != Vector2.zero)//change  item
        {
            ChangeWeapon(heldItem + (int)InputManager.instance.scrollDelta.y);
        }
        if(GetWeapon() && !GetIsChangingWeapon())
        {
            if (BuyMenu.instance.isMenuOpen) { return; }
            Weapon weapon = GetWeapon();
            if (weapon.GetCanFire())
            {
                if (weapon.isFullAuto)
                {
                    if (InputManager.instance.mouse1Hold)
                    {
                        anim.SetTrigger("Fire");
                        weapon.Fire(myCamera.transform.position, myCamera.transform.forward, muzzlePoint.transform.position);
                    }
                }
                else
                {
                    if (InputManager.instance.mouse1)
                    {
                        anim.SetTrigger("Fire");
                        weapon.Fire(myCamera.transform.position, myCamera.transform.forward, muzzlePoint.transform.position);
                    }
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
        anim.SetBool("WallRunningRight", isWallRunningRight);
        anim.SetBool("WallRunningLeft", isWallRunningLeft);
        anim.SetBool("Dead", GetIsDead());
        if (GetWeapon() != null) { 
            anim.SetBool("Reloading", GetWeapon().GetIsReloading());
            anim.SetInteger("HeldItem", (int)GetWeapon().weaponType+1);
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

        if (!BuyMenu.instance.isMenuOpen && !SettingsManager.instance.settingsOpen && InputManager.instance.mouseDelta != Vector2.zero)//Apply mouse rotations
        {
            AddMouseLook(new Vector2(InputManager.instance.mouseDelta.y * Time.deltaTime * mouseSensitivty, InputManager.instance.mouseDelta.x * Time.deltaTime * mouseSensitivty));
        }  
        if (Physics.Raycast(cameraParent.transform.position, cameraParent.transform.TransformVector(cameraOffset), out RaycastHit camHit, cameraOffset.magnitude))//Wall check for camera
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
        Weapon weapon = GetWeapon();
        isAiming = weapon!=null&&InputManager.instance.mouse2Hold;
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, isAiming ? weapon.zoomFov : baseFov, Time.deltaTime * 30f);
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
            AudioManager.instance.PlaySound(true, painSounds, Vector3.zero, 0.7f, UnityEngine.Random.Range(0.9f,1.05f), myView.ViewID);
        }
        if(health<=0) { Die(); }
    }
    [PunRPC]
    public void RPC_AddKills(int delta)
    {
        kills += delta;
        if (isMine)
        {
            RefreshKdaText();
        }
    }
    [PunRPC]
    public void RPC_AddDeaths(int delta)
    {
        deaths += delta;
        if (isMine)
        {
            RefreshKdaText();
        }
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
            AudioManager.instance.PlaySound(true, footsteps[UnityEngine.Random.Range(0,footsteps.Length)], transform.position-Vector3.up, 0.075f, 1f, int.MinValue);
        }
    }
    #endregion
    #region Projectiles
    public void SpawnProjectile(string preafabPath, Vector3 position, Quaternion rotation, Vector3 targetPosition)
    {
        GameObject go = PhotonNetwork.Instantiate(preafabPath, position, rotation);
        Projectile newProjectile = go.GetComponent<Projectile>();
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
        col.height = 0.1f;
    }
    public void Respawn()
    {
        ResetAllWeapons();
        health = 100;
        col.height = baseColliderHeight;
        transform.position = GameManager.instance.GetRandomSpawn();
        lastHitByViewId = int.MinValue;
    }
    #endregion

    private void OnDrawGizmos()
    {
    }
}
