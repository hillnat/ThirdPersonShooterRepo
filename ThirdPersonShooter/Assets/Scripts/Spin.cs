using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public Vector3 speed = Vector3.one;
    
    void Update()
    {
        transform.eulerAngles += speed * Time.deltaTime;
    }
}
