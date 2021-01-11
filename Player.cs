using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // configuration parameters
    [Header("Player")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float padding = 0.5f;
    [SerializeField] int health = 200;


    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] GameObject standardLaserPrefab;
    [SerializeField] GameObject powerUpLaserPrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFiringPeriod = 0.1f;

    [Header("Death FX")]
    [SerializeField] AudioClip deathSFX;
    [SerializeField] [Range(0, 2)] float deathSFXVolume = 1.75f;
   

    [Header("Shooting SFX")]
    [SerializeField] AudioClip laserSFX;
    [SerializeField] [Range(0, 1)] float laserSFXVolume = 0.5f;


    [Header("Power Up")]
    [SerializeField] AudioClip powerUpSFX;
    [SerializeField] [Range(0, 1)] float powerUpSFXVolume = 0.5f;
    [SerializeField] GameObject countdownTimer;

    bool activatedPowerUp = true;

    

    Coroutine firingCoroutine;

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    // Start is called before the first frame update
    void Start()
    {
        SetUpMoveBoundaries();
        countdownTimer.SetActive(false);
        
    }

   

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        PowerUp powerUp  = other.gameObject.GetComponent<PowerUp>();
        DamageDealer damageDealer = other.gameObject.GetComponent<DamageDealer>();

        if (powerUp)
        {
            Destroy(other.gameObject);
            AudioSource.PlayClipAtPoint(powerUpSFX, Camera.main.transform.position, powerUpSFXVolume); 
            StartCoroutine(ActivatePowerUp());
        }
        
        else if (damageDealer)
        {
            ProcessHit(damageDealer);
        }
        
        if(!powerUp && !damageDealer) { return; }
        
    }

    private IEnumerator ActivatePowerUp()
    {
        Debug.Log("Powerup Activated");
        StartCoroutine(StartCountdown());
        activatedPowerUp = false;
        projectileFiringPeriod = 0.01f;
        laserPrefab = powerUpLaserPrefab;

        yield return new WaitForSeconds(5f);
        laserPrefab = standardLaserPrefab;
        activatedPowerUp = true;
        projectileFiringPeriod = 0.1f;
    }

    IEnumerator StartCountdown()
    {
        countdownTimer.SetActive(true);
        for (int i = 5; i > 0; i--)
        {
            countdownTimer.GetComponent<Text>().text = i.ToString();
            yield return new WaitForSeconds(1);
        }
        countdownTimer.SetActive(false);
    }

    public bool NoActivePowerUp()
    {
        return activatedPowerUp;
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        if (health <= 0)
        {
            Die();
        }

    }

    
    

    private void Die()
    {
        FindObjectOfType<Level>().LoadGameOver();
        Destroy(gameObject);
        AudioSource.PlayClipAtPoint(deathSFX, Camera.main.transform.position, deathSFXVolume);
    }

    public int GetHealth()
    {
        return health;
    }

    private void Fire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            firingCoroutine = StartCoroutine(FireContinuously());  
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject laser = Instantiate(
              laserPrefab,
              transform.position,
              Quaternion.identity) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            AudioSource.PlayClipAtPoint(laserSFX, Camera.main.transform.position, laserSFXVolume);
            yield return new WaitForSeconds(projectileFiringPeriod);
        }
       
    }

    private void Move()
    {
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);
        transform.position = new Vector2(newXPos, newYPos);
        
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;
        xMin = gameCamera.ViewportToWorldPoint(new Vector3(0.3f, 0, 0)).x + padding;
        xMax = gameCamera.ViewportToWorldPoint(new Vector3(0.68f, 0, 0)).x - padding;
        yMin = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + padding;
        yMax = gameCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - padding;
    }
}
