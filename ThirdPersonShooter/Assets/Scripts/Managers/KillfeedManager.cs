using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillfeedManager : MonoBehaviour
{
    public static KillfeedManager instance;
    public PhotonView view;
    public TMP_Text killfeedText;
    public List<string> killfeedStrings;
    public int maxKillfeedElements = 5;
    private float removeElementTimer = 0f;
    public Image killfeedBackground;
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
        view = GetComponent<PhotonView>();
    }
    void Start()
    {
        RefreshKillfeedText();
    }

    void FixedUpdate()
    {
        removeElementTimer += Time.fixedDeltaTime;
        if (removeElementTimer > 5f && killfeedStrings.Count>0)
        {
            removeElementTimer = 0f;
            killfeedStrings.RemoveAt(0);
            RefreshKillfeedText();
        }
        if (killfeedBackground != null)
        {
            if (killfeedBackground.gameObject.activeInHierarchy)
            {
                if (killfeedStrings.Count == 0)
                {
                    killfeedBackground.gameObject.SetActive(false);
                }
            }
            else if (killfeedStrings.Count > 0)
            {
                killfeedBackground.gameObject.SetActive(true);
            }
        }
    }
    [PunRPC]
    public void AddKillfeedElement(int killerViewId, int victimViewId)
    {
        PlayerControllerBase killerPc = PhotonView.Find(killerViewId).transform.GetComponent<PlayerControllerBase>();
        PlayerControllerBase victimPc = PhotonView.Find(victimViewId).transform.GetComponent<PlayerControllerBase>();
        if (killerPc == null) { Debug.LogWarning($"KillfeedManager.AddKillfeedElement failed to find view ID for {killerViewId}");  return; }
        if (victimPc == null) { Debug.LogWarning($"KillfeedManager.AddKillfeedElement failed to find view ID for {victimViewId}");  return; }

        killfeedStrings.Add($"{killerPc.username} has slain {victimPc.username}");

        if (killfeedStrings.Count >= maxKillfeedElements)
        {
            killfeedStrings.RemoveAt(0);
        }

        RefreshKillfeedText();
        removeElementTimer = 0f;
    }
    private void RefreshKillfeedText()
    {
        string newText = "";
        for (int i = 0; i < killfeedStrings.Count; i++)
        {
            newText += killfeedStrings[i] + "\n";
        }

        killfeedText.text = newText;
    }
}
