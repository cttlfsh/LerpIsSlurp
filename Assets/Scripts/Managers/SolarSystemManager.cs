using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarSystemManager: MonoBehaviour
{
    public static SolarSystemManager Instance;

    public Planet[] bodies;

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
        Time.fixedDeltaTime = Universe.physicsTimeStep;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].UpdatePlanetVelocity(bodies, Universe.physicsTimeStep);
        }

        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].UpdatePlanetPosition(Universe.physicsTimeStep);
        }
    }



}
