using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 previousPosition;
    private bool hasFinished;

    private static readonly int Moving =
        Animator.StringToHash("Moving");

    private static readonly int Grounded =
        Animator.StringToHash("Grounded");

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

        // Chỉ xét chuyển động ngang để phân biệt Idle và Run.
        displacement.y = 0f;

        float horizontalSpeed =
            displacement.magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        animator.SetBool(Moving, horizontalSpeed > 0.1f);
        animator.SetBool(Grounded, controller.isGrounded);
    }

    public void PlayInteract()
    {
        if (hasFinished || !CanAnimate)
        {
            return;
        }

        animator.SetTrigger(Interact);
    }

    public void PlayDeath()
    {
        hasFinished = true;

        if (!CanAnimate)
        {
            return;
        }

        animator.ResetTrigger(Interact);
        animator.SetBool(Moving, false);
        animator.SetBool(Dead, true);
    }

    public void PlayWin()
    {
        hasFinished = true;

        if (!CanAnimate)
        {
            return;
        }

        animator.ResetTrigger(Interact);
        animator.SetBool(Moving, false);
        animator.SetBool(Grounded, true);

        // Khi thắng, dừng ở Idle kể cả đang nhảy/tương tác.
        animator.CrossFadeInFixedTime("Base Layer.Idle", 0.1f);
    }
}