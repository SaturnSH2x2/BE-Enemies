using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GenEggPawnBehavior : int {
	Seek = 0, Spotted = 1, Chase = 2, Attack = 3, Dance = 4
}

public class GenEggPawnControl : MonoBehaviour {
	public Animator anim;
	public GenEggPawnBehavior state;
	public float playerNoticeDistance;
	public float playerLoseDistance;
	public float playerAttackDistance;
	public float playerHitDistance;
	public float speed;
	public GameObject hitCollision;

	private GameObject playerObj;
	private Transform player;
	private EnemyBhysics physics;
	private Rigidbody rb;

	void Awake() {
		playerObj = GameObject.FindGameObjectWithTag("Player");
		player = playerObj.transform;
		physics = GetComponent<EnemyBhysics>();
		rb = GetComponent<Rigidbody>();
		hitCollision.SetActive(false);
	}

	void FixedUpdate() {
		switch (state) {
			case GenEggPawnBehavior.Seek:
				SeekDo();
				break;
			case GenEggPawnBehavior.Spotted:
				SpottedDo();
				break;
			case GenEggPawnBehavior.Chase:
				ChaseDo();
				break;
			case GenEggPawnBehavior.Attack:
				AttackDo();
				break;
			case GenEggPawnBehavior.Dance:
				DanceDo();
				break;
			default:
				Debug.Log("WARNING: Undefined behavior for Generic Egg Pawn Enemy.");
				break;
		}
	}

	void SeekDo() {
		// keep checking the distance between player and enemy
		float dist = Vector3.Distance(transform.position, player.position);

		Debug.Log("Current distance: " + dist);
		Debug.Log("Player Object Name: " + player.name);

		// transition to next state if player is within range
		if (dist < playerNoticeDistance) {
			changeState(GenEggPawnBehavior.Spotted);
			anim.SetBool("Player Spotted", true);
		}

	}

	void SpottedDo() {
		rotateTowardsPlayer();

		// notice animation automatically transitions to chase
		if (anim.GetCurrentAnimatorStateInfo(0).IsName("Chase")) {
			changeState(GenEggPawnBehavior.Chase);
		}
	}

	void ChaseDo() {
		rotateTowardsPlayer();

		// move towards player (taken from MotobugControl)
		var dir = player.position - transform.position;
		dir.y = 0; 	// don't take changes on Y-Axis into account
		physics.AddVelocity(dir.normalized * speed);
		Debug.Log("Current velocity: "  + rb.velocity);

		
		float dist = Vector3.Distance(player.position, transform.position);

		// check if player is too far away, if so, transition
		// back to seek
		if (dist > playerLoseDistance) {
			changeState(GenEggPawnBehavior.Seek);
			anim.SetBool("Player Spotted", false);
		}

		// check if the player is within attacking range, if
		// so, transition to attack state
		if (dist < playerAttackDistance) {
			changeState(GenEggPawnBehavior.Attack);
			anim.SetBool("Attack Range", true);
		}
	}

	void AttackDo() {
		AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(0);
		float dist = Vector3.Distance(player.position, transform.position);

		// if close enough, hit player and transition to dance animation
		if (animInfo.IsName("Attack")) {
			hitCollision.SetActive(true);
		} else {
			hitCollision.SetActive(false);
		}

		if (dist > playerLoseDistance) {			// out of chasing distance
			changeState(GenEggPawnBehavior.Seek);
			anim.SetBool("Attack Range", false);
			anim.SetBool("Player Spotted", false);
		} else if (dist > playerAttackDistance) {	// out of attack distance but not chasing distance
			changeState(GenEggPawnBehavior.Chase);
			anim.SetBool("Attack Range", false);
		}
	}

	void DanceDo() {

	}

	void changeState(GenEggPawnBehavior g) {
		state = g;
	}

	private void rotateTowardsPlayer() {
		// face player (taken from MotobugControl)
		// NOTE: we use the parent gameobject's transform
		// becausemodels ripped from Gens are rotated 90 
		// degrees and I was too lazy to fix that
		var dir = player.position - transform.position;
		dir.y = 0;
		Quaternion charRot = Quaternion.LookRotation(dir, transform.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, charRot, Time.deltaTime * 10);
	}
}
