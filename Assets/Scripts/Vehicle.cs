using UnityEngine;
using System.Collections;
using System.Collections.Generic;   //for generic list

[RequireComponent(typeof(CharacterController))]

abstract public class Vehicle : MonoBehaviour {

    //public to change the values inside the Inspector
    public float maxSpeed = 6.0f;
    public float maxForce = 3.0f;
    public float mass = 1.0f;
    public float radius = 1.0f;
    public float gravity = 20.0f;
    private float wanderTheta = 0;

    protected CharacterController charController;

    //Necessary for movement, inspired by our 2D Processing programming
    protected Vector3 acceleration;         //change in velocity per second
    protected Vector3 velocity;             //change in position per second
    protected Vector3 desiredVelocity;      //helps steer the Vehicle in the right direction

    //Publicly access the velocity
    public Vector3 Velocity {
        get { return velocity; }
        set { velocity = value; }
    }

    abstract protected void CalcSteeringForces();

	// Use this for initialization
	virtual public void Start () {
        //gets an internal reference to the Character Controller script that is on this vehicle
        charController = gameObject.GetComponent<CharacterController>();

        acceleration = Vector3.zero;
        velocity = transform.forward;

	}

    protected float findHeight(Vector3 currentPos)
    {
        float theHeight = Terrain.activeTerrain.SampleHeight(currentPos);
        return theHeight;
    }

	// Update is called once per frame
	protected void Update () {
        CalcSteeringForces();

        //update the velocity
        velocity += acceleration * Time.deltaTime;  //smooth movement, independent of frame rate
        velocity.y = 0;                      //keeps us in the X/Z plane
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);  //Limit the velocity vector

        //orient our transform to face where we're going
        if(velocity != Vector3.zero){
            transform.forward = velocity.normalized;
        }

        //keeps ourselves grounded - use gravity to stay on the ground
        velocity.y -= gravity * Time.deltaTime;

        //Move the character controller
        charController.Move(velocity * Time.deltaTime);

        //reset acceleration
        acceleration = Vector3.zero;
	}

    protected void ApplyForce(Vector3 steeringForce)
    {
        acceleration += steeringForce / mass;
    }

    protected Vector3 Flee(Vector3 enemyPos)
    {
        //find desired velocity
        desiredVelocity = transform.position - enemyPos;
        //set magnitude of the desired velocity
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        //subtract velocity from desired
        desiredVelocity -= velocity;
        //find Y plane
        desiredVelocity.y = 0;
        //return
        return desiredVelocity;
    }

    protected Vector3 Seek(Vector3 targetPos) {
        //find desired velocity
        desiredVelocity = targetPos - transform.position;
        //set magnitude of the desired velocity
        desiredVelocity = desiredVelocity.normalized * maxSpeed;
        //subtract velocity from desired
        desiredVelocity -= velocity;
        //find Y plane
        desiredVelocity.y = 0;
        //return
        return desiredVelocity;
    }

    protected Vector3 Wander()
    {
        float wanderDist = 100f;
        float changeDir = 0.3f;
        float wanderRad = 50f;
        wanderTheta += Random.Range(-changeDir, changeDir);
        Vector3 circleLoc = velocity.normalized * wanderDist + transform.position;
        float dir = Random.Range(0, 360);
        Vector3 offset = new Vector3(wanderRad * Mathf.Cos(wanderTheta + dir), wanderRad * Mathf.Sin(wanderTheta + dir));
        Vector3 location = circleLoc + offset;
        return Seek(location);
    }

    //Avoid Obstacle
    //Steers vehicles away from obstacles
    protected Vector3 AvoidObstacle(GameObject obst, float safeDistance)
    {
        float obstRad = obst.GetComponent<Obstacle>().Radius;
        desiredVelocity = Vector3.zero;
        Vector3 steer = Vector3.zero;
        Vector3 vecToCenter = obst.transform.position - transform.position;

        float distance = vecToCenter.magnitude;
        if (distance > safeDistance)
        {
            return steer;
        }
        float d = Vector3.Dot(vecToCenter, transform.forward);
        if (d <= 0)
        {
            return steer;
        }

        float rightDot = Vector3.Dot(vecToCenter, transform.right);
        if (Mathf.Abs(rightDot) > radius - obstRad + 10)
        {
            return steer;
        }

        if (rightDot > 0)
        {
            desiredVelocity = transform.right * -maxSpeed;
        }
        else
        {
            desiredVelocity = transform.right * maxSpeed;
        }

        steer = desiredVelocity - velocity;
        steer = steer * (safeDistance / distance);
        return steer;
    }

    protected Vector3 Arrival(Vector3 targetPos)
    {
        float inTheZone = 20f;
        Vector3 desiredVelocity = targetPos - transform.position;
        float distance = desiredVelocity.magnitude;

        if (distance < inTheZone)
        {
            desiredVelocity = desiredVelocity.normalized * maxSpeed * (distance / inTheZone);
        }
        else
        {
            desiredVelocity = desiredVelocity.normalized * maxSpeed;
        }

        return desiredVelocity - velocity;
    }

    //Separation
    //The list is controlled by the GameManager
    //CalcSteeringForces will get the centroid from the Game Manager
    //For separation, I need to know about every other flocker in my flock
    public Vector3 Separation(List<GameObject> flockers, float separationDist) {
        //How do I do this?
        Vector3 total = Vector3.zero;
        //For every neighbor in the flock
        foreach(GameObject f in flockers){
            //get the distance between me and each of my neighbors
            desiredVelocity = transform.position - f.transform.position;
            float dist = desiredVelocity.magnitude;

            //if my neighbor is too close....
            //then calculate the weight of how far/what direction to move in
            //flockers that are closer result in a larger calculation that moves me away more drastically
            if(dist > 0 && dist < separationDist){
                desiredVelocity *= separationDist / dist;
                desiredVelocity.y = 0;
                total += desiredVelocity;
            }
        }
        //move me in the correct direction using the normal formula
        total = total.normalized * maxSpeed;
        total -= velocity;
        return total;
    }

    //Cohesion
    //GameManager will calculate the centroid
    //CalcSteeringForce will pass in the cohesion vector
    //Simply seek that point!
    public Vector3 Cohesion(Vector3 cohesionVector) {
        return Seek(cohesionVector);
    }

    //Alignment
    //GameManager will calculate the average flock direction
    //CalcSteeringForce will pass in the direction vector
    //That vector IS our desired velocity!
    public Vector3 Alignment(Vector3 alignVector) {
        desiredVelocity = alignVector.normalized * maxSpeed;
        desiredVelocity -= velocity;
        desiredVelocity.y = 0;
        return Vector3.zero;
    }

    //Containment
    //Tether-type containment (keeps vehicles in a certain radius)
    //If the flocker moves outside of that boundary, move toward the center
    public Vector3 Containment(float boundary, Vector3 areaCenter){
        float distance = Vector3.Distance(transform.position, areaCenter);
       if ( distance > boundary){
           return Seek(areaCenter);
		}
		else
		return Vector3.zero;
    }

}
