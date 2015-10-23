using UnityEngine;
using System.Collections;
using System.Collections.Generic;   //for list

public class GameManager : MonoBehaviour {

    //Declare some gameObjects
    GameObject myGuy;       //seeker
    GameObject targ;        
    GameObject leader;
    GameObject wanderer;
    GameObject camera2;

    //Link some prefabs
    public GameObject GuyPrefab;
    public GameObject ObstaclePrefab;
    public GameObject LeaderPrefab;
    public GameObject WandererPrefab;
    public GameObject LionPrefab;

    //Array to hold my Obstacle objects
    private GameObject[] obstacles;
    public int numObs = 25;

    //FLOCKING STUFF
    //average flock direction
    private Vector3 flockDirection;
    public Vector3 FlockDirection {
        get { return flockDirection; }
    }

    //center of the flock
    private Vector3 centroid;
    public Vector3 Centroid {
        get { return centroid; }
    }

    //list of flockers
    private List<GameObject> flock;
    public List<GameObject> Flock {
        get { return flock; }
    }

    //list of pursuers
    private List<GameObject> lions;
    public List<GameObject> Lions
    {
        get { return lions; }
    }

    //list of wanderers
    private List<GameObject> wanderers;
    public List<GameObject> Wanderers
    {
        get { return wanderers; }
    }

    //number of flockers to create
    public int numFlockers;
    public int numLions;
    public int numWanderers;

	// Use this for initialization
	void Start () {

        //create the flocker/lion list
        flock = new List<GameObject>();
        lions = new List<GameObject>();
        wanderers = new List<GameObject>();

        camera2 = GameObject.FindGameObjectWithTag("Camera2");

        //make a leader
        Vector3 position = new Vector3(Random.Range(-40, 40), 3f, Random.Range(-40, 40));
        position.y = findHeight(position) + 3f;
        targ = (GameObject)GameObject.Instantiate(LeaderPrefab, position, Quaternion.identity);

        //make wanderer
        for (int i = 0; i < numWanderers; i++)
        {
            position = new Vector3(Random.Range(-40, 40), 2.8f, Random.Range(-40, 40));
            position.y = findHeight(position) + 5f;
            wanderer = (GameObject)GameObject.Instantiate(WandererPrefab, position, Quaternion.identity);
            wanderer.name = "Eli" + i;
            wanderers.Add(wanderer);
        }

        //make flockers
        for (int i = 0; i < numFlockers; i++ ) {
            position = new Vector3(Random.Range(-5,5), 2f, Random.Range(-5,5));
            position.y = findHeight(position) + 1f;
            GameObject guy = (GameObject)GameObject.Instantiate(GuyPrefab, position, Quaternion.identity);
            //this will change the google eye guy's name in the heirarchy
            guy.name = "Guy" + i;
            guy.GetComponent<Seeker>().target = targ;
            //add to the list
            flock.Add(guy);
        }

        //make lions
        for (int i = 0; i < numLions; i++)
        {
            position = new Vector3(Random.Range(-1, 1), 3f, Random.Range(-1, 1));
            position.y = findHeight(position) + 1f;
            GameObject lion = (GameObject)GameObject.Instantiate(LionPrefab, position, Quaternion.identity);
            //this will change the google eye guy's name in the heirarchy
            lion.name = "Lion" + i;
            //add to the list
            lions.Add(lion);
        }

        //Unity returns an array of GameObjects that have the Obstacle tag
        obstacles = GameObject.FindGameObjectsWithTag("Tree");

        //Tell the main camera to follow MyGuy
        Camera.main.GetComponent<SmoothFollow>().target = targ.transform;
	}

    private float findHeight(Vector3 currentPos)
    {
        float theHeight = Terrain.activeTerrain.SampleHeight(currentPos);
        return theHeight;
    }
	
	//Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Camera.main.GetComponent<SmoothFollow>().target = targ.transform;
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            Camera.main.GetComponent<SmoothFollow>().target = gameObject.transform;
        }
        else if (Input.GetKey(KeyCode.C))
        {
            Camera.main.GetComponent<SmoothFollow>().target = camera2.transform;
        }

        CalcCentroid();
        CalcFlockDirection();

        targ.transform.position = new Vector3(targ.transform.position.x, findHeight(targ.transform.position) + 2.1f, targ.transform.position.z);

        foreach (GameObject f in flock)
        {
            f.transform.position = new Vector3(f.transform.position.x, findHeight(f.transform.position) + 1.5f, f.transform.position.z);
        }

        foreach (GameObject l in lions)
        {
           l.transform.position = new Vector3(l.transform.position.x, findHeight(l.transform.position) + 1f, l.transform.position.z); 
        }

        foreach (GameObject w in wanderers)
        {
            w.transform.position = new Vector3(w.transform.position.x, findHeight(w.transform.position) + 2.5f, w.transform.position.z);
        }
    }

    //CalcCentroid
    //Calculates the center of the flock
    //Updates once per frame here in the Game Manager
    private void CalcCentroid() { 
        //add up all the positions of all flockers - divide by the number of flockers
        centroid = Vector3.zero;

        foreach (GameObject f in flock){
            centroid += f.transform.position;
        }

        centroid /= Flock.Count;
        //moves the MainGO with the flock
        gameObject.transform.position = centroid;
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, findHeight(gameObject.transform.position) + 2.8f, gameObject.transform.position.z);
    }

    //CalcFlockDirection
    //Calculates the direction of the flock
    //Updates once per frame here in the Game Manager
    private void CalcFlockDirection()
    {
        //add up all the directions (forward) of all flockers - divide by the number of flockers
        flockDirection = Vector3.zero;
        foreach (GameObject f in flock)
        {
            flockDirection += f.transform.forward;
        }
        flockDirection /= Flock.Count;
        //aligns the MainGO with the flock
        gameObject.transform.forward = flockDirection;
    }
}
