using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [Range(1, 250)]
    public int TrackLength;

    [Range(0,1.0f)]
    public float TurnRate = 0.1f;

    [Range(0, 1.0f)]
    public float RampRate = 0.025f;

    [Range(0, 1.0f)]
    public float SpecialRate = 0.05f;

    [Range(0, 50)]
    public int CheckpointEveryNSegments = 4;

    public Transform StartPoint;
    public GameObject[] StraightTemplateSegments;
    public GameObject[] RightCurveTemplateSegments;
    public GameObject[] LeftCurveTemplateSegments;
    public GameObject[] RampTemplateSegments;
    public GameObject[] SpecialTemplateSegments;
    public GameObject CheckpointTemplate;
    public GameObject FinaleTemplate;

    public List<GameObject> SavedCheckpoints = new List<GameObject>();

    private int turnDeviation = 0;

    void Start()
    {
        GenerateTrack();
    }


    private GameObject[] UseRightTurn()
    {
        turnDeviation++; 
        return RightCurveTemplateSegments;
    }


    private GameObject[] UseLeftTurn()
    {
        turnDeviation--; 
        return LeftCurveTemplateSegments;
    }

    [ContextMenu("Generate")]
    public void GenerateTrack()
    {
        int skipFirst = 1;
        foreach (Transform child in transform)
        {
            if (skipFirst-- != 1)
            {
                Destroy(child.gameObject);
            }
        }

        Transform CurrentNextPoint = StartPoint;

        for (int i = 0, tillCheckpointCounter = 0; i < TrackLength; i++, tillCheckpointCounter++)
        {
            if(tillCheckpointCounter == CheckpointEveryNSegments && i != TrackLength-1)
            {
                GameObject checkpoint = Instantiate(CheckpointTemplate, this.transform);
                checkpoint.transform.position = CurrentNextPoint.position;
                checkpoint.transform.rotation = CurrentNextPoint.rotation;
                //checkpoint.transform.position = checkpoint.transform.position;// + new Vector3(0, 5, 0);
                tillCheckpointCounter = 0;
            }

            float RandomRoll = Random.Range(0.0f, 1.0f + TurnRate + RampRate + SpecialRate);

            GameObject[] templateSetToUse;

            if (RandomRoll <= TurnRate)
            {
                if(turnDeviation == 0)
                {
                    if(Random.Range(0.0f, 1.0f) < 0.5f)
                    {
                        templateSetToUse = UseRightTurn();
                    }
                    else
                    {
                        templateSetToUse = UseLeftTurn();
                    }
                }
                else if(turnDeviation < 0)
                {
                    templateSetToUse = UseRightTurn();
                }
                else
                {
                    templateSetToUse = UseLeftTurn();
                }
            }
            else if(RandomRoll <= TurnRate+RampRate)
            {
                templateSetToUse = RampTemplateSegments;
            }
            else if(RandomRoll <= TurnRate+RampRate+SpecialRate)
            {
                templateSetToUse = SpecialTemplateSegments;
            }
            else
            {
                templateSetToUse = StraightTemplateSegments;
            }

            var selectedSegment = templateSetToUse[Random.Range(0, templateSetToUse.Length)];

            GameObject createdSegment = Instantiate(selectedSegment, this.transform);
            var inputTransform = createdSegment.transform.Find("InputPoint");
            var outputTransform = createdSegment.transform.Find("OutputPoint");

            float rotationAngle = Vector3.SignedAngle(createdSegment.transform.forward, CurrentNextPoint.transform.forward, Vector3.up);

            Vector3 rotationOffset = inputTransform.InverseTransformDirection(CurrentNextPoint.transform.forward);
            createdSegment.transform.rotation = Quaternion.LookRotation(rotationOffset, Vector3.up);

           

            Vector3 inputOffset = inputTransform.InverseTransformPoint(createdSegment.transform.position);
            inputOffset = Quaternion.AngleAxis(rotationAngle, Vector3.up) * inputOffset;

            createdSegment.transform.position = CurrentNextPoint.position + inputOffset;


            CurrentNextPoint = outputTransform;

            if(i == TrackLength - 1)
            {
                GameObject checkpoint = Instantiate(FinaleTemplate, this.transform);
                SavedCheckpoints.Add(checkpoint);
                checkpoint.transform.position = CurrentNextPoint.position;
                checkpoint.transform.rotation = CurrentNextPoint.rotation;
                //checkpoint.transform.position = checkpoint.transform.position;// + new Vector3(0, 5, 0);
            }
        }
    }

}
