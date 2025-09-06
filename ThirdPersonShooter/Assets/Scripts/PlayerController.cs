using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPunObservable
{
    //[Header("Components")]
    private Rigidbody rb;
    private Animator anim;
    private CapsuleCollider col;
    //[Header("Movement")]
    private const float moveSpeed = 1700f;
    private float curMoveSpeedMod => (isWallRunning ? 2f : 1f);
    public float currentSpeed;
    //[Header("Footsteps")]
    public bool shouldPlayFootstep => (isGrounded || isWallRunning) && ((Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z)) / 2) > 0.5f;
    private float lastFootstepTime = 0f;
    private float footstepDelay = 0.35f;
    //[Header("Camera")]
    public Vector3 cameraOffset = new Vector3(1f, 0.5f, -2f);
    private Camera myCamera;
    public Transform cameraParent;
    [SerializeField] private float baseFov = 90f;
    [SerializeField] private float zoomFov = 60f;
    private bool isAiming = false;
    //[Header("Mouse Look")]
    private Vector2 mouseLookXY = Vector2.zero;
    public float mouseSensitivty = 25f;
    //[Header("Jumping")]
    private const float jumpForce = 1050f;
    private bool canJump = false;
    private float lastJumpTime = 0f;
    private const float jumpDelay = 1.25f;
    private const float fakeGravity = 1400f;
    //[Header("Ground Checks")]
    private bool isGrounded = false;
    private const float groundedCheckDistance = 0.25f;
    private const float groundedCheckOriginOffset = -0.95f;
    private Vector3 currentGroundNormal = Vector3.zero;
    //[Header("Wall Running")]
    private bool isTouchingWallRight = false;
    private bool isTouchingWallLeft = false;
    private bool isTouchingWall => isTouchingWallLeft || isTouchingWallRight;
    private bool isWallRunningRight = false;
    private bool isWallRunningLeft = false;
    private bool isWallRunning => isWallRunningLeft || isWallRunningRight;
    //[Header("Wall Jumping")]
    private const float wallJumpCheckDistance = 1f;
    private const float wallJumpCheckOriginOffset = 0.3f;
    private bool canWallJump => isTouchingWall && wallJumpDelayElapsed;
    private const float wallJumpForce = 1200f;
    private float lastWallJumpTime = float.MinValue;
    private const float wallJumpDelay = 2f;
    private bool wallJumpDelayElapsed => GameManager.instance.time > lastWallJumpTime + wallJumpDelay;
    //[Header("Mantling")]
    private bool hasMantlePoint = false;
    private bool canMantle = true;
    private const float mantleForce = 950f;
    //[Header("Inventory")]
    public List<Weapon> allWeapons = new List<Weapon>();
    [SerializeField] private int heldItem = 0;
    private bool isChangingWeapon => GameManager.instance.time < lastChangeWeaponTime + 0.2f;
    public List<Weapon> currentWeapons = new List<Weapon>();
    public int maxWeapons = 2;
    public bool isInventoryFull => currentWeapons.Count >= maxWeapons;
    [Header("UI")]
    public TMP_Text ammoText;
    public Image reloadIndicator;
    public TMP_Text healthText;
    public TMP_Text kdaText;
    [Header("Audio")]
    public AudioClip jumpAudio;
    public AudioClip jump2Audio;
    public AudioClip painSounds;
    public string footstepSoundsPath = "Player/Footsteps/";
    [SerializeField]private AudioClip[] footsteps;
    //[Header("Networking")]
    private bool isMine = false;
    public PhotonView myView;
    public List<GameObject> destroyIfNotLocal = new List<GameObject>();

    public Transform muzzlePoint;
    public float currentAccuracyModifier = 0f;

    public float health = 100;
    public int kills = 0;
    public int deaths = 0;
    public bool isDead => health <= 0;
    public float lastDeathTime = 0f;
    
    private float baseColliderHeight = 0f;
    private int lastHitByViewId = int.MinValue;
    private float lastPainSoundTime = 0f;
    private float lastChangeWeaponTime = float.MinValue;
    
    public Weapon? GetWeapon()
    {
        if(currentWeapons.Count==0) { return null; }
        if(heldItem==-1) { return null; }
        if(heldItem >= currentWeapons.Count) { return null; }
        return currentWeapons[heldItem];
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
        lastChangeWeaponTime = GameManager.instance.time;
        Weapon currentWeapon = GetWeapon();
        if (currentWeapon != null && currentWeapon.GetIsReloading()) { currentWeapon.CancelReload(); }
        heldItem = newHeldItem;
        heldItem = Mathf.Clamp(heldItem, 0, currentWeapons.Count - 1);
        RefreshWeaponMeshes();
    }
    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (isMine)
        {
            stream.SendNext(heldItem);
            stream.SendNext(cameraParent.transform.eulerAngles);
        }
        else
        {
            int temp = heldItem;
            heldItem = (int)stream.ReceiveNext();
            if (temp != heldItem)
            {
                RefreshWeaponMeshes();
            }
            cameraParent.transform.eulerAngles = (Vector3)stream.ReceiveNext();
        }
    }
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
        heldItem = 0;
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
        health = 100;
        RefreshWeaponMeshes();
        RefreshKdaText();
        ResetAllWeapons();
        mouseSensitivty = SettingsManager.instance.settingsFile.sensitivity;
        GameManager.instance.view.RPC(nameof(GameManager.RPC_RefreshPlayerList), RpcTarget.All);//Tell GM to update player list once we have spawned
        footsteps = Resources.LoadAll<AudioClip>("Audio/" + footstepSoundsPath);
    }

    void Update()
    {
        if (isMine)
        {
            HandleUI();
            HandleGroundAndWallChecks();
            HandleCamera();
            HandleZoom();
            
            if (!isDead)
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
        }
    }
    private void FixedUpdate()
    {
        if (isMine)
        {
            if (!isDead)
            {
                HandleMovement();
            }
            HandleFakeGravity();
            HandleAnimatorStates();
            HandleAccuracyModifier();
        }
    }
    #endregion
    private void HandleGroundAndWallChecks()
    {
        currentSpeed = rb.velocity.magnitude;
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, groundedCheckOriginOffset, 0), Vector3.down, out RaycastHit groundHit, groundedCheckDistance);
        isTouchingWallRight = Physics.Raycast(transform.position + transform.right * wallJumpCheckOriginOffset, transform.right, wallJumpCheckDistance);
        isTouchingWallLeft = Physics.Raycast(transform.position + transform.right * -wallJumpCheckOriginOffset, -transform.right, wallJumpCheckDistance);
        hasMantlePoint = Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit mantleHit, 1f);

        canJump = (GameManager.instance.time > lastJumpTime + jumpDelay) && isGrounded;
        isWallRunningRight = isTouchingWallRight && !isGrounded && InputManager.instance.wasdInputs.x > 0;//We are wall running if we arent grounded, are touching wall, and ttrying to move into the wall
        isWallRunningLeft = isTouchingWallLeft && !isGrounded && InputManager.instance.wasdInputs.x < 0;

        //if (!canWallJump && isGrounded) { canWallJump = true; }
        //if (!canWallJump && isTouchingWall) { canWallJump = true; }
        if (!canMantle && isGrounded) { canMantle = true; }
        if (isGrounded) { currentGroundNormal = groundHit.normal; }
        else { currentGroundNormal = Vector3.zero; }

        Debug.DrawRay(transform.position + new Vector3(0, -1f, 0), Vector3.down * groundedCheckDistance, Color.red);
        Debug.DrawRay(transform.position + transform.right * wallJumpCheckOriginOffset, transform.right * wallJumpCheckDistance, isTouchingWall ? Color.green : Color.red);
        Debug.DrawRay(transform.position + transform.right * -wallJumpCheckOriginOffset, transform.right * -wallJumpCheckDistance, isTouchingWall ? Color.green : Color.red);
        Debug.DrawRay(transform.position, transform.forward, hasMantlePoint ? Color.green : Color.red);
    }
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
    }
    public void RefreshKdaText()
    {
        kdaText.text = $"K:{kills} D:{deaths}";
    }
    private void HandleAccuracyModifier()
    {
        if (isGrounded)
        {
            currentAccuracyModifier = Mathf.Lerp(currentAccuracyModifier, 1f, Time.fixedDeltaTime * 5f);
            if (currentAccuracyModifier > 0.95f)
            {
                currentAccuracyModifier = 1f;
            }
        }
        else
        {
            currentAccuracyModifier = Mathf.Lerp(currentAccuracyModifier, 0f, Time.fixedDeltaTime * 5f);
            if (currentAccuracyModifier < 0.05f)
            {
                currentAccuracyModifier = 0f;
            }
        }
    }
    private void HandleFakeGravity()
    {
        if (!isGrounded & !isWallRunning) { rb.AddForce(Vector3.down * fakeGravity * Time.fixedDeltaTime); }
    }
    private void HandleMovement()
    {
        if (InputManager.instance.wasdInputs != Vector2.zero && !BuyMenu.instance.isMenuOpen)
        {
            rb.AddForce(Vector3.ProjectOnPlane(transform.forward * InputManager.instance.wasdInputs.y * moveSpeed * curMoveSpeedMod * Time.fixedDeltaTime, currentGroundNormal));
            //Only apply sideways forces if not wall running. 
            if (InputManager.instance.wasdInputs.x > 0 && !isWallRunningRight) { rb.AddForce(transform.right * InputManager.instance.wasdInputs.x * moveSpeed * Time.fixedDeltaTime); }
            else if (InputManager.instance.wasdInputs.x < 0 && !isWallRunningLeft) { rb.AddForce(transform.right * InputManager.instance.wasdInputs.x * moveSpeed * Time.fixedDeltaTime); }
        }
    }
    private void HandleInputs()
    {
        if (InputManager.instance.jump && !BuyMenu.instance.isMenuOpen)
        {
            if (hasMantlePoint && canMantle)
            {
                AudioManager.instance.PlaySound(true, jump2Audio, Vector3.zero, 0.15f, myView.ViewID);
                canMantle = false;
                rb.AddForce(Vector3.up * mantleForce);
                anim.SetTrigger("Mantle");
                //Debug.Log("mantle");
            }
            else if (canJump)
            {
                rb.AddForce(Vector3.up * jumpForce);
                AudioManager.instance.PlaySound(true, jumpAudio, Vector3.zero, 0.15f, myView.ViewID);

                //Debug.Log("jump");
                anim.SetTrigger("Jump");
            }
            else if (isTouchingWall && canWallJump)
            {
                lastWallJumpTime = GameManager.instance.time;
                //rb.AddForce(transform.forward * InputManager.instance.wasdInputs.y * wallJumpForce * 0.2f);
                rb.AddForce(transform.right * InputManager.instance.wasdInputs.x * wallJumpForce);

                rb.AddForce(Vector3.up * jumpForce * 1.7f);

                //Debug.Log("walljump");
                anim.SetTrigger("Jump");
                AudioManager.instance.PlaySound(true, jump2Audio, Vector3.zero, 0.15f, myView.ViewID);

            }
        }
        if (InputManager.instance.scrollDelta != Vector2.zero)//change  item
        {
            ChangeWeapon(heldItem + (int)InputManager.instance.scrollDelta.y);
        }
        if(GetWeapon() && !isChangingWeapon)
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
    
    private void HandleAnimatorStates()
    {
        anim.SetInteger("MovementX", Mathf.RoundToInt(InputManager.instance.wasdInputs.x));
        anim.SetInteger("MovementZ", Mathf.RoundToInt(InputManager.instance.wasdInputs.y));
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("Aiming", isAiming);
        anim.SetBool("WallRunningRight", isWallRunningRight);
        anim.SetBool("WallRunningLeft", isWallRunningLeft);
        anim.SetBool("Dead", isDead);
        if (GetWeapon() != null) { 
            anim.SetBool("Reloading", GetWeapon().GetIsReloading());
            anim.SetInteger("HeldItem", (int)GetWeapon().thisWeapon+1);
        }
        else { 
            anim.SetBool("Reloading", false);
            anim.SetInteger("HeldItem", 0);
        }
    }
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
    public void RefreshWeaponMeshes()
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            allWeapons[i].gameObject.SetActive(false);
        }
        if (GetWeapon() != null) { GetWeapon().gameObject.SetActive(true); }
    }
    private void HandleZoom()
    {
        isAiming = InputManager.instance.mouse2Hold;
        Weapon weapon = GetWeapon();
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, isAiming ? weapon!=null?weapon.zoomFov:zoomFov : baseFov, Time.deltaTime * 35f);
    }
    [PunRPC]
    public void RPC_ChangeHealth(float delta)
    {
        if (!isMine || isDead) { return; }
        health += delta;
        if (delta < 0 && GameManager.instance.time>lastPainSoundTime+0.1f)
        {
            lastPainSoundTime = GameManager.instance.time;
            AudioManager.instance.PlaySound(true, painSounds, Vector3.zero, 0.7f, myView.ViewID);
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
    public void Die()
    {
        myView.RPC(nameof(RPC_AddDeaths), RpcTarget.AllBuffered, 1);//Add death
        if (lastHitByViewId != int.MinValue) {
            PhotonView.Find(lastHitByViewId).RPC(nameof(RPC_AddKills), RpcTarget.AllBuffered, 1);//Add kill to killer
        }
        health = 0;
        lastDeathTime = GameManager.instance.time;
        ResetAllWeapons();
        col.height = 0.3f;
        lastHitByViewId = int.MinValue;
    }
    public void Respawn()
    {
        ResetAllWeapons();
        health = 100;
        col.height = baseColliderHeight;
        transform.position = GameManager.instance.GetRandomSpawn();
        lastHitByViewId = int.MinValue;
    }
    private void ResetAllWeapons()
    {
        for (int i = 0; i < allWeapons.Count; i++)
        {
            allWeapons[i].ResetWeapon();
        }
    }
    private void HandleFootsteps()
    {
        if (shouldPlayFootstep && GameManager.instance.time > lastFootstepTime + footstepDelay)
        {
            lastFootstepTime = GameManager.instance.time;
            AudioManager.instance.PlaySound(true, footsteps[UnityEngine.Random.Range(0,footsteps.Length)], transform.position-Vector3.up, 0.075f, int.MinValue);
        }
    }
}
