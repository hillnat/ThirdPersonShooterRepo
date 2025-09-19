using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PhotonView))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public PhotonView myView;
    private AudioClip[] clipList;
    private List<AudioEmitter> emitters = new List<AudioEmitter>();
    private Dictionary<AudioClip, int> clipToIndex = new Dictionary<AudioClip, int>();
    public float masterVolumeMultiplier=1f;
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
        myView = GetComponent<PhotonView>();
    }
    private void Start()
    {
        masterVolumeMultiplier=SettingsManager.instance.settingsFile.masterVolume;
        emitters.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            emitters.Add(transform.GetChild(i).GetComponent<AudioEmitter>());
        }
        clipList = Resources.LoadAll<AudioClip>("Audio/");

        // Map clip -> index
        for (int i = 0; i < clipList.Length; i++)
        {
            clipToIndex[clipList[i]] = i;
            Debug.Log("Found audio clip " + clipList[i].name);
        }
    }

    public void PlaySound(bool isNetworked, AudioClip audioClip, Vector3 position, float volume, float pitch, int followViewID)//Universal call
    {
        if (audioClip == null) { Debug.LogWarning("PlaySound:: Sound was null"); return; }
        int clipIndex = clipToIndex[audioClip];
        SpawnSound(clipIndex,position,volume,pitch,followViewID);//Play instantly for us
        if (isNetworked)//Network if wanted
        {
            myView.RPC(nameof(RPC_SpawnSound), RpcTarget.Others, clipIndex, position, volume,pitch, followViewID);
        }
    }

    [PunRPC]
    public void RPC_SpawnSound(int clipIndex, Vector3 position, float volume,float pitch, int followViewID)//Just call normal spawn sound
    {
        SpawnSound(clipIndex, position, volume,pitch, followViewID);
    }
    private void SpawnSound(int clipIndex, Vector3 position, float volume, float pitch, int followViewID)//Actual finding of emitter and playing of sound
    {
        AudioEmitter myEmitter = GetFreeEmitter();
        if (myEmitter == null) { return; }
        
        myEmitter.aS.volume = volume * masterVolumeMultiplier;
        myEmitter.aS.pitch = pitch;
        myEmitter.aS.PlayOneShot(clipList[clipIndex]);

        if (followViewID != int.MinValue)
        {
            myEmitter.followObject = PhotonView.Find(followViewID).transform;
            myEmitter.transform.localPosition = position;
        }
        else { myEmitter.transform.position = position; }

        //Debug.Log("Spawned sound " + clipList[clipIndex].name);
    }
    private AudioEmitter? GetFreeEmitter()
    {
        for (int i = 0; i < emitters.Count; i++)
        {
            if(!emitters[i].aS.isPlaying) {return emitters[i]; }
        }
        return null;
    }
}
