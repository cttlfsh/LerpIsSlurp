using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float mouseSensitivityX;
    [SerializeField] private float mouseSensitivityY;

    [Header("Other")]
    [SerializeField] private CinemachineVirtualCamera vPlayerCam;
    [SerializeField] private float mass = 70;
    [SerializeField] private float smoothTime;

    private float speed;
    private float yaw;
    private float pitch;
    private float minPitch = 0f;
    private float maxPitch = 10f;
    private bool isGrounded = true;
    private Vector2 pitchClamp = new Vector2(0, 10);
    private Vector3 targetVelocity;
    private Vector3 smoothVelocity;
    private Vector3 initialCameraOffset;
    private Vector3 smoothedCurrentVelocity;
    private Rigidbody rb;
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
        yaw = Input.GetAxis("Mouse X");
        pitch = Input.GetAxis("Mouse Y");
        if (isGrounded)
        {
            Cinemachine.CinemachineTransposer tr = vPlayerCam.GetCinemachineComponent<Cinemachine.CinemachineTransposer>();
            tr.m_FollowOffset.y += pitch * mouseSensitivityY;
            tr.m_FollowOffset.y = Mathf.Clamp(tr.m_FollowOffset.y, minPitch, maxPitch);
        }

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

    private void Interact()
    {
        float maxDistanceRaycast = 10f;
        RaycastHit hitInfo;

        if (Input.GetKeyDown(KeyCode.E))
        {
            print("ciao");
            Debug.DrawRay(vPlayerCam.transform.position, vPlayerCam.transform.forward * 10, Color.red);
            if (Physics.Raycast(vPlayerCam.transform.position, vPlayerCam.transform.forward, out hitInfo, maxDistanceRaycast))
            {
                if (hitInfo.transform.TryGetComponent(out IInteractable tempInteractable))
                {
                    tempInteractable.Interact();
                }
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
        if (!isGrounded)
        {
            yaw = 0;
        }
        rb.rotation = Quaternion.FromToRotation(transform.up, -gravityDirection) 
            * rb.rotation 
            * Quaternion.Euler(new Vector3(0f, yaw * mouseSensitivityX * Time.deltaTime, 0f));
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
            isGrounded = true;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<ICollectable>(out ICollectable tempCollectables))
        {
            tempCollectables.Pickup();
        }
    }
    #endregion

    #region UNITY_METHODS

    private void Awake()
    {
        InitRigidbodySettings();
        transform = this.gameObject.transform;
        speed = movementSpeed;
        initialCameraOffset = vPlayerCam.GetCinemachineComponent<Cinemachine.CinemachineTransposer>().m_FollowOffset;
    }

    private void Start()
    {
        GameManager.Instance.playerReference = this;
        currentPlanet = GameManager.Instance.currentPlanet;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log($"Al momento in saccoccia ho {GameManager.Instance.money} monete!");
    }


    private void Update()
    {
        MovementHandler();
        // Da rimuovere una volta che integro i collezionabili
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameManager.Instance.money++;
            Debug.Log($"Che bello, ho raccolto una moneta, ora ho: {GameManager.Instance.money} monete!");
        }

        Interact();
    }

    private void FixedUpdate()
    {
        SimulateGravitationalPull();
    }

    #endregion
}
