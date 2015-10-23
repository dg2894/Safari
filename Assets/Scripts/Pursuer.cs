using UnityEngine;
using System.Collections;

public class Pursuer : Vehicle {

    //weights and distances!
    public float seekWt = 75.0f;
    public float avoidWt = 10.0f;
    public float avoidDist = 20.0f;
    public float separationDistance = 0f;
    public float separationWt = 0f;
    public float cohesionWt = 0f;
    public float alignWt = 0f;
    public float fleeWt = 0f;

    //reference to the object it's seeking

    //reference the GameManager script
    private GameManager gm;

    private GameObject[] obstacles;
    private GameObject[] water;
    private GameObject[] zebras;
    private GameObject[] wanderers;
   
    // Use this for initialization
    //Overridden from the base
    override public void Start()
    {
        base.Start();
        obstacles = GameObject.FindGameObjectsWithTag("Tree");
        water = GameObject.FindGameObjectsWithTag("Water");
        wanderers = GameObject.FindGameObjectsWithTag("Wanderer");
        zebras = GameObject.FindGameObjectsWithTag("Zebra");
        print(water.Length);
        gm = GameObject.Find("MainGO").GetComponent<GameManager>();
        
        maxSpeed = 2;
    }

    protected GameObject findZebra()
    {
        float distance;
        Vector3 currentDist;
        Vector3 closest = Vector3.zero;
        GameObject zebra = null;

        for (int i = 0; i < zebras.Length; i++)
        {
            currentDist = zebras[i].transform.position - transform.position;
            distance = currentDist.magnitude;
            if (zebra == null || closest.magnitude > distance)
            {
                closest = currentDist;
                zebra = zebras[i];
            }
        }
        return zebra;
    }

    //sum up all the vectors from the individual steering forces
    //using that to guide our movement
    protected override void CalcSteeringForces()
    {
        Vector3 force = Vector3.zero;

        //seek target
        force += Seek(findZebra().transform.position) * seekWt;

        //loop through all of the obstacles in the list
        foreach (GameObject o in obstacles)
        {
            force += AvoidObstacle(o, avoidDist) * avoidWt;
        }

        foreach (GameObject w in wanderers)
        {
            if (Vector3.Distance(w.transform.position, transform.position) < 10)
            {
                force += Flee(w.transform.position) * fleeWt;
            }
        }

        foreach (GameObject wat in water)
        {
            force += AvoidObstacle(wat, avoidDist) * avoidWt;
        }

        //call flocking forces here
        force += Separation(gm.Lions, separationDistance) * separationWt;
        force += Containment(490f, new Vector3(0, 0, 0));

        //limit the force by the max force float
        force = Vector3.ClampMagnitude(force, maxForce);

        //apply force
        ApplyForce(force);
    }

}
