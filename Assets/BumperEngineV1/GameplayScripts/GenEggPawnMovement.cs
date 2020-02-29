using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenEggPawnMovement : MonoBehaviour {
	[HideInInspector] public float speed = 1f;

	private GameObject parent;
	private Rigidbody rb;
	private EnemyBhysics physics;
	private Transform player;

	void Start() {
		parent = transform.parent.gameObject;
		rb = parent.GetComponent<Rigidbody>();
		physics = parent.GetComponent<EnemyBhysics>();
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	// Animator events, move enemy forward in sync with the animation
	public void JumpDo(int dummy) {
		//Debug.Log("Jump event");

		rotateTowardsPlayer();

		// move towards player (taken from MotobugControl)
		var dir = player.position - transform.position;
		dir.y = 0; 	// don't take changes on Y-Axis into account
		physics.AddVelocity(dir.normalized * speed * 10);
		Debug.Log("Current velocity: "  + rb.velocity);
	}

	public void LandDo(int dummy) {
		//Debug.Log("Land event");

		// completely halt all movement
		rb.velocity = Vector3.zero;
	}

	// copy-pasting woo
	private void rotateTowardsPlayer() {
		// face player (taken from MotobugControl)
		// NOTE: we use the parent gameobject's transform
		// because models ripped from Gens are rotated 90 
		// degrees and I was too lazy to fix that
		var dir = player.position - parent.transform.position;
		dir.y = 0;
		Quaternion charRot = Quaternion.LookRotation(dir, parent.transform.up);
		parent.transform.rotation = Quaternion.Lerp(parent.transform.rotation, charRot, Time.deltaTime * 10);
	}
}
