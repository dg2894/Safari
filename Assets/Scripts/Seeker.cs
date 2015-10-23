using UnityEngine;
using System.Collections;

public class Seeker : Vehicle {

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
    public GameObject target;

    //reference the GameManager script
    private GameManager gm;

    private GameObject[] obstacles;
    private GameObject[] lions;

	// Use this for initialization
    //Overridden from the base
	override public void Start () {
        base.Start();
        obstacles = GameObject.FindGameObjectsWithTag("Tree");
        lions = GameObject.FindGameObjectsWithTag("Lion");
        gm = GameObject.Find("MainGO").GetComponent<GameManager>();
	}

    public Vector3 LeaderFollowing(Vector3 leaderPos, Vector3 leaderForward)
    {
        Vector3 desiredVector = leaderPos - leaderForward * 3;
        float tooClose = 5f;
        if (Vector3.Distance(leaderPos, transform.position) < tooClose)
        {
            return Flee(leaderPos);
        }
        return Seek(desiredVector);
    }

    //sum up all the vectors from the individual steering forces
    //using that to guide our movement
    protected override void CalcSteeringForces() {
        Vector3 force = Vector3.zero;

        //seek target
        force += LeaderFollowing(target.transform.position, target.transform.forward) * seekWt;
        force += Arrival(LeaderFollowing(target.transform.position, target.transform.forward));

        foreach (GameObject l in lions)
        {
            if (Vector3.Distance(l.transform.position, transform.position) < 20)
            {
                force += Flee(l.transform.position) * fleeWt;
           } 
        }

        //loop through all of the obstacles in the list
        foreach(GameObject o in obstacles){
            force += AvoidObstacle(o, avoidDist) * avoidWt;
        }

        //call flocking forces here
        force += Separation(gm.Flock, separationDistance) * separationWt;
        force += Cohesion(gm.FlockDirection) * cohesionWt;
        force += Alignment(gm.Centroid) * alignWt;
        force += Containment(490f, new Vector3(0, 0, 0));


        //limit the force by the max force float
        force = Vector3.ClampMagnitude(force, maxForce);

        //apply force
        ApplyForce(force);
    }
	
}
