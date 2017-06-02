using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

    private Player controller;
    private new Rigidbody2D rigidbody;

    [SerializeField]
    private bool grounded;
    [SerializeField]
    private Vector2 velocity;

    [SerializeField]
    private bool rotateToSurfaceNormal = false;

    // Debugging aids
    private ContactPoint2D[] points = new ContactPoint2D[0];

    public void Possess(Player player) {
        controller = player;
    }

    void Awake() {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void Update() {

        const float walkingSpeed = 50f;
        const float walkingAcceleration = 50f;
        const float gravityAcceleration = 50;
        const float jetpackAcceleration = 50f;
        const float jumpVelocity = 30f;

        if (controller.Right && velocity.x < walkingSpeed) {
            // Move right
            velocity.x += walkingAcceleration * Time.deltaTime;
        } else if (!controller.Right && velocity.x > 0) {
            velocity.x = 0;
        }

        if (controller.Left && velocity.x > -walkingSpeed) {
            // Move left
            velocity.x -= walkingAcceleration * Time.deltaTime;
        } else if (!controller.Left && velocity.x < 0) {
            velocity.x = 0;
        }

        if (controller.Jump) {
            velocity.y += jumpVelocity;
            grounded = false;
        }
        if (controller.Up) {
            // Jetpack
            const float jetpackMaxVerticalSpeed = 10;
            if (velocity.y < jetpackMaxVerticalSpeed) {
                velocity.y = Mathf.Min(velocity.y + jetpackAcceleration, jetpackMaxVerticalSpeed);
            }
        } else if (!grounded) {
            // Get pulled down by gravity
            velocity.y -= gravityAcceleration * Time.deltaTime;
        }

        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);

        if (rotateToSurfaceNormal && !grounded) {
            const float rotationDampening = 1;
            rigidbody.MoveRotation(Mathf.Lerp(rigidbody.rotation, 0, Time.deltaTime * rotationDampening));
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        HandleCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision) {
        Debug.Log("OnCollisionStay");
        HandleCollision(collision);
        points = collision.contacts;
    }

    void OnCollisionExit2D(Collision2D collision) {
        Debug.Log("OnCollisionExit");
        grounded = false;
    }


    void HandleCollision(Collision2D collision) {
        Collider2D[] overlapping = new Collider2D[1];
        if (collision.collider.OverlapCollider(new ContactFilter2D(), overlapping) > 0) {
            Vector3 collisionNormal = collision.contacts[0].normal;
            float separation = collision.contacts[0].separation;

            // Kill velocity in the direction of the collision.
            if (Vector2.Dot(collisionNormal, velocity) < 0)
                velocity = Vector3.ProjectOnPlane(velocity, collisionNormal);

            // Reduce overlap
            if (separation < -0.1f) {
                transform.Translate(0.9f * -separation * collisionNormal);
                grounded = true;
            }
        }

        // Tilt character when appropriate
        if (rotateToSurfaceNormal) {
            const float tiltThreshold = 30;
            Vector2 collisionNormal = collision.contacts[0].normal;
            Vector2 up = Vector2.up;
            float angle = Vector2.Angle(up, collisionNormal);
            if (Mathf.Abs(angle) < tiltThreshold) {
                // tilt snap to surface
                rigidbody.MoveRotation(angle);
            }
        }

        // Step-assist
        // Iterate over contact points to determine if we should do a step-assist this frame
        bool applyStepAssistThisFrame = false;
        float highestPoint = float.MinValue;
        foreach (ContactPoint2D p in collision.contacts) {
            const float thresholdAngle = 45f;
            float angleRight = Vector2.Angle(p.normal, Vector2.right);
            float angleLeft = Vector2.Angle(p.normal, -Vector2.right);
            bool verticalWall = Mathf.Abs(angleRight) < thresholdAngle || Mathf.Abs(angleLeft) < thresholdAngle;

            if(verticalWall && grounded) {
                // Schedule step-assist
                // Do a 1-tile jump, because this way we don't need to worry about ceilings
                applyStepAssistThisFrame = true;
                highestPoint = Mathf.Max(highestPoint, p.point.y);
            }
        }

        // Apply the step-assist by moving the character to same y-value as the highest contact point.
        if(applyStepAssistThisFrame) {
            Vector2 position = transform.position;
            position.y = highestPoint;
            position.x += 0.1f;
            transform.position = position;
        }
    }



    void OnDrawGizmos() {
        //DrawGizmosContactPoints(points);
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
