using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingObstacle : MonoBehaviour
{
    [SerializeField] private LevelGameFlow levelFlow;

    [Tooltip(
        "Quãng đường theo trục thế giới từ vị trí ban đầu."
    )]
    [SerializeField] private Vector3 moveOffset =
        new Vector3(3f, 0f, 0f);

    [SerializeField] private float moveSpeed = 2f;

    private Rigidbody body;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool movingToEnd = true;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
    }

    private void Start()
    {
        startPosition = body.position;
        endPosition = startPosition + moveOffset;
    }

    private void FixedUpdate()
    {
        if (levelFlow != null &&
            levelFlow.HasEnded)
        {
            return;
        }

        Vector3 target = movingToEnd
            ? endPosition
            : startPosition;

        Vector3 nextPosition =
            Vector3.MoveTowards(
                body.position,
                target,
                moveSpeed * Time.fixedDeltaTime
            );

        body.MovePosition(nextPosition);

        if ((nextPosition - target).sqrMagnitude <
            0.0001f)
        {
            movingToEnd = !movingToEnd;
        }
    }
}