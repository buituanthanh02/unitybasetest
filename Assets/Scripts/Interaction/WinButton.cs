using UnityEngine;

public class WinButton : MonoBehaviour, IInteractable
{
    [SerializeField] private LevelGameFlow levelFlow;

    public bool CanInteract => isActiveAndEnabled && levelFlow != null && !levelFlow.HasEnded;

    public void Interact()
    {
        if (!CanInteract)
            return;

        levelFlow.Win();
    }
}