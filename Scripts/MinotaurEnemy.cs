using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinatourEnemy : MonoBehaviour
{
    public Transform checkWallPoint;  // Điểm kiểm tra va chạm
    public LayerMask wallLayer;   // Lớp layer của tường


    public int maxHealth = 15;
    public int currentHealth;
    public ThanhMau thanhMau;

    public bool facingLeft = false;
    public float moveSpeed = 1f;
    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;
    public bool inRange = false;
    private Transform player;
    public float attackRange = 10f;
    public float retrieveDistance = 4.5f;
    public float chaseSpeed = 4f;
    public Animator animator;

    public Transform attackPoint;
    public float attackRadius = 1.4f;
    public LayerMask attackLayer;

    public float leftLimit = -10f;  // Giới hạn bên trái
    public float rightLimit = 10f;  // Giới hạn bên phải

    private bool canChangeState = true; // Kiểm tra xem có thể thay đổi trạng thái hay không
    private float stateChangeCooldown = 1f; // Thời gian đợi trước khi thay đổi trạng thái (1 giây)
    public bool isDead = false;
    private bool isAttacking = false; // Biến kiểm tra quái đang tấn công
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
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
        if (player == null)
        {
            FindTarget();
        }
        // Kiểm tra nếu người chơi trong phạm vi tấn công
        inRange = Vector2.Distance(transform.position, player.position) <= attackRange;
        if (inRange && canChangeState)
        {
            // Quay mặt về phía người chơi
            if (player.position.x > transform.position.x && facingLeft)
            {
                FlipDirection(false); // Quay sang phải
            }
            else if (player.position.x < transform.position.x && !facingLeft)
            {
                FlipDirection(true); // Quay sang trái
            }
            // Bắt đầu bộ đếm để thay đổi trạng thái
            ChasePlayer(); // Đuổi theo player
        }
        else
        {
            Patrol(); // Quay lại tuần tra
        }
    }

    void FindTarget()
    {
        // Tìm mục tiêu trong game (thường là Player)
        GameObject player1 = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player = player1.transform;
        }
    }
    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);

        if (collInfo)
        {
            PlayerController playerController = collInfo.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Gây sát thương cho người chơi
                playerController.TakeDamage(1);

                // Tính toán hướng knockback từ người chơi tới quái vật (ngược lại hướng tấn công)
                Vector2 knockbackDirection = (collInfo.transform.position - transform.position).normalized;

                // Áp dụng knockback cho người chơi
                playerController.ApplyKnockback(knockbackDirection);
            }
        }
    }




    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        currentHealth -= damage;
        //CameraShake.instance.Shake(.11f, 3f);
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
    }
    private void ChasePlayer()
    {
        
        if (!IsFacingWall() && !IsFacingCliff())
        {

            // Nếu khoảng cách lớn hơn `retrieveDistance`, di chuyển lại gần
            if (Vector2.Distance(transform.position, player.position) > retrieveDistance)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            }
            else if(isAttacking == false)
            {
                StartCoroutine(AttackCoroutine());
            }
        }
        else
        {
            StartCoroutine(ChangeStateCooldown());
            Patrol();
        }
    }
    private IEnumerator AttackCoroutine()
    {
        if(currentHealth <= 8)
        {
            isAttacking = true; // Đặt cờ trạng thái đang tấn công
            animator.SetTrigger("taunt"); // Gửi trigger cho animation tấn công
            yield return new WaitForSeconds(2f);
            animator.SetTrigger("skill");
            // Chờ thời gian animation hoàn thành (tuỳ thuộc vào thời gian attack animation của bạn)
            animator.SetTrigger("rest");
            yield return new WaitForSeconds(3.7f);

            isAttacking = false; // Đặt lại trạng thái sau khi tấn công xong
        }
        else
        {
            isAttacking = true; // Đặt cờ trạng thái đang tấn công
            animator.SetTrigger("attack"); // Gửi trigger cho animation tấn công

            // Chờ thời gian animation hoàn thành (tuỳ thuộc vào thời gian attack animation của bạn)
            animator.SetTrigger("rest");
            yield return new WaitForSeconds(3.7f);

            isAttacking = false; // Đặt lại trạng thái sau khi tấn công xong
        }
    }

    private IEnumerator ChangeStateCooldown()
    {
        canChangeState = false; // Ngừng thay đổi trạng thái
        yield return new WaitForSeconds(stateChangeCooldown); // Chờ 1 giây
        canChangeState = true; // Cho phép thay đổi trạng thái lại
    }
    private void OnDrawGizmosSelected()
    {
        if (FindObjectOfType<GameManager>().isGameActive == false)
        {
            return;
        }

        if (checkPoint == null)
        {
            return;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(checkPoint.position, Vector2.down * distance);

        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Vector2 direction = facingLeft ? Vector2.left : Vector2.right;
        Gizmos.DrawRay(checkWallPoint.position, direction * 1f);
    }

    private void Patrol()
    {
        // Kiểm tra xem có chướng ngại vật (tường) hoặc vực phía trước không
        bool facingWall = IsFacingWall();
        bool facingCliff = IsFacingCliff();
        bool reachedLimit = HasReachedLimit();

        // Nếu gặp tường hoặc vực hoặc đã đạt giới hạn, đảo chiều
        if (facingWall || facingCliff)
        {
            if (facingWall || facingCliff) // Chỉ đảo chiều khi gặp tường hoặc vực
            {
                FlipDirection(!facingLeft); // Quay đầu lại
            }
        }

        transform.Translate(Vector2.right * Time.deltaTime * moveSpeed);
    }


    // Hàm kiểm tra có vực trước mặt hay không
    private bool IsFacingCliff()
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);
        return hit.collider == null; // Không có vật cản bên dưới
    }

    // Hàm kiểm tra đã đi hết giới hạn tuần tra hay chưa
    private bool HasReachedLimit()
    {
        return (transform.position.x <= leftLimit && facingLeft) || (transform.position.x >= rightLimit && !facingLeft);
    }


    // Hàm để quay hướng nhân vật
    private void FlipDirection(bool faceLeft)
    {
        transform.eulerAngles = faceLeft ? new Vector3(0, -180, 0) : new Vector3(0, 0, 0);
        facingLeft = faceLeft;
    }

    private bool IsFacingWall()
    {

        // Kiểm tra chạm tường từ checkWallPoint hướng về phía đối diện
        RaycastHit2D hit = Physics2D.Raycast(checkWallPoint.position, facingLeft ? Vector2.left : Vector2.right, 0.2f, wallLayer);
        if (hit)
        {
            Debug.Log("Cham tuong");
        }
        return hit.collider != null; // Nếu có tường phía trước, trả về true

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
        yield return new WaitForSeconds(1.5f);

        // Sau khi hiệu ứng hoàn thành, hủy đối tượng
        Destroy(this.gameObject);
    }
}
