using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EGameState { WaitingToStartMatch, PreRound, RoundPlaying, RoundOver, GameOver}
[RequireComponent(typeof(PhotonView))]
public class GameManager : MonoBehaviour, IPunObservable
{
    public EGameState gamestate = EGameState.WaitingToStartMatch;
    public static GameManager instance;
    public PlayerControllerBase localPlayer;
    public PlayerControllerBase[] allPlayers;
    public PhotonView view;
    public float localTime = 0f;
    public float gameStateTimer = 0f;
    public int maxRoundTimeMinutes = 10;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameStateTimer);
        }
        else
        {
            gameStateTimer = (float)stream.ReceiveNext();
        }
    }
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
        localTime = Time.time;
        if (PhotonNetwork.IsMasterClient)
        {
            
        }      
    }
    private void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            gameStateTimer += Time.fixedDeltaTime;

            switch (gamestate)
            {
                case EGameState.WaitingToStartMatch:
                    break;
                case EGameState.PreRound:
                    if (gameStateTimer > 15) { gameStateTimer = 0f; gamestate = EGameState.RoundPlaying; }
                    break;
                case EGameState.RoundPlaying:
                    if (gameStateTimer > (maxRoundTimeMinutes*60)) { 
                        gameStateTimer = 0f; gamestate = EGameState.PreRound; 
                    }
                    break;
                case EGameState.RoundOver:
                    if (gameStateTimer > 15) { gameStateTimer = 0f; gamestate = EGameState.PreRound; }
                    break;
                case EGameState.GameOver:
                    break;
            }
        }
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
        localPlayer = PhotonNetwork.Instantiate("CharacterFireWarrior", GetRandomSpawn(ETeams.Any), Quaternion.identity).GetComponent<PlayerControllerBase>();
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
        allPlayers = GameObject.FindObjectsOfType<PlayerControllerBase>();
    }

    
}
