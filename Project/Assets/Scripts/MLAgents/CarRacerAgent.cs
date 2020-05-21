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
    private float _maxEpisodeTime = 300f;
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

        foreach (var item in FindObjectsOfType<RemoveCheckpointAfterPassing>())
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
        
        if (!IsOnRoad() || collisionCount > this.MaxCollisionCount)
        {
            SetReward(-1f);
            _vehicle?.ResetPos();
            collisionCount = 0;
        }

        if(vectorAction[0] < 0)
        {
            AddReward(vectorAction[0] / 1.0f * 0.001f);
        }

        if (Time.time - _startTime > _maxEpisodeTime)
        {
            Debug.Log("Max time exceeded");
            EndEpisode();
        }

        AddReward(-0.0001f);
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
        RaycastHit hit;
        Debug.DrawRay(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z), -transform.up);
        if(Physics.Raycast(new Vector3(transform.position.x, transform.position.y -0.15f, transform.position.z + 0.5f), -transform.up, 0.5f) && Physics.Raycast(new Vector3(transform.position.x, transform.position.y - 0.15f, transform.position.z - 1.0f), -transform.up, 0.5f))
        {
            return true;
        }
        return false;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.name);

        
        {
            AddReward(-0.5f);
            collisionCount++;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("checkpoint"))
        {
            AddReward(2.0f);
        }
        else if (other.gameObject.CompareTag("finish"))
        {
            AddReward(10f);
            EndEpisode();
        }
    }
}
