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


    public Vector2 positionPhase1;
    public Vector2 positionPhase5;
    public Vector2 currentPosition;

    public float transitionDuration;
    public bool inTransition;
    public PlayerMovement.ChannelingState channelingState;

    // Start is called before the first frame update
    void Start()
    {
        playerScript = player.GetComponent<PlayerMovement>();
        //offset = transform.position - player.transform.position;
        channelingState = playerScript.getPlayerChannelingState();
        StartCoroutine(LerpFromTo(offset,
                            new Vector3(0, positionPhase1.x, positionPhase1.y),
                            transitionDuration
                            ));
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, smoothTime);

        if(channelingState != playerScript.getPlayerChannelingState())
        {           
            if (!inTransition)
            {
                inTransition = true;
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

    IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration)
    {
        //float distance = Vector3.Distance(pos1, pos2);
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            //transform.position = Vector3.Lerp(pos1, pos2, t / duration);
            
            offset = Vector3.Lerp(pos1, pos2, t / duration);
            yield return 0;
        }
        //transform.position = pos2;
        offset = pos2;
        inTransition = false;
    }
}
