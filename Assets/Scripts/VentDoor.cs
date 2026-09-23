using UnityEngine;

public class VentDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform movingPart;
    [SerializeField] private Vector3 openOffset =
        new Vector3(0f, 3f, 0f);

    [SerializeField] private float openSpeed = 2f;

    private Vector3 closedLocalPosition;
    private Vector3 openLocalPosition;
    private bool isOpen;

    public bool CanInteract => !isOpen;

    private void Awake()
    {
        closedLocalPosition = movingPart.localPosition;
        openLocalPosition = closedLocalPosition + openOffset;
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        movingPart.localPosition = Vector3.MoveTowards(
            movingPart.localPosition,
            openLocalPosition,
            openSpeed * Time.deltaTime
        );
    }

    public void Interact()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
    }
}