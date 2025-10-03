using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
	public PlayerControllerBase myPc;
	private Canvas mainCanvas;
	public TMP_Text healthText;

	public Image abilityIndicator1Radial;
	public Image abilityIndicator2Radial;
	public Image abilityIndicator3Radial;
	public Image abilityIndicator4Radial;
	public TMP_Text abilityIndicator1Timer;
	public TMP_Text abilityIndicator2Timer;
	public TMP_Text abilityIndicator3Timer;
	public TMP_Text abilityIndicator4Timer;
	public Image selector1;
	public Image selector2;
	private void Awake()
	{
		myPc = transform.root.GetComponent<PlayerControllerBase>();
		mainCanvas = GetComponentInChildren<Canvas>();
	}
	private void Update()
	{
		Refresh();
	}
	public void Refresh()
	{
		if (myPc == null) { return; }

		if (myPc.GetHeldItemNumber() < 1)
		{
			selector1.enabled = true;
			selector2.enabled = false;
		}
		else
		{
			selector1.enabled = false;
			selector2.enabled = true;
		}

		healthText.text = Mathf.CeilToInt(myPc.GetHealth()) + "";
		healthText.color = Color.Lerp(Color.red, Color.green, myPc.GetHealth() / 100f);

        if (myPc.allItems.Length >= 1 && abilityIndicator1Radial != null && abilityIndicator1Timer != null && abilityIndicator2Radial != null && abilityIndicator2Timer != null)
        {
            WeaponBase weapon = myPc.allItems[0];
            if (weapon != null)
            {
                if (weapon.GetPrimaryAbilityLevel() == -1)
                {
                    abilityIndicator1Radial.fillAmount = 1;
                    abilityIndicator1Timer.text = "X";
                }
                else
                {
                    if (weapon.GetIsReloading())
                    {
                        abilityIndicator1Radial.fillAmount = weapon.GetReloadTimeElapsed01();
                        abilityIndicator2Radial.fillAmount = abilityIndicator1Radial.fillAmount;
                        abilityIndicator1Timer.text = $"{weapon.GetReloadTimeLeft()}";
                        abilityIndicator2Timer.text = abilityIndicator1Timer.text;
                    }
                    else
                    {                   
                        if (weapon.GetPrimaryActionDelayElapsed())
                        {
                            abilityIndicator1Radial.fillAmount = 1;
                            if (weapon.primaryAmmoPerShot[weapon.GetPrimaryAbilityLevel()] == 0) { abilityIndicator1Timer.text = ""; }
                            else { abilityIndicator1Timer.text = $"{weapon.primaryAmmo}"; }
                        }
                        else
                        {
                            abilityIndicator1Radial.fillAmount = 1f - weapon.GetPrimaryFireDelayElapsedTime01();
                            abilityIndicator1Timer.text = $"{weapon.GetPrimaryActionDelayTimeLeft()}";
                        }
                    }
                }

                if (weapon.GetSecondaryAbilityLevel() == -1)
                {
                    abilityIndicator2Radial.fillAmount = 1;
                    abilityIndicator2Timer.text = "X";
                }
                else
                {
                    if (weapon.GetSecondaryActionDelayElapsed())
                    {
                        abilityIndicator2Radial.fillAmount = 1;
                        if (weapon.primaryAmmoPerShot[weapon.GetSecondaryAbilityLevel()] == 0) { abilityIndicator2Timer.text = ""; }
                        else { abilityIndicator2Timer.text = $"{weapon.secondaryAmmo}"; }
                    }
                    else
                    {
                        abilityIndicator2Radial.fillAmount = 1f - weapon.GetSecondaryFireDelayElapsedTime01();
                        abilityIndicator2Timer.text = $"{weapon.GetSecondaryActionDelayTimeLeft()}";
                    }
                }
            }
        }
        if (myPc.allItems.Length >= 2 && abilityIndicator3Radial != null && abilityIndicator3Timer != null && abilityIndicator4Radial != null && abilityIndicator4Timer != null)
		{
			WeaponBase weapon = myPc.allItems[1];
            if (weapon != null)
            {
                if (weapon.GetPrimaryAbilityLevel() == -1)
                {
                    abilityIndicator3Radial.fillAmount = 1;
                    abilityIndicator3Timer.text = "X";
                }
                else
                {
                    if (weapon.GetIsReloading())
                    {
                        abilityIndicator3Radial.fillAmount = weapon.GetReloadTimeElapsed01();
                        abilityIndicator4Radial.fillAmount = abilityIndicator3Radial.fillAmount;
                        abilityIndicator3Timer.text = $"{weapon.GetReloadTimeLeft()}";
                        abilityIndicator4Timer.text = abilityIndicator3Timer.text;
                    }
                    else
                    {
                        if (weapon.GetPrimaryActionDelayElapsed())
                        {
                            abilityIndicator3Radial.fillAmount = 1;
                            if (weapon.primaryAmmoPerShot[weapon.GetPrimaryAbilityLevel()] == 0) { abilityIndicator3Timer.text = ""; }
                            else { abilityIndicator3Timer.text = $"{weapon.primaryAmmo}"; }
                        }
                        else
                        {
                            abilityIndicator3Radial.fillAmount = 1f - weapon.GetPrimaryFireDelayElapsedTime01();
                            abilityIndicator3Timer.text = $"{weapon.GetPrimaryActionDelayTimeLeft()}";
                        }
                    }
                }

                if (weapon.GetSecondaryAbilityLevel() == -1)
                {
                    abilityIndicator4Radial.fillAmount = 1;
                    abilityIndicator4Timer.text = "X";
                }
                else
                {
                    if (weapon.GetSecondaryActionDelayElapsed())
                    {
                        abilityIndicator4Radial.fillAmount = 1;
                        if (weapon.primaryAmmoPerShot[weapon.GetSecondaryAbilityLevel()] == 0) { abilityIndicator4Timer.text = ""; }
                        else { abilityIndicator4Timer.text = $"{weapon.secondaryAmmo}"; }
                    }
                    else
                    {
                        abilityIndicator4Radial.fillAmount = 1f - weapon.GetSecondaryFireDelayElapsedTime01();
                        abilityIndicator4Timer.text = $"{weapon.GetSecondaryActionDelayTimeLeft()}";
                    }
                }
            }
        }
    }
}
