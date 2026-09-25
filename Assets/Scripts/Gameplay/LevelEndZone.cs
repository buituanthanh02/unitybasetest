using UnityEngine;

[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class LevelEndZone : MonoBehaviour
{
    public enum ZoneResult
    {
        Death,
        Win
    }

    [SerializeField] private LevelGameFlow levelFlow;
    [SerializeField] private ZoneResult result;

    private void Reset()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        zone.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (levelFlow == null || levelFlow.HasEnded)
            return;
        
        CharacterController enteringPlayer = other.GetComponentInParent<CharacterController>();

        if (enteringPlayer == null || enteringPlayer != levelFlow.Player)
            return;

        if (result == ZoneResult.Death)
            levelFlow.Lose();
        else
            levelFlow.Win();
    }
}