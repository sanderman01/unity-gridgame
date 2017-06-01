using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

    private Player controller;
    private new Rigidbody2D rigidbody;

    [SerializeField]
    private bool grounded;
    [SerializeField]
    private Vector2 velocity;

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

        if(controller.Right && velocity.x < walkingSpeed) {
            // Move right
                velocity.x += walkingAcceleration * Time.deltaTime;
        } else if(!controller.Right && velocity.x > 0) {
            velocity.x = 0;
        }

        if (controller.Left && velocity.x > -walkingSpeed) {
            // Move left
            velocity.x -= walkingAcceleration * Time.deltaTime;
        } else if (!controller.Left && velocity.x < 0) {
            velocity.x = 0;
        }

        if(controller.Jump) {
            velocity.y += jumpVelocity;
            grounded = false;
        }
        if (controller.Up) {
            // Jetpack
            const float jetpackMaxVerticalSpeed = 10;
            if (velocity.y < jetpackMaxVerticalSpeed) {
                velocity.y = Mathf.Min(velocity.y + jetpackAcceleration, jetpackMaxVerticalSpeed);
            }
        } else if(!grounded) {
            // Get pulled down by gravity
            velocity.y -= gravityAcceleration * Time.deltaTime;
        }

        if(controller.Down) {
            // Go down
            // Vector2 translation = new Vector3(0, -walkingSpeed * Time.deltaTime);
            // rigidbody.MovePosition(rigidbody.position + translation);
        }

        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        HandleCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision) {
        Debug.Log("OnCollisionStay");
        HandleCollision(collision);
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
            if(Vector2.Dot(collisionNormal, velocity) < 0)
                velocity = Vector3.ProjectOnPlane(velocity, collisionNormal);

            // Reduce overlap
            if (separation < -0.1f) {
                transform.Translate(0.9f * -separation * collisionNormal);
                grounded = true;
            }
        }
    }
}
