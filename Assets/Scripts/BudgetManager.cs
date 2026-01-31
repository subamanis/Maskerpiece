using UnityEngine;
using TMPro;

public class BudgetManager : MonoBehaviour
{
    public static BudgetManager Instance { get; private set; }

    [Header("Settings")]
    public int startingBudget = 1000;

    [Header("UI")]
    public TextMeshProUGUI budgetText;

    private int currentBudget;

    void Awake()
    {
        Instance = this;
        currentBudget = startingBudget;
    }

    void Start()
    {
        UpdateUI();
    }

    public bool CanAfford(int price)
    {
        return currentBudget >= price;
    }

    public bool TrySpend(int price)
    {
        if (!CanAfford(price)) return false;

        currentBudget -= price;
        UpdateUI();
        return true;
    }

    public void AddMoney(int amount)
    {
        currentBudget += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (budgetText != null)
            budgetText.text = $"${currentBudget}";
    }
}
