using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    public static PlayerController Instance {get; private set;}

    // objects
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator anim;
    [SerializeField] private LayerMask ground;
    public SceneLoader loader;

    private GameObject gameSystem;
    CinematicController cinematicController;
    private PlayerSystem playerSystem;

    public FadeController fader;
    [SerializeField] private GameObject timeLockEffect;
    bool isTimeLockEffectCreated = false;
    [SerializeField] private GameObject meleeEffect;
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private GameObject iceEffect;
    [SerializeField] private GameObject iceTrappedEffect;
    [SerializeField] private GameObject stunEffect;
    [SerializeField] private float stunRadius;
    [SerializeField] private Transform stunPoint;

    // vertical booster movement
    private GameObject verticalBooster;
    private bool isVerticalBoosting;

    // bools
    public bool isPlayerDead;
    public bool isMeleeAttacking;

    // screens
    [SerializeField] private GameObject damageScreen;
    [SerializeField] private GameObject deathScreen;

    // decs
    public float movementSpeed = 10f;
    public float jumpForce = 20f;
    public float playerHealth = 1f;
    public float currentPlayerHealth;
    // private float projectileDamageToTake;
    // private float meleeDamageToTake;
    private enum movementState { idle, running, jumping, falling };

    public bool isProjecting;
    public float maxEnergy = 1f;
    public float currentEnergy;
    private Coroutine energyRegen;

    private Vector3 currentPosition;
    private Vector3 lastPosition;

    // dashing objects
    public bool canDash = true;
    public bool isDashing;
    private float dashingSpeed = 40f;
    public float dashingCooldown = 1f;
    private float dashingTimer = 0.2f;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private GameObject soundBarrierEffect;

    // healing objects
    public bool canHeal = true;
    public float healCooldown = 20f;
    [SerializeField] private GameObject healingEffect;
    public bool isMutated = false;

    // stun objects
    public bool canStun = true;
    public float stunCooldown;

    // explosion objects
    public bool canExplode = true;
    public float explosionCooldown;

    // melee attack
    // based attack rate = !2f
    private float attackRate = 5f;
    private float nextAttackTime = 0f;

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask bossLayer;
    [SerializeField] private LayerMask summonLayer;
    private float comboCounts = 0;
    private float maxComboCounts = 3;
    private float lastClickTime = 0;
    private float nextComboDelay = 1f;
    [SerializeField] private GameObject meleeImpactEffect;
    
    // audio sources
    [SerializeField] private AudioSource[] meleeAudio;
    [SerializeField] private AudioSource projectileAudio;
    [SerializeField] private AudioSource playerImpact;
    [SerializeField] private AudioSource playerDash;
    [SerializeField] private AudioSource playerHeal;
    [SerializeField] private AudioSource ticking;
    [SerializeField] private AudioSource playerIceds;
    [SerializeField] private AudioSource stun;
    [SerializeField] private AudioSource step;
    [SerializeField] private AudioSource jump;

    // range attack
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform firePoint2;
    [SerializeField] private GameObject bomb;

    // draw gizmos
    void OnDrawGizmosSelected() {
        if (attackPoint == null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        Gizmos.DrawWireSphere(stunPoint.position, stunRadius);
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // objects tags
        gameSystem = GameObject.FindWithTag("GameSystem").gameObject;
        //verticalBooster = GameObject.FindWithTag("VerticalBooster").gameObject;

        isProjecting = false;
        currentEnergy = maxEnergy;

        currentPosition = transform.position;
        lastPosition = Vector3.zero;

        currentPlayerHealth = playerHealth;

        // bool
        isPlayerDead = false;
        isVerticalBoosting = false;

    }

    void Update() {

        // disable any updates while dashing
        if (isDashing) return;
        // // disable any updates with vertically boosting
        // if (isVerticalBoosting) return;

        // check for max combo delay
        if ((Time.time - lastClickTime) > nextComboDelay) {
            comboCounts = 0;
        }
        // incremented time
        if (Time.time >= nextAttackTime) {
            if (Input.GetMouseButtonDown(0)) {
                // return if death screen, paused menu...
                if (EventSystem.current.IsPointerOverGameObject()) {
                    return;
                }
                isMeleeAttacking = true;
                MeleeTrigger();
                GameObject thisMeleeEffect = Instantiate(meleeEffect, transform.position + new Vector3(0f, 0.1f, 0f), Quaternion.identity);
                Destroy(thisMeleeEffect, 0.5f);
                nextAttackTime = Time.time + 1f / attackRate;
            } else {
                isMeleeAttacking = false;
            }
        }

        // stun 
        if (Input.GetKeyDown(KeyCode.Q) && canStun) {
            GameObject thisStunEffect = Instantiate(stunEffect, transform.position + new Vector3(0f, 0.5f, 0f), Quaternion.identity);
            Destroy(thisStunEffect, 0.8f);
            stun.Play();
            StartCoroutine(Stunning());
        }

        // projectile attack
        if (Input.GetMouseButtonDown(1)) {
            RangeTrigger();
        }
        // big explosion attack
        if (Input.GetKeyDown(KeyCode.T) && canExplode) {
            StartCoroutine(BigExplosionTrigger());
        }

        // healing
        if (Input.GetKeyDown(KeyCode.R)) {
            if (currentPlayerHealth < playerHealth && canHeal) {
                playerHeal.Play();
                // instantiate heaing effect
                GameObject thisHealEffect = Instantiate(healingEffect, transform.position + new Vector3(0f,1f,0f), Quaternion.identity);
                Destroy(thisHealEffect, 1f);
                StartCoroutine(Healing());
            }
        }

        // if there is a damage screen instantiated...
        damageScreenFadeout();

    }

    // trigger melee attack
    public void MeleeTrigger() {
        lastClickTime = Time.time;
        comboCounts++;
        // perform a small dash before the combo
        if (transform.localEulerAngles.y == 180) {
            StartCoroutine(SmallDashing(-1));
        } else {
            StartCoroutine(SmallDashing(1));
        }
        if (comboCounts == 1) {
            anim.SetTrigger("attack1");
            meleeAudio[0].Play();
        } else if (comboCounts == 2) {
            anim.SetTrigger("attack2");
            meleeAudio[1].Play();
        } else if (comboCounts == maxComboCounts) {
            anim.SetTrigger("attack3");
            meleeAudio[2].Play();
            // reset combo counts
            comboCounts = 0;
        }
    }

    // trigger projectile attack
    public void RangeTrigger() {
        if ((currentEnergy - 0.1f) >= 0) {
            // reduce per energy
            currentEnergy -= 0.1f;
            // instatiate the projectile
            Instantiate(projectile, firePoint.position, firePoint.rotation);
            // play audio
            projectileAudio.Play();

            if (energyRegen != null) {
                StopCoroutine(energyRegen);
            }

            energyRegen = StartCoroutine(RefillEnergy());
        } else {
            Debug.Log("Not Enough Energy");
        }
    }
    // trigger big explosion
    public IEnumerator BigExplosionTrigger() {
        canExplode = false;
        // instatiate the projectile
        Instantiate(bomb, firePoint2.position, firePoint2.rotation);
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(explosionCooldown);
        canExplode = true;
    }
    // refill energy
    private IEnumerator RefillEnergy() {
        yield return new WaitForSeconds(2f);

        while(currentEnergy < maxEnergy) {
            currentEnergy += maxEnergy / 100;
            yield return new WaitForSeconds(0.1f);
        }

        if (currentEnergy > 1f) {
            currentEnergy = 1f;
        }

        energyRegen = null;
    }   
    // healing
    private IEnumerator Healing() {
        canHeal = false;
        currentPlayerHealth = playerHealth;
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(healCooldown);
        canHeal = true;
    } 
    // stunning
    public IEnumerator Stunning() {
        canStun = false;
        Collider2D[] inStunRad = Physics2D.OverlapCircleAll(stunPoint.position, stunRadius, enemyLayer);
        foreach(Collider2D stunnedEnemy in inStunRad) {
            StartCoroutine(stunnedEnemy.GetComponent<EnemyController>().GetStunned());
        }
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(stunCooldown);
        canStun = true;
    }
    // heal mutation
    public IEnumerator HealMutation() {
        isMutated = true;
        Color defaultColor = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(5f);
        spriteRenderer.color = defaultColor;
        isMutated = false;
    }

    // melee attack
    public void MeleeAttackDamage() {
        // enemy overlap detection
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach(Collider2D hitEnemy in hitEnemies) {
            // call the hit enemy "take damage" func
            hitEnemy.GetComponent<EnemyController>().TakeMeleeDamage();
            // while mutated
            if (isMutated && currentPlayerHealth < playerHealth) {
                currentPlayerHealth+=0.05f;
                if (currentPlayerHealth > playerHealth) {
                    currentPlayerHealth = playerHealth;
                }
            }
            // instantiate melee effect
            GameObject thisMeleeEffect = Instantiate(meleeImpactEffect, hitEnemy.transform.position + new Vector3(0,0.5f,0), Quaternion.identity);
            if (_Angle() == Vector2.right) {
                thisMeleeEffect.GetComponent<SpriteRenderer>().flipX = true;
            } else {
                thisMeleeEffect.GetComponent<SpriteRenderer>().flipX = false;
            }
            Destroy(thisMeleeEffect, 0.5f);
        }

    
    }
    public void BossMeleeDamage() {
        Collider2D[] hitBosses = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, bossLayer);
        foreach(Collider2D hitBoss in hitBosses) {
            hitBoss.GetComponent<BossController>().BossTakeMelee();

            // instantiate melee effect
            GameObject thisMeleeEffect = Instantiate(meleeImpactEffect, hitBoss.transform.position + new Vector3(0,0.5f,0), Quaternion.identity);
            if (_Angle() == Vector2.right) {
                thisMeleeEffect.GetComponent<SpriteRenderer>().flipX = true;
            } else {
                thisMeleeEffect.GetComponent<SpriteRenderer>().flipX = false;
            }
            Destroy(thisMeleeEffect, 0.5f);
        }
    }
    public void SummonMeleeDamage() {
        Collider2D[] hitSummons = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, summonLayer);
        foreach(Collider2D hitSummon in hitSummons) {
            hitSummon.GetComponent<SummonController>().SummonTakeMelee();

            // instantiate melee effect
            GameObject thisMeleeEffect = Instantiate(meleeImpactEffect, hitSummon.transform.position + new Vector3(0,0.5f,0), Quaternion.identity);
            if (_Angle() == Vector2.right) {
                thisMeleeEffect.GetComponent<SpriteRenderer>().flipX = true;
            } else {
                thisMeleeEffect.GetComponent<SpriteRenderer>().flipX = false;
            }
            Destroy(thisMeleeEffect, 0.5f);
        }
    }
    // take melee damage 
    public void playerTakeMeleeDamage(float damageToTake) {
        healthReduction(damageToTake);
        playerImpact.Play();
        // damage screen
        Color color = damageScreen.GetComponent<Image>().color;
        color.a = 1f;
        damageScreen.GetComponent<Image>().color = color;
        // shake camera
        CameraShake.Instance.ShakeCamera(6f, .1f);

        gameSystem.GetComponent<PlayerSystem>().OnHealthDamaged(damageToTake);
    }
    // take projectile damage
    public void playerTakeProjectileDamage(float damageToTake) {
        healthReduction(damageToTake);
        playerImpact.Play();
        // damage screen
        Color color = damageScreen.GetComponent<Image>().color;
        color.a = 1f;
        damageScreen.GetComponent<Image>().color = color;
        // shake camera
        CameraShake.Instance.ShakeCamera(6f, .1f);

        gameSystem.GetComponent<PlayerSystem>().OnHealthDamaged(damageToTake);
    }
    // damage screen fade out
    public void damageScreenFadeout() {
        if (damageScreen != null) {
            if (damageScreen.GetComponent<Image>().color.a > 0) {
                Color color = damageScreen.GetComponent<Image>().color;
                color.a -= 0.01f;
                damageScreen.GetComponent<Image>().color = color;
            }
        }
    }

    // health reduction
    private void healthReduction(float damageToTake) {
        if (currentPlayerHealth > 0f) {
            currentPlayerHealth -= damageToTake;
        } else {
            currentPlayerHealth = 0f;
        }
        // damage effect
        GameObject dmgEffect = Instantiate(damageEffect, transform.position + new Vector3(0f, 1f), Quaternion.identity);
        dmgEffect.transform.right = transform.right.normalized;
        Destroy(dmgEffect, 0.2f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // disable any updates while dashing
        if (isDashing) return;
        // disable any updates with vertically boosting
        if (isVerticalBoosting) return;

        // directional movement
        float dirX = Input.GetAxisRaw("Horizontal");
        // dir x axis
        rb.velocity = new Vector2(dirX * movementSpeed, rb.velocity.y);
        // dir y axis
        if (Input.GetButton("Jump") && isGrounded())  { rb.velocity = new Vector2(rb.velocity.x, jumpForce); }

        // dashing
        if (Input.GetKey(KeyCode.LeftShift) && canDash) {
            if (transform.localEulerAngles.y == 180) {
                StartCoroutine(Dashing(-1));
            } else {
                StartCoroutine(Dashing(1));
            }
        }

        // animation udpate
        AnimationUpdate();

        // check if current health < 0
        if (isDead()) {
            StartCoroutine(DoDeath());
        }


    }

    // check ifs
    // check if grounded
    private bool isGrounded() {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, ground);
    }
    // check if is slopped
    private bool isSlopped() {

        // initialize raycast
        RaycastHit2D hit = Physics2D.Raycast(coll.bounds.center, _Angle(), coll.bounds.extents.x + 0.05f, ground);

        Color rayColor;
        if (hit.collider != null) {
            rayColor = Color.green;
        } else {
            rayColor = Color.red;
        }

        // draw ra
        Debug.DrawRay(coll.bounds.center, _Angle() * (coll.bounds.extents.x + 0.05f), rayColor);

        return hit.collider != null;
    }

    // return angle on direction
    private Vector2 _Angle() {
        Vector2 _angle = new Vector2();
    
        if (transform.position != currentPosition) {
            lastPosition = (transform.position - currentPosition).normalized;
            if (lastPosition.x > .1f) {
                _angle = Vector2.right;
            } else if (lastPosition.x < -.1f) {
                _angle = Vector2.left;
            }
            currentPosition = transform.position;
        }

        return _angle;
    }

    // check if dead
    private bool isDead(){
        bool dead = (currentPlayerHealth < .1f) ? true : false;
        return dead;
    }

    // load level
    void LoadFirstLevel() {
        loader.LoadScene(2);
    }

    // damage & health
    private void OnTriggerEnter2D(Collider2D collider) {
        // take projectile damage
        if (collider.gameObject.CompareTag("GolemArm") || collider.gameObject.CompareTag("Laser") || collider.gameObject.CompareTag("Arrow")) {
            playerTakeProjectileDamage(0.2f);
        } else if (collider.gameObject.CompareTag("IceShard")) {
            playerTakeProjectileDamage(0.1f);
            StartCoroutine(movementReduction());
            // iced 
            playerIceds.Play();
            // ice effect
            GameObject thisIceEffect = Instantiate(iceEffect, transform.position + new Vector3(0.1f, 0.2f, 0f), Quaternion.identity);
            Destroy(thisIceEffect, 0.5f);
        } else if (collider.gameObject.CompareTag("Fireball") || collider.gameObject.CompareTag("Bat")) {
            playerTakeProjectileDamage(0.2f);
        } else if (collider.gameObject.CompareTag("Firebreath")) {
            StartCoroutine(PoisonDmg());
        } else if ( collider.gameObject.CompareTag("Frostbite")) {
            StartCoroutine(FrostBitten());
        }

        // end of cutscene
        if (collider.gameObject.layer == LayerMask.NameToLayer("EndOfCutscene")) {
            StartCoroutine(fader.FadeIn());
            Invoke("LoadFirstLevel", 6f);
            // Debug.Log("fader");
        }

        // boost movement vertically
        if (collider.gameObject.CompareTag("VerticalBooster")) {
            VerticalBoosting();
        }
    }
    // inflicted damage
    // 5 ticks dmgs
    public IEnumerator PoisonDmg() {
        Color defaultColor = spriteRenderer.color;
        spriteRenderer.color = Color.magenta;
        playerTakeMeleeDamage(0.05f);
        yield return new WaitForSeconds(0.3f);
        playerTakeMeleeDamage(0.05f);
        yield return new WaitForSeconds(0.3f);
        playerTakeMeleeDamage(0.05f);
        yield return new WaitForSeconds(0.3f);
        playerTakeMeleeDamage(0.05f);
        yield return new WaitForSeconds(0.3f);
        playerTakeMeleeDamage(0.05f);
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = defaultColor;
    }
    // frost bitten
    private IEnumerator FrostBitten() {
        playerTakeMeleeDamage(0.05f);
        yield return new WaitForSeconds(0f);
        StartCoroutine(IceTrapped());
    }

    // movement reduction from ice
    private IEnumerator movementReduction() {
        movementSpeed = 3f;
        yield return new WaitForSeconds(1f);
        movementSpeed = 10f;
    }
    // stop movement
    public IEnumerator bearTrapped() {
        movementSpeed = 0f;
        yield return new WaitForSeconds(3f);
        movementSpeed = 10f;
    }
    public IEnumerator IceTrapped() {
        movementSpeed = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GameObject icetrap = Instantiate(iceTrappedEffect, transform.position+new Vector3(0f,0f), Quaternion.identity);
        icetrap.transform.right = transform.right.normalized;
        yield return new WaitForSeconds(1.5f);
        rb.constraints = RigidbodyConstraints2D.None;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        movementSpeed = 10f;
    }

    // particle collisions
    // rain partsys sent message
    private void OnParticleCollision(GameObject other) {
        // take rain damage
        playerTakeRainDamage(0.01f);
    }
    // rain damage func
    private void playerTakeRainDamage(float damage) {
        healthReduction(damage);
        playerImpact.Play();
        // damage screen
        Color color = damageScreen.GetComponent<Image>().color;
        color.a = 1f;
        damageScreen.GetComponent<Image>().color = color;

        gameSystem.GetComponent<PlayerSystem>().OnHealthDamaged(damage);
    }

    // coroutines
    // do death
    IEnumerator DoDeath() {
        PlayerPrefs.DeleteKey(PlayerObjectsSaver.MELEE_KEY);
        PlayerPrefs.DeleteKey(PlayerObjectsSaver.PROJECTILE_KEY);
        PlayerPrefs.DeleteKey(PlayerObjectsSaver.COIN_KEY);

        isPlayerDead = true;
        ticking.Play();

        this.gameObject.GetComponent<PlayerController>().enabled = false;

        // death logics
        if (!isTimeLockEffectCreated) {
            Instantiate(timeLockEffect, transform.position + new Vector3(0f,0.5f,0f), Quaternion.identity);
            isTimeLockEffectCreated = true;
        }

        yield return new WaitForSeconds(1f);
        //this.gameObject.SetActive(false);
        coll.enabled = false;
        rb.constraints = RigidbodyConstraints2D.None;

        // start death screen and loading screen
        deathScreen.SetActive(true);
    }
    // dashing
    IEnumerator SmallDashing(float dir) {
        isDashing = true;
        float originGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(dir * 10, 0f);
        yield return new WaitForSeconds(dashingTimer);
        rb.gravityScale = originGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
    }
    IEnumerator Dashing(float dir) {
        GameObject thisSoundBarrierEffect = Instantiate(soundBarrierEffect, transform.position + new Vector3(0f, 0.1f, 0f), Quaternion.identity);
        if (transform.localEulerAngles.y == 180) {
            thisSoundBarrierEffect.GetComponent<SpriteRenderer>().flipX = false;
        } else {
            thisSoundBarrierEffect.GetComponent<SpriteRenderer>().flipX = true;
        }
        Destroy(thisSoundBarrierEffect, 0.8f);
        playerDash.Play();

        canDash = false;
        isDashing = true;
        float originGravity = rb.gravityScale;
        rb.gravityScale = 0;
        rb.velocity = new Vector2(dir * dashingSpeed, 0f);
        trail.emitting = true;
        yield return new WaitForSeconds(dashingTimer);
        trail.emitting = false;
        rb.gravityScale = originGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
    // vertical boosting
    private void VerticalBoosting() {
        //Debug.Log("boosting");
        rb.velocity = new Vector2(0f, dashingSpeed + 5f);
    }
    // step
    public void playFootStep() {
        step.Play();
    }
    // jump 
    public void playJump() {
        jump.Play();
    }

    // animation updates
    void AnimationUpdate() {
        movementState state;
        if (rb.velocity.x > .1f) {
            transform.rotation = Quaternion.Euler(0,360,0);
            state = movementState.running;
        } else if (rb.velocity.x < -.1f) {
            transform.rotation = Quaternion.Euler(0,180,0);
            state = movementState.running;
        } else {
            state = movementState.idle;
        }
        if (rb.velocity.y > .1f) {
            state = movementState.jumping;
        } else if (rb.velocity.y < -.1f) {
            state = movementState.falling;
        } 
        // set animation state
        anim.SetInteger("movementState", (int) state );
    }

}
