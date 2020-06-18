using System;
using System.Collections;
using UnityEngine;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {
	Player player;

	private InputMaster controls;

	void Awake() {
		controls = new InputMaster();
		controls.Player.Jump.started += context => { player.OnJumpInputDown(); player.isJumping = true; };
		controls.Player.Jump.canceled += context => { player.OnJumpInputUp(); };
    }

    void Start() {
		player = GetComponent<Player>();
	}

	void Update() {
		Vector2 directionalInput = controls.Player.Movement.ReadValue<Vector2>();
		player.SetDirectionalInput(directionalInput);
		player.animator.SetFloat("Speed", Mathf.Abs(directionalInput.x));
		player.animator.SetBool("IsJumping", player.isJumping);
		player.animator.SetBool("IsFalling", player.isFalling);
		player.animator.SetBool("IsWallSliding", player.wallSliding);

		if (directionalInput.x > 0 && !player.isFacingRight && !player.wallSliding) {
			player.Flip();
			player.isFacingRight = true;
        } else if (directionalInput.x < 0 && player.isFacingRight && !player.wallSliding) {
			player.Flip();
			player.isFacingRight = false;
        }
	}

    void OnEnable() {
		controls.Enable();
    }

    void OnDisable() {
		controls.Disable();
    }
}