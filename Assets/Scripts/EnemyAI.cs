using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Transform target;
    public Transform[] waypoints;
    public Transform waypointMiddle;    
    public GameObject portalEffect;
    public Image healthDisplay;
    // Different field abilities
    public GameObject rotatingWallsPrefab;
    public GameObject targetCirclePrefab;
    public GameObject groundQuarterStaticPrefab;
    public GameObject groundQuarterDoublePrefab;
    public GameObject bossAttackPrefab;

    public State state;

    [Header("EnemyAI modifiable variables")]
    public float teleportTimer = 1;
    public float bulletHellWaves = 10;
    public float healthPool;
    public float stateMachineTimer;
    public float currentMoveSpeed;
    public float moveSpeed;

    [HideInInspector] // Internal script variables
    public static EnemyAI enemyAI;
    private Phase phase;
    private int waypointsIndex;   
    private float currentHealth;
    private bool lookAtPlayer;
    // Coroutine references
    private Coroutine movingBulletHellCoroutine;
    private Coroutine rotatingBulletHellCoroutine;
    private Coroutine targetCircleCoroutine;   
    private Coroutine rotatingWallsCoroutine;
    private Coroutine quartercircleCoroutine;
    private Coroutine bossAttackCoroutine;

    private bool coroutineRunning;

    public Text tempText;

    private float chaseTimerMax;
    private float chaseTimer;

    private void Start()
    {
        enemyAI = this;
        currentHealth = healthPool;
        state = State.CASTING;
        phase = Phase.PHASE3;
        chaseTimerMax = 3f;

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
        PHASE1,
        PHASE2,
        PHASE3
    }

    public State getState()
    {
        return state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(StatsScript.ProjectileBaseDamageTag))
            takeDamage(other);

    }

    public void takeDamage(Collider projectile)
    {
        currentHealth -= projectile.GetComponent<Projectile>().getProjectileDamage();
        healthDisplay.fillAmount = currentHealth / healthPool;
    }


    public IEnumerator StateMachine()
    {
        
        while (GameMasterScript.gameRunning)
        {        
           

            if (phase == Phase.PHASE1)
            {
                stateMachineTimer = 3;
                phase1Routines();
            }
            else if (phase == Phase.PHASE2)
            {
                stateMachineTimer = 2;
                phase2Routines();
            }
            else if (phase == Phase.PHASE3)
            {
                stateMachineTimer = 1;
                phase3Routines();
            }
            
            yield return new WaitForSeconds(stateMachineTimer);
        }
        
    }

    private void phase1Routines()
    {
        if (!coroutineRunning && bossAttackCoroutine == null)
        {
            coroutineRunning = true;
            switch (Random.Range(1, 6))
            {
                case 1:
                    state = State.CASTING;
                    movingBulletHellCoroutine = StartCoroutine(MovingBulletHellEnum());                    
                    break;
                case 2:
                    state = State.CASTING;
                    rotatingBulletHellCoroutine = StartCoroutine(RotatingBulletHellEnum());                   
                    break;
                case 3:
                    state = State.CHASE;
                    chaseTimer = 0f;
                    targetCircleCoroutine = StartCoroutine(TargetCircleEnum());
                    break;
                case 4:
                    state = State.CASTING;
                    rotatingWallsCoroutine = StartCoroutine(RotatingWallsEnum());                   
                    break;
                case 5:
                    state = State.CHASE;
                    chaseTimer = 0f;
                    quartercircleCoroutine = StartCoroutine(QuarterCircleEnum("single", false));                   
                    break;
            }
        }
    }

    private void phase2Routines()
    {
        
        
    }

    private void phase3Routines()
    {
        // This is a mess
        state = State.CASTING;
        switch (Random.Range(1, 6))
        {
            case 1:
                if (mainRoutinesRunning())
                    movingBulletHellCoroutine = StartCoroutine(MovingBulletHellEnum());
                break;
            case 2:
                if (mainRoutinesRunning())                 
                    rotatingBulletHellCoroutine = StartCoroutine(RotatingBulletHellEnum());
                break;
            case 3:                                               
                if (targetCircleCoroutine == null)
                    targetCircleCoroutine = StartCoroutine(TargetCircleEnum());
                break;
            case 4:
                if (mainRoutinesRunning())
                    rotatingWallsCoroutine = StartCoroutine(RotatingWallsEnum());
                break;
            case 5:
                if (quartercircleCoroutine == null)
                    quartercircleCoroutine = StartCoroutine(QuarterCircleEnum("double", true));
                break;
        }
    }

    private bool mainRoutinesRunning() => 
        (rotatingBulletHellCoroutine == null && movingBulletHellCoroutine == null 
        && bossAttackCoroutine == null && rotatingWallsCoroutine == null);


    
    private void Update()
    {
        

        if(lookAtPlayer)
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
                if (bossAttackCoroutine == null)
                    bossAttackCoroutine = StartCoroutine(BossAttackEnum());

                chaseTimer = chaseTimerMax;
            }
        }
        else
        {
            chaseTimer -= Time.deltaTime;
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

    IEnumerator BossAttackEnum()
    {
        state = State.CASTING;
        lookAtPlayer = false;

        GameObject damageZone = Instantiate(bossAttackPrefab, new Vector3(transform.position.x, 0f, transform.position.z), transform.rotation);
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

        bossAttackCoroutine = null;
    }

    IEnumerator MovingBulletHellEnum()
    {

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
        movingBulletHellCoroutine = null;
        coroutineRunning = false;
        lookAtPlayer = false;
    }


    IEnumerator QuarterCircleEnum(string prefabVersion, bool willRotate)
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject quarterCircleZone;
            Image fillArea1 = null;
            Image fillArea2 = null;
            float angle = 90 * Random.Range(0, 4);

            if (prefabVersion.Equals("single"))
            {
                quarterCircleZone = Instantiate(groundQuarterStaticPrefab);               
            }
            else
            {
                quarterCircleZone = Instantiate(groundQuarterDoublePrefab);
                fillArea2 = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas2").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            }
            fillArea1 = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            Transform collider = quarterCircleZone.transform.Find("Collider").transform;
            quarterCircleZone.transform.rotation = Quaternion.Euler(0f, angle, 0f); 
            

            for (float j = 0; j < 1.01f; j += .005f)
            {
                fillArea1.fillAmount = j;
                if(fillArea2 != null)
                    fillArea2.fillAmount = j;
                               

                if(willRotate)
                    quarterCircleZone.transform.Rotate(new Vector3(0f, 1f, 0f), 30f * Time.deltaTime);

                yield return new WaitForSeconds(.01f);
            }
            Destroy(quarterCircleZone, .5f);
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
        }
        quartercircleCoroutine = null;
        coroutineRunning = false;
    }

   

    IEnumerator TargetCircleEnum()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject canvasCircle = Instantiate(targetCirclePrefab, new Vector3(target.position.x, 0f, target.position.z), Quaternion.identity);
            
            Transform collider = canvasCircle.transform.Find("TargetCircleCollider");
            RectTransform fillCircle = canvasCircle.transform.Find("TargetCircleCanvas").Find("Outer").Find("Inner").transform.GetComponent<RectTransform>();

            for (float j = 0; j <= 1.01f; j+=.02f)
            {          
                fillCircle.localScale = new Vector3(j, j, j);
                yield return new WaitForSeconds(.01f);
            }
            collider.gameObject.SetActive(true);
            Destroy(canvasCircle, 0.1f);
            yield return new WaitForSeconds(1f);
        }
        targetCircleCoroutine = null;
        coroutineRunning = false;
    }

    IEnumerator RotatingBulletHellEnum()
    {

        transform.position = waypointMiddle.transform.position;
        for (int i = 0; i < 10; i++)
        {
            SpawnBullet(ObjectPool.FrozenOrbStatic);
            yield return new WaitForSeconds(1f);
        }
        rotatingBulletHellCoroutine = null;
        coroutineRunning = false;
    }

    IEnumerator RotatingWallsEnum()
    {
        transform.position = waypointMiddle.transform.position;
        // Lag ut-animasjon
        Destroy(Instantiate(rotatingWallsPrefab), 20f);
        yield return new WaitForSeconds(20f);
        rotatingWallsCoroutine = null;
        coroutineRunning = false;
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
