using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEMVolumeGPU : MonoBehaviour
{
    
    public enum FieldType{Electric, Magnetic};

    public enum Scaling{Linear, Exponential, Logarithmic};

    public struct Field{
        public Field(Vector3 pos, Vector3 forw, Vector3 righ){
            position = pos;
            forward = forw;
            right = righ;
        }
        public Vector3 position;
        public Vector3 forward;
        public Vector3 right;
    };

    // Allow control over field in inspector
    public FieldType field;
    public Scaling scaling;

    const int MaxResolution = 100;

    [SerializeField, Range(10, MaxResolution)] int resolution = 20;
    [SerializeField, Range(1, 10)] int heldFields = 1;
    public float step;
    [SerializeField, Range(0.5f, 20)] float maxLength;
    [SerializeField, Range(10, 100)] int maxFields = 50;

    public List<ChargedParticle> particles;
    public List<StraightCurrent> currents;
    

    [SerializeField] ComputeShader computeShader;
    [SerializeField] Material material;
    [SerializeField] Mesh mesh;

    ComputeBuffer vectorBuffer;
    ComputeBuffer currentBuffer;
    ComputeBuffer currentPosBuffer;
    ComputeBuffer chargeBuffer;
    ComputeBuffer magsBuffer;
    ComputeBuffer fieldPosBuffer;
    ComputeBuffer forwardsBuffer;
    ComputeBuffer rightsBuffer;
    
    List<Vector4> particlesValues;
    List<Vector3> currentsPos;
    List<Vector4> currentsValues;
    List<Field> fields;
    List<Vector3> fieldPos;
    List<Vector3> forwards;
    List<Vector3> rights;

    // CPU references to GPU fields
    static readonly int vectorsId = Shader.PropertyToID("_Vectors"),
                        magsId = Shader.PropertyToID("_Mags"),
                        resolutionId = Shader.PropertyToID("_Resolution"),
                        stepId = Shader.PropertyToID("_Step"),
                        maxLengthId = Shader.PropertyToID("_MaxLength"),
                        scalingId = Shader.PropertyToID("_Scaling"),
                        particlesId = Shader.PropertyToID("_Charges"),
                        currentPosId = Shader.PropertyToID("_CurrentPos"),
                        currentsId = Shader.PropertyToID("_Currents"),
                        chargeCountId = Shader.PropertyToID("_ChargeCount"),
                        currentCountId = Shader.PropertyToID("_CurrentCount"),
                        fieldPosId = Shader.PropertyToID("_FieldPos"),
                        forwardsId = Shader.PropertyToID("_Forwards"),
                        rightsId = Shader.PropertyToID("_Rights");


    // Called on awake and hot reloads
    void OnEnable(){
        
        vectorBuffer = new ComputeBuffer(MaxResolution * MaxResolution * maxFields, 3 * 4 * sizeof(float));
        magsBuffer = new ComputeBuffer(MaxResolution * MaxResolution * maxFields, sizeof(float));

        particlesValues = new List<Vector4>();
        currentsPos = new List<Vector3>();
        currentsValues = new List<Vector4>();
        
        fields = new List<Field>();
        fieldPos = new List<Vector3>();
        forwards = new List<Vector3>();
        rights = new List<Vector3>();

        for(int i = 0; i < heldFields; i++){
            fields.Add(new Field(transform.position, transform.forward, transform.right));
        }
        
    }

    void OnDisable(){
        vectorBuffer.Release(); // Free space on GPU
        magsBuffer.Release();
        vectorBuffer = null; // Signal to Unity to garbage collect
        currentBuffer = null;
        currentPosBuffer = null;
        chargeBuffer = null;
        magsBuffer = null;
        fieldPosBuffer = null;
        forwardsBuffer = null;
        rightsBuffer = null;
    }

    void Update(){

        if(fields.Count > 0){

            if(!hide){
                for(int i = 0; i < heldFields; i++){
                    fields[i] = new Field(transform.position + transform.up * step * i, transform.forward, transform.right);
                }
            }

            currentBuffer = new ComputeBuffer(currents.Count, 4 * sizeof(float));
            currentPosBuffer = new ComputeBuffer(currents.Count, 3 * sizeof(float));
            chargeBuffer = new ComputeBuffer(particles.Count, 4 * sizeof(float));
            fieldPosBuffer = new ComputeBuffer(fields.Count, 3 * sizeof(float));
            forwardsBuffer = new ComputeBuffer(fields.Count, 3 * sizeof(float));
            rightsBuffer = new ComputeBuffer(fields.Count, 3 * sizeof(float));

            FindElements(); // Load the values needed for charged particles and currents

            // Send values to the GPU references
            computeShader.SetInt(resolutionId, resolution);
            computeShader.SetFloat(stepId, step);
            computeShader.SetFloat(maxLengthId, maxLength);
            computeShader.SetInt(scalingId, (int) scaling);
            computeShader.SetInt(currentCountId, currents.Count);
            computeShader.SetInt(chargeCountId, particles.Count);


            currentBuffer.SetData(currentsValues);
            currentPosBuffer.SetData(currentsPos);
            chargeBuffer.SetData(particlesValues);
            fieldPosBuffer.SetData(fieldPos);
            forwardsBuffer.SetData(forwards);
            rightsBuffer.SetData(rights);
            var kernelIndex = (int) field; // Choose which kernel to use
            computeShader.SetBuffer(kernelIndex, vectorsId, vectorBuffer);
            computeShader.SetBuffer(kernelIndex, currentsId, currentBuffer);
            computeShader.SetBuffer(kernelIndex, currentPosId, currentPosBuffer);
            computeShader.SetBuffer(kernelIndex, particlesId, chargeBuffer);
            computeShader.SetBuffer(kernelIndex, magsId, magsBuffer);
            computeShader.SetBuffer(kernelIndex, fieldPosId, fieldPosBuffer);
            computeShader.SetBuffer(kernelIndex, forwardsId, forwardsBuffer);
            computeShader.SetBuffer(kernelIndex, rightsId, rightsBuffer);

            
            int groups = Mathf.CeilToInt(resolution / 8f); // Calc number of thread groups
            computeShader.Dispatch(kernelIndex, groups, groups, fields.Count); // Complete Calculation on GPU
            
            // Render the vectors
            material.SetBuffer(vectorsId, vectorBuffer);
            material.SetFloat(stepId, step);
            var bounds = new Bounds(transform.position, new Vector3(resolution * step, 1, resolution * step));
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, resolution * resolution * fields.Count);

            currentBuffer.Release();
            currentPosBuffer.Release();
            chargeBuffer.Release();
            fieldPosBuffer.Release();
            forwardsBuffer.Release();
            rightsBuffer.Release();
        }
        
    }

    void FindElements(){
        particlesValues.Clear();
        currentsPos.Clear();
        currentsValues.Clear();
        fieldPos.Clear();
        forwards.Clear();
        rights.Clear();
        foreach(ChargedParticle part in particles){
            particlesValues.Add(new Vector4(part.transform.position.x, 
                                          part.transform.position.y,
                                          part.transform.position.z,
                                          part.charge));
        }

        foreach(StraightCurrent curr in currents){
            currentsPos.Add(new Vector3(curr.transform.position.x,
                                        curr.transform.position.y,
                                        curr.transform.position.z));
            currentsValues.Add(new Vector4(curr.direction.x,
                                           curr.direction.y,
                                           curr.direction.z,
                                           curr.current));
        }
        foreach(Field field in fields){
            fieldPos.Add(field.position);
            forwards.Add(field.forward);
            rights.Add(field.right);
        }
    }

    public void ToggleField(){
        if(field == FieldType.Electric){
            field = FieldType.Magnetic;
        } else{
            field = FieldType.Electric;
        }
    }

    int tDown = 0;

    public void PasteField(){
        if(tDown == 0 && !hide && fields.Count <= maxFields){
            for(int i = 0; i < heldFields; i++){
                fields.Add(new Field(transform.position + transform.up * step * i, transform.forward, transform.right));
                Debug.Log("New Field at: " + transform.position);
                Debug.Log("Fields: " + fields.Count);
            }
        }
        tDown = (tDown + 1) % 3;   
    }

    int yDown = 0;

    public void RemoveLastField(){
        if(yDown == 0 && (fields.Count > heldFields || hide)){
            fields.RemoveAt(fields.Count - 1);
        }
        yDown = (yDown + 1) % 3;   
    }

    int bDown = 0;
    bool hide = false;

    public void ToggleHideField(){
        if(bDown == 0){
            if(hide){
                for(int i = 0; i < heldFields; i++){
                    fields.Insert(i, new Field(transform.position + transform.up * step * i, transform.forward, transform.right));
                }
            } else{
                for(int i = 0; i < heldFields; i++){
                    fields.RemoveAt(0);
                }
            }
            hide = !hide;
        }
        bDown = (bDown + 1) % 3;   
    }

    int addDown = 0;

    public void AddHeldField(){
        if(addDown == 0){
            if(heldFields < 10){
                fields.Insert(heldFields, new Field(transform.position + transform.up * step * heldFields, transform.forward, transform.right));
                heldFields++;  
            }
        }
        addDown = (addDown + 1) % 3;  
    }

    int subDown = 0;

    public void SubHeldField(){
        if(subDown == 0){
            if(heldFields > 1){
                heldFields--;
                fields.RemoveAt(heldFields);
            }
        }
        
        subDown = (subDown + 1) % 3;  
        
    }






}
