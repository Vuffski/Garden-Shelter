using UnityEngine;
using TMPro;

public class TreatsDisplayView : MonoBehaviour
{
    [SerializeField] private TreatManager treatManager;
    [SerializeField] private TMP_Text treatsLabel;
    [SerializeField] private string prefix = "";

    private void Start()
    {
        if (treatManager != null)
        {
            UpdateLabel(treatManager.OwnedTreats);
        }
    }

    private void OnEnable()
    {
        if (treatManager != null)
        {
            treatManager.OnTreatsChanged += UpdateLabel;
        }
    }

    private void OnDisable()
    {
        if (treatManager != null)
        {
            treatManager.OnTreatsChanged -= UpdateLabel;
        }
    }

    private void UpdateLabel(int currentTreats)
    {
        if (treatsLabel != null)
        {
            treatsLabel.text = prefix + currentTreats;
        }
    }
}
