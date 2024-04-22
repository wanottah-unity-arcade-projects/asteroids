﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }
    public Bullet bulletPrefab;

    public float thrustSpeed = 1f;
    public bool thrusting { get; private set; }

    public float turnDirection { get; private set; } = 0f;
    public float rotationSpeed = 0.1f;

    public float respawnDelay = 3f;
    public float respawnInvulnerability = 3f;

    public bool screenWrapping = true;
    private Bounds screenBounds;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject[] boundaries = GameObject.FindGameObjectsWithTag("Boundary");

        // Disable all boundaries if screen wrapping is enabled
        for (int i = 0; i < boundaries.Length; i++) {
            boundaries[i].SetActive(!screenWrapping);
        }

        // Convert screen space bounds to world space bounds
        screenBounds = new Bounds();
        screenBounds.Encapsulate(Camera.main.ScreenToWorldPoint(Vector3.zero));
        screenBounds.Encapsulate(Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f)));
    }

    private void OnEnable()
    {
        // Turn off collisions for a few seconds after spawning to ensure the
        // player has enough time to safely move away from asteroids
        TurnOffCollisions();
        Invoke(nameof(TurnOnCollisions), respawnInvulnerability);
    }

    private void Update()
    {
        thrusting = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            turnDirection = 1f;
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            turnDirection = -1f;
        } else {
            turnDirection = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        if (thrusting) {
            rigidbody.AddForce(transform.up * thrustSpeed);
        }

        if (turnDirection != 0f) {
            rigidbody.AddTorque(rotationSpeed * turnDirection);
        }

        if (screenWrapping) {
            ScreenWrap();
        }
    }

    private void ScreenWrap()
    {
        // Move to the opposite side of the screen if the player exceeds the bounds
        if (rigidbody.position.x > screenBounds.max.x + 0.5f) {
            rigidbody.position = new Vector2(screenBounds.min.x - 0.5f, rigidbody.position.y);
        }
        else if (rigidbody.position.x < screenBounds.min.x - 0.5f) {
            rigidbody.position = new Vector2(screenBounds.max.x + 0.5f, rigidbody.position.y);
        }
        else if (rigidbody.position.y > screenBounds.max.y + 0.5f) {
            rigidbody.position = new Vector2(rigidbody.position.x, screenBounds.min.y - 0.5f);
        }
        else if (rigidbody.position.y < screenBounds.min.y - 0.5f) {
            rigidbody.position = new Vector2(rigidbody.position.x, screenBounds.max.y + 0.5f);
        }
    }

    private void Shoot()
    {
        Bullet bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.Shoot(transform.up);
    }

    private void TurnOffCollisions()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore Collisions");
    }

    private void TurnOnCollisions()
    {
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Asteroid"))
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = 0f;

            GameManager.Instance.OnPlayerDeath(this);
        }
    }

}
