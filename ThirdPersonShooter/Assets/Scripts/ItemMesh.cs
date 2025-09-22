using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMesh : MonoBehaviour
{
    public EWeapons representative;
    private void Awake()
    {
        gameObject.tag = "ItemMesh";
    }
}

