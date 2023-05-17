using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // basics
    [SerializeField] private bool isFlightType;
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private Animator anim;
    private SpriteRenderer spriteRender;
    private Vector3 currentPosition;
    private Vector3 generatedPosition;
    [SerializeField] private LayerMask ground;
    private bool hitWall;

    private Vector3 previousPosition;
    private Vector3 lastPosition;

    private Vector3 goToPosition;

    // objects
    private Transform player;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float roamingSpeed;
    [SerializeField] private float detectDistance;
    [SerializeField] private GameObject Coin;
    
    // health and damage system
    [SerializeField] private float enemyHealth;
    private float meleeDamageTaken;
    private float projectileDamageTaken;
    [SerializeField] private GameObject pfDamagePopup;
    float currentEnemyHealth;
    [SerializeField] private GameObject bloodEffect;

    // effects
    [SerializeField] private GameObject projectileImpactEffect;

    // bools
    private bool isCoinCreated = false;
    public bool isDestroyed;
    // audio sources
    [SerializeField] private AudioSource[] fleshHit;
    [SerializeField] private AudioSource death;
    [SerializeField] private AudioSource jump;
    // movement
    private enum MovementState {
        running, attack, impact, dead
    };
    private bool canMove;
    public bool turnRight;

    public static EnemyController Instance {get; private set;}

    // Start is called before the first frame update
    void Start()
    {

        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRender = GetComponent<SpriteRenderer>();

        player = GameObject.FindWithTag("Player").transform;

        currentEnemyHealth = enemyHealth;
        canMove = true;

        getRoamingPosition();

        previousPosition = transform.position;
        lastPosition = Vector3.zero;

        isDestroyed = false;

    }

    void FixedUpdate()
    {

        // update current damage
        meleeDamageTaken = PlayerPrefs.GetFloat(PlayerObjectsSaver.MELEE_KEY, 0.1f);
        projectileDamageTaken = PlayerPrefs.GetFloat(PlayerObjectsSaver.PROJECTILE_KEY, 0.1f);

        currentPosition = transform.position;

        // roaming logics and player detection...
        if (canMove) {
            if (Vector2.Distance(transform.position, player.position) > detectDistance) {
                // roaming logics
                if (Vector2.Distance(transform.position, goToPosition) > 0.5f) {
                    if (!isFlightType) {
                        if (!hitWall) {
                            rb.position = Vector2.MoveTowards(transform.position, goToPosition, roamingSpeed * Time.deltaTime);
                        } else {
                            rb.velocity = new Vector2(rb.transform.right.x * -10f, 18f);
                            getRoamingPosition();
                        }
                    }
                } else {
                    getRoamingPosition();
                }
                TargetedQuaternion(goToPosition);
            } else {
                if (!hitWall) {
                    if (Vector2.Distance(transform.position, player.position) > 1.5) {
                        if (!isFlightType) {
                            rb.position = Vector2.MoveTowards(transform.position, new Vector3(player.position.x, transform.position.y, transform.position.z), movementSpeed * Time.deltaTime);
                        } else {
                            rb.position = Vector2.MoveTowards(transform.position, new Vector3(player.position.x, player.position.y, transform.position.z), movementSpeed * Time.deltaTime);
                        }
                    }
                } else {
                    if (!isFlightType) {
                        rb.velocity = new Vector2(rb.transform.right.x * -10f, 18f);
                    } else {
                        rb.position += new Vector2(0f,0.1f);
                    }
                }
                TargetedQuaternion(player.position);
            }
        }

        // check if the transformer is dead...
        if(isDead()) {
            // StartCoroutine(DoDeath());
            StartCoroutine(Die());
        }

        // update jump & fall animation
        if (rb.velocity.y > .1f) {
            anim.SetBool("jump", true);
        } else if (rb.velocity.y < -.1f) {
            anim.SetBool("jump", false);
        } else {
            anim.SetBool("jump", false);
        }

        isWalled();

    }

    // roaming logics
    public void getRoamingPosition() {
        float dir = UnityEngine.Random.Range(-3f, 3f);
        goToPosition = new Vector3(dir + transform.position.x, transform.position.y, transform.position.z);
    }

    // check if collided with melee weapon
    public void TakeMeleeDamage() {
        // play flesh hit
        int hit = Random.Range(0,2);
        fleshHit[hit].Play();

        // reduce current health
        currentEnemyHealth -= meleeDamageTaken;
        // do impact
        pfDamagePopUpScript damagePopup = pfDamagePopup.GetComponent<pfDamagePopUpScript>();
        damagePopup.makePopup((this.gameObject.transform.position) + new Vector3(0f,2f,0f), (int)(meleeDamageTaken * 100));
        if (currentEnemyHealth > 0) {
            DoImpact();
        }
        // shake camera
        CameraShake.Instance.ShakeCamera(5f, .1f);
    }
    // check if collided with stun radius
    public IEnumerator GetStunned() {
        if (this.gameObject != null) {
            canMove = false;
            Color defaultColor = spriteRender.color;
            spriteRender.color = Color.green;
            yield return new WaitForSeconds(2f);
            spriteRender.color = defaultColor;
            canMove = true;
        }
    }

    // check for entered 2d triggers
    public void OnTriggerEnter2D(Collider2D coll) {
        // projectile collisions
        if (coll.gameObject.CompareTag("PlayerProjectile")) {
            currentEnemyHealth -= projectileDamageTaken;
            pfDamagePopUpScript damagePopup = pfDamagePopup.GetComponent<pfDamagePopUpScript>();
            damagePopup.makePopup((this.gameObject.transform.position) + new Vector3(0f,2f,0f), (int)(projectileDamageTaken * 100));
            
            if (currentEnemyHealth > 0) {
                DoImpact();
            }

            // impact effect
            // GameObject impactPref = Instantiate(projectileImpactEffect, transform.position + new Vector3(0.5f, 0.5f, 0f), Quaternion.identity);
            // Destroy(impactPref, 2f);
            // shake camera
            CameraShake.Instance.ShakeCamera(5f, .1f);
        }
    }

    // check for entered 2d collisions
    public void OnCollisionEnter2D(Collision2D collision) {
        // stop positioning
        if (collision.gameObject.CompareTag("Player")) {
            canMove = false;
        }
    }
    // check for exited 2d collisions
    public void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            canMove = true;
        }
    }

    // take explosion damage
    public void EnemyTakeExplosionDamage(float damage) {
        currentEnemyHealth -= damage;
    }

    // right raycast
    // check if is collided with walls
    private void isWalled() {
        RaycastHit2D hit;
        Vector3 startPos = coll.bounds.center + new Vector3(0f, -0.4f, 0f);
        if (isRight()) {
            //Debug.Log("is right");
            hit = Physics2D.Raycast(startPos, Vector2.right, coll.bounds.extents.x + 2f, ground);
            Debug.DrawRay(startPos, Vector2.right * (coll.bounds.extents.x + 2f), Color.red);
        } else {
            //Debug.Log("is left");
            hit = Physics2D.Raycast(startPos, -Vector2.right, coll.bounds.extents.x +2f, ground);
            Debug.DrawRay(startPos, -Vector2.right * (coll.bounds.extents.x + 2), Color.red);
        }

        if (hit.collider != null) {
            if (hit.collider.tag == "Ground") {
                hitWall = true;
            } 
        } else {
            hitWall = false;
        }
    }
    
    // return angle on direction
    private bool isRight() {
        if (transform.position != previousPosition) {
            lastPosition = (transform.position - previousPosition).normalized;
            previousPosition = transform.position;
        }

        bool isRight = (lastPosition.x > .1f) ? true : false;

        return isRight;
    }

    // death...
    public bool isDead() {
        bool dead = (currentEnemyHealth <= 0) ? true : false; 
        return dead;
    }

    // partical effects and effects
    void bloodExplode() {
        Instantiate(bloodEffect, transform.position + new Vector3(0f,1f,-10f), Quaternion.identity);
    }

    // animation update
    // target rotation
    private void TargetedQuaternion(Vector3 target) {
        if ((target.x - transform.position.x) > 0) {
            transform.rotation = Quaternion.Euler(0,180,0);
        } else if ((target.x - transform.position.x) < 0) {
            transform.rotation = Quaternion.Euler(0,360,0);
        }
    }
    // coroutines
    // impact
    void DoImpact() {
        anim.SetTrigger("impact");
        Invoke("bloodExplode", 0f);
    }
    // die
    IEnumerator Die() {
        death.Play();
        anim.SetBool("isDead", true);
        this.enabled = false;
        yield return new WaitForSeconds(1f);
        if (!isCoinCreated) {
            Instantiate(Coin, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
            isCoinCreated = true;
        }
        if (isFlightType) {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        // disable object
        Destroy(this.gameObject, 3f);
    }

    // audios
    public void JumpSound() {
        jump.Play();
    }

}
