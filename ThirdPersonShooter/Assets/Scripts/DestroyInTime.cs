using UnityEngine;

public class DestroyInTime : MonoBehaviour
{
    private float t = 0;
    public float lifeTime = 1f;
    void Update()
    {
        t += Time.deltaTime;
        if (t >= lifeTime) { Destroy(this.gameObject); }
    }
}
