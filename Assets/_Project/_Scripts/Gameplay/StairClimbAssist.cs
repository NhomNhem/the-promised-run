using UnityEngine;

namespace ThePromisedRun.Gameplay
{
    public sealed class StairClimbAssist : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider bodyCollider;

        [Header("Step Settings")]
        [SerializeField] private float stepHeight = 0.35f;
        [SerializeField] private float stepCheckDistance = 0.25f;
        [SerializeField] private float stepUpForce = 4f;
        [SerializeField] private LayerMask collisionMask = ~0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            if (bodyCollider == null)
                bodyCollider = GetComponent<Collider>();
        }

        private void FixedUpdate()
        {
            if (rb == null)
                return;

            Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 moveDir = planarVelocity.sqrMagnitude > 0.0001f ? planarVelocity.normalized : transform.forward;

            Vector3 lowCenter;
            Vector3 highCenter;
            float sideOffset;

            if (bodyCollider != null)
            {
                Bounds bounds = bodyCollider.bounds;
                lowCenter = new Vector3(bounds.center.x, bounds.min.y + 0.05f, bounds.center.z);
                highCenter = new Vector3(bounds.center.x, bounds.min.y + stepHeight, bounds.center.z);
                sideOffset = Mathf.Max(0.05f, Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.6f);
            }
            else
            {
                lowCenter = transform.position + Vector3.up * 0.05f;
                highCenter = transform.position + Vector3.up * stepHeight;
                sideOffset = 0.2f;
            }

            Vector3 right = Vector3.Cross(Vector3.up, moveDir).normalized;

            bool hitLow =
                Physics.Raycast(lowCenter, moveDir, stepCheckDistance, collisionMask, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(lowCenter + right * sideOffset, moveDir, stepCheckDistance, collisionMask, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(lowCenter - right * sideOffset, moveDir, stepCheckDistance, collisionMask, QueryTriggerInteraction.Ignore);

            if (!hitLow)
                return;

            bool hitHigh =
                Physics.Raycast(highCenter, moveDir, stepCheckDistance, collisionMask, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(highCenter + right * sideOffset, moveDir, stepCheckDistance, collisionMask, QueryTriggerInteraction.Ignore) ||
                Physics.Raycast(highCenter - right * sideOffset, moveDir, stepCheckDistance, collisionMask, QueryTriggerInteraction.Ignore);

            if (hitHigh)
                return;

            rb.AddForce(Vector3.up * stepUpForce, ForceMode.VelocityChange);
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            Vector3 forward = transform.forward;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.05f, transform.position + Vector3.up * 0.05f + forward * stepCheckDistance);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position + Vector3.up * stepHeight, transform.position + Vector3.up * stepHeight + forward * stepCheckDistance);
        }
    }
}

