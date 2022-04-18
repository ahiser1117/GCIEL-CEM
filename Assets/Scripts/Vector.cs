using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector : MonoBehaviour
{

    public Vector3 vector;

    Vector3 initScale;
    public CEMVolume vol;
    public Material eMat;
    public Material mMat;

    void Start(){
        initScale = transform.localScale;
    }

    
    public void CalcM(List<StraightCurrent> strCur) {
        vector = Vector3.zero;
        //GetComponentInChildren<MeshRenderer>().materials[0] = mMat;

        foreach(StraightCurrent cur in strCur){
            Vector3 rp = (cur.transform.position - transform.position);
            vector += (cur.current * (Vector3.Cross(cur.direction, rp))) / Mathf.Pow(rp.magnitude, 3);
        }

        // Apply the changes to the gameObject
        transform.rotation = Quaternion.FromToRotation(Vector3.up, vector);
        //= new Vector3(transform.localScale.x, vector.magnitude, transform.localScale.z);
        switch(vol.scaling){
            case CEMVolume.Scaling.Linear:
                if(vector.magnitude > vol.maxLength){
                    transform.localScale = new Vector3(initScale.x, initScale.y * vol.maxLength, initScale.z);
                } else{
                    transform.localScale = new Vector3(initScale.x, initScale.y * vector.magnitude, initScale.z);
                }
                break;
            case CEMVolume.Scaling.Exponential:
                if(Mathf.Sqrt(vector.magnitude) > vol.maxLength){
                    transform.localScale = new Vector3(initScale.x, initScale.y * vol.maxLength, initScale.z);
                } else{
                    transform.localScale = new Vector3(initScale.x, initScale.y * Mathf.Sqrt(vector.magnitude), initScale.z);
                }
                break;
            case CEMVolume.Scaling.Logarithmic:
                if(Mathf.Log(vector.magnitude + 1, vol.logBase) > vol.maxLength){
                    transform.localScale = new Vector3(initScale.x, initScale.y * vol.maxLength, initScale.z);
                } else{
                    transform.localScale = new Vector3(initScale.x, initScale.y * Mathf.Log(vector.magnitude + 1, vol.logBase), initScale.z);
                }
                break;
        }

    }
    public void CalcE(List<ChargedParticle> particles) {

        // Reset the vector
        vector = Vector3.zero;
        //GetComponentInChildren<MeshRenderer>().materials[0] = eMat;

        // Go through all particles and adjust the vector based on the particles charge and location
        foreach(ChargedParticle par in particles){
            float dist = (par.transform.position - transform.position).magnitude;
            vector += (par.charge / (dist * dist) * (par.transform.position - transform.position).normalized);
        }

        // Apply the changes to the gameObject
        transform.rotation = Quaternion.FromToRotation(Vector3.up, vector);
        //= new Vector3(transform.localScale.x, vector.magnitude, transform.localScale.z);
        switch(vol.scaling){
            case CEMVolume.Scaling.Linear:
                if(vector.magnitude > vol.maxLength){
                    transform.localScale = new Vector3(initScale.x, initScale.y * vol.maxLength, initScale.z);
                } else{
                    transform.localScale = new Vector3(initScale.x, initScale.y * vector.magnitude, initScale.z);
                }
                break;
            case CEMVolume.Scaling.Exponential:
                if(Mathf.Sqrt(vector.magnitude) > vol.maxLength){
                    transform.localScale = new Vector3(initScale.x, initScale.y * vol.maxLength, initScale.z);
                } else{
                    transform.localScale = new Vector3(initScale.x, initScale.y * Mathf.Sqrt(vector.magnitude), initScale.z);
                }
                break;
            case CEMVolume.Scaling.Logarithmic:
                if(Mathf.Log10(vector.magnitude) > vol.maxLength){
                    transform.localScale = new Vector3(initScale.x, initScale.y * vol.maxLength, initScale.z);
                } else{
                    transform.localScale = new Vector3(initScale.x, initScale.y * Mathf.Log10(vector.magnitude), initScale.z);
                }
                break;
        }
        
    }
}
