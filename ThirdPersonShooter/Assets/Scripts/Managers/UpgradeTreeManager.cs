using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeTreeManager : MonoBehaviour
{
    public static UpgradeTreeManager instance;
    public List<Image> branch1TierImages;
    public List<Image> branch2TierImages;
    public List<Image> branch3TierImages;
    public List<Image> branch4TierImages;
    public List<Button> upgradeButtons;
    public TMP_Text pointsSpendableText;
    private Canvas mainCanvas;
    public Color tierOwnedColor = Color.green;
    public Color tierNotOwnedColor = Color.red;
    public bool menuOpen { get; private set; } = false;

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
        mainCanvas = GetComponentInChildren<Canvas>();
    }
    private void Start()
    {
        upgradeButtons[0].onClick.RemoveAllListeners();
        upgradeButtons[0].onClick.AddListener(delegate { TrySpendPoint(1); });
        upgradeButtons[1].onClick.RemoveAllListeners();
        upgradeButtons[1].onClick.AddListener(delegate { TrySpendPoint(2); });
        upgradeButtons[2].onClick.RemoveAllListeners();
        upgradeButtons[2].onClick.AddListener(delegate { TrySpendPoint(3); });
        upgradeButtons[3].onClick.RemoveAllListeners();
        upgradeButtons[3].onClick.AddListener(delegate { TrySpendPoint(4); });
        RefreshBranchImages();
        ToggleMenu(false);
    }
    private void Update()
    {
        if (InputManager.instance.openUpgradeTree)
        {
            ToggleMenu(!menuOpen);
        }
    }
    public void ToggleMenu(bool state)
    {
        mainCanvas.enabled=state;
        menuOpen=state;
        Cursor.visible = state;
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
    }
    private void TrySpendPoint(int branchNum)
    {
        PlayerControllerBase pc = GameManager.instance.localPlayer;
        if (pc == null) { Debug.LogWarning("Tried to spend point on upgrade tree before player was valid!"); return; }
        pc.myUpgradeTree.IncrementBranch(branchNum);
        RefreshBranchImages();
    }
    private void RefreshBranchImages()
    {
        PlayerControllerBase pc = GameManager.instance.localPlayer;
        if (pc == null) { Debug.LogWarning("Tried to update upgrade tree images before player was valid!"); return; }
        pointsSpendableText.text = $"Points to spend : {pc.myUpgradeTree.pointsToSpend}\nPoints spent : {pc.myUpgradeTree.GetNumPointsSpent()} / {pc.myUpgradeTree.maxPointsSpendable}";

        for (int i = 0; i < branch1TierImages.Count; i++)
        {
            bool condition = i < pc.myUpgradeTree.GetLevelOfBranch(1);
            branch1TierImages[i].color = (condition ? tierOwnedColor : tierNotOwnedColor);
        }
        for (int i = 0; i < branch2TierImages.Count; i++)
        {
            bool condition = i < pc.myUpgradeTree.GetLevelOfBranch(2);
            branch2TierImages[i].color = (condition ? tierOwnedColor : tierNotOwnedColor);
        }
        for (int i = 0; i < branch3TierImages.Count; i++)
        {
            bool condition = i < pc.myUpgradeTree.GetLevelOfBranch(3);
            branch3TierImages[i].color = (condition ? tierOwnedColor : tierNotOwnedColor);
        }
        for (int i = 0; i < branch4TierImages.Count; i++)
        {
            bool condition = i < pc.myUpgradeTree.GetLevelOfBranch(4);
            branch4TierImages[i].color = (condition ? tierOwnedColor : tierNotOwnedColor);
        }
        for (int i = 0; i < upgradeButtons.Count; i++)
        {
            upgradeButtons[i].gameObject.SetActive(false);
        }
        if (pc.myUpgradeTree.pointsToSpend > 0)
        {
            if (pc.myUpgradeTree.GetPointsLeftOnBranch(1) > 0) { upgradeButtons[0].gameObject.SetActive(true); }
            if (pc.myUpgradeTree.GetPointsLeftOnBranch(2) > 0) { upgradeButtons[1].gameObject.SetActive(true); }
            if (pc.myUpgradeTree.GetPointsLeftOnBranch(3) > 0) { upgradeButtons[2].gameObject.SetActive(true); }
            if (pc.myUpgradeTree.GetPointsLeftOnBranch(4) > 0) { upgradeButtons[3].gameObject.SetActive(true); }
        }
    }
}
public class UpgradeTree
{
    public int pointsToSpend = 10;
    public int maxPointsSpendable = 10;
    public const int maxPointsPerBranch = 4;
    private int branch1 = 0;
    private int branch2 = 0;
    private int branch3 = 0;
    private int branch4 = 0;

    public void IncrementBranch(int branchNum)
    {
        Debug.Log($"Trying to increment points on branch num : {branchNum}");
        if (GetMaxPointsSpent(0)) { Debug.LogWarning($"Failed to add points due to GetMaxPointsSpent() returning true. Branch num : {branchNum}"); return; }
        if (GetPointsLeftOnBranch(branchNum) <= 0) { Debug.LogWarning($"Failed to add points due to GetPointsLeftOnBranch() returning true. Branch num : {branchNum}"); return; }
        if (pointsToSpend <= 0) { Debug.LogWarning($"Failed to add points to due pointsToSpend <= 0. Branch num : {branchNum}"); return; }
        pointsToSpend--;
        switch (branchNum)
        {
            case 1:
                branch1++;
                if(branch1>=maxPointsPerBranch){branch1=maxPointsPerBranch;}
                break;
            case 2:
                branch2++;
                if (branch2 >= maxPointsPerBranch) { branch2 = maxPointsPerBranch; }
                break;
            case 3:
                branch3++;
                if (branch3 >= maxPointsPerBranch) { branch3 = maxPointsPerBranch; }
                break;
            case 4:
                branch4++;
                if (branch4 >= maxPointsPerBranch) { branch4 = maxPointsPerBranch; }
                break;
            default:
                break;
        }
    }

    private bool GetMaxPointsSpent(float delta)
    {
        return (branch1 + branch2 + branch3 + branch4 + delta) >= maxPointsSpendable;
    }
    public int GetNumPointsSpent()
    {
        return branch1 + branch2 + branch3 + branch4;
    }
    public int GetPointsLeftOnBranch(int branchNum)
    {
        switch (branchNum)
        {
            case 1:
                return maxPointsPerBranch - branch1;
                break;
            case 2:
                return maxPointsPerBranch - branch2;
                break;
            case 3:
                return maxPointsPerBranch - branch3;
                break;
            case 4:
                return maxPointsPerBranch - branch4;
                break;
            default:
                return 0;
                break;
        }
    }
   
    public int GetLevelOfBranch(int branchNum)
    {
        switch (branchNum)
        {
            case 1:
                return branch1;
                break;
            case 2:
                return branch2;
                break;
            case 3:
                return branch3;
                break;
            case 4:
                return branch4;
                break;
            default:
                return 0;
                break;
        }
    }
}