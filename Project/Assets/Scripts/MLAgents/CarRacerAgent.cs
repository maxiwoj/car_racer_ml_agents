using System;
using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using VehicleBehaviour;

public class CarRacerAgent : Agent
{
    public Generator Generator;

    private Rigidbody body;
    private WheelVehicle _vehicle;
    private float _startTime;
    private float _maxEpisodeTime = 300f;
    private float _steering = 0.0f;

    private int _tillRouteGeneration = 5;
    private readonly int _stepsToGeneration = 5;

    private Vector3 positionLastUpdate;


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
        collisionCount = 0;
        _vehicle?.ResetPos();
        _startTime = Time.time;
        positionLastUpdate = transform.position;

        if (_tillRouteGeneration == 0)
        {
            _tillRouteGeneration = _stepsToGeneration;
            Generator.TrackLength = (int)Academy.Instance.FloatProperties.GetPropertyWithDefault("track_length", 10);
            Generator.TurnRate = Academy.Instance.FloatProperties.GetPropertyWithDefault("turn_rate", 0.07f);
            Generator.GenerateTrack();
        }

        foreach (var item in Generator.SavedCheckpoints)
        {
            item.GetComponent<BoxCollider>().enabled = true;
        } 
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
        
        if (!IsOnRoad() || collisionCount > MaxCollisionCount)
        {
            AddReward(-2.75f);
            EndEpisode();
            collisionCount = 0;
        }

        if(vectorAction[0] < 0 && body.velocity.magnitude < 10)
        {
            AddReward(vectorAction[0] * 0.008f );
        }

        if (Time.time - _startTime > _maxEpisodeTime)
        {
            Debug.Log("Max time exceeded");
            EndEpisode();
        }

        float distanceThisFrame = Vector3.Distance(positionLastUpdate, transform.position);

        float movementScore = 0.0f;

        movementScore += Mathf.Pow(Mathf.Clamp(distanceThisFrame - 0.03f, -0.05f, 0.015f), 3f) * 0.06f;

        positionLastUpdate = transform.position;

        movementScore += (-0.00001f);

        AddReward(Mathf.Clamp(movementScore, -0.01f, 0.005f));
    }

    private void UpdateSteeringValue(float steeringAction)
    {
        if (steeringAction > _steering + 0.1f)
        {
            _steering += 0.1f;
        }
        else if (steeringAction < _steering - 0.1f)
        {
            _steering -= 0.1f;
        }
        else
        {
            _steering = steeringAction;
        }
        Mathf.Clamp(_steering, -1f, 1f);
        _vehicle.Steering = _steering;

        // if (steeringAction > 0.5f && _steering < 1f)
        // {
        //     _steering += 0.1f;
        // }
        // else if (Math.Abs(steeringAction) < 0.5f)
        // {
        //     if (Math.Abs(_steering) < 0.05f)
        //     {
        //         _steering = 0f;
        //     }
        //     else
        //     {
        //         _steering += _steering < 0 ? 0.05f : -0.05f;
        //     }
        // }
        // else if (steeringAction < -0.5f && _steering > -1f)
        // {
        //     _steering -= 0.1f;
        // }
        // _vehicle.Steering = _steering;
        // _vehicle.Steering = steeringAction;
    }

    private bool IsOnRoad()
    {
        //RaycastHit hit;
        //Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z), -transform.up);
        if(    Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z + 0.5f), -transform.up, 0.5f) 
            && Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z - 1.0f), -transform.up, 0.5f)
            && Physics.Raycast(new Vector3(transform.position.x + 0.5f, transform.position.y - 0.15f, transform.position.z), -transform.up, 0.5f)
            && Physics.Raycast(new Vector3(transform.position.x - 0.5f, transform.position.y - 0.15f, transform.position.z), -transform.up, 0.5f))
        {
            return true;
        }
        return false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {        
        AddReward(-0.5f);
        collisionCount++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("checkpoint"))
        {
            AddReward(1.25f);
        }
        else if (other.gameObject.CompareTag("finish"))
        {
            _tillRouteGeneration--;
            AddReward(8f);
            EndEpisode();
        }
    }
}
