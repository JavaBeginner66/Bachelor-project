using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EnemyAI : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Transform target;
    public Transform[] waypoints;
    public Transform waypointMiddle;    
    public GameObject portalEffect;
    public GameObject portalStands;
    public Image healthDisplay;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI currentPercentText;

    // Different field abilities
    public GameObject rotatingWallsPrefab;
    public GameObject targetCirclePrefab;
    public GameObject groundQuarterStaticPrefab;
    public GameObject groundQuarterDoublePrefab;
    public GameObject bossAttackPrefab;

    public State state;
    public Phase phase;

    [Header("EnemyAI modifiable variables")]
    public float teleportTimer;    
    public float phaseMachineTimer;
    public float currentMoveSpeed;
    public float moveSpeed;
    public int[] healthPoolsArray;

    [HideInInspector] // Internal script variables
    public static EnemyAI enemyAI;
    private int waypointsIndex; 
    private float healthPoolMax;
    private float currentHealth;
    private bool lookAtPlayer;
    private float chaseTimerMax;
    private float chaseTimer;
    private bool phaseRunning;
    private float bossActiveTime;
    private bool invulnerable;
    // Damage done
    private float score;

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
    

    private void Start()
    {
        enemyAI = this;      
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

    public enum State
    {
        CHASE,
        PATROL,
        CASTING,
        IDLE
    }

    public enum Phase
    {
        PHASE0 = 0, PHASE1 = 1, PHASE2 = 2,
        PHASE3 = 3, PHASE4 = 4, PHASE5 = 5,
        PHASE6 = 6, PHASE7 = 7, PHASE8 = 8
    }

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

    private void OnTriggerEnter(Collider playerProjectile)
    {
        if (playerProjectile.tag.Equals(StatsScript.PlayerProjectile))
            takeDamage(playerProjectile);

    }

    public void takeDamage(Collider projectile)
    {

        if (!invulnerable)
        {
            // Store currenthealth to add to score if health <= 0
            float currentHealthTemp = currentHealth;
            currentHealth -= projectile.GetComponent<Projectile>().getProjectileDamage();

            if (currentHealth <= 0)
            {
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

    private IEnumerator fillUpHealthBar()
    {
        // Colors?
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
     * The current routines will finish before the next phase starts.
     */
    public void nextPhase()
    {
        phase++;
    }

    /*
     * Runs the coroutines given in parameter list in order
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

    }


    private void Chase()
    {
        if (bossChaseEnabled)
        {
            if (chaseTimer <= 0f)
            {
                lookAtPlayer = true;
                currentMoveSpeed = moveSpeed;
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
            }
        }    
    }

    /*
    private void Patrol()
    {

        if(Vector3.Distance(this.transform.position, waypoints[waypointsIndex].transform.position) >= 2)
        {
            agent.SetDestination(waypoints[waypointsIndex].transform.position);

        }
        else if(Vector3.Distance(this.transform.position, waypoints[waypointsIndex].transform.position) <= 2)
        {
            waypointsIndex = (waypointsIndex + 1) % waypoints.Length;
        }
    }
    */

    IEnumerator EnableBossChaseEnum()
    {
        bossChaseEnabled = true;
        state = State.CHASE;
        yield return new WaitForSeconds(bossActiveTime);
        state = State.CASTING;
        bossChaseEnabled = false;
    }

    IEnumerator BossAttackEnum()
    {
        bossAttackCoroutine = true;
        state = State.CASTING;
        lookAtPlayer = false;

        GameObject damageZone = Instantiate(bossAttackPrefab, new Vector3(transform.position.x, .1f, transform.position.z), transform.rotation);
        Image fillArea = damageZone.transform.Find("FillAreaCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
        Transform collider = damageZone.transform.Find("Collider").transform;

        for (float i = 0; i < 1.01f; i += .01f)
        {
            fillArea.fillAmount = i;

            yield return new WaitForSeconds(.01f);
        }
        Destroy(damageZone, .5f);
        collider.gameObject.SetActive(true);        
        
        if(state == State.CASTING)
            state = State.CHASE;

        bossAttackCoroutine = false;
    }

    IEnumerator MovingBulletHellEnum()
    {
        movingBulletHellCoroutine = true;
        lookAtPlayer = true;
        Vector3 nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
        for (int i = 0; i<5; i++)
        {
            
            transform.position = nextPos;

            yield return new WaitForSeconds(teleportTimer);
            SpawnBullet(ObjectPool.FrozenOrb);
            nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
            // Bør pooles
            if(i <= 3)
                Destroy(Instantiate(portalEffect, new Vector3(nextPos.x, nextPos.y + .5f, nextPos.z), Quaternion.identity), 10f);
            
            yield return new WaitForSeconds(teleportTimer*3);
        }
        movingBulletHellCoroutine = false;
        lookAtPlayer = false;
    }


   
    IEnumerator SingleQuarterCircleEnum()
    {
        for (int i = 0; i < 2; i++)
        {
            float angle = 90 * Random.Range(0, 4);


            GameObject quarterCircleZone = Instantiate(groundQuarterStaticPrefab);

            Image fillArea = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            Transform collider = quarterCircleZone.transform.Find("Collider").transform;
            quarterCircleZone.transform.rotation = Quaternion.Euler(0f, angle, 0f);


            for (float j = 0; j < 1.01f; j += .005f)
            {
                fillArea.fillAmount = j;
                quarterCircleZone.transform.Rotate(new Vector3(0f, 1f, 0f), 30f * Time.deltaTime);

                yield return new WaitForSeconds(.01f);
            }
            Destroy(quarterCircleZone, .5f);
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator DoubleQuarterCircleEnum()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject quarterCircleZone;
            Image fillArea1 = null;
            Image fillArea2 = null;
            float angle = 90 * Random.Range(0, 4);


            quarterCircleZone = Instantiate(groundQuarterDoublePrefab);
            fillArea2 = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas2").transform.Find("Outer").Find("Inner").GetComponent<Image>();

            fillArea1 = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            Transform collider = quarterCircleZone.transform.Find("Collider").transform;
            quarterCircleZone.transform.rotation = Quaternion.Euler(0f, angle, 0f);


            for (float j = 0; j < 1.01f; j += .005f)
            {
                fillArea1.fillAmount = j;
                if (fillArea2 != null)
                    fillArea2.fillAmount = j;


                quarterCircleZone.transform.Rotate(new Vector3(0f, 1f, 0f), 30f * Time.deltaTime);

                yield return new WaitForSeconds(.01f);
            }
            Destroy(quarterCircleZone, .5f);
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator TargetCircleEnum()
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject canvasCircle = Instantiate(targetCirclePrefab, new Vector3(target.position.x, 0f, target.position.z), Quaternion.identity);
            
            Transform collider = canvasCircle.transform.Find("TargetCircleCollider");
            RectTransform fillCircle = canvasCircle.transform.Find("TargetCircleCanvas").Find("Outer").Find("Inner").transform.GetComponent<RectTransform>();

            for (float j = 0; j <= 1.01f; j+=.015f)
            {          
                fillCircle.localScale = new Vector3(j, j, j);
                yield return new WaitForSeconds(.01f);
            }
            collider.gameObject.SetActive(true);
            Destroy(canvasCircle, 0.1f);
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator RotatingBulletHellEnum()
    {
        rotatingBulletHellCoroutine = true;
        transform.position = waypointMiddle.transform.position;
        for (int i = 0; i < 10; i++)
        {
            SpawnBullet(ObjectPool.FrozenOrbStatic);
            yield return new WaitForSeconds(1f);
        }
        rotatingBulletHellCoroutine = false;
    }

    IEnumerator RotatingWallsEnum()
    {
        rotatingWallsCoroutine = true;
        transform.position = waypointMiddle.transform.position;
        // Lag ut-animasjon
        Destroy(Instantiate(rotatingWallsPrefab), 20f);
        yield return new WaitForSeconds(20f);
        rotatingWallsCoroutine = false;
    }
    


    private void SpawnBullet(string type)
    {
        GameObject obj = ObjectPool.objectPool.getStoredObject(type);
        if (obj != null)
        {
            obj.transform.position = transform.position;          
            obj.transform.rotation = transform.rotation;
            obj.SetActive(true);
            
        }
    }
}
