// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame {

    public class PlayerCharacter : MonoBehaviour {

        internal Player player;
        private new Rigidbody2D rigidbody;

        [SerializeField]
        private bool grounded;
        [SerializeField]
        private bool allowJetpack = true;

        [SerializeField]
        private bool showCollisionGizmos;

        // Debugging aides
        private ContactPoint2D[] points = new ContactPoint2D[0];

        // Fields used for step-assist
        private bool applyStepAssistNextFrame;
        private float stepAssistHeight;
        private float previousFrameHorizontalVelocity;

        new private Collider2D collider;

        void Awake() {
            rigidbody = GetComponent<Rigidbody2D>();
            collider = GetComponent<Collider2D>();
        }

        void FixedUpdate() {

            const float maxWalkingSpeed = 15f;
            const float walkingAcceleration = 50f;
            const float gravityAcceleration = 50;
            const float jetpackAcceleration = 50f;
            const float dragAcceleration = 10f;
            const float jumpVelocity = 10f;

            if (player.Right && rigidbody.velocity.x < maxWalkingSpeed) {
                // Move right
                Vector2 vel = rigidbody.velocity;
                vel.x += walkingAcceleration * Time.fixedDeltaTime;
                rigidbody.velocity = vel;
            }

            if (player.Left && rigidbody.velocity.x > -maxWalkingSpeed) {
                // Move left
                Vector2 vel = rigidbody.velocity;
                vel.x -= walkingAcceleration * Time.fixedDeltaTime;
                rigidbody.velocity = vel;
            }

            if (!(player.Left || player.Right)) {
                float drag = dragAcceleration * Time.fixedDeltaTime * rigidbody.velocity.x;
                Vector2 vel = rigidbody.velocity;
                vel.x -= drag;
                rigidbody.velocity = vel;
            }

            if (player.Jump && grounded) {
                Vector2 vel = rigidbody.velocity;
                vel.y += jumpVelocity;
                rigidbody.velocity = vel;
                grounded = false;
            }
            if (allowJetpack && player.Jump && !grounded) {
                // Jetpack
                const float jetpackMaxVerticalSpeed = 10;
                Vector3 vel = rigidbody.velocity;
                if (vel.y < jetpackMaxVerticalSpeed) {
                    vel.y = Mathf.Min(rigidbody.velocity.y + jetpackAcceleration, jetpackMaxVerticalSpeed);
                    rigidbody.velocity = vel;
                }
            } else if (!grounded) {
                // Get pulled down by gravity
                Vector2 vel = rigidbody.velocity;
                vel.y -= gravityAcceleration * Time.fixedDeltaTime;
                rigidbody.velocity = vel;
            }

            rigidbody.MovePosition(rigidbody.position + rigidbody.velocity * Time.fixedDeltaTime);

            // Apply the step-assist by moving the character to same y-value as the highest contact point.
            Vector2 position = transform.position;
            if (applyStepAssistNextFrame && stepAssistHeight < position.y + 1.1f) {
                position.y = stepAssistHeight;
                transform.position = position;
                Vector2 vel = rigidbody.velocity;
                vel.x = previousFrameHorizontalVelocity;
                rigidbody.velocity = vel;
            }
            applyStepAssistNextFrame = false;
            stepAssistHeight = transform.position.y;

            DoGroundedRaycasts();
        }

        private void DoGroundedRaycasts() {
            Bounds bounds = collider.bounds;
            Vector2 bottomLeft = new Vector2(bounds.min.x - 0.1f, bounds.min.y - 0.01f);
            Vector2 bottomRight = new Vector2(bounds.max.x + 0.1f, bounds.min.y - 0.01f);
            Vector2 rayCastDir = Vector2.down;
            float raycastLength = 0.1f;

            bool leftHit = Physics2D.Raycast(bottomLeft, rayCastDir, raycastLength);
            bool rightHit = Physics2D.Raycast(bottomRight, rayCastDir, raycastLength);

            grounded = leftHit || rightHit;
        }

        void OnCollisionEnter2D(Collision2D collision) {
            HandleCollision(collision);
            if (showCollisionGizmos) points = collision.contacts;
        }

        void OnCollisionStay2D(Collision2D collision) {
            HandleCollision(collision);
            if (showCollisionGizmos) points = collision.contacts;
        }

        void OnCollisionExit2D(Collision2D collision) {
        }


        void HandleCollision(Collision2D collision) {
            Vector3 oldVelocity = rigidbody.velocity;
            Collider2D[] overlapping = new Collider2D[1];
            if (collision.collider.OverlapCollider(new ContactFilter2D(), overlapping) > 0) {
                Vector3 collisionNormal = collision.contacts[0].normal;
                float separation = collision.contacts[0].separation;

                // Kill rigidbody.velocity in the direction of the collision.
                if (Vector2.Dot(collisionNormal, rigidbody.velocity) < 0)
                    rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, collisionNormal);

                // Reduce overlap
                if (separation < -0.1f) {
                    transform.Translate(0.9f * -separation * collisionNormal);
                    Vector3 oldPosition = transform.position;
                    if (Mathf.Abs(collision.contacts[0].point.y - oldPosition.y) < 0.1f)
                        grounded = true;
                }
            }

            // Step-assist
            // Iterate over contact points to determine if we should do a step-assist this frame
            foreach (ContactPoint2D p in collision.contacts) {
                const float thresholdAngle = 45f;
                float angleRight = Vector2.Angle(p.normal, Vector2.right);
                float angleLeft = Vector2.Angle(p.normal, -Vector2.right);
                bool verticalWall = Mathf.Abs(angleRight) < thresholdAngle || Mathf.Abs(angleLeft) < thresholdAngle;

                if (verticalWall && grounded) {
                    // Schedule step-assist
                    // Do a 1-tile jump, because this way we don't need to worry about ceilings
                    applyStepAssistNextFrame = true;
                    stepAssistHeight = Mathf.Max(stepAssistHeight, p.point.y);
                    previousFrameHorizontalVelocity = oldVelocity.x;
                }
            }
        }

        void OnDrawGizmos() {
            if (showCollisionGizmos) DrawGizmosContactPoints(points);
        }

        void DrawGizmosContactPoints(ContactPoint2D[] points) {
            foreach (ContactPoint2D p in points) {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(p.point, 0.1f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(p.point, p.point + p.normal);
            }
        }
    }
}