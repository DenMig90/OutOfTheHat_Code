using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AlignCameraToView : MonoBehaviour {

    public bool locked = false;

	// Use this for initialization
	void Start () {
		
	}

#if UNITY_EDITOR
    // Update is called once per frame
    void OnRenderObject () {
        if (locked && !Application.isPlaying && Application.isEditor && UnityEditor.SceneView.lastActiveSceneView != null)
        {
            transform.position = new Vector3(UnityEditor.SceneView.lastActiveSceneView.camera.transform.position.x, UnityEditor.SceneView.lastActiveSceneView.camera.transform.position.y, transform.position.z);
        }
    }
#endif

}
