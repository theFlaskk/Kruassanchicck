using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 2f;
    public int damage = 10;

    private void Start()
    {
        // Уничтожить пулю через время, чтобы не засорять память
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Если попали во врага (позже добавим тег Enemy)
        if (collision.CompareTag("Enemy"))
        {
            // Тут будет логика урона
            Debug.Log("Hit Enemy!");
            Destroy(gameObject);
        }
        // Если попали в стену (тег Wall)
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}