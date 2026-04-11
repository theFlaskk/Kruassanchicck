using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // За кем следить (игрок)
    public float smoothSpeed = 0.1f; // Плавность (0.01 = очень плавно, 1 = мгновенно)
    public Vector3 offset;         // Смещение камеры (обычно по Z)

    private void LateUpdate()
    {
        if (target == null) return;

        // Желаемая позиция камеры = позиция игрока + смещение
        Vector3 desiredPosition = target.position + offset;

        // Плавно двигаемся к этой позиции (только X и Y, Z не трогаем!)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Сохраняем Z камеры, чтобы она не улетела вглубь
        smoothedPosition.z = transform.position.z;

        transform.position = smoothedPosition;

        // ⚠️ ВАЖНО: Мы НЕ меняем rotation камеры! Она всегда смотрит вниз.
    }
}