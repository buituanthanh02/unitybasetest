using UnityEngine;

public class WinPanelVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem winVFX;
    [SerializeField] private Transform player;

    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);

    private void OnEnable()
    {
        if (winVFX == null || player == null)
            return;
        
        winVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        winVFX.transform.position = player.position + offset;
        winVFX.Play();
    }

    private void OnDisable()
    { 
        if (winVFX != null)
            winVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}