using UnityEngine;

public class InteractionZone : MonoBehaviour
{
    [SerializeField] private MonoBehaviour interactableSource;

    private IInteractable interactable;

    private void Awake()
    {
        interactable = interactableSource as IInteractable;

        if (interactable == null)
        {
            Debug.LogError(
                name + ": Interactable Source không triển khai IInteractable."
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerInteractor player =
            other.GetComponentInParent<PlayerInteractor>();

        if (player != null && interactable != null)
        {
            player.SetInteractable(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInteractor player =
            other.GetComponentInParent<PlayerInteractor>();

        if (player != null && interactable != null)
        {
            player.ClearInteractable(interactable);
        }
    }
}