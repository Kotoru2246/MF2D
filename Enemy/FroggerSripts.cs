using UnityEngine;

public class FroggerSripts : Enemy
{
    [SerializeField] private Animator animator;

    protected override void Awake()
    {
        base.Awake();
    }
    
    protected override void FixedUpdate()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.gameObject.activeSelf == true)
        {
            // Flip enemy to face player
            if (PlayerController.Instance.transform.position.x > transform.position.x)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
            
            // Push back
            if (pushCounter > 0)
            {
                pushCounter -= Time.deltaTime;
                if (moveSpeed > 0)
                {
                    moveSpeed = -moveSpeed;
                }
                if (pushCounter <= 0)
                {
                    moveSpeed = Mathf.Abs(moveSpeed);
                }
            }
            
            // Move towards player
            direction = (PlayerController.Instance.transform.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, direction.y * moveSpeed);
            
            // Set animator moving bool based on direction
            if (direction == Vector3.zero)
            {
                animator.SetBool("moving", false);
            }
            else
            {
                animator.SetBool("moving", true);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("moving", false);
        }
    }
}
