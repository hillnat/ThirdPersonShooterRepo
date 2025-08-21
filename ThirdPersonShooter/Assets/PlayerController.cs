using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float moveSpeed;
    public Vector3 cameraOffset = new Vector3(1f, 0.5f, -2f);
    private Camera myCamera;
    public Vector2 mouseLookXY = Vector2.zero;
    public float mouseSensitivty = 25f;
    public Transform cameraParent;
    public Animator anim;
    public float currentSpeed;
    public float jumpForce = 9000f;
    public bool canJump = false;
    private float lastJumpTime = 0f;
    public float fakeGravity = 20f;
    public bool isGrounded = false;
    private float groundedCheckDistance = 0.25f;
    private float groundedCheckOriginOffset = -0.95f;
    public bool isTouchingWall = false;
    private float wallJumpCheckDistance = 0.5f;
    private float wallJumpCheckOriginOffset = 0.5f;
    public bool canWallJump = false;
    public float wallJumpForce = 4000f;
    public int heldItem = 0;
    public Transform muzzlePoint;
    public GameObject impactParticles;
    public float currentAccuracyModifier = 0f;

    public List<GameObject> allItemMeshes = new List<GameObject>();
    public List<Weapon> allWeapons = new List<Weapon>();

    public Weapon? GetWeapon()
    {
        if (heldItem == 0 || (heldItem < allWeapons.Count - 1)) { return null; }
        else{return allWeapons[heldItem-1]; }
    }

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        myCamera = Camera.main;
        myCamera.transform.parent = cameraParent;
        myCamera.transform.localPosition = cameraOffset;
    }
    void Start()
    {
        RefreshItemMesh();
    }

    void Update()
    {

        currentSpeed = rb.velocity.magnitude;
        canJump = GameManager.instance.time > lastJumpTime + 0.25f && isGrounded;
        isGrounded = Physics.Raycast(transform.position + new Vector3(0, groundedCheckOriginOffset, 0), Vector3.down, groundedCheckDistance);
        isTouchingWall = 
            Physics.Raycast(transform.position + transform.right * wallJumpCheckOriginOffset, transform.right, wallJumpCheckDistance) ||
            Physics.Raycast(transform.position + transform.right * -wallJumpCheckOriginOffset, -transform.right, wallJumpCheckDistance);

        if (!canWallJump && isGrounded) { canWallJump = true; }

        Debug.DrawRay(transform.position + new Vector3(0, -1f, 0), Vector3.down* groundedCheckDistance, Color.red);
        Debug.DrawRay(transform.position + transform.right * wallJumpCheckOriginOffset, transform.right * wallJumpCheckDistance,Color.red);
        Debug.DrawRay(transform.position + transform.right * -wallJumpCheckOriginOffset, transform.right * -wallJumpCheckDistance,Color.red);

        HandleMouseLook();

        if (InputManager.instance.jump)
        {
            if (canJump)
            {
                rb.AddForce(Vector3.up * jumpForce);
                Debug.Log("jump");
                anim.SetTrigger("Jump");
            }
            else if (isTouchingWall && canWallJump)
            {
                canWallJump = false;
                rb.AddForce(transform.forward * InputManager.instance.wasdInputs.y * wallJumpForce);
                rb.AddForce(transform.right * InputManager.instance.wasdInputs.x * wallJumpForce);
                Debug.Log("walljump");
                anim.SetTrigger("Jump");
            }
        }
        if (InputManager.instance.scrollDelta != Vector2.zero)
        {
            heldItem += (int)InputManager.instance.scrollDelta.y;
            heldItem = Mathf.Clamp(heldItem, 0, 2);
            RefreshItemMesh();
        }
        if (InputManager.instance.mouse1)
        {
            if (heldItem == 0 || (heldItem < allWeapons.Count - 1)) { return; }

            if (GetWeapon() != null) { GetWeapon().Fire(myCamera.transform.position, myCamera.transform.forward, muzzlePoint.transform.position); }
        }
    }
    private void FixedUpdate()
    {
        if (!isGrounded) { rb.AddForce(Vector3.down * fakeGravity * Time.fixedDeltaTime); }

        if (InputManager.instance.wasdInputs != Vector2.zero)
        {
            rb.AddForce(transform.forward * InputManager.instance.wasdInputs.y * moveSpeed * Time.fixedDeltaTime);
            rb.AddForce(transform.right * InputManager.instance.wasdInputs.x * moveSpeed * Time.fixedDeltaTime);
        }

        anim.SetInteger("MovementX", Mathf.RoundToInt(InputManager.instance.wasdInputs.x));
        anim.SetInteger("MovementZ", Mathf.RoundToInt(InputManager.instance.wasdInputs.y));
        anim.SetInteger("HeldItem", heldItem);
        anim.SetBool("Grounded", isGrounded);

        if (isGrounded)
        {
            currentAccuracyModifier = Mathf.Lerp(currentAccuracyModifier, 1f, Time.fixedDeltaTime*5f);
            if (currentAccuracyModifier > 0.95f)
            {
                currentAccuracyModifier = 1f;
            }
        }
        else
        {
            currentAccuracyModifier = Mathf.Lerp(currentAccuracyModifier, 0f, Time.fixedDeltaTime*5f);
            if (currentAccuracyModifier < 0.05f)
            {
                currentAccuracyModifier = 0f;
            }
        }
    }
    private void HandleMouseLook()
    {
        cameraParent.transform.localEulerAngles = new Vector3(mouseLookXY.x, 0, 0);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, mouseLookXY.y, transform.eulerAngles.z);

        if (InputManager.instance.mouseDelta != Vector2.zero)
        {
            mouseLookXY.x -= InputManager.instance.mouseDelta.y * Time.deltaTime * mouseSensitivty;
            mouseLookXY.y += InputManager.instance.mouseDelta.x * Time.deltaTime * mouseSensitivty;

            mouseLookXY.x = Mathf.Clamp(mouseLookXY.x, -90f, 90f);
            if (mouseLookXY.y > 360f) { mouseLookXY.y -= 360f; }
            if (mouseLookXY.y < -360f) { mouseLookXY.y += 360f; }
        }
    }
    public void RefreshItemMesh()
    {
        for (int i = 0; i < allItemMeshes.Count; i++)
        {
            allItemMeshes[i].gameObject.SetActive(false);
        }
        if (GetWeapon() != null) { allItemMeshes[GetWeapon().meshIndex].gameObject.SetActive(true); }

    }
}
