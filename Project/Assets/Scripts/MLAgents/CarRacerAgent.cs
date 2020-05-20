using System;
using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using VehicleBehaviour;

public class CarRacerAgent : Agent
{
    private Rigidbody body;
    private WheelVehicle _vehicle;
    private float _startTime;
    private float _maxEpisodeTime = 500f;
    private float _steering = 0.0f;


    [SerializeField]
    private int maxCollisionCount;
    public int MaxCollisionCount
    {
        get => maxCollisionCount;
        set => maxCollisionCount = value;
    }

    private int collisionCount;
    
    public override void Initialize()
    {
        body = GetComponent<Rigidbody>();
        _vehicle = GetComponent<WheelVehicle>();
    }

    public override void OnEpisodeBegin()
    {
        this.collisionCount = 0;
        _vehicle?.ResetPos();
        _startTime = Time.time;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
        sensor.AddObservation(body.velocity);
        sensor.AddObservation(_steering);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (_vehicle != null)
        {
            _vehicle.Throttle = vectorAction[0];
            UpdateSteeringValue(vectorAction[1]);
        }
        
        if (!IsOnRoad() || collisionCount > this.MaxCollisionCount)
        {
            SetReward(-1f);
            _vehicle?.ResetPos();
            collisionCount = 0;
        }

        if (Time.time - _startTime > _maxEpisodeTime)
        {
            Debug.Log("Max time exceeded");
            EndEpisode();
        }

        AddReward(-0.01f);
    }

    private void UpdateSteeringValue(float steeringAction)
    {
        if (steeringAction > 0.5f && _steering < 1f)
        {
            _steering += 0.1f;
        }
        else if (Math.Abs(steeringAction) < 0.5f)
        {
            if (Math.Abs(_steering) < 0.05f)
            {
                _steering = 0f;
            }
            else
            {
                _steering += _steering < 0 ? 0.05f : -0.05f;
            }
        }
        else if (steeringAction < -0.5f && _steering > -1f)
        {
            _steering -= 0.1f;
        }

        _vehicle.Steering = _steering;
    }
    
    private bool IsOnRoad()
    {
        // TODO: Figure out to check if the vehicle is on the road
        return true;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("checkpoint"))
        {
            AddReward(0.8f);
        }
        else if (collision.gameObject.CompareTag("finish"))
        {
            AddReward(1f);
            EndEpisode();
        }
        else
        {
            AddReward(-0.8f);
            collisionCount++;
        }
    }
}
