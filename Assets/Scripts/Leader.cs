using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LeaderStates
{
    arriving,
    stopping,
    seeking
}

public class Leader : Vehicle {

    int pointSeeking = 0;
    float searchTime = 2f;

    //weights and distances!
    public float seekWt = 75.0f;
    public float avoidWt = 10.0f;
    public float avoidDist = 20.0f;
    public float fleeWt;
    public GameObject target;

    //reference the GameManager script
    private GameManager gm;

    private GameObject[] obstacles;
    private List<Vector3> wp = new List<Vector3>();

    public LeaderStates leaderStates;

    // Use this for initialization
    //Overridden from the base
    override public void Start()
    {
        base.Start();
        leaderStates = LeaderStates.seeking;
        obstacles = GameObject.FindGameObjectsWithTag("Tree");
        gm = GameObject.Find("MainGO").GetComponent<GameManager>();

        for (int i = 0; i < 6; i++)
        {
            wp.Add(GameObject.Find("Plant" + i).transform.position);
        }
    }

    //sum up all the vectors from the individual steering forces
    //using that to guide our movement
    protected override void CalcSteeringForces()
    {
        //start with a zeroed-out vector
        Vector3 force = Vector3.zero;

        switch (leaderStates)
        {
            case LeaderStates.arriving:
                force += Arrival(wp[pointSeeking]);
                if (charController.velocity.magnitude < 0.3f)
                {
                    velocity *= 0.1f;
                    leaderStates = LeaderStates.stopping;
                }
                break;

            case LeaderStates.stopping:
                searchTime -= Time.deltaTime;
                if (searchTime <= 0)
                {
                    pointSeeking++;
                    if (pointSeeking >= wp.Count)
                    {
                        pointSeeking = 0;
                    }
                    leaderStates = LeaderStates.seeking;
                    searchTime = 2f;
                }
                break;

            case LeaderStates.seeking:
                force += Seek(wp[pointSeeking]) * seekWt;
                float inTheZone = 20f;
                if (Vector3.Distance(wp[pointSeeking], transform.position) < inTheZone)
                {
                    leaderStates = LeaderStates.arriving;
                }
                break;
            default:
                break;
        }

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
