using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(AudioSource))]
public class CollapsingPlatform : MonoBehaviour
{
    [Header("Collapse")]
    [SerializeField] private float collapseDelay = 3f;
    [SerializeField] private float fallSpeed = 6f;
    [SerializeField] private float hideAfter = 2f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem collapseVFX;
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip collapseClip;

    [SerializeField, Range(0f, 1f)]
    private float warningVolume = 0.6f;

    [SerializeField, Range(0f, 1f)]
    private float collapseVolume = 0.8f;

    private BoxCollider triggerCollider;
    private Collider solidCollider;
    private AudioSource audioSource;

    private bool countdownStarted;
    private bool collapsed;
    private float timer;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider currentCollider in colliders)
        {
            if (currentCollider is BoxCollider boxCollider &&
                boxCollider.isTrigger)
            {
                triggerCollider = boxCollider;
            }
            else if (!currentCollider.isTrigger &&
                     solidCollider == null)
            {
                solidCollider = currentCollider;
            }
        }

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
        }

        if (solidCollider == null)
        {
            Debug.LogError(
                name + ": Không tìm thấy collider đứng của platform.",
                this
            );

            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (countdownStarted) return;

        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null) return;

        countdownStarted = true;
        timer = 0f;

        if (warningClip != null)
        {
            audioSource.PlayOneShot(
                warningClip,
                warningVolume
            );
        }
    }

    private void Update()
    {
        if (!countdownStarted) return;

        timer += Time.deltaTime;

        if (!collapsed && timer >= collapseDelay)
        {
            Collapse();
        }

        if (!collapsed) return;

        transform.position +=
            Vector3.down * fallSpeed * Time.deltaTime;

        if (timer >= collapseDelay + hideAfter)
        {
            gameObject.SetActive(false);
        }
    }

    private void Collapse()
    {
        collapsed = true;
        solidCollider.enabled = false;

        if (collapseVFX != null)
        {
            collapseVFX.Play();
        }

        if (collapseClip != null)
        {
            audioSource.PlayOneShot(
                collapseClip,
                collapseVolume
            );
        }
    }
}