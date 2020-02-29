using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour {

    public int MaxHealth = 1;
    public bool runDeathAnimation = false;
    public float secondsBeforeDeath = 1.0f;
    public Animator animator;
    public bool knockback = false;
    public float knockbackMultiplier = 10.0f;
    public AudioSource hitSoundEffect;
    public GameObject Explosion;
    public EnemySpawnerEternal SpawnReference { get; set; }

    
    private int HP; 

    void Awake()
    {
        HP = MaxHealth;
    }

    public void DealDamage(int Damage)
    {
        HP -= Damage;
        if(HP <= 0)
        {
            if (runDeathAnimation) {
                if (animator == null) {
                    Debug.Log("Run Death Animation enabled without animator reference. Skipping.");
                    OnDeath();
                    return;
                }

                // this script expects the animator to have a Dead parameter
                animator.SetBool("Dead", true);

                if (hitSoundEffect != null) {
                    hitSoundEffect.Play();
                }

                if (knockback) {
                    var physics = GetComponent<EnemyBhysics>();
                    var player = GameObject.FindGameObjectWithTag("Player").transform;
                    var kbVector = transform.position - player.position;
                    kbVector.y = 0f;
                    physics.AddVelocity(-kbVector * knockbackMultiplier);
                }

                // get rid of homing target once defeated
                var children = GetComponentInChildren<Transform>();
                foreach (Transform child in GetComponentsInChildren<Transform>()) {
                    if (child.tag == "HomingTarget")
                        child.gameObject.SetActive(false);
                }

                StartCoroutine(DeathAnimationWait());
            } else {
                OnDeath();
            }
        }
    }

    IEnumerator DeathAnimationWait() {
        yield return new WaitForSeconds(secondsBeforeDeath);
        OnDeath();
    }

    public void OnDeath() {
        if(SpawnReference != null)
        {
            SpawnReference.ResartSpawner();
        }
        GameObject.Instantiate(Explosion, transform.position,Quaternion.identity);
        Destroy(gameObject);
    }

}
