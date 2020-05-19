using System;
using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using VehicleBehaviour;
using Random = UnityEngine.Random;

public class CarRacerAgent : Agent
{
    private Rigidbody body;
    private WheelVehicle _vehicle;


    [SerializeField]
    private int maxCollisionCount;
    public int MaxCollisionCount { get => maxCollisionCount; set => maxCollisionCount = value; }
    
    private int _collisionCount;
    
    public override void InitializeAgent()
    {
        body = GetComponent<Rigidbody>();
        _vehicle = GetComponent<WheelVehicle>();
    }

    public override void OnEpisodeBegin()
    {
        this._collisionCount = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(body.velocity);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (_vehicle != null)
        {
            _vehicle.Throttle = vectorAction[0];
            _vehicle.Steering = vectorAction[1];
        }
        
        if (!IsOnRoad() || _collisionCount > this.MaxCollisionCount)
        {
            SetReward(-1f);
            _vehicle.ResetPos();
            _collisionCount = 0;
        }
    }

    private bool IsOnRoad()
    {
        const double minTrackHeight = -10;
        return body.position.y > minTrackHeight;
    }
    
    private void OnCollisionEnter(Collision other)
    {
        AddReward(-1f);
        _collisionCount++;
    }
}
