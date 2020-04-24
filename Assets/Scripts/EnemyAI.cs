using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{

    [Header("Inspector Objects")]
    public NavMeshAgent agent;
    public Transform target;
    public Transform[] waypoints;
    public Transform waypointMiddle;    
    public GameObject portalEffect;
    public Image healthDisplay;
    // Different field abilities
    public GameObject rotatingWalls;
    public GameObject targetCircle;
    public GameObject groundQuarterStatic;
    public GameObject groundQuarterDouble;

    [Header("EnemyAI modifiable variables")]
    public float chaseSpeed;
    public float patrolSpeed;
    public float speed;
    public float teleportTimer = 1;
    public float bulletHellWaves = 10;
    public float healthPool;
    public float stateMachineTimer;

    [HideInInspector] // Internal script variables
    public static EnemyAI enemyAI;    
    private State state;
    private Phase phase;
    private int waypointsIndex;   
    private float currentHealth;
    // Coroutine references
    private IEnumerable movingBulletHellCoroutine;
    private IEnumerable rotatingBulletHellCoroutine;
    private IEnumerable targetCircleCoroutine;
    private IEnumerable quartercircleCoroutine;
    private IEnumerable movingWallsCoroutine;
    private List<IEnumerable> coroutineList;

    private Coroutine currentRunning1;

    public Text tempText;




    public enum State
    {
        CHASE,
        PATROL,
        CASTING
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

    private void Start()
    {

        enemyAI = this;
        currentHealth = healthPool;
        agent = GetComponent<NavMeshAgent>();
        state = State.CASTING;
        phase = Phase.PHASE1;
        agent.stoppingDistance = 3f;
        speed = agent.speed;

        // Setting coroutine references
        movingBulletHellCoroutine = MovingBulletHellEnum();
        rotatingBulletHellCoroutine = RotatingBulletHellEnum();
        targetCircleCoroutine = TargetCircleEnum();        
        movingWallsCoroutine = rotatingWallsEnum();
        // Delegates?
        quartercircleCoroutine = QuarterCircleEnum("double", false, 5);
        //quartercircleCoroutine = QuarterCircleEnum(null, false, 0);
        //quartercircleCoroutine = QuarterCircleEnum(null, false, 0);

        // Adding coroutines to list     
        /*
        coroutineList.Add(movingBulletHellCoroutine);
        coroutineList.Add(rotatingBulletHellCoroutine);
        coroutineList.Add(targetCircleCoroutine);
        coroutineList.Add(quartercircleCoroutine);
        coroutineList.Add(movingWallsCoroutine);
        */
        
        // Start the statemachine
        //StartCoroutine(StateMachine());

        //StartCoroutine(test());

    }


    public IEnumerator StateMachine()
    {
        
        while (GameMasterScript.gameRunning)
        {        
           
            if (state == State.CASTING)
            {
                if (phase == Phase.PHASE1)
                {
                    phase1Routines();
                }
                else if (phase == Phase.PHASE2)
                {
                    phase2Routines();
                }
                else if (phase == Phase.PHASE3)
                {
                    phase3Routines();
                }
            }
            yield return new WaitForSeconds(stateMachineTimer);
        }
        
    }

    private void phase1Routines()
    {
        // Run through routines and pick out 1 at a time. Dont pick same twice in a row
        
        if (currentRunning1 == null)
        {           
            IEnumerator newRoutine = movingBulletHellCoroutine.GetEnumerator();
            currentRunning1 = StartCoroutine(newRoutine);
        }


    }

    private void phase2Routines()
    {
        // Run through routines and pick out 2 at a time + rotation. Dont pick same twice in a row
    }

    private void phase3Routines()
    {
        // Run through routines and pick out 3 at a time + rotation. Dont pick same twice in a row
    }

    /*
    private void Update()
    {
        if (GameMasterScript.gameRunning)
        {
            speed = agent.speed;
            BattleMonitor();



            if (state == State.CHASE)
                Chase();

            if (state == State.PATROL)
                Patrol();

            if (state == State.CASTING)
                BullethellStage1();
        }
    }
    */
    private void BattleMonitor()
    {
        
    }

    private void Chase()
    {
        agent.speed = chaseSpeed;
        if (Vector3.Distance(this.transform.position, target.transform.position) >= 3f)
        {
            agent.SetDestination(target.transform.position);  
        }
       
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if(Vector3.Distance(this.transform.position, waypoints[waypointsIndex].transform.position) >= 2)
        {
            agent.SetDestination(waypoints[waypointsIndex].transform.position);

        }
        else if(Vector3.Distance(this.transform.position, waypoints[waypointsIndex].transform.position) <= 2)
        {
            waypointsIndex = (waypointsIndex + 1) % waypoints.Length;
        }
    }

    private void BullethellStage1()
    {
        transform.LookAt(target);
        /*
        if (movingBulletHellCoroutine == null)
            movingBulletHellCoroutine = StartCoroutine(FrozenOrbEnum());
        */
    }

    IEnumerable MovingBulletHellEnum()
    {
        state = State.CASTING;
        agent.speed = 0f;
        Vector3 nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
        for (int i = 0; i<5; i++)
        {
            
            transform.position = nextPos;

            yield return new WaitForSeconds(teleportTimer);
            SpawnBullet(ObjectPool.FrozenOrb);
            nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
            // Bør pooles
            Destroy(Instantiate(portalEffect, new Vector3(nextPos.x, nextPos.y + .5f, nextPos.z), Quaternion.identity), 10f);
            
            yield return new WaitForSeconds(teleportTimer*3);
        }
    }


    IEnumerable QuarterCircleEnum(string prefabVersion, bool willRotate, int rounds)
    {
        for (int i = 0; i < rounds; i++)
        {
            GameObject quarterCircleZone;
            Image fillArea1 = null;
            Image fillArea2 = null;
            float angle = 90 * Random.Range(0, 4);

            if (prefabVersion.Equals("single"))
            {
                quarterCircleZone = Instantiate(groundQuarterStatic);               
            }
            else
            {
                quarterCircleZone = Instantiate(groundQuarterDouble);
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
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerable TargetCircleEnum()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject canvasCircle = Instantiate(targetCircle, new Vector3(target.position.x, 0f, target.position.z), Quaternion.identity);
            Destroy(canvasCircle, 1f);
            Transform collider = canvasCircle.transform.Find("TargetCircleCollider");
            RectTransform fillCircle = canvasCircle.transform.Find("TargetCircleCanvas").Find("Outer").Find("Inner").transform.GetComponent<RectTransform>();

            for (float j = 0; j <= 1.01f; j+=.02f)
            {          
                fillCircle.localScale = new Vector3(i, i, i);
                yield return new WaitForSeconds(.01f);
            }
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
        }           
    }

    IEnumerable RotatingBulletHellEnum()
    {

        transform.position = waypointMiddle.transform.position;
        for (int i = 0; i < 10; i++)
        {
            SpawnBullet(ObjectPool.FrozenOrbStatic);
            yield return new WaitForSeconds(1f);
        }

    }

    IEnumerable rotatingWallsEnum()
    {
        Destroy(Instantiate(rotatingWalls), 15f);
        yield return new WaitForSeconds(15f);     
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
