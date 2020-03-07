using System.Collections;
using System.Collections.Generic;
using SO;
using UnityEngine;
using Zenject;

public class PlanetController : MonoBehaviour
{
    [SerializeField] private float orbitRadius;
    [SerializeField] private float angularVelocity;
    [SerializeField] private float rotationVelocity;
    [SerializeField] private bool clockwise;
    
    private float currentAngleToSun = 0;
    private RocketController.Factory _rocketFactory;

    [Inject]
    void Construct(RocketController.Factory rocketFactory)
    {
        _rocketFactory = rocketFactory;
    }
    void Start()
    {
        
    }

    void Update()
    {
        var positionX = orbitRadius * Mathf.Cos(currentAngleToSun);
        var positionY = orbitRadius * Mathf.Sin(currentAngleToSun);
        currentAngleToSun += angularVelocity * Time.deltaTime;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
        var selfRotationDelta = rotationVelocity * Time.deltaTime;
        transform.Rotate(Vector3.forward, clockwise ? selfRotationDelta : -selfRotationDelta);
    }

    public void Shoot()
    {
        _rocketFactory.Create(transform.position, transform.rotation, RocketType.Fast);
    }
}
