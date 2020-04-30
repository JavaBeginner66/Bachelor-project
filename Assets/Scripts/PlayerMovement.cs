using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerMovement : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Animator anim;
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;

    // Prefabs
    public GameObject projectileToShoot;
    public GameObject projectile1;
    public GameObject projectile2;
    public GameObject projectile3;
    public GameObject projectile4;
    public GameObject projectileSpawnPoint;
    public GameObject shootingEffect1;
    public GameObject shootingEffect2;
    public GameObject shootingEffect3;
    public GameObject shootingEffect4;
    public GameObject teleportStartEffect;
    public GameObject teleportEndEffect;

    public Image[] dashCharges;
    public Image[] shieldCharges;
    public Image shootTimerDisplay;

    public GameObject[] visualShieldCharges;

    public TextMeshProUGUI attackPowerText;


    [Header("Player modifiable variables ")]
    public float desiredRotationSpeed;
    public float groundDistance = 0.4f;
    public float runningSpeed;
    public float ChannelStateSpeed;
    public float attackPower;
    public float maxAttackPower;
    public float attackPowerModifier;
    public float attackPowerModifierStage1;
    public float attackPowerModifierStage2;
    public float attackPowerModifierStage3;
    public float attackPowerModifierStage4;
    public float channelStage2;
    public float channelStage3;
    public float channelStage4;

    public int availableDashes;
    public int availableShields;
    public float dashFillTime;
    public float maxDashFillTime;
    public float shieldFillTime;
    public float maxShieldFillTime;
    public float teleportDistance;
    public float attackCooldown;
    public float attackCooldownMax;
    public float teleportLagMaxDuration;

    public ChannelingState cState;

    //[HideInInspector] // Internal script variables
    private float inputX;
    private float inputZ;    
    private float animationSpeedMagnitude;
    private float playerSpeed;
    private bool playerIsShooting;
    private bool playerInTeleport;
    private float teleportLagDuration;
    private float maxShieldAmount;
    private float gravity;
    private int currentPhase;

    private Vector3 addedVelocity;

    private Color[] colorArray = {Color.black, Color.blue, Color.red, Color.yellow, Color.cyan };


    private void Start()
    {
        attackPower = StatsScript.ProjectileBaseDamage;
        playerSpeed = runningSpeed;
        attackPowerModifier = attackPowerModifierStage1;
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);      
        shootingEffect3.SetActive(false);
        shootingEffect4.SetActive(false);
        teleportStartEffect.SetActive(false);
        teleportEndEffect.SetActive(false);

        projectileToShoot = projectile1;
        cState = ChannelingState.PHASE_ZERO;
        availableDashes = 1;
        availableShields = 0;
        attackCooldown = attackCooldownMax;
        teleportLagDuration = teleportLagMaxDuration;
        maxShieldAmount = 3;
        gravity = -1f;
        attackPowerText.outlineColor = Color.blue;


    }

    public enum ChannelingState
    {
        PHASE_ZERO = 0,
        PHASE1 = 1,
        PHASE2 = 2,
        PHASE3 = 3,
        PHASE4 = 4,
    }

    public ChannelingState getPlayerChannelingState()
    {
        return this.cState;
    }

    /**
     * Method monitors which phase channeling is in, and 
     * sets a new attack multiplier and adds new effects for corresponding phase
     */
    private void monitorChannelEffects()
    {       

        cState = ChannelingState.PHASE1;
        currentPhase = (int)cState;
        //Debug.Log(currentPhase + "      " + (int)cState);
        if (currentPhase != (int)cState)
        {
            
            lerpTextSizeAndColor(5, colorArray[(int)cState]); 
            currentPhase = (int)cState;
        }

        if (attackPower > channelStage2)
        {
            
            cState = ChannelingState.PHASE2;
            Debug.Log((int)cState);
            shootingEffect1.SetActive(false);
            attackPowerModifier = attackPowerModifierStage2;
            projectileToShoot = projectile2;
            if(!shootingEffect2.activeSelf)
                shootingEffect2.SetActive(true);
        }
        if(attackPower > channelStage3)
        {
            cState = ChannelingState.PHASE3;
            shootingEffect2.SetActive(false);
            attackPowerModifier = attackPowerModifierStage3;
            projectileToShoot = projectile3;
            if (!shootingEffect3.activeSelf)
                shootingEffect3.SetActive(true);
        }
        if (attackPower > channelStage4)
        {
            cState = ChannelingState.PHASE4;
            shootingEffect3.SetActive(false);
            attackPowerModifier = attackPowerModifierStage4;
            projectileToShoot = projectile4;
            if (!shootingEffect4.activeSelf)
                shootingEffect4.SetActive(true);
        }
    }

    /**
     * Method is called from Update()
     * Controlls everything from playermovement to skillset and abilities
     */
    private void playerMovement()
    {

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        addedVelocity.y += gravity * Time.deltaTime;
        moveDirection = moveDirection.normalized;

        // Stops player movement if in teleport
        if (!playerInTeleport)
        {
            // If player is in channeling mode, change movement to mouse-oriented
            if (playerIsShooting)
            {
                monitorChannelEffects();

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float zVel = transform.InverseTransformDirection(moveDirection).z;
                float xVel = transform.InverseTransformDirection(moveDirection).x;
                anim.SetFloat("zVel", zVel, .1f, Time.deltaTime);
                anim.SetFloat("xVel", xVel, .1f, Time.deltaTime);

                if (Physics.Raycast(ray, out hit, 100))
                {
                    transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
                controller.Move(moveDirection * ChannelStateSpeed * Time.deltaTime);


            }
            else if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), desiredRotationSpeed);
                controller.Move(moveDirection * playerSpeed * Time.deltaTime);
            }

            controller.Move(addedVelocity * Time.deltaTime);


            // Calculate input magnitude
            animationSpeedMagnitude = new Vector2(inputX, inputZ).sqrMagnitude;
            anim.SetFloat("Blend", animationSpeedMagnitude, 0f, Time.deltaTime);
            anim.SetFloat("InputX", inputX);
            anim.SetFloat("InputZ", inputX);
      
        

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

        // Code block listens for GetKeyDown and executes code to teleport player and manage dashes
        if (playerIsShooting)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !playerInTeleport && availableDashes >= 1 && dashCharges[0].fillAmount == 1)
            {
                dashCharges[availableDashes - 1].fillAmount = 0f;
                availableDashes--;
                TeleportMode();
                RaycastHit hitRay;
                RaycastHit hitLine;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out hitRay, 100f))
                {
                    if (Physics.Linecast(transform.position, new Vector3(hitRay.point.x, transform.position.y, hitRay.point.z), out hitLine))
                    {
                        Vector3 newPos = Vector3.Lerp(transform.position, new Vector3(hitLine.point.x, transform.position.y, hitLine.point.z), .75f);
                        StartCoroutine(TeleportLag(newPos, teleportLagDuration));
                    }
                    else
                    {
                        StartCoroutine(TeleportLag(new Vector3(hitRay.point.x, transform.position.y, hitRay.point.z), teleportLagDuration));
                    }
                }       
            }
        }

        // Channeling animation
        anim.SetBool("Shoot", playerIsShooting);

        // Fill up dash charges corresponding the channeling phase player is in
        dashFillTime += Time.deltaTime;
        if ((int)cState + 1 > availableDashes)
        {
            if (dashFillTime >= maxDashFillTime)
            {
                dashCharges[0].fillAmount = 1;
                availableDashes++;
                dashFillTime = 0;
            }
        }

        // Manage shield charges if player is isnt charging shot
        if (!playerIsShooting)
        {
            if(!(availableShields >= maxShieldAmount+1))
                shieldFillTime += Time.deltaTime;

            if (shieldFillTime >= maxShieldFillTime)
            {
                shieldCharges[0].fillAmount = 1;                   
                shieldFillTime = 0;
                if (availableShields <= maxShieldAmount)
                {
                    availableShields++;
                    calculateShields();                                 
                }
            }

            // Fill up shield charges visually
            if (availableShields <= maxShieldAmount)
                shieldCharges[availableShields].fillAmount = shieldFillTime / maxShieldFillTime;
                
            
        }

        // Fill up dash charges visually
        if(availableDashes >= 1)
            dashCharges[availableDashes-1].fillAmount = dashFillTime / maxDashFillTime;

        // Increase attack power while player is charging
        if(playerIsShooting)
            if (attackPower <= maxAttackPower)
                attackPower += attackPowerModifier;

        // Display attack power on GUI text
        attackPowerText.text = attackPower.ToString("F0");
    }

    private void lerpTextSizeAndColor(int newSize, Color color)
    {
        // Animasjon for size
        attackPowerText.outlineColor = Color.Lerp(attackPowerText.outlineColor, color, 5f);
        
    }

    // Teleporting freezes player-movement for a bit and triggers animation
    IEnumerator TeleportLag(Vector3 newPos, float lagDuration)
    {
        teleportEndEffect.SetActive(false);
        teleportStartEffect.SetActive(true);
        yield return new WaitForSeconds(lagDuration);
        teleportStartEffect.SetActive(false);
        controller.enabled = false;
        transform.position = newPos;
        controller.enabled = true;
        teleportEndEffect.SetActive(true);
        playerInTeleport = false;
    }

    private void playerChannelAttack()
    {
        playerIsShooting = true;
        shootingEffect1.SetActive(true);
        // Remove the current charging shield charge
        if (availableShields <= maxShieldAmount)
        {
            shieldCharges[availableShields].fillAmount = 0f;
            shieldFillTime = 0f;
        }
    }

    public void playerShieldDamage()
    {   
        if(availableShields >= maxShieldAmount+1)
            shieldCharges[availableShields-1].fillAmount = 0f;
        else
            shieldCharges[availableShields].fillAmount = 0f;

        
        availableShields--;
        calculateShields();
    }

    private void calculateShields()
    {
        // Disable all shields
        for (int i = 0; i < visualShieldCharges.Length; i++)
            visualShieldCharges[i].SetActive(false);
        // Enable shields
        for (int i = 0; i < availableShields; i++)
            visualShieldCharges[i].SetActive(true);
    }

    private void playerReleaseAttack()
    {
        // Reset dash displays
        for (int i = 0; i < dashCharges.Length; i++)
        {
            dashCharges[i].fillAmount = 0f;
        }
        // Reset everything to default or start value
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);
        shootingEffect3.SetActive(false);
        shootingEffect4.SetActive(false);
        availableDashes = 0;
        dashFillTime = maxDashFillTime;
        attackPowerModifier = attackPowerModifierStage1;
        cState = ChannelingState.PHASE_ZERO;
        lerpTextSizeAndColor(5, Color.blue);

        // Creating a projectile, and setting the projectile damage in this current instance
        GameObject proj = Instantiate(projectileToShoot, projectileSpawnPoint.transform.position, transform.rotation);
        Destroy(proj, 10f);
        proj.GetComponent<Projectile>().setProjectileDamage(attackPower);
        attackPower = StatsScript.ProjectileBaseDamage;
        projectileToShoot = projectile1;

        // Exit shooting mode
        playerIsShooting = false;    
    }


    private void TeleportMode()
    {      
        playerInTeleport = true;
    }

    private void Update()
    {
        playerMovement();
    }
}
