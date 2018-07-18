// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

namespace AmarokGames.GridGame {

    public class PlayerCharacter : MonoBehaviour {

        internal Player _player;
        private Rigidbody2D _rigidbody;

        [SerializeField]
        private bool _grounded;
        [SerializeField]
        private bool _allowJetpack = true;

        [SerializeField]
        private bool _showCollisionGizmos;

        // Debugging aides
        private ContactPoint2D[] _points = new ContactPoint2D[0];

        // Fields used for step-assist
        private bool _applyStepAssistNextFrame;
        private float _stepAssistHeight;
        private float _previousFrameHorizontalVelocity;

        new private Collider2D _collider;

        void Awake() {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
        }

        void FixedUpdate() {

            const float maxWalkingSpeed = 15f;
            const float walkingAcceleration = 50f;
            const float gravityAcceleration = 50;
            const float jetpackAcceleration = 50f;
            const float dragAcceleration = 10f;
            const float jumpVelocity = 20f;

            const float groundedRaycastYOffset = -0.01f;
            const float groundedRaycastWidthOffset = -0.1f;
            const float groundedRaycastLength = 0.1f;
            _grounded = _rigidbody.velocity.y >= 0 && DoGroundedRaycasts(_collider.bounds, groundedRaycastYOffset, groundedRaycastWidthOffset, groundedRaycastLength);

            if (_player.Right && _rigidbody.velocity.x < maxWalkingSpeed) {
                // Move right
                Vector2 vel = _rigidbody.velocity;
                vel.x += walkingAcceleration * Time.fixedDeltaTime;
                _rigidbody.velocity = vel;
            }

            if (_player.Left && _rigidbody.velocity.x > -maxWalkingSpeed) {
                // Move left
                Vector2 vel = _rigidbody.velocity;
                vel.x -= walkingAcceleration * Time.fixedDeltaTime;
                _rigidbody.velocity = vel;
            }

            if (!(_player.Left || _player.Right)) {
                float drag = dragAcceleration * Time.fixedDeltaTime * _rigidbody.velocity.x;
                Vector2 vel = _rigidbody.velocity;
                vel.x -= drag;
                _rigidbody.velocity = vel;
            }

            if (_player.Jump && _grounded) {
                Vector2 vel = _rigidbody.velocity;
                vel.y += jumpVelocity;
                _rigidbody.velocity = vel;
                _grounded = false;
            }
            if (_allowJetpack && _player.Jump && !_grounded) {
                // Jetpack
                const float jetpackMaxVerticalSpeed = 10;
                Vector3 vel = _rigidbody.velocity;
                if (vel.y < jetpackMaxVerticalSpeed) {
                    vel.y = Mathf.Min(_rigidbody.velocity.y + jetpackAcceleration, jetpackMaxVerticalSpeed);
                    _rigidbody.velocity = vel;
                }
            } else if (!_grounded) {
                // Get pulled down by gravity
                Vector2 vel = _rigidbody.velocity;
                vel.y -= gravityAcceleration * Time.fixedDeltaTime;
                _rigidbody.velocity = vel;
            }

            _rigidbody.MovePosition(_rigidbody.position + _rigidbody.velocity * Time.fixedDeltaTime);

            // Apply the step-assist by moving the character to same y-value as the highest contact point.
            Vector2 position = transform.position;
            if (_applyStepAssistNextFrame && _stepAssistHeight < position.y + 1.1f) {
                position.y = _stepAssistHeight;
                transform.position = position;
                Vector2 vel = _rigidbody.velocity;
                vel.x = _previousFrameHorizontalVelocity;
                _rigidbody.velocity = vel;
            }
            _applyStepAssistNextFrame = false;
            _stepAssistHeight = transform.position.y;
        }

        // Casts raycasts downwards based on the specified bounding box to determine if the character owning the bounding box is grounded.
        private bool DoGroundedRaycasts(Bounds bounds, float yOffset, float widthOffset, float raycastLength)
        {
            Vector2 bottomLeft = new Vector2(bounds.min.x - widthOffset, bounds.min.y + yOffset);
            Vector2 bottomRight = new Vector2(bounds.max.x + widthOffset, bounds.min.y + yOffset);
            Vector2 rayCastDir = Vector2.down;
            bool leftHit = Physics2D.Raycast(bottomLeft, rayCastDir, raycastLength);
            bool rightHit = Physics2D.Raycast(bottomRight, rayCastDir, raycastLength);

            bool grounded = leftHit || rightHit;
            return grounded;
        }

        void OnCollisionEnter2D(Collision2D collision) {
            HandleCollision(collision);
            if (_showCollisionGizmos) _points = collision.contacts;
        }

        void OnCollisionStay2D(Collision2D collision) {
            HandleCollision(collision);
            if (_showCollisionGizmos) _points = collision.contacts;
        }

        void OnCollisionExit2D(Collision2D collision) {
        }


        void HandleCollision(Collision2D collision) {
            Vector3 oldVelocity = _rigidbody.velocity;
            Collider2D[] overlapping = new Collider2D[1];
            if (collision.collider.OverlapCollider(new ContactFilter2D(), overlapping) > 0) {
                Vector3 collisionNormal = collision.contacts[0].normal;
                float separation = collision.contacts[0].separation;

                // Kill rigidbody.velocity in the direction of the collision.
                if (Vector2.Dot(collisionNormal, _rigidbody.velocity) < 0)
                    _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, collisionNormal);

                // Reduce overlap
                if (separation < -0.1f) {
                    transform.Translate(0.9f * -separation * collisionNormal);
                    Vector3 oldPosition = transform.position;
                    if (Mathf.Abs(collision.contacts[0].point.y - oldPosition.y) < 0.1f)
                        _grounded = true;
                }
            }

            // Step-assist
            // Iterate over contact points to determine if we should do a step-assist this frame
            foreach (ContactPoint2D p in collision.contacts) {
                const float thresholdAngle = 45f;
                float angleRight = Vector2.Angle(p.normal, Vector2.right);
                float angleLeft = Vector2.Angle(p.normal, -Vector2.right);
                bool verticalWall = Mathf.Abs(angleRight) < thresholdAngle || Mathf.Abs(angleLeft) < thresholdAngle;

                if (verticalWall && _grounded) {
                    // Schedule step-assist
                    // Do a 1-tile jump, because this way we don't need to worry about ceilings
                    _applyStepAssistNextFrame = true;
                    _stepAssistHeight = Mathf.Max(_stepAssistHeight, p.point.y);
                    _previousFrameHorizontalVelocity = oldVelocity.x;
                }
            }
        }

        void OnDrawGizmos() {
            if (_showCollisionGizmos) DrawGizmosContactPoints(_points);
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