using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardBoss : MonoBehaviour
{
    public GameObject projectilePrefab; // Prefab của đạn
    public Transform firePoint;        // Điểm bắn đạn
    public float attackCooldown = 3f;  // Thời gian giữa các lần bắn
    public float projectileSpeed = 10f; // Tốc độ bay của đạn
    public float detectionRange = 15f; // Tầm phát hiện mục tiêu
    private float cooldownTimer = 0f;

    private Transform target;

    public Animator animator;
    public bool facingLeft = true;

    public int maxHealth = 25;
    public int currentHealth;
    public ThanhMau thanhMau;

    public bool isDead = false;
    public Rigidbody2D rb;

    public bool inRange = false;
    public bool isFlying = false;
    // Start is called before the first frame update
    void Start()
    {
        // Tìm mục tiêu ban đầu (Player)
        target = GameObject.FindWithTag("Player").transform;
        currentHealth = maxHealth;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
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
            isDead = true;
            return;
        }
        // Tìm kiếm mục tiêu (ví dụ: player)
        if (target == null)
        {
            FindTarget();
        }
        else
        {
            // Kiểm tra nếu người chơi trong phạm vi tấn công
            inRange = Vector2.Distance(transform.position, target.position) <= detectionRange;
            if(inRange)
            {
                // Quay mặt về phía người chơi
                if (target.position.x > transform.position.x && facingLeft)
                {
                    FlipDirection(false); // Quay sang phải
                }
                else if (target.position.x < transform.position.x && !facingLeft)
                {
                    FlipDirection(true); // Quay sang trái
                }
                // Tính khoảng cách tới mục tiêu
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= detectionRange)
                {
                    //if(isFlying == false)
                    //{
                    //    StartCoroutine(FlyUp());

                    //}
                    //else
                    //{
                        Attack();
                    //}
                    
                }
            }
        }
    }
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        animator.SetTrigger("hurt");
        currentHealth -= damage;
        //CameraShake.instance.Shake(.11f, 3f);
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
    }

    void FindTarget()
    {
        // Tìm mục tiêu trong game (thường là Player)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
  
    // Coroutine sẽ thay đổi trạng thái và tạo đạn sau một thời gian chờ
    private IEnumerator AttackWithCooldown()
    {
        animator.SetTrigger("attack");
        yield return new WaitForSeconds(1f); // Chờ 1 giây

        // Tạo đạn sau khi bay lên
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Tính toán hướng đạn đến vị trí mục tiêu
        Vector3 targetOffset = target.position + new Vector3(0, 1f, 0); // Thêm 1f vào trục Y để bắn cao hơn
        Vector3 projectileDirection = targetOffset - firePoint.position;

        float angle = Mathf.Atan2(projectileDirection.y, projectileDirection.x) * Mathf.Rad2Deg;

        // Quay đạn về phía người chơi
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Áp dụng vận tốc cho đạn
        Rigidbody2D rbProjectile = projectile.GetComponent<Rigidbody2D>();
        if (rbProjectile != null)
        {
            rbProjectile.velocity = projectileDirection.normalized * projectileSpeed;
        }

    }

    void Attack()
    {
        // Đếm ngược cooldown
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            cooldownTimer = attackCooldown;

            // Bắt đầu coroutine
            StartCoroutine(AttackWithCooldown());
        }
    }

    private void FlipDirection(bool faceLeft)
    {
        transform.eulerAngles = faceLeft ? new Vector3(0, 0, 0) : new Vector3(0, 180, 0);
        facingLeft = faceLeft;
    }

    void Die()
    {
        animator.SetTrigger("death");
        // Kích hoạt hiệu ứng Die (nếu có)
        StartCoroutine(DieCoroutine());
        // Thông báo cho SceneManagement
        FindObjectOfType<SceneManagement>().OnEnemyDefeated();
    }
    private IEnumerator DieCoroutine()
    {
        // Chờ cho hiệu ứng Die kết thúc
        yield return new WaitForSeconds(2.5f);

        // Sau khi hiệu ứng hoàn thành, hủy đối tượng
        Destroy(this.gameObject);
    }
}
