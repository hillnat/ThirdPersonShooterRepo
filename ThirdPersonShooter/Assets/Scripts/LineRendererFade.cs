using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererFade : MonoBehaviour
{
    public float lifetime = 0.5f;
    private float startTime = 0f;
    private LineRenderer lr;
    private Color baseColor;
    private Color targetColor;
    private void Awake()
    {
        startTime = GameManager.instance.localTime;
        lr = GetComponent<LineRenderer>();
        baseColor = lr.startColor;
        targetColor = new Color(baseColor.r, baseColor.g, baseColor.b,0f);
    }
    void Update()
    {
        Color color = Color.Lerp(baseColor, targetColor, (GameManager.instance.localTime - startTime) / lifetime);
        lr.startColor = color;
        lr.endColor = color;
    }
}
