using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    public static Money Instance { get; private set; }

    public float startMoney = 0f;
    public float Balance { get; private set; }

    [SerializeField] private TMP_Text cashLabel;
    [SerializeField] private string cashPrefix = "CASH: ";

    private int lastShown = int.MinValue;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Balance = startMoney;

        RefreshLabel();
    }

    public void AddMoney(float amount)
    {
        Balance += amount;
        RefreshLabel();
    }

    public bool RemoveMoney(float amount)
    {
        if (Balance < amount)
            return false;

        Balance -= amount;
        RefreshLabel();
        return true;
    }

    public void Bankrot()
    {
        Balance = 0f;
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (!cashLabel)
            return;

        int shown = Mathf.RoundToInt(Balance);

        if (shown == lastShown)
            return;

        lastShown = shown;

        cashLabel.text = cashPrefix + shown.ToString("N0");
    }
}
