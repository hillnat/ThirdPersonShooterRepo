using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    public PhotonView myView;
    private GameObject[] particleFxList;
    private GameObject[] lineFxList;
    private Dictionary<GameObject, int> particleToIndex = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, int> lineFxToIndex = new Dictionary<GameObject, int>();

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
        particleFxList = Resources.LoadAll<GameObject>("Particles/");
        lineFxList = Resources.LoadAll<GameObject>("LineFx/");

        // Map clip -> index
        for (int i = 0; i < particleFxList.Length; i++)
        {
            particleToIndex[particleFxList[i]] = i;
        }
        for (int i = 0; i < lineFxList.Length; i++)
        {
            lineFxToIndex[lineFxList[i]] = i;
        }
    }
    public void PlayParticle(bool isNetworked, GameObject particle, Vector3 position, Quaternion rotation)
    {
        if (particle == null) { return; }
        int particleIndex = particleToIndex[particle];
        SpawnParticle(particleIndex, position, rotation);
        if (isNetworked)
        {
            myView.RPC(nameof(RPC_SpawnParticle), RpcTarget.Others, particleIndex, position, rotation);
        }
    }
    [PunRPC]
    public void RPC_SpawnParticle(int particleIndex, Vector3 position, Quaternion rotation)
    {
        SpawnParticle(particleIndex, position, rotation);
    }
    private void SpawnParticle(int particleIndex, Vector3 position, Quaternion rotation)
    {
        Instantiate(particleFxList[particleIndex], position, rotation);
    }



    public void PlayLineFx(bool isNetworked, GameObject lineFx, Vector3[] positions)
    {
        if (lineFx == null) { return; }
        int particleIndex = lineFxToIndex[lineFx];
        SpawnLineFx(particleIndex, positions);
        if (isNetworked)
        {
            myView.RPC(nameof(RPC_SpawnLineFx), RpcTarget.Others, particleIndex, positions);
        }
    }
    [PunRPC]
    public void RPC_SpawnLineFx(int lineFxIndex, Vector3[] positions)
    {
        SpawnLineFx(lineFxIndex, positions);
    }
    private void SpawnLineFx(int lineFxIndex, Vector3[] positions)
    {
        LineRenderer lr = Instantiate(lineFxList[lineFxIndex], Vector3.zero, Quaternion.identity).gameObject.GetComponent<LineRenderer>();
        if (lr == null) { return; }
        lr.SetPositions(positions);
    }
}
