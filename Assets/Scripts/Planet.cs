using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class Planet : MonoBehaviour
{
    private Rigidbody rb;
    private bool recomputeMass = false; // JUST FOR EDIT MODE DEBUG ORBIT
    
    public float mass { get; protected set;}
    public float radius { get; protected set; }
    public float gravity { get { return planetGravity; } }
    public Vector3 velocity { get; protected set; }
    public Vector3 position { get { return rb.position; } }

    public string name;
    public Vector3 initialVelocity;
    [SerializeField] protected float planetGravity;
    [SerializeField] protected float rotationSpeed;


    #region PUBLIC_METHODS
    /// <summary>
    ///  Questa funzione calcola la distanza e la direzione tra i pianeti in
    ///  modo da poter poi calcolare forza e accelerazione del pianeta
    ///  ad ogni `timeStep` e poi aggiornare la velocita'
    ///
    ///  Questo perche' la formula che calcola la forza attrattiva tra 
    ///  pianeti e': 
    ///
    ///             F = G* m1*m2/r^2
    ///
    ///  Dove:
    ///   - G: e' la costante gravitazionale dell'universo
    ///   - m1 e m2: sono le masse dei due pianeti
    ///   - r^2: e' la distanza al quadrato tra i due pianeti
    /// </summary>
    /// <param name="planets">Lista dei pianeti che subiscono la forza gravitazionale</param>
    /// <param name="timeStep">Quantita' di tempo che mi scandisce lo scorrere del tempo</param>
    public void UpdatePlanetVelocity(Planet[] planets, float timeStep)
    {
        foreach (Planet planet in planets)
        {
            if (planet != this)
            {
                Vector3 distance = (planet.rb.position - rb.position);
                float sqrDistance = distance.sqrMagnitude;
                Vector3 forceDirection = distance.normalized;
                //Vector3 force = Universe.gravitationalConstant * forceDirection * planet.mass / sqrDistance;
                Vector3 acceleration = /*Universe.gravitationalConstant * */forceDirection * planet.mass / sqrDistance;
                velocity += acceleration * timeStep;

            }
        }
    }

    /// <summary>
    /// Function which updates the rigidbody position based on the current velocity
    /// </summary>
    /// <param name="timeStep"></param>
    public void UpdatePlanetPosition(float timeStep)
    {
        rb.MovePosition(rb.position + velocity * timeStep);
    }
    #endregion

    #region PRIVATE_METHODS
    private void RotatePlanet()
    {
        transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0f));
    }
    #endregion

    #region UNITY_METHODS
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        mass = rb.mass;
        radius = GetComponent<OctahedronSphere>().sphereRadius;
        velocity = initialVelocity;
    }

    private void Update()
    {
        if (recomputeMass)
        {
            mass = rb.mass;
        }
        RotatePlanet();
    }

    private void OnValidate()
    {
        recomputeMass = true;
    }
    #endregion
}
