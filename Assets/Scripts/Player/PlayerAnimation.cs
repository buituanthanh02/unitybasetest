using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Idle")]
    [SerializeField, Min(0f)] private float relaxDelay = 3f;

    private CharacterController controller;
    private Vector3 previousPosition;
    private float idleTimer;
    private bool hasFinished;

    private static readonly int Moving =
        Animator.StringToHash("Moving");

    private static readonly int Grounded =
        Animator.StringToHash("Grounded");

    private static readonly int Relaxed =
        Animator.StringToHash("Relaxed");

    private static readonly int Dead =
        Animator.StringToHash("Dead");

    private static readonly int Interact =
        Animator.StringToHash("Interact");

    private bool CanAnimate =>
        animator != null &&
        animator.isActiveAndEnabled &&
        animator.runtimeAnimatorController != null;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        previousPosition = transform.position;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void LateUpdate()
    {
        Vector3 displacement = transform.position - previousPosition;
        previousPosition = transform.position;

        if (hasFinished || !CanAnimate)
        {
            return;
        }

        // Đọc tốc độ ngang thực tế của Player.
        displacement.y = 0f;

        float horizontalSpeed =
            displacement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        bool moving = horizontalSpeed > 0.1f;
        bool grounded = controller.isGrounded;

        animator.SetBool(Moving, moving);
        animator.SetBool(Grounded, grounded);

        // Chỉ đếm thời gian nghỉ khi đứng yên trên mặt đất
        // và không đang thực hiện động tác tương tác.
        if (moving || !grounded || IsInteracting())
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.deltaTime;
        }

        animator.SetBool(Relaxed, idleTimer >= relaxDelay);
    }

    private bool IsInteracting()
    {
        AnimatorStateInfo currentState =
            animator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName("Base Layer.Interact"))
        {
            return true;
        }

        // Tính cả khoảng thời gian đang chuyển vào Interact.
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState =
                animator.GetNextAnimatorStateInfo(0);

            return nextState.IsName("Base Layer.Interact");
        }

        return false;
    }

    public void PlayInteract()
    {
        if (hasFinished || !CanAnimate)
        {
            return;
        }

        idleTimer = 0f;
        animator.SetBool(Relaxed, false);

        // Không xếp thêm lần phát khi đang chạy động tác này.
        if (IsInteracting())
        {
            return;
        }

        animator.ResetTrigger(Interact);
        animator.SetTrigger(Interact);
    }

    public void PlayDeath()
    {
        if (hasFinished)
        {
            return;
        }

        hasFinished = true;

        if (!CanAnimate)
        {
            return;
        }

        animator.ResetTrigger(Interact);
        animator.SetBool(Moving, false);
        animator.SetBool(Relaxed, false);
        animator.SetBool(Dead, true);
    }

    public void PlayWin()
    {
        if (hasFinished)
        {
            return;
        }

        hasFinished = true;

        if (!CanAnimate)
        {
            return;
        }

        animator.ResetTrigger(Interact);
        animator.SetBool(Moving, false);
        animator.SetBool(Grounded, true);
        animator.SetBool(Relaxed, true);
        animator.SetBool(Dead, false);

        // Khi thắng, chuyển về tư thế nghỉ.
        animator.CrossFadeInFixedTime(
            "Base Layer.RelaxedIdle",
            0.1f,
            0
        );
    }
}