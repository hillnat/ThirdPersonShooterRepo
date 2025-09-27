using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPhotonView : MonoBehaviour
{
    public Transform followObject;
    private void FixedUpdate()
    {
        if (followObject != null)
        {
            transform.position = followObject.position;
        }
    }
}
