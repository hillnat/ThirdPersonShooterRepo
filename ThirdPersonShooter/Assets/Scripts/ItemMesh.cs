using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMesh : MonoBehaviour
{
    public int indexInAnimator;
    private void Awake()
    {
        gameObject.tag = "ItemMesh";
    }
}

