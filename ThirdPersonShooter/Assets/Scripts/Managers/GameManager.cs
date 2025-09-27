using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        localPlayer = PhotonNetwork.Instantiate("CharacterSniper", GetRandomSpawn(ETeams.Any), Quaternion.identity).GetComponent<PlayerController>();
        RPC_RefreshPlayerList();
    }
    public Vector3 GetRandomSpawn(ETeams team)
    {
        List<GameObject> spawns = new List<GameObject>();
        switch (team) {
            case ETeams.None:
                spawns = GameObject.FindGameObjectsWithTag("Spawns").ToList();
                break;
            case ETeams.Red:
                spawns = GameObject.FindGameObjectsWithTag("RedSpawn").ToList();
                break;
            case ETeams.Blue:
                spawns = GameObject.FindGameObjectsWithTag("BlueSpawn").ToList();
                break;
            case ETeams.Any:
                spawns.AddRange(GameObject.FindGameObjectsWithTag("BlueSpawn"));
                spawns.AddRange(GameObject.FindGameObjectsWithTag("RedSpawn"));
                spawns.AddRange(GameObject.FindGameObjectsWithTag("Spawns"));
                break;
            default:
                break;
        }
        if (spawns.Count == 0)
        {
            Debug.LogWarning($"No appropriate spawns found for PlayerTeams {team}!");
            return new Vector3(-9999, -9999, -9999);
        }
        else
        {
            return spawns[Random.Range(0, spawns.Count)].transform.position;
        }
    }
    [PunRPC]
    public void RPC_RefreshPlayerList()
    {
        allPlayers = GameObject.FindObjectsOfType<PlayerController>();
    }
}
