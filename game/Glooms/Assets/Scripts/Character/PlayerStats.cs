﻿/* Written by Dennis Lavrinov */
/* PlayerHealth.cs */
using UnityEngine;
using System.Collections;


public class PlayerStats : MonoBehaviour {
	public int maxHealth = 25;
	float currentHealth = 0;
    public string fraction;

    public int maxExp = 3;
    public int experience = 0;
    public int turnExperience = 0;
    public int level = 1;

    public bool finishedTurn;

    public GameObject expGain;
    public GameObject levelUp;
    public GameObject level2Aura;
    public AudioClip levelUpSound;
    private ParticleSystem expGainPS;
    private ParticleSystem levelUpPS;
    private ParticleSystem level2AuraPS;

    //Sounds
    public string playerHitSound;
    public string playerJumpSound;
    public string playerDeathSound;
    public AudioClip expGainSound;

    //UI
	public SimpleHealthBar healthBar;
    public SimpleHealthBar expBar;

    //etc
    public GameObject blood, deadHead, deadBody, deadLeftLeg, deadLeftHand, deadRightLeg, deadRightHand;

    private void Awake ()
    {
        SoundManager.PlaySound(playerJumpSound);
        expGainPS = expGain.GetComponent<ParticleSystem>();
        levelUpPS = levelUp.GetComponent<ParticleSystem>();
        level2AuraPS = level2Aura.GetComponent<ParticleSystem>();
    }

    void Start ()
	{
		// Set the current health to max values.
		currentHealth = maxHealth;
        healthBar.UpdateBar( currentHealth, maxHealth );
        expBar.UpdateBar(experience, maxExp);
	}

    //-------------------------Health-Functions------------------------
    public void HealPlayer ()
    {
        // Increase the current health by 25%.
        currentHealth += ( maxHealth / 4 );

        // If the current health is greater than max, then set it to max.
        if( currentHealth > maxHealth )
            currentHealth = maxHealth;

        // Update the Simple Health Bar with the new Health values.
        healthBar.UpdateBar( currentHealth, maxHealth );
    }

    public void TakeDamage ( int damage )
	{
		currentHealth -= damage;
        if (currentHealth > 0) SoundManager.PlaySound(playerHitSound);
        if ( currentHealth <= 0 )
		{
		    // Set the current health to zero.
			currentHealth = 0;

			// Run the Death function since the player has died.
			Death();
		}
		healthBar.UpdateBar( currentHealth, maxHealth );
    }

    public void Death ()
	{
        SoundManager.PlaySound(playerDeathSound);
        
        //Animation
        Instantiate(blood, transform.position, Quaternion.identity);
        Instantiate(deadBody, transform.position, Quaternion.identity);
        Instantiate(deadHead, transform.position, Quaternion.identity);
        Instantiate(deadLeftLeg, transform.position, Quaternion.identity);
        Instantiate(deadRightLeg, transform.position, Quaternion.identity);
        Instantiate(deadLeftHand, transform.position, Quaternion.identity);
        Instantiate(deadRightHand, transform.position, Quaternion.identity);
        StartCoroutine("ShakeCamera");

        //Deactivate Player
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<PolygonCollider2D>().enabled = false;
        gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        level2Aura.SetActive(false);
        expBar.gameObject.SetActive(false);

        Invoke("DeactivatePlayer", 30);
    }

    //---------------------------RPG-Functions-------------------------
    public void ExpGain(int exp)
    {
        finishedTurn = false;
        turnExperience += exp;
    }

    public IEnumerator AddExperience()
    {
        Vector3 animationPosition = gameObject.transform.position;
        animationPosition.y += 0.8f;
        while (turnExperience > 0)
        {
            SoundManager.PlayAudioClip(expGainSound);
            experience++;
            turnExperience--;
            expGainPS.Play();
            expBar.UpdateBar(experience, maxExp);
            yield return new WaitUntil(() => !expGainPS.IsAlive());

            //LevelUP
            if (experience == maxExp)
            {
                level++;
                experience = 0;
                maxExp++;
                levelUpPS.Play();
                SoundManager.PlayAudioClip(levelUpSound);
                yield return new WaitUntil(() => !levelUpPS.IsAlive());
                if (level == 2)
                {
                    level2AuraPS.Play();
                    level2Aura.GetComponent<AudioSource>().Play();
                }

                if (level == 3)
                {
                    //todo
                }
                expBar.UpdateBar(experience, maxExp);
                yield return new WaitForSeconds(2);
            }
        }

        //Let´s the camera switch to the next Player
        Invoke("EndTurn", 1);
    }

    private void EndTurn()
    {
        finishedTurn = true;
    }

    //-----------------------------Tools----------------------------
    private void DeactivatePlayer()
    {
        gameObject.SetActive(false);
    }

    public void PlayJumpSound()
    {
        SoundManager.PlaySound(playerJumpSound);
    }

	IEnumerator ShakeCamera ()
	{
		// Store the original position of the camera.
		Vector2 origPos = Camera.main.transform.position;
        for( float t = 0.0f; t < 0.1f; t += Time.deltaTime * 2.0f )
        {
			// Create a temporary vector2 with the camera's original position modified by a random distance from the origin.
			Vector2 tempVec = origPos + Random.insideUnitCircle/5;

			// Apply the temporary vector.
			Camera.main.transform.position = tempVec;

			// Yield until next frame.
			yield return null;
		}

		// Return back to the original position.
		Camera.main.transform.position = origPos;
	}
}
