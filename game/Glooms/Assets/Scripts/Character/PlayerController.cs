﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : PhysicsObject
{
    public Button movementButton;
    public Button weaponButton;
    public float maxSpeed = 2f;
    public float jumpTakeOffSpeed = 5;
    public bool aimingMode = false;
    public bool movingMode = false;
    public bool passiveMode;
    public bool jumped;
    public bool flipped = false;

    private ShootingWeapon shootingWeaponScript;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform canvasTransform;

    // Use this for initialization
    void Awake()
    {
        shootingWeaponScript = gameObject.GetComponentInChildren<ShootingWeapon>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        canvasTransform = transform.Find("Canvas");
        movementButton = GameObject.Find("Movement Button").GetComponent<Button>();
        movementButton.onClick.AddListener((UnityEngine.Events.UnityAction)this.MovingModeActive);
        weaponButton = GameObject.Find("Weapon Button").GetComponent<Button>();
        weaponButton.onClick.AddListener((UnityEngine.Events.UnityAction)this.AimingModeActive);
    }

    protected override void HandlePlayer()
    {
        if (!passiveMode)
        {
            if (movingMode)
            {
                Moving();
            }
            if (aimingMode)
            {
                animator.SetTrigger("Aim");
            }
        } else
        {
            animator.SetTrigger("Idle");
        }
    }

    private void Moving()
    {
        //Animation
        if (!inAir)
        {
            if (Input.GetAxis("Horizontal") != 0)
            {
                animator.SetTrigger("Walk");
            }
            else
            {
                animator.SetTrigger("Idle");
            }
        }
        else
        {
            if (jumped)
            {
                animator.SetTrigger("Offground");
            }
        }

        Vector2 move = Vector2.zero;
        move.x = Input.GetAxis("Horizontal");

        //Jump
        if (Input.GetButtonDown("Jump") && grounded)
        {
            jumped = true;
            SoundManager.PlaySound(gameObject.GetComponent<PlayerHealth>().playerJumpSound);
            velocity.y = jumpTakeOffSpeed;
        }   
        else if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > 0)
            {
                velocity.y = velocity.y * 0.5f;
            }
        }

        //Flip player when moving
        bool flipSprite = (flipped ? (move.x > 0.01f) : (move.x < -0.01f));
        if (flipSprite)
        {
            Flip();
        }

        targetVelocity = move * maxSpeed;
    }

    //Flip
    public void Flip()
    {
        flipped = !flipped;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
        canvasTransform.localScale = newScale;
        newScale = shootingWeaponScript.transform.localScale;
        newScale.x *= -1;
        newScale.y *= -1;
        shootingWeaponScript.transform.localScale = newScale;
    }

    //Set To Aiming Mode
    public void AimingModeActive()
    {
        if (!passiveMode)
        {
            movingMode = false;
            aimingMode = true;
            transform.Find("ShootingWeapon").gameObject.GetComponent<ShootingWeapon>().SetActive(true);
        }
    }

    //Set To Moving Mode
    public void MovingModeActive()
    {

        if (!passiveMode)
        {
            aimingMode = false;
            movingMode = true;
            transform.Find("ShootingWeapon").gameObject.GetComponent<ShootingWeapon>().SetActive(false);

        }
    }

    //Player is Passive
    public void SetPassive()
    {
        movingMode = false;
        aimingMode = false;
        passiveMode = true;
        transform.Find("ShootingWeapon").gameObject.GetComponent<ShootingWeapon>().SetActive(false);
    }

    //Player is Activ
    public void SetActive()
    {
        passiveMode = false;
    }

    //Flip Character VERALTET******************
    /*public void FlipX(bool bo)
    {
        spriteRenderer.flipX = bo;
    }*/

    public void SetJumped(bool bo)
    {
        jumped = bo;
    }
}
