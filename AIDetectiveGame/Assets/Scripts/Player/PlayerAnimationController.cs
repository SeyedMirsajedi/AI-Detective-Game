using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("Animation Reference")]
    [SerializeField] private Animator animator;

    private Rigidbody2D playerRigidbody;

    private static readonly int MoveSpeedHash =
        Animator.StringToHash("MoveSpeed");

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (animator == null)
        {
            return;
        }

        float horizontalSpeed =
            Mathf.Abs(playerRigidbody.linearVelocity.x);

        animator.SetFloat(MoveSpeedHash, horizontalSpeed);
    }
}