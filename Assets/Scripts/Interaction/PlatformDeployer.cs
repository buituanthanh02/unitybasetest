using UnityEngine;

public class PlatformDeployer : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform[] platforms;

    [Tooltip("Vị trí thu vào so với vị trí hiện tại")]
    [SerializeField] private Vector3 retractedOffset =
        new Vector3(0f, -3f, 0f);

    [SerializeField] private float deploySpeed = 2f;

    private Vector3[] deployedPositions;
    private bool isActivated;

    public bool CanInteract => !isActivated;

    private void Awake()
    {
        deployedPositions = new Vector3[platforms.Length];

        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] == null)
            {
                continue;
            }

            deployedPositions[i] = platforms[i].position;

            platforms[i].position =
                deployedPositions[i] + retractedOffset;
        }
    }

    private void Update()
    {
        if (!isActivated)
        {
            return;
        }

        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i] == null)
            {
                continue;
            }

            platforms[i].position = Vector3.MoveTowards(
                platforms[i].position,
                deployedPositions[i],
                deploySpeed * Time.deltaTime
            );
        }
    }

    public void Interact()
    {
        if (isActivated)
        {
            return;
        }

        isActivated = true;
    }
}