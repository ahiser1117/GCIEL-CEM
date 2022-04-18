using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Solenoid : MonoBehaviour
{
    public int rings;
    public int subdivisions;
    public float separation;
    public float radius;
    public float current;
    [SerializeField, Range(0.1f, 5)] float scale;
    public CEMVolume vol;
    public CEMVolumeGPU volGPU;
    public GameObject strCur;
    List<StraightCurrent> currents;

    void Awake(){
        currents = new List<StraightCurrent>();
        for(int i = 0; i < rings; i++){
            for(int j = 0; j < subdivisions; j++){
                GameObject newCur = Instantiate(strCur, transform.position + scale * (Vector3.forward * (i - rings/2) * separation 
                                                        + radius * Vector3.right * Mathf.Cos((float)j/subdivisions * 2 * Mathf.PI) 
                                                        + radius * Vector3.up * Mathf.Sin((float)j/subdivisions * 2 * Mathf.PI)), 
                                                        transform.rotation * Quaternion.Euler(0, 0, 360 * (float)j/subdivisions), 
                                                        this.transform);
                currents.Add(newCur.GetComponent<StraightCurrent>());
                if(vol)
                    vol.straightCurrents.Add(newCur.GetComponent<StraightCurrent>());
                if(volGPU)
                    volGPU.currents.Add(newCur.GetComponent<StraightCurrent>());
            }
        }
        CurrentChanged();
    }

    void CurrentChanged(){
        foreach(StraightCurrent cur in currents){
            cur.current = current;
        }
    }


}
