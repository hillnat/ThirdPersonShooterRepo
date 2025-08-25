using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public PhotonView myView;
    private AudioClip[] clipList;
    private List<AudioSource> emitters = new List<AudioSource>();
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        myView = GetComponent<PhotonView>();
        emitters.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            emitters.Add(transform.GetChild(i).GetComponent<AudioSource>());
        }
        clipList = Resources.LoadAll<AudioClip>("Audio/");
    }

    [PunRPC]
    public void RPC_SpawnSound(bool isNetworked, int clipIndex, Vector3 position, float volume)
    {
        AudioSource myEmitter = GetFreeEmitter();
        if (myEmitter == null) { return; }
        myEmitter.volume= volume;
        myEmitter.transform.position = position;
        myEmitter.PlayOneShot(clipList[clipIndex]);

        if (isNetworked)
        {
            myView.RPC("RPC_SpawnSound", RpcTarget.Others, false, clipIndex, position, volume);
        }
    }
    private AudioSource? GetFreeEmitter()
    {
        for (int i = 0; i < emitters.Count; i++)
        {
            if(!emitters[i].isPlaying) {return emitters[i]; }
        }
        return null;
    }
}
