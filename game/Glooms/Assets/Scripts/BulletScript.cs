﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {

    private Rigidbody2D rb2D;
    private bool inAir = true;
    public int dmg = 10;

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //rb2D.isKinematic = true;
        Debug.Log(collision.gameObject.name);
        inAir = false;
        Invoke("DestroyProjectile", 1f);
        // Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name == "Player2" || collision.gameObject.name == "Player1")
        {
            var hit = collision.gameObject;
            var health = hit.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(dmg);
                Debug.Log("Schaden: " + dmg + " / Leben: " + health);
            }
        }

    }

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (inAir)
        {
            Vector2 v = rb2D.velocity;
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
