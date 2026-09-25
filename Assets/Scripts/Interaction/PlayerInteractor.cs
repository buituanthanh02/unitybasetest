using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private GameObject interactionButton;
    [SerializeField] private PlayerAnimation playerAnimation;

    private CharacterController controller;
    private IInteractable currentInteractable;
    private bool isPaused;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerAnimation == null)
            playerAnimation = GetComponent<PlayerAnimation>();
        ShowButton(false);
    }

    private void Update()
    {
        ShowButton(CanUseCurrent());
    }

    // Pause chỉ khoá sử dụng; không xoá đối tượng đang đứng gần.
    public void SetPaused(bool paused)
    {
        isPaused = paused;
        ShowButton(CanUseCurrent());
    }

    private bool CanUseCurrent()
    {
        if (!isActiveAndEnabled || isPaused || currentInteractable == null)
            return false;
        if (currentInteractable is MonoBehaviour source)
        {
            if (source == null || !source.isActiveAndEnabled) return false;
        }
        return controller != null && controller.isGrounded &&
            currentInteractable.CanInteract;
    }

    public void SetInteractable(IInteractable interactable)
    {
        if (!isActiveAndEnabled) return;
        currentInteractable = interactable;
        ShowButton(CanUseCurrent());
    }

    public void ClearInteractable(IInteractable interactable)
    {
        if (!ReferenceEquals(currentInteractable, interactable)) return;
        currentInteractable = null;
        ShowButton(false);
    }

    public void Interact()
    {
        if (!CanUseCurrent()) return;
        currentInteractable.Interact();
        if (playerAnimation != null) playerAnimation.PlayInteract();
        ShowButton(CanUseCurrent());
    }

    private void OnDisable()
    {
        currentInteractable = null;
        isPaused = false;
        ShowButton(false);
    }

    private void ShowButton(bool visible)
    {
        if (interactionButton != null && interactionButton.activeSelf != visible)
            interactionButton.SetActive(visible);
    }
}
