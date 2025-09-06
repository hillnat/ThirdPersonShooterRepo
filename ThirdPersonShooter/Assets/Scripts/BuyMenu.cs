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
        Debug.Log(BuyM1911.onClick);
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
        string newText= "Weapons :\n";
        
        for(int i=0; i<pc.currentWeapons.Count; i++)
        {
            newText += pc.currentWeapons[i].weaponName + "\n";
        }
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
            else if(!pc.isInventoryFull){ pc.AddWeapon(weapon); } 
        }
        RefreshCurrentWeaponsText();
    }
}
