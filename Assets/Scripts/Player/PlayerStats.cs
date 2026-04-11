using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float moveSpeed = 5f;
    public float fireRate = 1f; // Выстрелов в секунду
    public int maxHealth = 100;

    // Текущие значения (могут меняться артефактами)
    public float CurrentMoveSpeed { get; private set; }
    public float CurrentFireRate { get; private set; }

    private void Awake()
    {
        // Инициализация текущих статов базовыми
        ResetStats();
    }

    public void ResetStats()
    {
        CurrentMoveSpeed = moveSpeed;
        CurrentFireRate = fireRate;
    }

    // Метод, который позже будут вызывать артефакты
    public void ModifyStat(string statName, float modifier)
    {
        // Пока заглушка, позже здесь будет логика сложения модификаторов
        if (statName == "Speed") CurrentMoveSpeed += modifier;
        if (statName == "FireRate") CurrentFireRate += modifier;

        Debug.Log($"Stat changed: {statName} -> {modifier}");
    }
}