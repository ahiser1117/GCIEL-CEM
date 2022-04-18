using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightCurrent : MonoBehaviour
{

    public float current;
    public Vector3 direction;

    void Start(){
        direction = transform.rotation * direction;
        
    }

    void Update(){
        // Apply the changes to the gameObject
        transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    }
}
