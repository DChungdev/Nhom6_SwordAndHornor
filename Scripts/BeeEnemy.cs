using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeeEnemy : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public ThanhMau thanhMau;

    public Transform player;
    public Animator animator;

    public float attackRange = 10f; // Phạm vi để quái bắt đầu bay đến
    public float chaseSpeed = 4f; // Tốc độ bay đến
    public float retrieveDistance = 5.5f;
    public float flyUpDistance = 2f; // Khoảng cách bay lên sau khi tấn công
    public float flyUpSpeed = 5f; // Tốc độ bay lên sau khi tấn công
    public float patrolRange = 2f; // Khoảng cách quái đi qua trái và phải
    public float patrolSpeed = 2f; // Tốc độ di chuyển khi tuần tra

    private Vector3 originalPosition; // Vị trí ban đầu
    private bool inRange = false; // Kiểm tra người chơi có trong phạm vi không
    public bool facingLeft = true;
    private bool isFlyingUp = false; // Quái đang bay lên sau khi tấn công
    private Vector3 flyUpTargetPosition; // Vị trí đích để quái bay lên sau khi tấn công
    private bool movingLeft = true; // Hướng di chuyển khi tuần tra

    private bool isWaiting = false;   // Trạng thái đang chờ
    private float waitTimer = 0f;     // Thời gian đã chờ
    private float waitDuration = 0.5f; // Thời gian cần chờ (1 giây)

    public Transform attackPoint;
    public float attackRadius = 1.4f;
    public LayerMask attackLayer;

    // Thêm AudioSource và AudioClip cho âm thanh tấn công
    public AudioClip beeFlyingSound; // Âm thanh phát khi bay
    private AudioSource audioSource;
    private bool isPlayingFlyingSound = false; // Để theo dõi trạng thái âm thanh

    void Start()
    {
        originalPosition = transform.position; // Ghi nhớ vị trí ban đầu

        currentHealth = maxHealth;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);

        // Khởi tạo AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        if (beeFlyingSound != null)
        {
            audioSource.clip = beeFlyingSound;
            audioSource.loop = true; // Lặp âm thanh khi bay
        }
    }

    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            inRange = true;
            PlayFlyingSound(); // Bật âm thanh khi trong phạm vi
        }
        else
        {
            inRange = false;
            StopFlyingSound(); // Tắt âm thanh khi ra khỏi phạm vi
        }

        // Nếu đang chờ, thì tăng bộ đếm thời gian chờ
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            // Sau khi chờ đủ 1 giây, thực hiện hành động tiếp theo
            if (waitTimer >= waitDuration)
            {
                isWaiting = false;  // Kết thúc trạng thái chờ
                waitTimer = 0f;             // Reset bộ đếm thời gian
                Debug.Log("1 second wait after flying up completed.");
                // Tiến hành các hành động tiếp theo tại đây (nếu có)
                Debug.Log("Cho xog.");
            }

            return; // Nếu đang chờ, bỏ qua phần còn lại của logic
        }

        if (isFlyingUp)
        {
            HandleFlyUp();


            return; // Nếu đang bay lên, bỏ qua các xử lý khác
        }

        if (inRange)
        {
            // Quay mặt về hướng người chơi
            if (player.position.x > transform.position.x && facingLeft == true)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (player.position.x < transform.position.x && facingLeft == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingLeft = true;
            }

            // Tính toán độ cao mong muốn
            float desiredY = Mathf.Lerp(transform.position.y, player.position.y + 2f, Time.deltaTime * chaseSpeed);

            // Di chuyển theo trục X và điều chỉnh trục Y
            if (Vector2.Distance(transform.position, player.position) > retrieveDistance)
            {
                // Đuổi theo người chơi
                transform.position = new Vector3(
                    Mathf.MoveTowards(transform.position.x, player.position.x, chaseSpeed * Time.deltaTime),
                    desiredY,
                    transform.position.z
                );
            }
            else
            {
                // Kích hoạt trigger Attack khi bắt đầu tấn công
                animator.SetTrigger("Attack");
                PrepareFlyUp(); // Chuẩn bị cho quái bay lên sau khi tấn công
            }
        }
        else
        {
            Patrol(); // Quái tuần tra nếu người chơi ngoài phạm vi
        }
    }
    private void PlayFlyingSound()
    {
        if (!isPlayingFlyingSound && beeFlyingSound != null)
        {
            audioSource.Play();
            isPlayingFlyingSound = true;
        }
    }

    private void StopFlyingSound()
    {
        if (isPlayingFlyingSound)
        {
            audioSource.Stop();
            isPlayingFlyingSound = false;
        }
    }

    private void PrepareFlyUp()
    {
        isFlyingUp = true; // Bật trạng thái bay lên
        flyUpTargetPosition = new Vector3(transform.position.x, transform.position.y + flyUpDistance, transform.position.z);
    }

    //private void HandleFlyUp()
    //{
    //    // Di chuyển quái đến vị trí trên đầu
    //    transform.position = Vector3.MoveTowards(transform.position, flyUpTargetPosition, flyUpSpeed * Time.deltaTime);

    //    // Khi đạt đến vị trí đích, tắt trạng thái bay lên
    //    if (Vector2.Distance(transform.position, flyUpTargetPosition) < 0.1f)
    //    {
    //        isFlyingUp = false; // Hoàn thành việc bay lên
    //    }
    //}
    private void HandleFlyUp()
    {
        if (isFlyingUp && !isWaiting)
        {
            // Di chuyển quái đến vị trí trên đầu
            transform.position = Vector3.MoveTowards(transform.position, flyUpTargetPosition, flyUpSpeed * Time.deltaTime);

            // Khi đạt đến vị trí đích, tắt trạng thái bay lên và bắt đầu chờ
            if (Vector2.Distance(transform.position, flyUpTargetPosition) < 0.1f)
            {
                isFlyingUp = false;            // Hoàn thành việc bay lên
                isWaiting = true;      // Bắt đầu trạng thái chờ
                Debug.Log("Flying up completed, waiting for 1 second.");
            }
        }
        

    }

    private void Patrol()
    {
        float patrolLeft = originalPosition.x - patrolRange;
        float patrolRight = originalPosition.x + patrolRange;

        // Di chuyển qua trái hoặc phải
        if (movingLeft)
        {
            transform.position = new Vector3(
                Mathf.MoveTowards(transform.position.x, patrolLeft, patrolSpeed * Time.deltaTime),
                transform.position.y,
                transform.position.z
            );

            // Quay đầu nếu chạm biên trái
            if (Mathf.Abs(transform.position.x - patrolLeft) < 0.1f)
            {
                movingLeft = false;
                transform.eulerAngles = new Vector3(0, -180, 0);
            }
        }
        else
        {
            transform.position = new Vector3(
                Mathf.MoveTowards(transform.position.x, patrolRight, patrolSpeed * Time.deltaTime),
                transform.position.y,
                transform.position.z
            );

            // Quay đầu nếu chạm biên phải
            if (Mathf.Abs(transform.position.x - patrolRight) < 0.1f)
            {
                movingLeft = true;
                transform.eulerAngles = new Vector3(0, 0, 0);
            }
        }
    }


    public void Attack()
    {
        //// Phát âm thanh tấn công
        //PlayAttackSound();

        Collider2D collInfo = Physics2D.OverlapCircle(attackPoint.position, attackRadius, attackLayer);

        if (collInfo)
        {
            if (collInfo.gameObject.GetComponent<PlayerController>() != null)
            {
                collInfo.gameObject.GetComponent<PlayerController>().TakeDamage(1);
            }
        }
    }

    //// Hàm phát âm thanh
    //private void PlayAttackSound()
    //{
    //    if (beeAttackSound != null && audioSource != null)
    //    {
    //        audioSource.PlayOneShot(beeAttackSound);
    //    }
    //}

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            return;
        }
        animator.SetTrigger("Hurt");
        currentHealth -= damage;
        thanhMau.CapNhatThanhMau((float)currentHealth, (float)maxHealth);
        //CameraShake.instance.Shake(.11f, 3f);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange); // Vẽ phạm vi tấn công

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(originalPosition.x - patrolRange, originalPosition.y, originalPosition.z),
                        new Vector3(originalPosition.x + patrolRange, originalPosition.y, originalPosition.z)); // Vẽ phạm vi tuần tra
    }
}
