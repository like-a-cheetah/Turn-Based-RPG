using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

//namespace SVS.AI
//{
    public class AIMeleeAttackDetector : MonoBehaviour
    {
        public LayerMask targetLayer;

        public UnityEvent<GameObject> OnPlayerDetected;

        [Range(.1f, 1)]
        public float radius;

        [Header("Gizmo parameters")]
        public Color gizmoColor = Color.red;
        public bool showGizmos = true;

        public static bool PlayerDetected { get; internal set; }
        
        // Update is called once per frame
        private void Update()
        {
            var collider = Physics2D.OverlapCircle(transform.position, radius, targetLayer);
            PlayerDetected = collider != null;
            if (PlayerDetected)
        {
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
//}