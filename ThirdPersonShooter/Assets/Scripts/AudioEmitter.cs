using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource), typeof(FollowPhotonView))]
public class AudioEmitter : MonoBehaviour
{
    public AudioSource aS;
    public FollowPhotonView followPhotonView;
    void Start()
    {
        aS = GetComponent<AudioSource>();
        if (aS == null) { Destroy(this.gameObject); }
    }

    private void Awake()
    {
        followPhotonView = GetComponent<FollowPhotonView>();
    }
    private void LateUpdate()
    {
        if (!aS.isPlaying) { followPhotonView.followObject = null; transform.position = Vector3.zero; }
    }
}
