﻿using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

    public float paddleSpeed = 1f;
    public string dstring = "Merge this shit digger!";

    public string dstring = "Merge this!";

    private Vector3 playerPos = new Vector3 (0, -9.5f, 0);

    void Update () 
    {
        float xPos = transform.position.x + (Input.GetAxis("Horizontal") * paddleSpeed);
        playerPos = new Vector3 (Mathf.Clamp (xPos, -8f, 8f), -9.5f, 0f);
        transform.position = playerPos;

    }
}