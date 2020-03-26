using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    public Transform player;
    public PlayerMovement playerScript;
    public Vector3 offset;

    public float smoothTime;
    private Vector3 velocity = Vector3.zero;

    public Vector2 positionPhase0;
    public Vector2 positionPhase1;
    public Vector2 positionPhase2;
    public Vector2 positionPhase3;
    public Vector2 positionPhase4;
    public Vector2 currentPosition;

    public float transitionDuration;
    public bool inTransition;
    public PlayerMovement.ChannelingState channelingState;

    /**
     *  0: 9 -5,7
        1: 12  -7.46
        2: 15  -9.85
        3: 18   -12.47
        4: 21  -14.05
     */

    // Start is called before the first frame update
    void Start()
    {
        playerScript = player.GetComponent<PlayerMovement>();
        //offset = transform.position - player.transform.position;
        channelingState = playerScript.getPlayerChannelingState();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, smoothTime);

        if(channelingState != playerScript.getPlayerChannelingState())
        {
            
           // if (!inTransition)
            //{
                Debug.Log("YO");
                inTransition = true;
                switch (playerScript.getPlayerChannelingState())
                {
                    case PlayerMovement.ChannelingState.PHASE_ZERO:
                        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase0.x, positionPhase0.y), transitionDuration));
                        channelingState = PlayerMovement.ChannelingState.PHASE_ZERO;
                        break;
                    case PlayerMovement.ChannelingState.PHASE1:
                        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase1.x, positionPhase1.y), transitionDuration));
                        channelingState = PlayerMovement.ChannelingState.PHASE1;
                        break;
                    case PlayerMovement.ChannelingState.PHASE2:
                        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase2.x, positionPhase2.y), transitionDuration));
                        channelingState = PlayerMovement.ChannelingState.PHASE2;
                        break;
                    case PlayerMovement.ChannelingState.PHASE3:
                        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase3.x, positionPhase3.y), transitionDuration));
                        channelingState = PlayerMovement.ChannelingState.PHASE3;
                        break;
                    case PlayerMovement.ChannelingState.PHASE4:
                        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase4.x, positionPhase4.y), transitionDuration));
                        channelingState = PlayerMovement.ChannelingState.PHASE4;
                        break;
                }
            //}
        }   
    }

    IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            //transform.position = Vector3.Lerp(pos1, pos2, t / duration);'
            offset = Vector3.Lerp(pos1, pos2, t / duration);
            yield return 0;
        }
        //transform.position = pos2;
        offset = pos2;
        inTransition = false;
    }
}
