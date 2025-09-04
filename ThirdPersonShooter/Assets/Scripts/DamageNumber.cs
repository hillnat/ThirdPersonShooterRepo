using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    private TMP_Text text;
    private float timer = 0;
    public Color color = Color.red;
    public float lifetime = 1f;
    public float scaleFactor = 2f;
    private void Awake()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform);
        transform.eulerAngles += Vector3.up * 180f;
        transform.localScale = Vector3.one * Vector3.Distance(transform.position, Camera.main.transform.position) * scaleFactor;
        timer += Time.deltaTime;

        text.color = color * (Color.white * (1f - timer));
        if (timer > 1) { Destroy(this.gameObject); }
    }
    public void SetNumber(float number)
    {
        text.text = $"{number:F2}";
    }
}
