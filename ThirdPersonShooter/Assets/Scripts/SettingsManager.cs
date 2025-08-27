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
    
    public TMP_InputField sensitivityInputField;
    public TMP_InputField usernameInputField;
    
    public Button exitMatchButton;
    public Button quitGameButton;
    private bool isInGameScene = false;
    private string settingsPath=> Path.Combine(Application.streamingAssetsPath, "SettingsFile.json");
    private bool settingsFileIsDirty = false;
    private float dirtySettingsCheckTimer = 0;
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
        LoadSettingsFile();
    }
    void Start()
    {
        mainCanvas = GetComponentInChildren<Canvas>();
        //Setup UI
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(delegate { OnMasterVolumeChanged(); });
            masterVolumeSlider.value = settingsFile.masterVolume;
        }

        if (sensitivityInputField != null)
        {
            sensitivityInputField.onValueChanged.RemoveAllListeners();
            sensitivityInputField.onValueChanged.AddListener(delegate { OnSensitivityChanged(); });
            sensitivityInputField.text = $"{settingsFile.sensitivity:F4}";
        }

        if (usernameInputField != null)
        {
            usernameInputField.onValueChanged.RemoveAllListeners();
            usernameInputField.onValueChanged.AddListener(delegate { OnUsernameChanged(); });
            usernameInputField.text = $"{settingsFile.username}";
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
        dirtySettingsCheckTimer += Time.deltaTime;
        if (settingsFileIsDirty && dirtySettingsCheckTimer>1f)//Check each second if settings have change and if so save them. This is to get around writing many times when each value is changed like a slider
        {
            settingsFileIsDirty = false;
            dirtySettingsCheckTimer = 0;
            WriteSettingsFile();
        }
    }
    public void OnMasterVolumeChanged() {
        settingsFile.masterVolume = Mathf.Clamp(masterVolumeSlider.value,0f,100f);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.masterVolumeMultiplier = settingsFile.masterVolume;
        }
        settingsFileIsDirty = true;
    }
    public void OnSensitivityChanged()
    {
        if (float.TryParse(sensitivityInputField.text, out float value))
        {
            settingsFile.sensitivity = Mathf.Clamp(value,0f,1000f);
            sensitivityInputField.text = $"{settingsFile.sensitivity:F4}";
            if (isInGameScene)
            {
                GameManager.instance.localPlayer.mouseSensitivty = settingsFile.sensitivity;
            }
            settingsFileIsDirty = true;
        }       
    }
    public void OnUsernameChanged()
    {
        settingsFile.username = usernameInputField.text;
        usernameInputField.text = $"{settingsFile.username}";
        settingsFileIsDirty = true;
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
    public void LoadSettingsFile()
    {

        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            settingsFile = JsonUtility.FromJson<SettingsFile>(json);         
        }
    }
    public void WriteSettingsFile()
    {
        string json = JsonUtility.ToJson(settingsFile, true); // pretty print
        File.WriteAllText(settingsPath, json);
        Debug.Log("Wrote settings");
    }
}
[System.Serializable]
public struct SettingsFile
{
    public float sensitivity;
    public string username;
    public float masterVolume;
}