using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Animator anim;
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;

    public GameObject arrow;
    public GameObject projectileSpawnPoint;
    public GameObject shootingEffect1;
    public GameObject shootingEffect2;
    public GameObject shootingEffect3;

    public Image dodgeTimerDisplay;
    public Image shootTimerDisplay;
    public Image damageMeter1;
    public Image damageMeter2;
    public Image damageMeter3;

    public Projectile projectileScriptPrefab;

    [Header("Player modifiable variables ")]
    public float desiredRotationSpeed;
    public float groundDistance = 0.4f;
    public float normalSpeed;
    public float rollSpeed;
    public float rollTimerMax;
    public float attackPower;
    public float maxAttackPower;
    public float attackPowerModifier;
    public float attackPowerModifierStage1;
    public float attackPowerModifierStage2;
    public float attackPowerModifierStage3;
    public float channelStage2;
    public float channelStage3;


    [HideInInspector] // Internal script variables
    private float inputX;
    private float inputZ;    
    private float animationSpeedMagnitude;
    private float playerSpeed;
    private bool playerIsShooting;
    private bool playerInAnimation;
    private float rollTimer;
    private bool rollCooldown;
    private bool rollAnimationActive;

    

    

    private void Start()
    {
        attackPower = StatsScript.ProjectileBaseDamage;
        attackPowerModifier = attackPowerModifierStage1;
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        rollTimer = rollTimerMax;
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);      
        shootingEffect3.SetActive(false);

    }

    /* Method gets called by animation event on "Pushback" animation on  Player*/
    public void shootingModeExit()
    {
        playerInAnimation = false;
    }
    // Method gets called by animation event on "Roll", and is used to notify that roll animation is over
    public void rollModeExit()
    {
        rollAnimationActive = false;
        playerInAnimation = false;
    }

    private void monitorChannelEffects()
    {
        if(attackPower <= maxAttackPower)
            attackPower += attackPowerModifier;

        damageMeter1.fillAmount = attackPower / channelStage2;

        if (attackPower > channelStage2)
        {
            shootingEffect1.SetActive(false);
            attackPowerModifier = attackPowerModifierStage2;
            damageMeter2.fillAmount = attackPower / channelStage3;
            if(!shootingEffect2.activeSelf)
                shootingEffect2.SetActive(true);
        }
        if(attackPower > channelStage3)
        {
            shootingEffect2.SetActive(false);
            attackPowerModifier = attackPowerModifierStage3;
            damageMeter3.fillAmount = attackPower / maxAttackPower;
            if (!shootingEffect3.activeSelf)
                shootingEffect3.SetActive(true);
        }
    }

    private void playerMovement()
    {
        anim.ResetTrigger("PushbackTrigger");

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        moveDirection = moveDirection.normalized;

        if (playerIsShooting)
        {
            

            monitorChannelEffects();

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float zVel = transform.InverseTransformDirection(moveDirection).z;
            float xVel = transform.InverseTransformDirection(moveDirection).x;
            anim.SetFloat("zVel", zVel, .1f, Time.deltaTime);
            anim.SetFloat("xVel", xVel, .1f, Time.deltaTime);
            if (!rollAnimationActive)
            {
                if (Physics.Raycast(ray, out hit, 100))
                {
                    transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
                controller.Move(moveDirection * playerSpeed / 3 * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), desiredRotationSpeed);
                controller.Move(moveDirection * playerSpeed * Time.deltaTime);
            }
        }
        else if (moveDirection != Vector3.zero)
        {           
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), desiredRotationSpeed);
            controller.Move(moveDirection * playerSpeed * Time.deltaTime);
        }
        


        // Calculate input magnitude
        animationSpeedMagnitude = new Vector2(inputX, inputZ).sqrMagnitude;
        anim.SetFloat("Blend", animationSpeedMagnitude, 0f, Time.deltaTime);
        anim.SetFloat("InputX", inputX);
        anim.SetFloat("InputZ", inputX);
        
        if (!playerInAnimation && !rollCooldown) 
        {
            if (Input.GetMouseButtonDown(1))
            {
                playerChannelAttack();              
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (playerIsShooting)
                {
                    playerReleaseAttack();                  
                }                            
            }
        }

        if (moveDirection != Vector3.zero)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !rollCooldown) 
            {
                //playerIsShooting = false;               
                RollMode();
            }
        }

        if (rollCooldown)
        {
            rollTimer -= Time.deltaTime;
            dodgeTimerDisplay.fillAmount = rollTimer/1.1f;
            if (rollTimer <= 0)
            {
                rollTimer = rollTimerMax;
                dodgeTimerDisplay.fillAmount = 100;
                rollCooldown = false;
            }
        }

        if (rollAnimationActive)
            playerSpeed = rollSpeed;
        else
            playerSpeed = normalSpeed;
        

        anim.SetBool("Shoot", playerIsShooting);
    }

    private void playerChannelAttack()
    {
        playerIsShooting = true;
        shootingEffect1.SetActive(true);
    }

    private void playerReleaseAttack()
    {
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);
        shootingEffect3.SetActive(false);
        damageMeter1.fillAmount = 0f;
        damageMeter2.fillAmount = 0f;
        damageMeter3.fillAmount = 0f;
        attackPowerModifier = attackPowerModifierStage1;
        // Creating a projectile, and setting the projectile damage in this current instance

        Projectile projClone = Instantiate(projectileScriptPrefab, projectileSpawnPoint.transform.position, transform.rotation);
        projClone.setProjectileDamage(attackPower);
        attackPower = StatsScript.ProjectileBaseDamage;

        anim.SetTrigger("PushbackTrigger");
        playerIsShooting = false;
        playerInAnimation = true;       
    }


    private void RollMode()
    {
        anim.SetTrigger("Roll");
        rollCooldown = true;
        rollAnimationActive = true;
    }

    private void Update()
    {
        playerMovement();
    }
}
