using UnityEngine;
using System.Collections;

[System.Serializable]
public class ParallaxData
{
    public GameObject layer;
    public Vector3 startPos;
    //[HideInInspector]
    //public Renderer[] renderers;
    [Tooltip("0 = Not moving; 1 = Moving same speed as camera")]
    [Range(-1,1)]
    public float speedX;
    [Tooltip("0 = Not moving; 1 = Moving same speed as camera")]
    [Range(-1, 1)]
    public float speedY;
}

[ExecuteInEditMode]
public class BackgroundParallax : MonoBehaviour {

    public ParallaxData[] parallaxes;

    //[SerializeField]
    private Vector2 startPos;
    private Vector2 prevPosition;

    private void Awake()
    {
        //foreach (ParallaxData parallax in parallaxes)
        //{
        //    parallax.renderers = parallax.layer.GetComponentsInChildren<Renderer>();
        //}
        startPos = new Vector2(transform.position.x, transform.position.y);
        prevPosition = startPos;
    }

    // Use this for initialization
    void Start () {
        //foreach (ParallaxData parallax in parallaxes)
        //{
        //    if (parallax.layer != null)
        //    {
        //        parallax.startPos = parallax.layer.transform.position;
        //    }
        //}
        //transform.position = new Vector3(GameController.instance.mainCamera.transform.position.x, GameController.instance.mainCamera.transform.position.y, transform.position.z);


    }
	
	// Update is called once per frame
	void Update () {
        Vector2 actualPosition = new Vector2(transform.position.x, transform.position.y);

        /* movement based on start position */
        //Vector2 moventDelta = actualPosition - startPos;

        /* movement frame by frame */
        Vector2 movementDelta = actualPosition - prevPosition;

        //Debug.Log(movementDelta);
        //transform.position = new Vector3(GameController.instance.mainCamera.transform.position.x, GameController.instance.mainCamera.transform.position.y, transform.position.z);
        foreach (ParallaxData parallax in parallaxes)
        {
            if (parallax.layer != null)
            {
                //foreach (Renderer renderer in parallax.renderers)
                //{
                //if (renderer.isVisible)
                //Debug.Log(new Vector2((transform.position.x - startX) * parallax.speed * Time.deltaTime,0));

                /* movement based on start position */
                //parallax.layer.transform.position = parallax.startPos + new Vector3(moventDelta.x * parallax.speedX, moventDelta.y * parallax.speedY, 0);

                /* movement frame by frame */
                parallax.layer.transform.position += new Vector3(movementDelta.x * parallax.speedX, movementDelta.y * parallax.speedY, 0);

            }
            //}
        }

        /* movement frame by frame */
        prevPosition = actualPosition;
    }
}
