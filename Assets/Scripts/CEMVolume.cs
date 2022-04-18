using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEMVolume : MonoBehaviour
{

    public enum FieldType{
        Electric,
        Magnetic
    };

    public enum Scaling{
        Linear,
        Exponential,
        Logarithmic
    };

    public float density = 2;
    public int size = 10;
    public float maxLength;
    public FieldType field;
    public Scaling scaling;
    public float logBase;
    public GameObject VecPrefab;
    public List<Vector> vectors;
    public List<ChargedParticle> particles;
    public List<StraightCurrent> straightCurrents;

    void Start(){
        vectors = new List<Vector>();
        Initialize();
    }


    // Update is called once per frame
    void Update()
    {
        // Go through all of the vectors in the space and calculate their directions and magnitudes
        foreach(Vector vec in vectors){
            if(field == FieldType.Electric){
                vec.CalcE(particles);
            }
            else if(field == FieldType.Magnetic){
                vec.CalcM(straightCurrents);
            }
            
        }
    }

    void Initialize(){
        for(int i = 0; i < size; i++){
            for(int j = 0; j < size; j++){
                Vector newVec = Instantiate(VecPrefab, new Vector3(i * density, 0, j * density), Quaternion.identity, transform).GetComponent<Vector>();
                vectors.Add(newVec);
                newVec.vol = this;
            }
        }
    }
}
