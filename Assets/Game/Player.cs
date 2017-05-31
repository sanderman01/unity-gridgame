using UnityEngine;

public class Player {

    private const KeyCode KeyLeft = KeyCode.LeftArrow;
    private const KeyCode KeyRight = KeyCode.RightArrow;
    private const KeyCode KeyUp = KeyCode.UpArrow;
    private const KeyCode KeyDown = KeyCode.DownArrow;

    public bool Left { get; set; }
    public bool Right { get; set; }
    public bool Down { get; set; }
    public bool Up { get; set; }

	public void Update () {
        Left = Input.GetKey(KeyLeft);
        Right = Input.GetKey(KeyRight);
        Up = Input.GetKey(KeyUp);
        Down = Input.GetKey(KeyDown);
	}
}
