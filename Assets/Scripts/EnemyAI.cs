using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EnemyAI : MonoBehaviour
{

    [Header("Inspector Objects")]
    public GameObject gameMaster;               // Reference to gamemaster gameobject
    public Transform target;                    // Reference to Player
    public Transform[] waypoints;               // Positions which boss can teleport to
    public Transform waypointMiddle;            // Middle position
    public GameObject portalEffect;             // Waypoint particle effect
    public GameObject portalStands;             // Portal model
    public Image healthDisplay;                 // UI health display on boss
    public TextMeshProUGUI stageText;           // Number that displays current stage boss is on
    public TextMeshProUGUI currentPercentText;  // Number that displays current percent boss is on
    public Animator animator;

    // Different field ability prefabs
    public GameObject rotatingWallsPrefab;      
    public GameObject targetCirclePrefab;
    public GameObject groundQuarterStaticPrefab;
    public GameObject groundQuarterDoublePrefab;
    public GameObject bossAttackPrefab;

    public State state;                         // Current boss state
    public Phase phase;                         // Current boss phase

    [Header("EnemyAI modifiable variables")]
    public float teleportTimer;                 // Timer between teleports in MovingBulletHellEnum coroutine
    public float phaseMachineTimer;             // Timer to maintain/controll boss phases
    public float currentMoveSpeed;              // Current movement speed of boss
    public float moveSpeed;                     // Speed value currentModeSpeed takes from
    public int[] healthPoolsArray;              // The different health pools for the different stages

    [HideInInspector] // Internal script variables
    public static EnemyAI enemyAI;              // Static reference to this script so GameMaster can easily access it
    private ObjectPool pool;                    // Reference to ObjectPool script               
    private float healthPoolMax;                // Max health pool that changes every stage
    private float currentHealth;                // Current health that copies from healthPoolMax
    private bool lookAtPlayer;                  // Controlls whether boss should look at player
    private float chaseTimerMax;                // Countdown roof
    private float chaseTimer;                   // Puts a cooldown on boss movement after attacking
    private bool phaseRunning;                  // Limits boss to run 1 phase at a time
    private float bossActiveTime;               // How long boss will chase player at a time
    private bool invulnerable;                  // Used to stop boss from taking damage when recovering health after phase change
    private bool isWalking;                     // Animation condition
    private bool isCasting;                     // Animation condition
    private float score;                        // Damage taken

    // Coroutine running checks
    private bool movingBulletHellCoroutine;
    private bool rotatingBulletHellCoroutine;  
    private bool rotatingWallsCoroutine;
    private bool bossAttackCoroutine;
    private bool bossChaseEnabled;

    // Coroutine strings
    private readonly string MovingBulletHellEnumRef = "MovingBulletHellEnum";
    private readonly string RotatingBulletHellEnumRef = "RotatingBulletHellEnum";
    private readonly string TargetCircleEnumRef = "TargetCircleEnum";
    private readonly string QuarterDoubleCircleEnumRef = "DoubleQuarterCircleEnum";
    private readonly string QuarterSingleCircleEnumRef = "SingleQuarterCircleEnum";
    private readonly string RotatingWallsEnumRef = "RotatingWallsEnum";
    private readonly string EnableBossChaseEnumRef = "EnableBossChaseEnum";
    

    /**
     *  Start is used to get references and set up values
     */
    private void Start()
    {    
        enemyAI = this;
        animator = transform.GetChild(0).GetComponent<Animator>();
        pool = gameMaster.GetComponent<ObjectPool>();
        state = State.CASTING;
        phase = Phase.PHASE0;
        chaseTimerMax = 2f;
        teleportTimer = 1f;
        bossActiveTime = 10f;
        healthPoolMax = healthPoolsArray[0];
        currentHealth = healthPoolMax;
        score = 0;
        currentPercentText.text = "100%";
    }

    /**
     *  Enum for different states
     */
    public enum State
    {
        CHASE,
        PATROL,
        CASTING,
        IDLE
    }

    /**
     *  Enum for different phases
     */
    public enum Phase
    {
        PHASE0 = 0, PHASE1 = 1, PHASE2 = 2,
        PHASE3 = 3, PHASE4 = 4, PHASE5 = 5,
        PHASE6 = 6, PHASE7 = 7, PHASE8 = 8
    }
    /**
    *  Get and set methods
    */
    public int getPhase()
    {
        return (int)phase;
    }

    public float getPlayerScore()
    {
        return score;
    }

    public void setPhase(Phase newPhase)
    {
        phase = newPhase;
    }

    /**
     *  Monobehavior method used to detect collision
     */
    private void OnTriggerEnter(Collider playerProjectile)
    {
        if (playerProjectile.tag.Equals(StatsScript.PlayerProjectile))
            takeDamage(playerProjectile);

    }

    /**
     *  Method for when boss gets hit by a player projectile
     */
    public void takeDamage(Collider projectile)
    {

        if (!invulnerable)
        {
            if (!bossAttackCoroutine)
                animator.SetTrigger("hit");
            // Store currenthealth to add to score if health <= 0
            float currentHealthTemp = currentHealth;
            currentHealth -= projectile.GetComponent<Projectile>().getProjectileDamage();
           
            if (currentHealth <= 0)
            {
                // If projectile deals more than current healthpool, add the remaining healthpool pre-shot to score
                score += currentHealthTemp;
                if ((int)phase >= 8)
                {
                    StartCoroutine(fillUpHealthBar());
                }
                else
                {
                    nextPhase();
                    // Cast phase into int to get next phase healthpool
                    healthPoolMax = healthPoolsArray[(int)phase];
                    StartCoroutine(fillUpHealthBar());
                    stageText.text = ((int)phase).ToString();
                }
            }
            else
            {
                score += projectile.GetComponent<Projectile>().getProjectileDamage();
            }
            healthDisplay.fillAmount = currentHealth / healthPoolMax;
        }
        currentPercentText.text = (healthDisplay.fillAmount*100).ToString("N0") + "%";
    }

    /**
     *  IEnumerator run as a coroutine for filling up the healthbar slowly and making boss invulnerable for the duration
     */
    private IEnumerator fillUpHealthBar()
    {
        invulnerable = true;
        for (float j = 0; j <= 1.01f; j += .01f)
        {
            healthDisplay.fillAmount = j;
            currentPercentText.text = (healthDisplay.fillAmount * 100).ToString("N0") + "%";
            yield return new WaitForSeconds(.01f);
        }
        
        healthDisplay.fillAmount = 1f;
        currentHealth = healthPoolMax;
        
        invulnerable = false;
    }

    /*
     * Runs coroutines based on which PHASE the boss is currently in.
     */
    public IEnumerator PhaseMachine()
    {
        phaseMachineTimer = 3;
        while (GameMasterScript.gameRunning)
        {
            // The current routines will finish before the next phase starts.
            if (!phaseRunning)
            {               
                switch (phase)
                {
                    case Phase.PHASE0:                                          
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef));
                        break;
                    case Phase.PHASE1:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, RotatingWallsEnumRef));                        
                        break;
                    case Phase.PHASE2:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, QuarterSingleCircleEnumRef));                                             
                        break;
                    case Phase.PHASE3:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, RotatingBulletHellEnumRef));
                        break;
                    case Phase.PHASE4:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, TargetCircleEnumRef, RotatingWallsEnumRef));
                        break;
                    case Phase.PHASE5:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, QuarterDoubleCircleEnumRef, MovingBulletHellEnumRef));
                        break;
                    case Phase.PHASE6:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, TargetCircleEnumRef, MovingBulletHellEnumRef, QuarterSingleCircleEnumRef));
                        break;
                    case Phase.PHASE7:
                        StartCoroutine(doCoroutines(EnableBossChaseEnumRef, QuarterDoubleCircleEnumRef, 
                                                    RotatingBulletHellEnumRef, TargetCircleEnumRef, RotatingWallsEnumRef));
                        break;
                    case Phase.PHASE8:
                        StartCoroutine(doCoroutines(QuarterDoubleCircleEnumRef, MovingBulletHellEnumRef,TargetCircleEnumRef,
                                                      RotatingWallsEnumRef, QuarterSingleCircleEnumRef, RotatingBulletHellEnumRef));
                        break;

                }
            }
            yield return new WaitForSeconds(phaseMachineTimer);
        }      
    }

    /* 
     * Method used to transition to next phase outside of script.
     */
    public void nextPhase()
    {
        phase++;
    }

    /*
     * IEnumerator runs the coroutines given in parameter list in order
     */
    IEnumerator doCoroutines(params string[] routines)
    {
        
        phaseRunning = true;
        Phase currentPhase = phase;

        while (currentPhase == phase)
        {
            foreach (var routine in routines)
            {
                // If a major routine where the boss transform is not occupied is not running, start a new routine
                while (mainRoutinesRunning())
                    yield return new WaitForSeconds(1f);
              
                StartCoroutine(routine);               
            }
        }
        phaseRunning = false;
    }

    private bool mainRoutinesRunning() =>
        rotatingBulletHellCoroutine || movingBulletHellCoroutine || bossAttackCoroutine || rotatingWallsCoroutine || bossChaseEnabled;


    private void Update()
    {
        
        if (lookAtPlayer)
            transform.LookAt(target);

        if (GameMasterScript.gameRunning)
        {

            if (state == State.CHASE)
                Chase();

            if (state == State.PATROL)
                state = State.PATROL;

            if (state == State.CASTING && state == State.IDLE)
                currentMoveSpeed = 0f;
        }

       

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isCasting", isCasting);
            
    }

    /*
     * Method makes boss run towards player and activate an attack under certain conditions
     */
    private void Chase()
    {
        if (bossChaseEnabled)
        {
            if (chaseTimer <= 0f)
            {
                lookAtPlayer = true;
                currentMoveSpeed = moveSpeed;
                isWalking = true;

                if (Vector3.Distance(this.transform.position, target.transform.position) >= 3f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, target.position, currentMoveSpeed * Time.deltaTime);
                }
                if (Vector3.Distance(this.transform.position, target.transform.position) < 3f)
                {
                    if (!bossAttackCoroutine)
                        StartCoroutine(BossAttackEnum());

                    chaseTimer = chaseTimerMax;
                }
            }
            else
            {
                chaseTimer -= Time.deltaTime;
                isWalking = false;
            }
        }    
    }

    /*
     * IEnumerator which enables Chase() method to be run
     */
    IEnumerator EnableBossChaseEnum()
    {
        bossChaseEnabled = true;
        state = State.CHASE;
        yield return new WaitForSeconds(bossActiveTime);
        state = State.CASTING;
        bossChaseEnabled = false;
    }

    /*
     * IEnumerator for the boss close range attack
     */
    IEnumerator BossAttackEnum()
    {
        bossAttackCoroutine = true;
        state = State.CASTING;
        lookAtPlayer = false;
        animator.SetTrigger("attack");
        // Instantiates the UI image prefab 
        GameObject damageZone = Instantiate(bossAttackPrefab, new Vector3(transform.position.x, .1f, transform.position.z), transform.rotation);
        // Get the Image reference to be able to gradually fill it
        Image fillArea = damageZone.transform.Find("FillAreaCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
        // Get the collider reference to activate it at end of coroutine
        Transform collider = damageZone.transform.Find("Collider").transform;

        for (float i = 0; i < 10.1; i += Time.deltaTime * 6.5f)
        {
            fillArea.fillAmount = i/10;

            yield return new WaitForSeconds(.1f * Time.deltaTime);
        }
        Destroy(damageZone, .5f);
        collider.gameObject.SetActive(true);        
        
        if(state == State.CASTING)
            state = State.CHASE;

        bossAttackCoroutine = false;
    }

    /*
     * IEnumerator for the moving bullethell boss attack
     */
    IEnumerator MovingBulletHellEnum()
    {
        movingBulletHellCoroutine = true;
        lookAtPlayer = true;
        isCasting = true;
        isWalking = false;
        Vector3 nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
        for (int i = 0; i<5; i++)
        {
            
            transform.position = nextPos;

            yield return new WaitForSeconds(teleportTimer);
            SpawnBullet(ObjectPool.FrozenOrb);
            nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
            // Instatiating and destroying particle system to indicate next boss position
            if(i <= 3)
                Destroy(Instantiate(portalEffect, new Vector3(nextPos.x, nextPos.y + .5f, nextPos.z), Quaternion.identity), 10f);
            
            yield return new WaitForSeconds(teleportTimer*3);
        }
        movingBulletHellCoroutine = false;
        lookAtPlayer = false;
        isCasting = false;
    }


    /*
      * IEnumerator for the single quarter circle area effect boss attack
      */
    IEnumerator SingleQuarterCircleEnum()
    {
        for (int i = 0; i < 2; i++)
        {
            // Get a random angle 
            float angle = 90 * Random.Range(0, 4);
            // Instantiate the prefab
            GameObject quarterCircleZone = Instantiate(groundQuarterStaticPrefab);
            // Get the image reference to gradually fill it
            Image fillArea = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            // Get the collider to activate it at the end of the coroutine
            Transform collider = quarterCircleZone.transform.Find("Collider").transform;
            quarterCircleZone.transform.rotation = Quaternion.Euler(0f, angle, 0f);


            for (float j = 0; j < 1.01f; j += .005f)
            {
                // filling up image to indicate when it will damage player
                fillArea.fillAmount = j;
                quarterCircleZone.transform.Rotate(new Vector3(0f, 1f, 0f), 30f * Time.deltaTime);

                yield return new WaitForSeconds(.01f);
            }
            Destroy(quarterCircleZone, .5f);
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
    }

    /*
     * IEnumerator for the double quarter circle area effect boss attack
     */
    IEnumerator DoubleQuarterCircleEnum()
    {
        for (int i = 0; i < 5; i++)
        {
            // Get a random angle
            float angle = 90 * Random.Range(0, 4);
            // Instantiate the prefab
            GameObject quarterCircleZone = Instantiate(groundQuarterDoublePrefab);
            // Get the image references to gradually fill them
            Image fillArea2 = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas2").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            Image fillArea1 = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            // Get the collider to activate it at the end of the coroutine
            Transform collider = quarterCircleZone.transform.Find("Collider").transform;
            quarterCircleZone.transform.rotation = Quaternion.Euler(0f, angle, 0f);


            for (float j = 0; j < 1.01f; j += .005f)
            {
                // filling up images to indicate when it will damage player
                fillArea1.fillAmount = j;
                fillArea2.fillAmount = j;

                quarterCircleZone.transform.Rotate(new Vector3(0f, 1f, 0f), 30f * Time.deltaTime);

                yield return new WaitForSeconds(.01f);
            }
            Destroy(quarterCircleZone, .5f);
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
    }

    /*
     * IEnumerator for the target circle that aims in on player
     */
    IEnumerator TargetCircleEnum()
    {
        for (int i = 0; i < 9; i++)
        {
            // Instantiate the prefab
            GameObject canvasCircle = Instantiate(targetCirclePrefab, new Vector3(target.position.x, 0f, target.position.z), Quaternion.identity);
            // Get the collider to activate it at the end of coroutine
            Transform collider = canvasCircle.transform.Find("TargetCircleCollider");
            // Get reference to the inner UI element to gradually scale it up to base size
            RectTransform fillCircle = canvasCircle.transform.Find("TargetCircleCanvas").Find("Outer").Find("Inner").transform.GetComponent<RectTransform>();

            for (float j = 0; j <= 1.01f; j+=.015f)
            {  
                // Scaling up inner circle to indicate when it will damage player
                fillCircle.localScale = new Vector3(j, j, j);
                yield return new WaitForSeconds(.01f);
            }
            collider.gameObject.SetActive(true);
            Destroy(canvasCircle, 0.1f);
            yield return new WaitForSeconds(2f);
        }
    }

    /*
     * IEnumerator for the static rotating bullethell boss attack
     */
    IEnumerator RotatingBulletHellEnum()
    {
        rotatingBulletHellCoroutine = true;
        isCasting = true;
        isWalking = false;
        transform.position = waypointMiddle.transform.position;
        for (int i = 0; i < 10; i++)
        {
            SpawnBullet(ObjectPool.FrozenOrbStatic);
            yield return new WaitForSeconds(1f);
        }
        rotatingBulletHellCoroutine = false;
        isCasting = false;
    }

    /*
     * IEnumerator for the rotating walls boss attack
     */
    IEnumerator RotatingWallsEnum()
    {
        isCasting = true;
        isWalking = false;
        rotatingWallsCoroutine = true;
        transform.position = waypointMiddle.transform.position;
        // Lag ut-animasjon
        Destroy(Instantiate(rotatingWallsPrefab), 20f);
        yield return new WaitForSeconds(20f);
        rotatingWallsCoroutine = false;
        isCasting = false;
    }


    /*
     * Method gets a unused orb from the object pool script
     * @param type describes which bullethell type 
     */
    private void SpawnBullet(string type)
    {       
        GameObject obj = pool.getStoredObject(type);
        if (obj != null)
        {
            obj.transform.position = transform.position;          
            obj.transform.rotation = transform.rotation;
            obj.SetActive(true);
            
        }
    }
}
