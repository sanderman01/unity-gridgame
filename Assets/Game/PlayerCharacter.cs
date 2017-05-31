using UnityEngine;

public class PlayerCharacter : MonoBehaviour {

    private Player controller;

    public void Possess(Player player) {
        controller = player;
    }

    void Update() {

        const float walkingSpeed = 10f;

        if(controller.Right) {
            // Move right
            transform.Translate(walkingSpeed * Time.deltaTime, 0, 0, Space.World);
        }

        if(controller.Left) {
            // Move left
            transform.Translate(-walkingSpeed * Time.deltaTime, 0, 0, Space.World);
        }

        if(controller.Up) {
            // Jump or use Jetpack
            transform.Translate(0, walkingSpeed * Time.deltaTime, 0, Space.World);
        } else {
            // Get pulled down by gravity
        }

        if(controller.Down) {
            // anything?
        }
    }
}
