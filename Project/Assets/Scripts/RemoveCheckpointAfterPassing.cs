using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCheckpointAfterPassing : MonoBehaviour
{
    private void OnTriggerExit(Collider collision)
    {
        if (collision.transform.parent.name == "Body")
        {
            Debug.Log("Points assigned");
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
