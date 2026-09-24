using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private GameObject interactionButton;
    [SerializeField] private PlayerAnimation playerAnimation;

    private CharacterController controller;
    private IInteractable currentInteractable;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerAnimation == null)
        {
            playerAnimation = GetComponent<PlayerAnimation>();
        }

        ShowButton(false);
    }

    private void Update()
    {
        ShowButton(CanUseCurrent());
    }

    private bool CanUseCurrent()
    {
        if (!isActiveAndEnabled || currentInteractable == null)
        {
            return false;
        }

        // Tránh gọi vào object đã bị hủy hoặc bị tắt.
        if (currentInteractable is MonoBehaviour source)
        {
            if (source == null || !source.isActiveAndEnabled)
            {
                return false;
            }
        }

        // Bản này chỉ cho tương tác khi đang đứng trên mặt đỡ.
        return controller.isGrounded && currentInteractable.CanInteract;
    }

    public void SetInteractable(IInteractable interactable)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        currentInteractable = interactable;
        ShowButton(CanUseCurrent());
    }

    public void ClearInteractable(IInteractable interactable)
    {
        if (!ReferenceEquals(currentInteractable, interactable))
        {
            return;
        }

        currentInteractable = null;
        ShowButton(false);
    }

    public void Interact()
    {
        if (!CanUseCurrent())
        {
            return;
        }

        currentInteractable.Interact();

        if (playerAnimation != null)
        {
            playerAnimation.PlayInteract();
        }

        ShowButton(CanUseCurrent());
    }

    private void OnDisable()
    {
        currentInteractable = null;
        ShowButton(false);
    }

    private void ShowButton(bool visible)
    {
        if (interactionButton != null &&
            interactionButton.activeSelf != visible)
        {
            interactionButton.SetActive(visible);
        }
    }
}