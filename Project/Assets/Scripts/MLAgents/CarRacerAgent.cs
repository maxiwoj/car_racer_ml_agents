using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using VehicleBehaviour;

public class CarRacerAgent : Agent
{
    private Rigidbody body;
    private WheelVehicle _vehicle;
    private float _startTime;
    private float _maxEpisodeTime = 900f;


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
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (_vehicle != null)
        {
            _vehicle.Throttle = vectorAction[0];
            _vehicle.Steering = vectorAction[1];
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

        AddReward(-0.001f);
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
            AddReward(1f);
        }
        else if (collision.gameObject.CompareTag("finish"))
        {
            AddReward(1f);
            EndEpisode();
        }
        else
        {
            Debug.Log("Collision with " + collision.gameObject.tag);
            AddReward(-0.5f);
            collisionCount++;
        }
    }
}
