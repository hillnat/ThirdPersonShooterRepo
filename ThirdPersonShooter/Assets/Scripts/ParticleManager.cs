using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    public PhotonView myView;
    private GameObject[] clipList;
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
        clipList = Resources.LoadAll<GameObject>("Particles/");
    }
    [PunRPC]
    public void RPC_SpawnParticle(bool isNetworked, int particleIndex, Vector3 position, Quaternion rotation)
    {
        Instantiate(clipList[particleIndex], position, rotation);

        if (isNetworked)
        {
            myView.RPC("RPC_SpawnParticle", RpcTarget.Others, false, particleIndex, position, rotation);
        }
    }
}
