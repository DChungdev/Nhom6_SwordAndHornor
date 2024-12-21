using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{

    public Transform checkWallPoint;  // Điểm kiểm tra va chạm
    public LayerMask wallLayer;   // Lớp layer của tường


    public int maxHealth = 5;
    public int currentHealth;
    public ThanhMau thanhMau;

    public bool facingLeft = true;
    public float moveSpeed = 2f;
    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;
    public bool inRange = false;
    public Transform player;
    public float attackRange = 15f;
    public float retrieveDistance = 2.5f;
    public float chaseSpeed = 4f;
    public Animator animator;

    public Transform attackPoint;
    public float attackRadius = 1.4f;
    public LayerMask attackLayer;

    public float leftLimit = -10f;  // Giới hạn bên trái
    public float rightLimit = 10f;  // Giới hạn bên phải

    private bool canChangeState = true; // Kiểm tra xem có thể thay đổi trạng thái hay không
    private float stateChangeCooldown = 1f; // Thời gian đợi trước khi thay đổi trạng thái (1 giây)

    // Thêm trường AudioSource và AudioClip
    private AudioSource audioSource;
    public AudioClip attackSound;  // File âm thanh khi tấn công

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);

        // Lấy component AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Kiểm tra nếu người chơi trong phạm vi tấn công
        inRange = Vector2.Distance(transform.position, player.position) <= attackRange;

        // Nếu trong phạm vi tấn công và có thể thay đổi trạng thái
        if (inRange && canChangeState)
        {
             // Bắt đầu bộ đếm để thay đổi trạng thái
            ChasePlayer(); // Đuổi theo player
        }
        else
        {
            Patrol(); // Quay lại tuần tra
        }
    }

    private IEnumerator ChangeStateCooldown()
    {
        canChangeState = false; // Ngừng thay đổi trạng thái
        yield return new WaitForSeconds(stateChangeCooldown); // Chờ 1 giây
        canChangeState = true; // Cho phép thay đổi trạng thái lại
    }


    public void Attack()
    {
        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);

        if (collInfo)
        {
            if (collInfo.gameObject.GetComponent<PlayerController>() != null)
            {
                collInfo.gameObject.GetComponent<PlayerController>().TakeDamage(1);

                // Phát âm thanh khi tấn công
                if (attackSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(attackSound);  // Phát âm thanh
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
        animator.SetTrigger("Hurt");
        currentHealth -= damage;
        //CameraShake.instance.Shake(.11f, 3f);
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
    }

    void Die()
    {
        Debug.Log(this.transform.name + " Died.");
        Destroy(this.gameObject);
        // Thông báo cho SceneManagement
        FindObjectOfType<SceneManagement>().OnEnemyDefeated();
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        Gizmos.color = Color.red;
        Vector2 direction = facingLeft ? Vector2.left : Vector2.right;
        Gizmos.DrawRay(checkWallPoint.position, direction * 1f);
    }


    // Hàm ChasePlayer đã sửa để di chuyển đúng hướng
    private void ChasePlayer()
    {
        if (!IsFacingWall() && !IsFacingCliff())
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

            // Nếu khoảng cách lớn hơn `retrieveDistance`, di chuyển lại gần
            if (Vector2.Distance(transform.position, player.position) > retrieveDistance)
            {
                animator.SetBool("Attack1", false);
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            }
            else
            {
                // Tấn công khi ở gần người chơi
                animator.SetBool("Attack1", true);
            }
        }
        else
        {

            animator.SetBool("Attack1", false); // Không tấn công khi gặp vực
            StartCoroutine(ChangeStateCooldown());
            Patrol();
        }
    }

    // Hàm Patrol đã sửa để di chuyển đúng hướng
    private void Patrol()
    {
        // Kiểm tra xem có chướng ngại vật (tường) hoặc vực phía trước không
        bool facingWall = IsFacingWall();
        bool facingCliff = IsFacingCliff();
        bool reachedLimit = HasReachedLimit();

        // Nếu gặp tường hoặc vực hoặc đã đạt giới hạn, đảo chiều
        if (facingWall || facingCliff || reachedLimit)
        {
            if (facingWall || facingCliff) // Chỉ đảo chiều khi gặp tường hoặc vực
            {
                FlipDirection(!facingLeft); // Quay đầu lại
            }
        }

        transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);
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
        transform.eulerAngles = faceLeft ? new Vector3(0, 0, 0) : new Vector3(0, -180, 0);
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
}
