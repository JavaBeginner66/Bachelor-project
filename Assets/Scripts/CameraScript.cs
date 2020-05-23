using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
 * Script handles the camera zoom/follow and music
 */
public class CameraScript : MonoBehaviour
{

    public Transform player;                    // Player position reference
    public PlayerMovement playerScript;         // Player script reference
    public Vector3 offset;                      // Offset in position to player
    public AudioSource audio;                   // Audio source component

    public float smoothTime;                    // Smooths camera movement
    private Vector3 velocity = Vector3.zero;    // Template variable to fill parameterlist

    public Vector2 positionPhase1;              // Position when player is in base mode
    public Vector2 positionPhase5;              // Position when player is in channeling mode          

    public float transitionDuration;            // How long a transition from base to channeling position will take
    public bool inTransition;                   // Is camera currently in transition
    public PlayerMovement.ChannelingState channelingState;  // Player channeling state


    void Start()
    {
        playerScript = player.GetComponent<PlayerMovement>();
        channelingState = playerScript.getPlayerChannelingState();
        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase1.x, positionPhase1.y),
                            transitionDuration
                            ));
        // Set volume according to volume slider in menu
        audio.volume = PlayerPrefs.GetFloat(MenuScript.volumeKey);
    }

    /*
     * Update listens for a change in state on player, and triggers a 
     * coroutine that changes position
     */
    void Update()
    {
        // Follow player position
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, smoothTime);

        // If the channeling state is changed since last frame
        if(channelingState != playerScript.getPlayerChannelingState())
        {           
            if (!inTransition)
            {
                inTransition = true;
                // If player state is higher than state 1, camera should change position to channeling mode
                if(playerScript.getPlayerChannelingState() >= PlayerMovement.ChannelingState.STATE1)
                {
                    StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase5.x, positionPhase5.y), 
                            transitionDuration
                            ));
                    channelingState = PlayerMovement.ChannelingState.STATE1;
                }
                else
                {                   
                    StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase1.x, positionPhase1.y),
                            transitionDuration
                            ));
                    channelingState = PlayerMovement.ChannelingState.STATE_ZERO;
                }              
            }
        }   
    }

    /**
     * IEnumerator will gradually change the camera position in the Y-axis
     * @param pos1 current position
     * @param pos2 next position
     * @param duration how long transition should take
     */
    IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            offset = Vector3.Lerp(pos1, pos2, t / duration);
            yield return 0;
        }
        offset = pos2;
        inTransition = false;
    }
}
