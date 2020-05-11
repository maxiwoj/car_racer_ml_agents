using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using VehicleBehaviour;

public class CarRacerAgent : Agent
{
    private Rigidbody body;
    private WheelVehicle _vehicle;


    [SerializeField]
    private int maxCollisionCount;
    public int MaxCollisionCount
    {
        get => maxCollisionCount;
        set => maxCollisionCount = value;
    }

    private int collisionCount;
    
    public override void InitializeAgent()
    {
        body = GetComponent<Rigidbody>();
        _vehicle = GetComponent<WheelVehicle>();
    }

    public override void OnEpisodeBegin()
    {
        this.collisionCount = 0;
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
            _vehicle.ResetPos();
            collisionCount = 0;
        }
    }

    private bool IsOnRoad()
    {
        // TODO: Figure out to check if the vehicle is on the road 
        return true;
    }
    
    private void OnCollisionEnter(Collision other)
    {
        AddReward(-1f);
        collisionCount++;
    }
}
