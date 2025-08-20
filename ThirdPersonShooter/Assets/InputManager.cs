using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public Vector2 wasdInputs = Vector2.zero;
    public bool mouse1 = false;
    public bool jump = false;
    public bool mouse1Hold = false;
    public bool mouse2Hold = false;
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
    }
    private void LateUpdate()
    {

        if (mouse1) { mouse1 = false; }
        if (jump) { jump = false; }
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

    private void OnMouse1() { mouse1 = true; }
    private void OnMouse1Hold(InputValue iv) { mouse1Hold = iv.Get<float>() > 0; }
    private void OnMouse2Hold(InputValue iv) { mouse2Hold = iv.Get<float>() > 0; }
}
