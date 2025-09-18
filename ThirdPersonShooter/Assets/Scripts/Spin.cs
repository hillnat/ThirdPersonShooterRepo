using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour
{
    public bool isOn = true;
    public Vector3 speed = Vector3.one;
    
    void Update()
    {
        if (isOn)
        {
            transform.Rotate(speed * Time.deltaTime);
        }
    }
}
