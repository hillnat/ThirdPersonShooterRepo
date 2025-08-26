using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioEmitter : MonoBehaviour
{
    public AudioSource aS;
    public Transform followObject;
    // Start is called before the first frame update
    void Start()
    {
        aS = GetComponent<AudioSource>();
        if (aS == null) { Destroy(this.gameObject); }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (followObject!=null)
        {
            transform.position = followObject.position;
        }
    }
    private void LateUpdate()
    {
        if (!aS.isPlaying) { followObject = null; transform.position = Vector3.zero; }
    }
}
