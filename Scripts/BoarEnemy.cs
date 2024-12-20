using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarEnemy : MonoBehaviour
{
    public Transform checkWallPoint;  // Điểm kiểm tra va chạm
    public LayerMask wallLayer;   // Lớp layer của tường
    public bool meetWall = false;

    public int maxHealth = 5;
    public int currentHealth;
    public ThanhMau thanhMau;

    public Transform player;
    public Animator animator;

    public float patrolRange = 5f; // Khoảng cách quái đi qua trái và phải
    public float patrolSpeed = 2f; // Tốc độ di chuyển khi tuần tra
    public float chaseSpeed = 4f; // Tốc độ khi lao tới người chơi
    public float retreatDistance = 2f; // Khoảng cách lùi lại từ người chơi
    public float attackRange = 8f; // Phạm vi phát hiện người chơi
    public float attackRadius = 2f; // Phạm vi tấn công gần
    public LayerMask attackLayer; // Lớp đối tượng bị tấn công
    public float chargeDelay = 0.5f; // Thời gian chờ trước khi lao tới
    public float restTime = 1f; // Thời gian nghỉ giữa các lần tấn công

    private Vector3 originalPosition; // Vị trí ban đầu
    private bool inRange = false; // Kiểm tra người chơi có trong phạm vi không
    private bool isRetreating = false; // Trạng thái lùi lại
    private bool isCharging = false; // Trạng thái lao tới
    private bool isAttacking = false; // Trạng thái đang tấn công
    private bool isResting = false; // Trạng thái nghỉ
    public bool movingLeft = true; // Hướng di chuyển khi tuần tra
    public bool facingLeft = true;

    public Transform attackPoint;

    public bool canChangeState = true; // Kiểm tra xem có thể thay đổi trạng thái hay không
    private float stateChangeCooldown = 1f; // Thời gian đợi trước khi thay đổi trạng thái (1 giây)

    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;

    // Thêm biến âm thanh
    private AudioSource audioSource; // Đối tượng AudioSource để phát âm thanh
    public AudioClip boarAttackSound; // Âm thanh tấn công của boar

    void Start()
    {
        originalPosition = transform.position; // Ghi nhớ vị trí ban đầu
        currentHealth = maxHealth;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);

        // Tạo AudioSource và gán cho đối tượng này
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // Không phát âm thanh ngay khi khởi tạo
    }

    void Update()
    {
        if(currentHealth <= 0)
        {
            Die();
            return;
        }
        // Kiểm tra nếu người chơi trong phạm vi tấn công
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            inRange = true;
        }
        else
        {
            inRange = false;
        }

        // Nếu trong phạm vi tấn công và không nghỉ, thực hiện tấn công
        if (inRange && !isResting && canChangeState)
        {
            //FaceDirection(player.position);
            // Quay mặt về phía người chơi
            if (player.position.x > transform.position.x && facingLeft)
            {
                FlipDirection(false); // Quay sang phải
            }
            else if (player.position.x < transform.position.x && !facingLeft)
            {
                FlipDirection(true); // Quay sang trái
            }
            //transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            StartAttack();
        }
        else if (!isResting)
        {
            Patrol(); // Quái tuần tra nếu người chơi ngoài phạm vi
        }
    }

    private void StartAttack()
    {
        if (!IsFacingWall() && !IsFacingCliff())
        {
            if (isAttacking) return; // Nếu đang tấn công thì không tấn công lại

            // Di chuyển đến người chơi cho đến khi đủ gần
            animator.SetBool("inRange", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);

            // Kiểm tra xem BoarEnemy đã đến được mục tiêu chưa
            if (Vector2.Distance(transform.position, player.position) < attackRadius)
            {
                // Nếu đã gần đến mục tiêu, dừng lại và thực hiện hành động tấn công
                if (!isAttacking)
                {
                    isAttacking = true;

                    // Phát âm thanh tấn công
                    PlayAttackSound();

                    // Kiểm tra va chạm và gây sát thương
                    Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);
                    if (collInfo && collInfo.gameObject.GetComponent<PlayerController>() != null)
                    {
                        collInfo.gameObject.GetComponent<PlayerController>().TakeDamage(1);
                    }

                    // Sau khi tấn công xong, chuyển sang trạng thái nghỉ
                    Invoke(nameof(EndAttack), 0.5f); // Giả lập thời gian tấn công (0.5s)
                }
            }
        }
        else
        {
            animator.SetBool("inRange", false);
            StartCoroutine(ChangeStateCooldown());
            Patrol();
        }
    }

    private void PlayAttackSound()
    {
        // Kiểm tra xem có âm thanh để phát không
        if (boarAttackSound != null)
        {
            audioSource.PlayOneShot(boarAttackSound); // Phát âm thanh tấn công
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
        animator.SetBool("inRange", false);

        // Chuyển sang trạng thái nghỉ
        StartResting();
    }

    private void StartResting()
    {
        isResting = true;
        animator.SetBool("rest", true); // Đặt biến "rest" trong Animator thành true

        // Đợi trong một khoảng thời gian nhất định, sau đó quay lại tấn công
        Invoke(nameof(EndResting), restTime);
    }

    private void EndResting()
    {
        isResting = false;
        animator.SetBool("rest", false); // Đặt biến "rest" trong Animator thành false

        // Nếu người chơi vẫn trong phạm vi, tiếp tục tấn công
        if (inRange)
        {
            StartAttack();
        }
    }

    private void Patrol()
    {
        animator.SetBool("inRange", false);
        float patrolLeft = originalPosition.x - patrolRange;
        float patrolRight = originalPosition.x + patrolRange;

        // Di chuyển qua trái hoặc phải
        if (movingLeft)
        {
            if (IsFacingWall() || IsFacingCliff())  // Kiểm tra nếu đang chạm tường
            {
                movingLeft = false;  // Quay lại hướng phải
                FlipDirection(false);  // Quay đầu sang phải
            }

            transform.position = new Vector3(
                Mathf.MoveTowards(transform.position.x, patrolLeft, patrolSpeed * Time.deltaTime),
                transform.position.y,
                transform.position.z
            );

            // Quay đầu nếu chạm biên trái
            if (Mathf.Abs(transform.position.x - patrolLeft) < 0.1f)
            {
                movingLeft = false; // Thay đổi hướng sang phải
                FlipDirection(false); // Quay đầu sang phải
            }
        }
        else
        {
            if (IsFacingWall() || IsFacingCliff())  // Kiểm tra nếu đang chạm tường
            {
                movingLeft = true;  // Quay lại hướng trái
                FlipDirection(true); // Quay đầu sang trái
            }

            transform.position = new Vector3(
                Mathf.MoveTowards(transform.position.x, patrolRight, patrolSpeed * Time.deltaTime),
                transform.position.y,
                transform.position.z
            );

            // Quay đầu nếu chạm biên phải
            if (Mathf.Abs(transform.position.x - patrolRight) < 0.1f)
            {
                movingLeft = true;  // Thay đổi hướng sang trái
                FlipDirection(true); // Quay đầu sang trái
            }
        }
    }
    private void FaceDirection(Vector3 targetPosition)
    {
        if ((targetPosition.x > transform.position.x && !facingLeft) ||
            (targetPosition.x < transform.position.x && facingLeft))
        {
            facingLeft = !facingLeft;
            transform.eulerAngles = new Vector3(0, facingLeft ? -180 : 0, 0);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        animator.SetTrigger("hit");
        currentHealth -= damage;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
    }

    void Die()
    {
        Debug.Log(this.transform.name + " Died.");
        Destroy(this.gameObject);
        FindObjectOfType<SceneManagement>().OnEnemyDefeated();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(checkPoint.position, Vector2.down * distance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // Vẽ phạm vi tấn công

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange - retreatDistance); // Vẽ phạm vi lùi lại
    }

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
    private bool IsFacingCliff()
    {
        RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);
        return hit.collider == null; // Không có vật cản bên dưới
    }

    private IEnumerator ChangeStateCooldown()
    {
        canChangeState = false; // Ngừng thay đổi trạng thái
        yield return new WaitForSeconds(stateChangeCooldown); // Chờ 1 giây
        canChangeState = true; // Cho phép thay đổi trạng thái lại
    }
}





