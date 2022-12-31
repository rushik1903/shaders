using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class distanceTesting : MonoBehaviour
{
    public GameObject camProxy;
    public GameObject vertex;
    private Vector3 vertexPosition;
    // Start is called before the first frame update
    void Start()
    {
        vertexPosition = vertex.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(findDistance());
    }

    private float findDistance()
    {
        float dist = Vector3.Dot(vertexPosition - camProxy.transform.position, gameObject.transform.position - camProxy.transform.position);
        dist /= Vector3.Distance(gameObject.transform.position, camProxy.transform.position);
        int wt = (int)Mathf.Ceil(dist * 1);
        return wt;
    }
}
