using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class AIFollowDetector : MonoBehaviour
{
    public LayerMask targetLayer;

    public UnityEvent<GameObject> OnPlayerDetected;

    [Range(0, 30)]
    public float radius;

    [Header("Gizmo parameters")]
    public Color gizmoColor = Color.green;
    public bool showGizmos = true;

    public bool PlayerDetected { get; internal set; }

    // Update is called once per frame
    private void Update()
    {
        var collider = Physics2D.OverlapCircle(transform.position, radius, targetLayer);
        PlayerDetected = collider != null;
        if (PlayerDetected)
        {
            Gizmos.color = Color.red;
            OnPlayerDetected?.Invoke(collider.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
