using UnityEngine;

public class VentDoor : MonoBehaviour, IInteractable
{
    [System.Serializable]
    private class DoorPart
    {
        public Transform movingPart;
        public Vector3 openOffset = new Vector3(0f, 3f, 0f);
    }

    [SerializeField] private DoorPart[] doorParts;
    [SerializeField] private float openSpeed = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource openAudio;

    private Vector3[] openLocalPositions;
    private bool isOpen;

    public bool CanInteract => !isOpen;

    private void Awake()
    {
        openLocalPositions = new Vector3[doorParts.Length];

        for (int i = 0; i < doorParts.Length; i++)
        {
            if (doorParts[i].movingPart == null)
            {
                continue;
            }

            openLocalPositions[i] =
                doorParts[i].movingPart.localPosition
                + doorParts[i].openOffset;
        }
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        for (int i = 0; i < doorParts.Length; i++)
        {
            Transform movingPart = doorParts[i].movingPart;

            if (movingPart == null)
            {
                continue;
            }

            movingPart.localPosition = Vector3.MoveTowards(
                movingPart.localPosition,
                openLocalPositions[i],
                openSpeed * Time.deltaTime
            );
        }
    }

    public void Interact()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;

        if (openAudio != null)
        {
            openAudio.Play();
        }
    }
}