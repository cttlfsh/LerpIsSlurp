using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetRotation : MonoBehaviour
{

    //VARIABILE DA PRENDERE DA UN MANAGER
    [Range(0, 360)] public float planetRotation;

    private new Transform transform;

    private void Awake()
    {
        transform = this.gameObject.transform;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(new Vector3(0f, planetRotation, 0f));
    }
}
