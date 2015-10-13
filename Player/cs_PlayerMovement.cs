/*
TODO:
	- Implement crouching, walking, sprinting, idling
	- Make a gravity system that uses independently determined gravity, and standalone jumping
	- Add double jump
	- Add wallclimb
	- Add slope fall (if the slope is too steep, fall)
*/
using UnityEngine;
using System.Collections;

public class cs_PlayerMovement : MonoBehaviour {
	Vector3 acceldir; // normalized direction that the player has requested to move (taking into account the movement keys and look direction)	  
	Vector3 prevVelocity; // The current velocity of the player, before any additional calculations

	float friction = 8f;
	float ground_accelerate = 50f;
	float max_velocity_ground = 4f;
	float air_accelerate = 1000f;
	float max_velocity_air = 2f;

	bool isGrounded = false;
	bool isFirstGroundFrame = false;

	void FixedUpdate() {
		//isGrounded = Physics.CheckSphere(groundCheck.transform.position, 0.1f, groundLayer);

		acceldir = transform.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
		prevVelocity = GetComponent<Rigidbody>().velocity;

		if (isGrounded) {
			prevVelocity = Accelerate(acceldir, prevVelocity, ground_accelerate, max_velocity_ground);
			if (!isFirstGroundFrame) {
				prevVelocity = MoveGround(acceldir, prevVelocity);
			}
			else {
				isFirstGroundFrame = false;
			}
		}
		else {
			prevVelocity = MoveAir(acceldir, prevVelocity);
		}

		if (isGrounded && Input.GetButton("Jump")) {
			isGrounded = false;
			GetComponent<Rigidbody>().AddForce(new Vector3(0, 140f, 0f));
		}
		GetComponent<Rigidbody>().velocity = prevVelocity;
	}

	void Crouch() {
	}

	void Stand() {
	}

	private Vector3 Accelerate(Vector3 accelDir, Vector3 prevVelocity, float accelerate, float max_velocity) {
		float projVel = Vector3.Dot(prevVelocity, accelDir); // Vector projection of Current velocity onto accelDir.
		float accelVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

		// If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
		if (projVel + accelVel > max_velocity)
			accelVel = max_velocity - projVel;

		return prevVelocity + accelDir * accelVel;
	}

	private Vector3 MoveGround(Vector3 accelDir, Vector3 prevVelocity) {
		// Apply Friction
		float speed = prevVelocity.magnitude;
		if (speed != 0) // To avoid divide by zero errors
		{
			float drop = speed * friction * Time.fixedDeltaTime;
			prevVelocity *= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
		}

		// ground_accelerate and max_velocity_ground are server-defined movement variables
		return Accelerate(accelDir, prevVelocity, ground_accelerate, max_velocity_ground);
	}

	private Vector3 MoveAir(Vector3 accelDir, Vector3 prevVelocity) {
		// air_accelerate and max_velocity_air are server-defined movement variables
		return Accelerate(accelDir, prevVelocity, air_accelerate, max_velocity_air);
	}
}
