using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCam : MonoBehaviour
{
    public AnimationCurve velocityCurve;
    public float velocityFactor = 1;
    public float planetRadius = 5;
    public GameObject planet;
    private Vector3 planetOrigin;
    private bool camMoving = false;
    // Start is called before the first frame update
    void Start()
    {
        planetOrigin = planet.GetComponent<Transform>().position;
        planetRadius = planet.GetComponent<SphereMaker>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeVelocity();
        Move();
    }

    private void Move()
    {
        //Debug.Log(gameObject.GetComponent<Rigidbody>().velocity);
        if (Input.GetKey(KeyCode.UpArrow))
        {
            gameObject.GetComponent<Rigidbody>().velocity = gameObject.transform.forward.normalized * velocityFactor;
            camMoving = true;
        }
        else if(Input.GetKey(KeyCode.DownArrow))
        {
            gameObject.GetComponent<Rigidbody>().velocity = -gameObject.transform.forward.normalized * velocityFactor;
            camMoving = true;
        }
        else
        {
            gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            camMoving = false;
        }
        if (camMoving)
        {
            planet.GetComponent<SphereMaker>().UpdateMeshOnMoveCam();
        }
    }

    private void ChangeVelocity()
    {
        float dist = (gameObject.transform.position - planetOrigin).magnitude - planetRadius;
        dist /= planetRadius;
        dist = Mathf.Clamp01(dist);
        velocityFactor = (velocityCurve.Evaluate(dist));
        if(dist != 0)
        {
            velocityFactor = 0.5f;
        }
    }
}
