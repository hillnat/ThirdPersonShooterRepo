using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public PlayerController localPlayer;
    public PlayerController[] allPlayers;
    public PhotonView view;
    public float time = 0f;
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
        view=GetComponent<PhotonView>();
    }
    void Update()
    {
        time = Time.time;
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("ConnectToMaster");
            Destroy(this);
        }

        PhotonNetwork.SendRate = 25;
        PhotonNetwork.SerializationRate = 50;
        localPlayer = PhotonNetwork.Instantiate("Player", GetRandomSpawn(), Quaternion.identity).GetComponent<PlayerController>();
        RPC_RefreshPlayerList();
    }
    public Vector3 GetRandomSpawn()
    {
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Spawns");
        return spawns[Random.Range(0, spawns.Length)].transform.position;
    }
    [PunRPC]
    public void RPC_RefreshPlayerList()
    {
        allPlayers = GameObject.FindObjectsOfType<PlayerController>();
    }
}
