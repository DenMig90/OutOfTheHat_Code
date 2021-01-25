using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeShaderNoiseBehaviour : MonoBehaviour {
    
    private Vector3 startPosition;
    Material mat;
    private Renderer r;

    public static Dictionary<Material,MaterialPropertyBlock> blocks = new Dictionary<Material, MaterialPropertyBlock>();

    private MaterialPropertyBlock block;

    // Use this for initialization
    void Start () {
        startPosition = transform.position;
        r = GetComponent<Renderer>();
        mat = null;
        foreach (Material m in GetComponent<Renderer>().sharedMaterials)
        {
            if(m.shader.name=="Shader/BaseMat")
            {
                mat = m;
                if(blocks.ContainsKey(mat))
                {
                    block = blocks[mat];
                }
                else
                {
                    block = new MaterialPropertyBlock();
                    blocks.Add(mat, block);
                }
                break;
            }
        }

        if(mat==null)
        {
            Debug.LogWarning("Problema");
            return;
        }

        //GetComponent<Renderer>().enabled = false;

    }
	
	// Update is called once per frame
	void Update () {
        if (mat == null)
            return;

        if (!r.isVisible)
            return;
        

        block.SetVector("_PositionDifference", startPosition - transform.position);
        r.SetPropertyBlock(block);

        /*Matrix4x4 m4x4 = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Graphics.DrawMesh(GetComponent<MeshFilter>().mesh, m4x4, mat, gameObject.layer);*/

    }
}
