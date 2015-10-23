using UnityEngine;
using System.Collections;

public class Wanderer : Vehicle {

    //weights and distances!
    public float seekWt = 75.0f;
    public float avoidWt = 10.0f;
    public float avoidDist = 20.0f;
    public float wanderWt = 5f;
    public float containmentWt = 50f;
    public float fleeWt;

    public GameObject target;

    //reference the GameManager script
    private GameManager gm;

    private GameObject[] obstacles;
    private GameObject[] predators;

    // Use this for initialization
    //Overridden from the base
    override public void Start()
    {
        base.Start();

        obstacles = GameObject.FindGameObjectsWithTag("Tree");
        predators = GameObject.FindGameObjectsWithTag("Lion");
        gm = GameObject.Find("MainGO").GetComponent<GameManager>();

    }

    //sum up all the vectors from the individual steering forces
    //using that to guide our movement
    protected override void CalcSteeringForces()
    {
        //start with a zeroed-out vector
        Vector3 force = Vector3.zero;

        force += Wander() * wanderWt;
        force += Containment(490f, new Vector3(0, 0, 0)) * containmentWt;

        //loop through all of the obstacles in the list
        foreach (GameObject o in obstacles)
        {
            force += AvoidObstacle(o, avoidDist) * avoidWt;
        }

        //limit the force by the max force float
        force = Vector3.ClampMagnitude(force, maxForce);

        //apply force
        ApplyForce(force);
    }

}