using System;
using System.Collections;
using UnityEngine;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	float accelerationTimeAirborne = .2f;
	float accelerationTimeGrounded = .1f;
	[SerializeField] float moveSpeed = 6;

	public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;
	public bool wallStick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	PlayerInput playerInput;
	Controller2D controller;
	public Animator animator;

	Vector2 directionalInput;
	public bool wallSliding;
	int wallDirX;

	public bool isFacingRight = true;
	public bool isJumping = false;
	public bool isFalling = false;

	void Start() {
		playerInput = GetComponent<PlayerInput>();
		controller = GetComponent<Controller2D>();
		animator = GetComponent<Animator>();

		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
	}

	void Update() {
		CalculateVelocity();
		HandleWallSliding();

		if (!wallStick) {
			controller.Move(velocity * Time.deltaTime, directionalInput);
		} else {
			controller.Move(new Vector2(0, velocity.y) * Time.deltaTime, directionalInput);
		}

		if (controller.collisions.above || controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
			isJumping = false;
			//isFalling = false;
		}

		if (velocity.y < 0 && !wallSliding) {
			isFalling = true;
        } else {
			isFalling = false;
        }
	}

	public void Flip() {
		isFacingRight = !isFacingRight;

		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void SetDirectionalInput(Vector2 input) {
		directionalInput = input;
	}

	public void OnJumpInputDown() {
		if (wallSliding) {
			if (wallDirX == directionalInput.x) {
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			} else if (directionalInput.x == 0) {
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			} else {
				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;
			}
		}
		if (controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) { // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
					velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			} else {
				velocity.y = maxJumpVelocity;
			}
		}
	}

	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}

	void HandleWallSliding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		wallStick = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;
				wallStick = true;

				if (directionalInput.x != wallDirX && directionalInput.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				} else {
					timeToWallUnstick = wallStickTime;
				}
			} else {
				timeToWallUnstick = wallStickTime;
				wallStick = false;
			}
		}
	}

	void CalculateVelocity() {
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}
}