using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents.Actuators;

public class flying_dutchman : Agent
{

    public GameObject aircraft;
    public GameObject scene;

    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    //[SerializeField]
    //Text displayText = null;

    float thrustPercent;
    float brakesTorque;
    float time_elapsed;
    float max_alt;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    public override void Initialize()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        max_alt = 0f;
        time_elapsed = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(aircraft.transform.position-scene.transform.position);
        sensor.AddObservation(aircraft.transform.rotation);
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(rb.angularVelocity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Pitch = actions.DiscreteActions[0]-1;
        Roll = actions.DiscreteActions[1]-1;
        Yaw = actions.DiscreteActions[2]-1;

        if (actions.DiscreteActions[3]==1)
        {
            thrustPercent = thrustPercent > 0 ? 0 : 1f;
        }
        if (actions.DiscreteActions[4]==1)
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (actions.DiscreteActions[5]==1)
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }
        /**
        displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "T: " + (int)(thrustPercent * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";
        **/
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        time_elapsed += Time.fixedDeltaTime;
        RequestDecision();
        SetControlSurfacesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }

        if(max_alt< transform.position.y)
        {
            max_alt = transform.position.y;
        }
        
        if (time_elapsed>30 || transform.position.y<0)
        {
            AddReward(max_alt);

            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }

    public override void OnEpisodeBegin()
    {
        thrustPercent=0f;
        brakesTorque=0f;
        max_alt = 0f;
        Pitch = Yaw = Roll = Flap = 0;
        time_elapsed = 0f;

        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
        SetControlSurfacesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(0f);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        aircraft.transform.position = Vector3.zero + new Vector3(0,1f,0) + scene.transform.position;
        aircraft.transform.rotation = Quaternion.Euler(Vector3.zero);
        
    }

    public void SetControlSurfacesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

}
