using UnityEngine;
using System.Collections;

public class Camera_Controller : MonoBehaviour {

	public float speedH = 2.0f;
	public float speedV = 2.0f;

	public float speedMovement = 100f;

	private float yaw = 0.0f;
	private float pitch = 0.0f;

	private bool inputActive = true;

	void LateUpdate () {
		if (Input.GetKeyUp (KeyCode.P)) {
			inputActive = !inputActive;
		}

		if (inputActive == true) {
			// Calculate movement
			if (Input.GetKey (KeyCode.UpArrow) || Input.GetKey (KeyCode.W)) {
				float speed = speedMovement * Time.deltaTime;
				gameObject.transform.position += new Vector3 ((transform.forward.x * speed), (transform.forward.y * speed), (transform.forward.z * speed));
			} else if (Input.GetKey (KeyCode.DownArrow) || Input.GetKey (KeyCode.S)) {
				float speed = -speedMovement * Time.deltaTime;
				gameObject.transform.position += new Vector3 ((transform.forward.x * speed), (transform.forward.y * speed), (transform.forward.z * speed));
			}

			// Calculate rotation of camera based on mouse input.
			// Used to test effect at all angles
			yaw += speedH * Input.GetAxis ("Mouse X");
			pitch -= speedV * Input.GetAxis ("Mouse Y");
			transform.eulerAngles = new Vector3 (pitch, yaw, 0.0f);
		}
	}
}
