using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class AIEnemyDetector : MonoBehaviour
{
    public LayerMask targetLayer;

    public UnityAction<Player> OnPlayerDetected;

    [Range(0, 5)]
    public float radius;

    [Header("Gizmo parameters")]
    public Color detectColor = Color.red;
    [Header("Gizmo parameters")]
    public Color gizmoColor = Color.green;
    public bool showGizmos = true;

    public bool PlayerDetected { get; internal set; }

    private void Update()
    {
        if (!PlayerDetected)
        {
            var collider = Physics2D.OverlapCircle(transform.position, radius, targetLayer);

            PlayerDetected = collider != null;
            if (PlayerDetected)
            {
                gizmoColor = detectColor;
                Player player = collider.gameObject.GetComponent<Player>();
                if(player) OnPlayerDetected?.Invoke(player);
            }
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
