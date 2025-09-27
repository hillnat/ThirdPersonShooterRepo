using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PhotonView))]
public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    public PhotonView myView;
    public GameObject[] particleFxList;
    public GameObject[] lineFxList;
    public Dictionary<GameObject, int> particleToIndex = new Dictionary<GameObject, int>();
    public Dictionary<GameObject, int> lineFxToIndex = new Dictionary<GameObject, int>();
    public GameObject damageNumber;

    public List<GameObject> goreParticles;
    public List<GameObject> impactParticles;
    public List<GameObject> defaultLineFx;
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
    public void PlayGoreParticles(bool isNetworked, Vector3 position, Quaternion rotation, int followViewID)
    {
        PlayParticle(isNetworked, goreParticles[Random.Range(0, goreParticles.Count)], position, rotation, followViewID, int.MinValue);
    }
    public void PlayImpactParticles(bool isNetworked, Vector3 position, Quaternion rotation, int followViewID)
    {
        PlayParticle(isNetworked, impactParticles[Random.Range(0, impactParticles.Count)], position, rotation, followViewID, int.MinValue);
    }
    #region Particle Spawning
    public void PlayParticle(bool isNetworked, GameObject particle, Vector3 position, Quaternion rotation, int followViewID, int particleID)//Call this one
    {
        if (particle == null) { return; }
        int particleIndex = particleToIndex[particle];
        SpawnParticle(particleIndex, position, rotation, followViewID, particleID);
        if (isNetworked)
        {
            myView.RPC(nameof(RPC_SpawnParticle), RpcTarget.Others, particleIndex, position, rotation, followViewID, particleID);
        }
    }


    [PunRPC]
    private void RPC_SpawnParticle(int particleIndex, Vector3 position, Quaternion rotation, int followViewID, int particleID)
    {
        SpawnParticle(particleIndex, position, rotation, followViewID, particleID);
    }
    private void SpawnParticle(int particleIndex, Vector3 position, Quaternion rotation, int followViewID, int particleID)
    {
        GameObject newParticle = Instantiate(particleFxList[particleIndex], position, rotation);
        if (followViewID != int.MinValue)
        {
            FollowPhotonView fpv = newParticle.AddComponent<FollowPhotonView>();
            fpv.followObject = PhotonView.Find(followViewID).transform;
        }
        if (particleID != int.MinValue)
        {
            ParticleUniqueID pID = newParticle.AddComponent<ParticleUniqueID>();
            pID.uniqueId = particleID;
        }
    }
    public void DestroyParticleWithUniqueID(int ID)
    {
        ParticleUniqueID[] particleUniqueIDs = GameObject.FindObjectsOfType<ParticleUniqueID>();

        for (int i = 0; i < particleUniqueIDs.Length; i++)
        {
            if (particleUniqueIDs[i].uniqueId == ID)
            {
                Destroy(particleUniqueIDs[i].gameObject);
                return;
            }
        }
    }

    #endregion
    #region Line FX
    public void PlayDefaultLineFx(bool isNetworked, Vector3[] positions)
    {
        PlayLineFx(isNetworked, defaultLineFx[Random.Range(0, defaultLineFx.Count)], positions);
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
    #endregion
    public void SpawnDamageNumber(Vector3 position, float number)
    {
        DamageNumber dm = Instantiate(damageNumber, position, Quaternion.identity).GetComponent<DamageNumber>();
        dm.SetNumber(number);
    }
}
