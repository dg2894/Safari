using UnityEngine;
using System.Collections;

/// <summary>
/// An Obstacle is a GameObject that we should avoid
/// All Obstacles should return their radius
/// and be tagged "Obstacle"
/// </summary>


public class Obstacle : MonoBehaviour {

    //public for now, so we can change the value in the Inspector if need be
    public float radius = 1.414f;


    //Property necessary if I change the variable to private
    public float Radius {
        get { return radius; }
    }
}
