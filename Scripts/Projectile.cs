using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1; // Lượng sát thương mà đạn gây ra
    public float lifetime = 5f; // Thời gian tồn tại của đạn (nếu không trúng mục tiêu)

    void Start()
    {
        Destroy(gameObject, lifetime);  // Xóa đạn sau một khoảng thời gian
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Kiểm tra va chạm với Player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Gọi phương thức nhận sát thương trên Player
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(damage);
        }

        // Hủy đạn khi va chạm
        Destroy(gameObject);
    }
}
