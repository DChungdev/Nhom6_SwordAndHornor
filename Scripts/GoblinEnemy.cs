using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinEnemy : MonoBehaviour
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
    public float attackRange = 10f;
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
