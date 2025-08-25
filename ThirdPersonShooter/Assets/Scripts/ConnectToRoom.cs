using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectToRoom : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected) { SceneManager.LoadScene(0); }//Load connect to master scene if not connected
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ConnectToGame()
    {
        StartCoroutine(TryJoinCreate());
    }
    IEnumerator TryJoinCreate()
    {
        PhotonNetwork.JoinRoom("testroomforeveryone");
        yield return new WaitForSeconds(1);
        PhotonNetwork.CreateRoom("testroomforeveryone");
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("MainGame");
    }
}
