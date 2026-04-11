using UnityEngine;

public class DebugFlip : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera cam;
    private bool lastFlipValue = false;

    private void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        cam = Camera.main;

        Debug.Log($"[DEBUG] SpriteRenderer: {sr != null}");
        Debug.Log($"[DEBUG] Камера: {cam != null}");

        // Проверка на ДРУГИЕ SpriteRenderer
        var allRenderers = GetComponentsInChildren<SpriteRenderer>();
        Debug.Log($"[DEBUG] Найдено SpriteRenderer: {allRenderers.Length}");
        foreach (var renderer in allRenderers)
        {
            Debug.Log($"[DEBUG]   - {renderer.gameObject.name} на {renderer.transform.parent?.name ?? "ROOT"}");
        }
    }

    private void LateUpdate()
    {
        if (sr == null || cam == null) return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)mousePos - (Vector2)transform.position;

        bool shouldFlip = dir.x < 0;

        // Логируем ТОЛЬКО когда значение меняется
        if (shouldFlip != lastFlipValue)
        {
            Debug.Log($"[DEBUG] Кадр {Time.frameCount}: СМЕНА! Было {lastFlipValue} → Стало {shouldFlip}");
            lastFlipValue = shouldFlip;
        }

        // Применяем
        sr.flipX = shouldFlip;

        // Проверяем что получилось
        if (sr.flipX != shouldFlip)
        {
            Debug.LogError($"[DEBUG] ⚠️ КОНФЛИКТ! Установили {shouldFlip}, но стало {sr.flipX}");
        }
    }
}