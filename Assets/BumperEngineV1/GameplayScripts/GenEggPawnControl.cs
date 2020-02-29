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
	public float speed;
	public GameObject hitCollision;

	private GameObject playerObj;
	private Transform player;
	private GenEggPawnMovement moveController;
	private HurtControl hurtControl;

	void Start() {
		var pobjs = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in pobjs) {
			if (p.name == "CharacterCapsule")
				continue;
			playerObj = p;
			break;
		}
		player = playerObj.transform;
		hitCollision.SetActive(false);
		moveController = GetComponentInChildren<GenEggPawnMovement>();
		hurtControl = playerObj.GetComponent<HurtControl>();

		// set speed
		moveController.speed = speed;
	}

	void FixedUpdate() {
		// already dead, don't do anything
		if (anim.GetNextAnimatorStateInfo(0).IsName("Dead"))
			return;

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

		//Debug.Log("Current distance: " + dist);
		//Debug.Log("Player Object Name: " + player.name);

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
		// movement is handled in GenEggPawnMovement.cs, this only handles state transitions
		float dist = Vector3.Distance(player.position, transform.position);

		// it's important we check this first
		if (hurtControl.IsHurt) {	// player just got hurt, do a victory animation
			changeState(GenEggPawnBehavior.Dance);
			anim.SetBool("Successful Hit", true);
		}

		// check if player is too far away, if so, transition
		// back to seek
		if (dist > playerLoseDistance) {
			changeState(GenEggPawnBehavior.Seek);
			anim.SetBool("Player Spotted", false);
			CancelInvoke();				// stop repeating movement methods
		}

		// check if the player is within attacking range, if
		// so, transition to attack state
		if (dist < playerAttackDistance) {
			changeState(GenEggPawnBehavior.Attack);
			anim.SetBool("Attack Range", true);
			CancelInvoke();
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

		if (hurtControl.IsHurt) {	// player just got hurt, do a victory animation
			changeState(GenEggPawnBehavior.Dance);
			anim.SetBool("Successful Hit", true);
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
		AnimatorStateInfo animInfo = anim.GetCurrentAnimatorStateInfo(0);

		if (!animInfo.IsName("Dance")) {
			float dist = Vector3.Distance(player.position, transform.position);

			if (dist > playerLoseDistance) {			// out of chasing distance
				changeState(GenEggPawnBehavior.Seek);
				anim.SetBool("Successful Hit", false);
				anim.SetBool("Attack Range", false);
				anim.SetBool("Player Spotted", false);
			} else if (dist > playerAttackDistance) {	// out of attack distance but not chasing distance
				changeState(GenEggPawnBehavior.Chase);
				anim.SetBool("Successful Hit", false);
				anim.SetBool("Attack Range", false);
			}
		}
	}

	void changeState(GenEggPawnBehavior g) {
		state = g;
	}

	private void rotateTowardsPlayer() {
		// face player (taken from MotobugControl)
		// NOTE: we use the parent gameobject's transform
		// because models ripped from Gens are rotated 90 
		// degrees and I was too lazy to fix that
		var dir = player.position - transform.position;
		dir.y = 0;
		Quaternion charRot = Quaternion.LookRotation(dir, transform.up);
		transform.rotation = Quaternion.Lerp(transform.rotation, charRot, Time.deltaTime * 10);
	}
}
