using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public Vector2 wasdInputs = Vector2.zero;
    public bool mouse1 = false;
    public bool mouse2 = false;
    public bool jump = false;
    public bool reload = false;
    public bool mouse1Hold = false;
    public bool mouse2Hold = false;
    public bool openSettings = false;
    public bool alpha1 = false;
    public bool alpha2 = false;
    public bool alpha3 = false;
    public bool alpha4 = false;
    public bool alpha5 = false;
    public bool alpha6 = false;
    public bool alpha7 = false;
    public bool alpha8 = false;
    public bool alpha9 = false;
    public bool alpha0 = false;
    public bool buyMenu = false;
    public Vector2 mousePosition = Vector2.zero;
    public Vector2 mouseDelta = Vector2.zero;
    public Vector2 scrollDelta = Vector2.zero;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Cursor.lockState = CursorLockMode.Locked;   
    }
    private void LateUpdate()
    {
        if (mouse1) { mouse1 = false; }
        if (mouse2) { mouse2 = false; }
        if (jump) { jump = false; }
        if (reload) { reload = false; }
        if (openSettings) { openSettings = false; }
        if (alpha1) { alpha1 = false; }
        if (alpha2) { alpha2 = false; }
        if (alpha3) { alpha3 = false; }
        if (alpha4) { alpha4 = false; }
        if (alpha5) { alpha5 = false; }
        if (alpha6) { alpha6 = false; }
        if (alpha7) { alpha7 = false; }
        if (alpha8) { alpha8 = false; }
        if (alpha9) { alpha9 = false; }
        if (alpha0) { alpha0 = false; }
        if (buyMenu) { buyMenu = false; }
    }
    private void Update()
    {
        scrollDelta = Input.mouseScrollDelta;
        mouseDelta = Mouse.current.delta.ReadValue();
    }
    private void OnWASD(InputValue iv)
    {
        wasdInputs = iv.Get<Vector2>();
        wasdInputs.x = Mathf.Clamp(wasdInputs.x, -1f, 1f);
        wasdInputs.y = Mathf.Clamp(wasdInputs.y, -1f, 1f);
        wasdInputs.Normalize();
    }
    private void OnJump() { jump = true; }
    private void OnReload() { reload = true; }
    private void OnMouse1() { mouse1 = true; }
    private void OnMouse2() { mouse2 = true; }
    private void OnOpenSettings() { openSettings = true; }
    private void OnAlpha1() { alpha1 = true; }
    private void OnAlpha2() { alpha2 = true; }
    private void OnAlpha3() { alpha3 = true; }
    private void OnAlpha4() { alpha4 = true; }
    private void OnAlpha5() { alpha5 = true; }
    private void OnAlpha6() { alpha6 = true; }
    private void OnAlpha7() { alpha7 = true; }
    private void OnAlpha8() { alpha8 = true; }
    private void OnAlpha9() { alpha9 = true; }
    private void OnAlpha0() { alpha0 = true; }
    private void OnBuyMenu() { buyMenu = true; }
    private void OnMouse1Hold(InputValue iv) { mouse1Hold = iv.Get<float>() > 0; }
    private void OnMouse2Hold(InputValue iv) { mouse2Hold = iv.Get<float>() > 0; }
}
