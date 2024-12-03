using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarEnemy : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public ThanhMau thanhMau;

    public Transform player;
    public Animator animator;

    //public float retrieveDistance = 5.5f;
    public float patrolRange = 2f; // Khoảng cách quái đi qua trái và phải
    public float patrolSpeed = 2f; // Tốc độ di chuyển khi tuần tra

    private Vector3 originalPosition; // Vị trí ban đầu
    private bool inRange = false; // Kiểm tra người chơi có trong phạm vi không
    public bool facingLeft = true;
    private bool movingLeft = true; // Hướng di chuyển khi tuần tra

    public Transform attackPoint;
    public float attackRadius = 1.4f;
    public LayerMask attackLayer;


    void Start()
    {
        originalPosition = transform.position; // Ghi nhớ vị trí ban đầu

        currentHealth = maxHealth;
    }

    void Update()
    {

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
        }
        else
        {
            Patrol(); // Quái tuần tra nếu người chơi ngoài phạm vi
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
   
}
