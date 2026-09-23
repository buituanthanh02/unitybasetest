using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private GameObject interactionButton;

    private IInteractable currentInteractable;

    private void Start()
    {
        HideInteraction();
    }

    public void SetInteractable(IInteractable interactable)
    {
        if (interactable == null || !interactable.CanInteract)
        {
            return;
        }

        currentInteractable = interactable;
        interactionButton.SetActive(true);
    }

    public void ClearInteractable(IInteractable interactable)
    {
        if (currentInteractable != interactable)
        {
            return;
        }

        HideInteraction();
    }

    public void Interact()
    {
        if (currentInteractable == null ||
            !currentInteractable.CanInteract)
        {
            return;
        }

        currentInteractable.Interact();

        if (!currentInteractable.CanInteract)
        {
            HideInteraction();
        }
    }

    private void HideInteraction()
    {
        currentInteractable = null;

        if (interactionButton != null)
        {
            interactionButton.SetActive(false);
        }
    }
}