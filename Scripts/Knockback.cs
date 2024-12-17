using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    public float knockbackForce = 5f;   // Lực knockback
    public float knockbackDuration = 0.2f; // Thời gian knockback
    private bool isKnockedBack = false;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Gọi từ bên ngoài để thực hiện knockback
    public void ApplyKnockback(Vector2 direction)
    {
        if (!isKnockedBack)
        {
            StartCoroutine(KnockbackCoroutine(direction));
        }
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction)
    {
        isKnockedBack = true;
        rb.velocity = direction * knockbackForce; // Áp dụng lực knockback

        // Chờ thời gian knockback
        yield return new WaitForSeconds(knockbackDuration);

        rb.velocity = Vector2.zero;  // Dừng lại sau khi knockback
        isKnockedBack = false;
    }
}

