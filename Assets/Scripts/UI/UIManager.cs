using UnityEngine;

/// <summary>
/// Менеджер UI для подсказок подбора оружия.
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton для доступа из любого места
    public static UIManager Instance { get; private set; }

    [Header("Pickup Prompt")]
    [Tooltip("Объект с подсказкой подбора")]
    public GameObject pickupPromptObject;

    private void Awake()
    {
        // Singleton паттерн
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Скрываем подсказку при старте
        if (pickupPromptObject != null)
            pickupPromptObject.SetActive(false);
    }

    /// <summary>
    /// Показать/скрыть подсказку подбора
    /// </summary>
    public void ShowPickupPrompt(bool show)
    {
        if (pickupPromptObject != null)
        {
            pickupPromptObject.SetActive(show);
        }

        Debug.Log($"[UIManager] {(show ? "✅" : "❌")} Подсказка подбора: {(show ? "ВКЛ" : "ВЫКЛ")}");
    }
}