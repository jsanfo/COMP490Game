using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapyTestController : MonoBehaviour
{
    public GameObject capy;

    public float leftTurn = 0.2f;
    public float rightTurn = 0.8f;

    private Animator capyAnimator;

    // Start is called before the first frame update
    void Start()
    {
        capyAnimator = capy.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Jumps: 1 = Jump1, 2 = Jump2, 3 = Jump3
        if (Input.GetKey(KeyCode.I))
            capyAnimator.SetTrigger("Jump1");
        else if (Input.GetKey(KeyCode.O))
            capyAnimator.SetTrigger("Jump2");
        else if (Input.GetKey(KeyCode.P))
            capyAnimator.SetTrigger("Jump3");

        //Q = Dance

        if (Input.GetKey(KeyCode.Q))
        {
            capyAnimator.SetTrigger("Dance");
        }

        if (Input.GetKey(KeyCode.W))
        {
            if (!Input.GetKey(KeyCode.LeftShift)) //Walking
                capyAnimator.SetFloat("Speed", 0.5f);
            else //Running
                capyAnimator.SetFloat("Speed", 1.0f);
        }
        else
            capyAnimator.SetFloat("Speed", 0f);

        if (Input.GetKey(KeyCode.A))
        {
            capyAnimator.SetFloat("Turn", leftTurn);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            capyAnimator.SetFloat("Turn", rightTurn);
        }
        else
            capyAnimator.SetFloat("Turn", 0.5f);
        
    }

}
