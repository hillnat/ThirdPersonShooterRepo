using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyMenu : MonoBehaviour
{
    public static BuyMenu instance;
    public Canvas buyMenuCanvas;
    public bool isMenuOpen = false;
    public TMP_Text currentWeaponsText;
    public Button BuyM4A1;
    public Button BuyM1911;
    public Button BuyShotgun;
    public Button BuyTec9;
    public Button BuySniper;
    public Button BuyRingblade;
    public Button BuyRecallDagger;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        buyMenuCanvas.gameObject.SetActive(false);
        RefreshCurrentWeaponsText();

        if (BuyM4A1 != null)
        {
            BuyM4A1.onClick.RemoveAllListeners();
            BuyM4A1.onClick.AddListener(delegate { BuySellWeapon(EWeapons.M4A1); });
        }
        if (BuyM1911 != null)
        {
            BuyM1911.onClick.RemoveAllListeners();
            BuyM1911.onClick.AddListener(delegate { BuySellWeapon(EWeapons.M1911); });
        }
        if (BuyShotgun != null)
        {
            BuyShotgun.onClick.RemoveAllListeners();
            BuyShotgun.onClick.AddListener(delegate { BuySellWeapon(EWeapons.Shotgun); });
        }
        if (BuyTec9 != null)
        {
            BuyTec9.onClick.RemoveAllListeners();
            BuyTec9.onClick.AddListener(delegate { BuySellWeapon(EWeapons.Tec9); });
        }
        if (BuySniper != null)
        {
            BuySniper.onClick.RemoveAllListeners();
            BuySniper.onClick.AddListener(delegate { BuySellWeapon(EWeapons.Sniper); });
        }
        if (BuyRingblade != null)
        {
            BuyRingblade.onClick.RemoveAllListeners();
            BuyRingblade.onClick.AddListener(delegate { BuySellWeapon(EWeapons.RingBlade); });
        }
        if (BuyRecallDagger != null)
        {
            BuyRecallDagger.onClick.RemoveAllListeners();
            BuyRecallDagger.onClick.AddListener(delegate { BuySellWeapon(EWeapons.RecallDagger); });
        }
    }
    private void Update()
    {
        if (InputManager.instance.buyMenu)
        {
            isMenuOpen = !isMenuOpen;
            buyMenuCanvas.gameObject.SetActive(isMenuOpen);
            Cursor.lockState = isMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isMenuOpen;

        }
        if (InputManager.instance.openSettings && isMenuOpen)
        {
            buyMenuCanvas.gameObject.SetActive(false);
        }
    }
    public void RefreshCurrentWeaponsText()
    {
        if (currentWeaponsText == null) { return; }
        PlayerController pc = GameManager.instance.localPlayer;
        if (pc == null) {return; }
        string newText= $"Weapons ({pc.currentWeapons.Count}/{pc.maxWeapons}):\n";
        
        for(int i=0; i<pc.currentWeapons.Count; i++)
        {
            newText += pc.currentWeapons[i].weaponName + "\n";
        }
        if (pc.currentWeapons.Count == pc.maxWeapons) { newText += "\nInventory Full!"; }
        currentWeaponsText.text = newText;
    }
    public void BuySellWeapon(EWeapons weapon)
    {
        Debug.Log($"BuySellWeapon {weapon}");
        PlayerController pc = GameManager.instance.localPlayer;
        if (pc != null)
        {
            if(pc.HasWeapon(weapon))
            {
                pc.RemoveWeapon(weapon);
            }
            else if(!pc.GetIsInventoryFull()){ pc.AddWeapon(weapon); } 
        }
        RefreshCurrentWeaponsText();
    }
}
