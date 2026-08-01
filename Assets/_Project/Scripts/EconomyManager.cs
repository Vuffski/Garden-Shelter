using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public int startingGold = 100;
    private int currentGold;
    public TMP_Text goldLabel;

    private void Awake()
    {
        currentGold = startingGold;
        UpdateLabel();
    }

    public bool CanAfford(int cost)
    {
        return currentGold >= cost;
    }

    public void Spend(int cost)
    {
        currentGold -= cost;
        UpdateLabel();
    }

    public void Earn(int amount)
    {
        if (amount <= 0) return;
        currentGold += amount;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (goldLabel != null)
        {
            goldLabel.text = "$" + currentGold;
        }
    }
}
