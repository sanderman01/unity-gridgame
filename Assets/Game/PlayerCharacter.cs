// Copyright(C) 2017 Amarok Games, Alexander Verbeek

using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

    private Player controller;
    private new Rigidbody2D rigidbody;

    [SerializeField]
    private bool grounded;
    [SerializeField]
    private Vector2 velocity;

    [SerializeField]
    private bool showCollisionGizmos;

    // Debugging aides
    private ContactPoint2D[] points = new ContactPoint2D[0];

    // Fields used for step-assist
    private bool applyStepAssistNextFrame;
    private float stepAssistHeight;
    private float previousFrameHorizontalVelocity;

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
        const float dragAcceleration = 5f;
        const float jumpVelocity = 30f;

        if (controller.Right && velocity.x < walkingSpeed) {
            // Move right
            velocity.x += walkingAcceleration * Time.deltaTime;
        }

        if (controller.Left && velocity.x > -walkingSpeed) {
            // Move left
            velocity.x -= walkingAcceleration * Time.deltaTime;
        }

        if(!(controller.Left || controller.Right)) {
            float drag = dragAcceleration * Time.deltaTime * velocity.x;
            velocity.x -= drag;
        }

        if (controller.Jump && grounded) {
            velocity.y += jumpVelocity;
            grounded = false;
        }
        if (controller.Jump && !grounded) {
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

        // Apply the step-assist by moving the character to same y-value as the highest contact point.
        Vector2 position = transform.position;
        if (applyStepAssistNextFrame && stepAssistHeight < position.y + 1.1f) {
            position.y = stepAssistHeight;
            transform.position = position;
            velocity.x = previousFrameHorizontalVelocity;
        }
        applyStepAssistNextFrame = false;
        stepAssistHeight = transform.position.y;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        HandleCollision(collision);
        if(showCollisionGizmos) points = collision.contacts;
    }

    void OnCollisionStay2D(Collision2D collision) {
        HandleCollision(collision);
        if(showCollisionGizmos) points = collision.contacts;
    }

    void OnCollisionExit2D(Collision2D collision) {
        grounded = false;
    }


    void HandleCollision(Collision2D collision) {
        Vector3 oldVelocity = velocity;
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

            if(verticalWall && grounded) {
                // Schedule step-assist
                // Do a 1-tile jump, because this way we don't need to worry about ceilings
                applyStepAssistNextFrame = true;
                stepAssistHeight = Mathf.Max(stepAssistHeight, p.point.y);
                previousFrameHorizontalVelocity = oldVelocity.x;
            }
        }
    }

    void OnDrawGizmos() {
        if(showCollisionGizmos) DrawGizmosContactPoints(points);
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
