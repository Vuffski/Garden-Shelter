using System;
using UnityEngine;

public class TreatManager : MonoBehaviour
{
    public int OwnedTreats { get; private set; }

    public event Action<int> OnTreatsChanged;

    public void AddTreats(int amount)
    {
        if (amount <= 0) return;
        OwnedTreats += amount;
        OnTreatsChanged?.Invoke(OwnedTreats);
    }

    public bool TrySpendTreats(int amount)
    {
        if (amount <= 0) return false;
        
        if (OwnedTreats >= amount)
        {
            OwnedTreats -= amount;
            OnTreatsChanged?.Invoke(OwnedTreats);
            return true;
        }
        return false;
    }
}
