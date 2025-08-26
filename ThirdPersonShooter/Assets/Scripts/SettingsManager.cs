using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;
    public SettingsFile settingsFile;

    private Canvas mainCanvas;
    public bool settingsOpen = false;
    public Slider masterVolumeSlider;
    public float masterVolume = 100f;
    public TMP_InputField sensitivityInputField;
    public float sensitivity = 25f;
    public Button exitMatchButton;
    public Button quitGameButton;
    private bool isInGameScene = false;
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
    void Start()
    {
        mainCanvas = GetComponentInChildren<Canvas>();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(delegate { OnMasterVolumeChanged(); });
        }

        if (sensitivityInputField != null)
        {
            sensitivityInputField.onValueChanged.RemoveAllListeners();
            sensitivityInputField.onValueChanged.AddListener(delegate { OnSensitivityChanged(); });
        }
            


        if (exitMatchButton != null)
        {
            exitMatchButton.onClick.RemoveAllListeners();
            exitMatchButton.onClick.AddListener(delegate { ExitMatch(); });
        }
            
        if (quitGameButton != null)
        {

            quitGameButton.onClick.RemoveAllListeners();
            quitGameButton.onClick.AddListener(delegate { QuitGame(); });
        }
            

        //settingsOpen = false;
        mainCanvas.enabled = settingsOpen;
        isInGameScene = SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainGame");
        if (isInGameScene)
        {
            if (exitMatchButton != null) { exitMatchButton.gameObject.SetActive(true); }
        }
        else
        {
            if (exitMatchButton != null) { exitMatchButton.gameObject.SetActive(false); }
        }
    }

    private void Update()
    {
        if (isInGameScene && InputManager.instance.openSettings)
        {
            settingsOpen = !settingsOpen;


            mainCanvas.enabled = settingsOpen;

            Cursor.visible = settingsOpen;
            Cursor.lockState = settingsOpen ? CursorLockMode.None : CursorLockMode.Locked; 
        }
    }
    public void OnMasterVolumeChanged() {
        masterVolume = Mathf.Clamp(masterVolumeSlider.value,0f,100f);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.masterVolumeMultiplier = masterVolume;
        }
    }
    public void OnSensitivityChanged()
    {
        if (float.TryParse(sensitivityInputField.text, out float value))
        {
            sensitivity = Mathf.Clamp(value,0f,1000f);
            sensitivityInputField.text = $"{sensitivity:F4}";
            if (isInGameScene)
            {
                GameManager.instance.localPlayer.mouseSensitivty = sensitivity;
            }
        }       
    }
    public void ExitMatch()
    {
        if (!isInGameScene) { return; }
        if (GameManager.instance.localPlayer != null) { PhotonNetwork.Destroy(GameManager.instance.localPlayer.myView); }
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        if (GameManager.instance!=null && GameManager.instance.localPlayer != null && PhotonNetwork.InRoom) { PhotonNetwork.Destroy(GameManager.instance.localPlayer.myView); }
        if (PhotonNetwork.InRoom) { PhotonNetwork.LeaveRoom(); }
        if (PhotonNetwork.InLobby) { PhotonNetwork.LeaveLobby(); }
        Application.Quit();
    }
    public void LoadSettingsFile()//Pickup here
    {
        string path = Path.Combine(Application.streamingAssetsPath, "SettingsFile");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            Debug.Log(json);

            settingsFile = JsonUtility.FromJson<SettingsFile>(json);
        }
        else
        {
            Debug.LogError("JSON file not found: " + path);
        }
    }
}
[System.Serializable]
public struct SettingsFile
{
    float sensitivity;
    string username;
    float masterVolume;
}