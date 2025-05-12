using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerController : MonoBehaviour
{
    // Start is called before the first frame update
    public float battery;
    public float kwh;

    float prevMotor = 0;
    float calcMotor = 0;

    public float mult = 0;  // Used to enable / disable power consumption outside of navigation terrain.  Should be set to 0.02f by StartCollider.cs
    CarController car;

    void Start()
    {
        car = gameObject.GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float getPowerEfficiency()
    {
        return car.getPowerEfficiency();
    }

    void FixedUpdate()
    {
        prevMotor = car.prevMotor;
        calcMotor = car.calcMotor;
        // Acceleration fuel draw
        kwh += ((((calcMotor + prevMotor)/2) * mult)/46)/3600;
        // Static fuel draw
        kwh += 0.0001f;
    }
}
