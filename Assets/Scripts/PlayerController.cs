using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float jumpForce;

    [Header("Other")]
    [SerializeField] private float mass = 70;
    [SerializeField] private float smoothTime;

    private float speed;
    private bool isGrounded = true;
    private bool canJump = true;

    private Rigidbody rb;
    private Vector3 targetVelocity;
    private Vector3 smoothVelocity;
    private Vector3 smoothedCurrentVelocity;
    private Planet currentPlanet;
    private new Transform transform;

    #region PRIVATE_METHODS

    /// <summary>
    /// Sets the parameters for the player Rigidbody
    /// </summary>
    private void InitRigidbodySettings()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.mass = mass;
    }

    /// <summary>
    /// Handles the movement of the player, getting the values from the 
    /// Horizontal and Vertical axis and computing the velocity to give to the
    /// player
    /// </summary>
    private void MovementHandler()
    {
        float fwdInput = Input.GetAxis("Vertical");
        float rghtInput = Input.GetAxis("Horizontal");
        float fwdMovement = fwdInput * speed * Time.deltaTime;
        float rghtMovment = rghtInput * speed * Time.deltaTime;

        Vector3 movement = new Vector3(rghtMovment, 0, fwdMovement);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = runningSpeed;
        } else
        {
            speed = movementSpeed;
        }
        targetVelocity = transform.TransformDirection(movement.normalized) * speed;
        // sembrerebbe che SmoothDamp sia piu' indicato nei casi in cui il target sia dinamico, quindi vari nel tempo
        smoothVelocity = Vector3.SmoothDamp(smoothVelocity, targetVelocity, ref smoothedCurrentVelocity, smoothTime);

        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                isGrounded = false;
            } else
            {
                rb.AddForce(-transform.up * currentPlanet.gravity, ForceMode.Acceleration);
            } 
        }
    }


    #region GRAVITY_SIMULATION

    /// <summary>
    /// Method which calculate the attractive force of two bodies, in this case of the player towards a planet
    /// </summary>
    /// <param name="planet">Planet of which we want to calculate the attractive force</param>
    /// <param name="sqrDst">Squared distance between the player and the planet</param>
    /// <param name="acceleration">Attractive force towards the planet divided by the mass of the attracted body</param>
    private void ComputeAttractiveForceTowardPlanet(Planet planet, out float sqrDst, out Vector3 acceleration)
    {
        // Compute the attractive force towards the planet
        Vector3 distance = planet.position - rb.position;
        sqrDst = distance.sqrMagnitude;
        Vector3 direction = distance.normalized;
        acceleration = direction * planet.mass / sqrDst;
        rb.AddForce(acceleration);
    }

    /// <summary>
    /// Finds the nearest planet and set its gravity as the main attrative pull
    /// </summary>
    /// <param name="nearestPlanetGravity">Placeholder for the gravity of the nearest planet</param>
    /// <param name="nearestPlanetDistance">Placeholder for the distance from the nearest planet</param>
    /// <param name="planet">Planet from which we are calculating distance</param>
    /// <param name="sqrDst">Squared distance between the player and the planet</param>
    /// <param name="acceleration">Attractive pull towars the planet, aka GRAAAAVITY</param>
    private void FindNearestPlanet(ref Vector3 nearestPlanetGravity, ref float nearestPlanetDistance, Planet planet, float sqrDst, Vector3 acceleration)
    {
        // Check the nearest planet amd set its gravitational pull as the main pull
        float distanceToSurface = Mathf.Sqrt(sqrDst) - planet.radius;
        if (distanceToSurface < nearestPlanetDistance)
        {
            nearestPlanetDistance = distanceToSurface;
            nearestPlanetGravity = acceleration;
        }
    }

    /// <summary>
    /// Simulates the real world attractive force which keeps the player on the planet groud, at least until a much stronger
    /// gravitational pull arrives.
    /// </summary>
    private void SimulateGravitationalPull()
    {
        Vector3 nearestPlanetGravity = Vector3.zero;
        float nearestPlanetDistance = float.MaxValue;

        foreach (Planet planet in SolarSystemManager.Instance.bodies)
        {
            float sqrDst;
            Vector3 acceleration;
            ComputeAttractiveForceTowardPlanet(planet, out sqrDst, out acceleration);
            FindNearestPlanet(ref nearestPlanetGravity, ref nearestPlanetDistance, planet, sqrDst, acceleration);

        }
        // Align the player towards the planet
        Vector3 gravityDirection = nearestPlanetGravity.normalized;
        rb.rotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * rb.rotation;
        rb.MovePosition(rb.position + smoothVelocity * Time.fixedDeltaTime);
    }
    #endregion

    #endregion PRIVATE_METHODS

    #region PUBLIC_METHODS
    /// <summary>
    /// Sets the local reference to the planet the player is currently on.
    /// </summary>
    /// <param name="newPlanet">The planet where the player is.</param>
    public void ChangePlanet(Planet newPlanet)
    {
        currentPlanet = newPlanet;
    }
    #endregion

    #region COLLISION/TRIGGER METHODS
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);
        //TODO: Da sistemare cambiando il check
        if (collision.gameObject.name == "Earth")
        {
            Debug.Log("INSIDE COLLSION IF");
            isGrounded = true;
        }
    }
    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        InitRigidbodySettings();
        transform = this.gameObject.transform;
        speed = movementSpeed;
    }

    private void Start()
    {
        GameManager.Instance.playerReference = this;
    }


    private void Update()
    {
        MovementHandler();
    }

    private void FixedUpdate()
    {
        SimulateGravitationalPull();
    }

    #endregion
}
