using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public ThanhMau thanhMau;
    public Text hp;
    public string mapName;

    public Animator animator;
    public Rigidbody2D rb;

    private float movement;
    public float moveSpeed = 5f;
    public float jumpHeight = 5f;
    private bool facingRight = true;
    public bool isGround = true;

    public Transform attackPoint;
    public float attackRadius = 1.8f;
    public LayerMask attackLayer;

    private bool isDead = false;
    public Button restartButton;

    private float m_timeSinceAttack = 0.0f;
    private int m_currentAttack = 0;

    private bool block = false;

    public Transform groundCheck; // Điểm kiểm tra vị trí gần chân nhân vật
    public float checkRadius = 0.2f; // Bán kính kiểm tra
    public LayerMask groundLayer;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
        hp.text = currentHealth + " / " + maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead == true)
        {
            return;
        }

        if (currentHealth <= 0)
        {
            Die();
            animator.SetTrigger("Death");
            isDead = true;
        }

        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        

        movement = Input.GetAxis("Horizontal");

        if (movement < 0f && facingRight)
        {
            transform.eulerAngles = new Vector3(0f, -180f, 0f);
            facingRight = false;
        }
        else if (movement > 0f && !facingRight)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
            facingRight = true;
        }

        if (Input.GetKey(KeyCode.Space) && isGround)
        {
            Jump();
            //isGround = false;
            animator.SetBool("Jump", true);
        }

        if(Mathf.Abs(movement) > .1f)
        {
            animator.SetFloat("Run", 1f);
        }
        else if (movement < .1f)
        {
            animator.SetFloat("Run", 0f);
        }

        if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f)
        {

            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1))
        {
            block = true;
            animator.SetTrigger("Block");
            animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
        {
            block = false;
            animator.SetBool("IdleBlock", false);
        }

        // Kiểm tra nếu nhân vật đứng trên mặt đất
        isGround = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGround)
        {
            animator.SetBool("Jump", !isGround);
        }
        
    }

    private void FixedUpdate()
    {
        transform.position += new Vector3(movement, 0f, 0f) * Time.fixedDeltaTime * moveSpeed;

        if (isDead)
        {
            restartButton.gameObject.SetActive(true);
        }

    }

    void Jump()
    {
        //rb.AddForce(new Vector2(0f, jumpHeight), ForceMode2D.Impulse);
        rb.velocity = new Vector2(rb.velocity.x, 0); // Đặt vận tốc trục y về 0
        rb.AddForce(new Vector2(0f, jumpHeight), ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        if (block)
        {
            animator.SetTrigger("BlockAttack");
            return;
        }
        currentHealth -= damage;
        animator.SetTrigger("Hurt");
        hp.text = currentHealth + " / " + maxHealth;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
        //CameraShake.instance.Shake(.11f, 3f);
    }
    void Die()
    {
        
        FindObjectOfType<GameManager>().isGameActive = false;

        //Destroy(this.gameObject);
    }

    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);
        if (collInfo)
        {
            if (collInfo.gameObject.GetComponent<PatrolEnemy>() != null)
            {
                collInfo.gameObject.GetComponent<PatrolEnemy>().TakeDamage(1);
            }
            if (collInfo.gameObject.GetComponent<BeeEnemy>() != null)
            {
                collInfo.gameObject.GetComponent<BeeEnemy>().TakeDamage(1);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "VictoryPoint")
        {
            FindObjectOfType<SceneManagement>().LoadLevel();
        }
    }

    void OnDrawGizmosSelected()
    {
        if(attackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    //if(collision.gameObject.tag == "Ground")
    //    //{
    //    //    isGround = true;
    //    //    animator.SetBool("Jump", false);
    //    //}
    //    if (collision.gameObject.tag == "Ground")
    //    {
    //        foreach (ContactPoint2D contact in collision.contacts)
    //        {
    //            // Kiểm tra nếu tiếp xúc từ phía dưới
    //            if (contact.normal.y > 0.5f)
    //            {
    //                isGround = true;
    //                animator.SetBool("Jump", false);
    //                return; // Dừng kiểm tra thêm nếu đã xác nhận là mặt đất
    //            }
    //        }
    //    }
    //}

    public void RestartGame()
    {
        SceneManager.LoadScene(mapName);
    }

}
