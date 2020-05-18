using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/**
 * Class takes care of everything that has to do with player.
 * This includes player movement, player GUI, 
 * player abilities such as shields and channeling, collision detection,
 * animations, and current state of alive/dead
 */

    
public class PlayerMovement : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Animator anim;                       // Animation controller
    public CharacterController controller;      // Character controller
    public Transform groundCheck;               // Empty gameobject placed at players feet to check if touching ground
    public LayerMask groundMask;                // Mask that describes what is ground

    // Prefabs   
    public GameObject projectileSpawnPoint;     // Location of where a projectile should fire from
    public GameObject teleportStartEffect;      // Teleport particle effect
    public GameObject teleportEndEffect;        // Teleport particle effect
    public GameObject[] projectileList;         // List of projectile particle effects
    public GameObject[] shootingEffectList;     // List of shooting particle effects
    public GameObject projectileToShoot;        // References the active shooting effect in shooting mode so the correct projectile shoots
    public GameObject destructablePlayerModel;  // A model of player that is in pieces

    public Image[] dashCharges;                 // Array of images used both logically in dash code and for display in UI
    public Image[] shieldCharges;               // Array of images used both logically in shield code and for display in UI
    public GameObject[] visualShieldCharges;    // Array of shields rotating player to display current amount

    public TextMeshProUGUI attackPowerText;     // Text displaying attack power
    public TextMeshProUGUI shieldChargesText;   // Text displaying amount of shield charges
    public TextMeshProUGUI dashChargesText;     // Text displaying amount of dash charges

    public Transform shieldBreakPosition;       // Location to trigger shield-breaking effect
    public GameObject playerHitEffect;          // Shield breaking effect

    private GameObject currentShootingEffect;   // References the active shooting effect in shooting mode so I can disable/enable the next/previous

    [Header("Player modifiable variables ")]
    public float desiredRotationSpeed;          // How fast player can rotate when running in base mode
    public float groundDistance = 0.4f;         // Distance to where ground will register as touched
    public float runningSpeed;                  // Base speed of player
    public float ChannelStateSpeed;             // Speed of player while in channel mode
    public float attackPower;                   // Attack power value
    public float maxAttackPower;                // Maximum attack power value
    public float attackPowerModifier;           // Variable attackpower gets multiplied to with increasing channel-stages
    public float[] attackPowerModifierStages;   // Array of values attackpower can get multiplied by
    public float nextPhaseTimer;                // Countdown timer that manages when the next channeling stage will start/end

    public int availableDashes;                 // Value holds current available dashes
    public int availableShields;                // Value holds current available shields
    public float dashFillTime;                  // How long each dash charge takes to fill
    public float maxDashFillTime;               // Countdown roof
    public float shieldFillTime;                // How long each shield charge takes to fill
    public float maxShieldFillTime;             // Countdown roof   
    private float shootingCooldown;             // How long before player can start channeling after releasing
    private float shootingCooldownMax;          // Countdown roof
    public float teleportLagMaxDuration;        // Value roof

    public ChannelingState cState;              // The current channeling state of player

    //[HideInInspector] // Internal script variables
    private float inputX;                       // Detects x-axis input from player
    private float inputZ;                       // Detects z-axis input from player
    private float animationSpeedMagnitude;      // Connects animation speed with player input
    private float playerSpeed;                  // Current player speed   
    private bool playerIsShooting;              // True if player is currently in channeling mode
    private bool playerInTeleport;              // True if player is in the middle of a teleport
    private float teleportLagDuration;          // The duration which player can't be controlled during a teleport
    private float maxShieldAmount;              // Max shield amount
    private float gravity;                      // Variable used to keep player grounded
    private float nextPhaseTimerMax;            // The value which nextPhaseTimer will copy from on new channeling phase
    private float nextPhaseTimerBase;           // The value which nextPhaseTimerMax will copy from on release
    private int baseFontSize;                   // Base font size used to set attackpower size back to normal on release
    
    private Vector3 addedVelocity;              // Takes in the gravity value and adds it to the overall movement

    private Color[] colorArray = {Color.black, Color.blue, Color.green, // Color array used on attack power font
                                Color.red, new Color(1f,0f,.67f, 1f), new Color(.5f,0f,.5f, 1),
                                Color.white, Color.cyan, Color.yellow, Color.black};

    /**
     * Start is used to get some references and set up variable values
     */
    private void Start()
    {
        anim = transform.GetComponentInChildren<Animator>();

        attackPower = StatsScript.ProjectileBaseDamage;
        playerSpeed = runningSpeed;
        attackPowerModifier = attackPowerModifierStages[0];
        controller = GetComponent<CharacterController>();
        
        for (int i = 0; i < shootingEffectList.Length; i++)
        {
            shootingEffectList[i].SetActive(false);
        }
        teleportStartEffect.SetActive(false);
        teleportEndEffect.SetActive(false);

        projectileToShoot = projectileList[0];
        cState = ChannelingState.STATE_ZERO;
        availableDashes = 1;
        availableShields = 0;
        teleportLagDuration = teleportLagMaxDuration;
        maxShieldAmount = 3;
        gravity = -1f;
        attackPowerText.outlineColor = Color.blue;
        nextPhaseTimerBase = 2f;
        nextPhaseTimerMax = nextPhaseTimerBase;
        baseFontSize = 50;
        shootingCooldownMax = 2f;
        shieldChargesText.text = availableShields.ToString();
        dashChargesText.text = (availableDashes - 1).ToString();

    }

    /**
     * Enum that holds all channeling states
     */
    public enum ChannelingState
    {
        STATE_ZERO = 0,
        STATE1 = 1,
        STATE2 = 2,
        STATE3 = 3,
        STATE4 = 4,
        STATE5 = 5,
        STATE6 = 6,
        STATE7 = 7,
        STATE8 = 8,
        STATE9 = 9,
    }

    /**
     * Enum get method
     */
    public ChannelingState getPlayerChannelingState()
    {
        return this.cState;
    }

    /**
     * Method monitors which phase channeling is in, and 
     * sets a new attack multiplier and adds new effects for corresponding phase
     */

    private void nextChannelingPhase(ChannelingState state)
    {
        // Makes next phase 1 second longer
        nextPhaseTimerMax++;
        // Manages attack power text
        StartCoroutine(lerpTextSizeAndColor(colorArray[(int)cState]));       
        
        // state starts at 1, so to target the previous state effect, the correct check is -2
        if((int)state - 2 >= 0)
            shootingEffectList[(int)state-2].SetActive(false);

        // ChannelingState index has 0 occupied with no-shooting state, so (int)state-1 is to target the right index
        attackPowerModifier = attackPowerModifierStages[(int)state];
        projectileToShoot = projectileList[(int)state-1];
        if (!shootingEffectList[(int)state - 1].activeSelf)
        {
            shootingEffectList[(int)state - 1].SetActive(true);
            currentShootingEffect = shootingEffectList[(int)state - 1];
        }
             
    }

    /**
     * Method is called from Update()
     * Controlls everything from playermovement to skillset and abilities
     */
    private void playerMovement()
    {
        // Detects input from player
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        // Puts direction into a Vector3
        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        // Adds gravity 
        addedVelocity.y += gravity * Time.deltaTime;
        // Normalizing direction to avoid increased speed obliquely
        moveDirection = moveDirection.normalized;

        // Stops player movement if in teleport
        if (!playerInTeleport)
        {
            // If player is in channeling mode, change movement to mouse-oriented
            if (playerIsShooting)
            {
                // Manage timer for when next channeling phase starts
                if (nextPhaseTimer <= 0)
                {
                    if(cState <= ChannelingState.STATE8)
                        nextChannelingPhase(++cState);
                    nextPhaseTimer = nextPhaseTimerMax;
                }
                else
                {
                    nextPhaseTimer -= Time.deltaTime;
                }

                
                RaycastHit hit;
                // Find location of mouse in 3d space
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                // Find direction in world space as in north/west/south/east (not locally), 
                //so player can trigger the correct walking animations while aiming somewhere else.
                float zVel = transform.InverseTransformDirection(moveDirection).z;
                float xVel = transform.InverseTransformDirection(moveDirection).x;
                anim.SetFloat("zVel", zVel, .1f, Time.deltaTime);
                anim.SetFloat("xVel", xVel, .1f, Time.deltaTime);

                // If mouse is hovering over a collider (collider is 3x the size of the arena to guarantee a hit)
                if (Physics.Raycast(ray, out hit, 100))
                {
                    // Look at mouse position
                    transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));                  
                }
                controller.Move(moveDirection * ChannelStateSpeed * Time.deltaTime);


            }
            // If player isn't shooting, use normal player movement (locally)
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

           
            shootingCooldown -= Time.deltaTime;

            // If player clicks or holds right mouse button, activate shooting mode
            if (Input.GetMouseButtonDown(1))
            {
                if (shootingCooldown <= 0)
                    playerChannelAttack();

            }
            // If player releases, go back to base mode
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
                
                // If player is hovering over a ground with a collider
                if (Physics.Raycast(ray, out hitRay, 100f))
                {
                    // If player is attempting to teleport outside of the arena, limit the teleport position to be inside
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
                shieldCharges[availableShields].fillAmount = 1;
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
                attackPower += attackPowerModifier * Time.deltaTime;

        // Display attack power on GUI text
        attackPowerText.text = attackPower.ToString("N0");

        // Update shield and dash charge texts
        shieldChargesText.text = availableShields.ToString();
        dashChargesText.text = (availableDashes - 1).ToString();
    }

    /**
     * IEnumerator that changes color and size of attack power font on new channeling phase
     */
    private IEnumerator lerpTextSizeAndColor(Color color)
    {
        if(cState == ChannelingState.STATE9)
        {
            attackPowerText.outlineColor = Color.Lerp(attackPowerText.outlineColor, Color.black, 1f);
            attackPowerText.faceColor = Color.Lerp(attackPowerText.faceColor, Color.white, 1f);
        }            
        else
        {
            attackPowerText.outlineColor = Color.Lerp(attackPowerText.outlineColor, color, 1f);
        }
        
        for (int i = 0; i < 20; i++)
        {
            attackPowerText.fontSize += 1f;
            yield return new WaitForSeconds(.0005f);
        }
        for (int i = 0; i < 15; i++)
        {
            attackPowerText.fontSize -= 1f;
            yield return new WaitForSeconds(.0001f);
        }
    }
    /**
     * IEnumerator freezes player-movement for a bit and triggers animation teleport animation
     */
    private IEnumerator TeleportLag(Vector3 newPos, float lagDuration)
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

    /**
     * Method for setting playerIsShooting to true and removing currently charging shields 
     */
    private void playerChannelAttack()
    {
        playerIsShooting = true;
        shootingEffectList[0].SetActive(true);
        // Remove the current charging shield charge
        if (availableShields <= maxShieldAmount)
        {
            shieldCharges[availableShields].fillAmount = 0f;
            shieldFillTime = 0f;
        }
    }

    /**
     * Method to deal with player taking damage
     */
    public void playerShieldDamage()
    {
        // Shield breaking particle effect
        Destroy(Instantiate(playerHitEffect, shieldBreakPosition.position, Quaternion.identity), 5f);

        if(availableShields >= maxShieldAmount+1)
            shieldCharges[availableShields-1].fillAmount = 0f;
        else
            shieldCharges[availableShields].fillAmount = 0f;
        
        availableShields--;
        
        if (availableShields < 0)
            StartCoroutine(GameOver());  
        
        calculateShields();
    }

    /**
     * Local gameover method for player
     */
    private IEnumerator GameOver()
    {
        // Stop current running effect
        if (currentShootingEffect != null)
          currentShootingEffect.SetActive(false);
        // Stop animations
        anim.SetBool("GameOver", GameMasterScript.gameRunning);
        // Shatter player
        Instantiate(destructablePlayerModel, transform.position, Quaternion.identity);
        // Disable player model
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(1f); 
        // Call gameover in main script
        GameMasterScript.gameMasterScript.GameOver();
    }

    /**
     * Method recalculates shields after taking damage
     */
    private void calculateShields()
    {
        // Disable all shields
        for (int i = 0; i < visualShieldCharges.Length; i++)
            visualShieldCharges[i].SetActive(false);
        // Enable shields
        for (int i = 0; i < availableShields; i++)
            visualShieldCharges[i].SetActive(true);       
    }

    /**
     * Method resets player to the state before shooting  mode started
     */
    private void playerReleaseAttack()
    {
        // Reset dash displays
        for (int i = 0; i < dashCharges.Length; i++)
        {
            dashCharges[i].fillAmount = 0f;
        }
        
        for (int i = 0; i < shootingEffectList.Length; i++)
            shootingEffectList[i].SetActive(false);

        // Reset everything to default or start value
        availableDashes = 0;
        dashFillTime = maxDashFillTime;
        attackPowerModifier = attackPowerModifierStages[0];
        attackPowerText.fontSize = baseFontSize;
        attackPowerText.outlineColor = colorArray[1];
        attackPowerText.faceColor = Color.black;
        cState = ChannelingState.STATE_ZERO;
        nextPhaseTimer = 0f;
        nextPhaseTimerMax = nextPhaseTimerBase;
        shootingCooldown = shootingCooldownMax;

        // Creating a projectile, and setting the projectile damage in this current instance
        GameObject proj = Instantiate(projectileToShoot, projectileSpawnPoint.transform.position, transform.rotation);
        Destroy(proj, 10f);
        // Getting a reference to the Projectile script on projectile, and using method to set attack power
        proj.GetComponent<Projectile>().setProjectileDamage(attackPower);
        // Resetting attack power after projectile inherited it
        attackPower = StatsScript.ProjectileBaseDamage;
        projectileToShoot = projectileList[0];

        // Exit shooting mode
        playerIsShooting = false;    
    }


    private void TeleportMode()
    {      
        playerInTeleport = true;
    }

    private void Update()
    {
        if(!GameMasterScript.gameIsPaused && GameMasterScript.gameRunning)
            playerMovement();

    }
}
