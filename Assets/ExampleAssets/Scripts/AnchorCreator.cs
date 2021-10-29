using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

//
// This script allows us to create anchors with
// a prefab attached in order to visbly discern where the anchors are created.
// Anchors are a particular point in space that you are asking your device to track.
//

[RequireComponent(typeof(ARAnchorManager))]
[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class AnchorCreator : MonoBehaviour
{
   
    // a list of objects's anchor
    List<ARAnchor> m_AnchorPoints; 
    // The ARAnchorManager handles the processing of all anchors and updates their position and rotation.
    ARRaycastManager m_RaycastManager;
    // The ARRaycastManager allows us to perform raycasts so that we know where to place an anchor.
    ARAnchorManager m_AnchorManager;
    // The ARPlaneManager detects surfaces we can place our objects on.
    ARPlaneManager m_PlaneManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    // list of objects
    public GameObject[] AnchorPrefabs;
    // current object (use to replace)
    private GameObject currentObj;
    private int index;
    // a list of objects
    private List<GameObject> m_GameObject; 
    // resizing original distance 
    private float fingerdistance;
    // delete all
    private bool is_delete;
    // double tap 1
    private int TapCount;
    private float MaxDubbleTapTime;
    private float NewTime;
    private bool is_doubletap;


    // Removes all the anchors that have been created.
    public void RemoveAllAnchors()
    {
        foreach (var anchor in m_AnchorPoints)
        {
            Destroy(anchor);
        }
        foreach (var gObject in m_GameObject)
        {
            Destroy(gObject);
        }
        index =0;
        
    }



    // On Awake(), we obtains a reference to all the required components.
    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();
        m_AnchorPoints = new List<ARAnchor>();
        is_delete = false;
        index = 0;
        fingerdistance = 0;
        m_GameObject = new List<GameObject>();
        is_doubletap = false;
        MaxDubbleTapTime = 0.3f;
        NewTime = Time.time;
    }


    void Update()
    {
        // no tap
        if (Input.touchCount == 0) {
             return;
         }

        var touch = Input.GetTouch(0);
       
        // three-fingers-tap: delete
        if (Input.touchCount > 2) {
            RemoveAllAnchors();
        }

        // two-finger-tap: resizing
        if (Input.touchCount == 2) {
            var touch2 = Input.GetTouch(1);
            float delta;
            if (fingerdistance == 0){
                fingerdistance = Vector2.Distance(touch.position, touch2.position);
                delta = 1;
            } else {
                var newdistance = Vector2.Distance(touch.position, touch2.position);
                delta =  newdistance / fingerdistance;
                fingerdistance = newdistance;
            }
            currentObj.transform.localScale *= delta;
        }

        // single-finger-tap
        if (Input.touchCount == 1) {

            // double tap detection
            doubleTapDetection(touch);

            if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // one tap : drag
                if (!is_doubletap){
                    if (touch.phase != TouchPhase.Moved)
                        return;
                    var hitPose = s_Hits[0].pose;
                    currentObj.transform.position = hitPose.position;
                }

                // double tap: drop
                if (is_doubletap){
                    is_doubletap = false;
                    if (touch.phase != TouchPhase.Began)
                        return;
                    var hitPose = s_Hits[0].pose;
                    var hitTrackableId = s_Hits[0].trackableId;
                    var hitPlane = m_PlaneManager.GetPlane(hitTrackableId);

                    // This attaches an anchor to the area on the plane corresponding to the raycast hit,
                    // and afterwards instantiates an instance of your chosen prefab at that point.
                    // This prefab instance is parented to the anchor to make sure the position of the prefab is consistent
                    // with the anchor, since an anchor attached to an ARPlane will be updated automatically by the ARAnchorManager as the ARPlane's exact position is refined.
                    var anchor = m_AnchorManager.AttachAnchor(hitPlane, hitPose);
                    currentObj = Instantiate(AnchorPrefabs[index], anchor.transform);
                    index += 1;
                    if (index == AnchorPrefabs.Length){
                        index =0;
                    }

                    if (anchor == null)
                    {
                        Debug.Log("Error creating anchor.");
                    }
                    else
                    {
                        // Stores the anchor so that it may be removed later.
                        m_AnchorPoints.Add(anchor);
                        m_GameObject.Add(currentObj);
                    }
                }
            }
        }

        if (Time.time > NewTime) {
            TapCount = 0;
            is_doubletap = false;
            fingerdistance = 0;
        }
        if (m_GameObject.Count > AnchorPrefabs.Length){
            var a = m_AnchorPoints[0];
            var b = m_GameObject[0];
            m_AnchorPoints.RemoveAt(0);
            m_GameObject.RemoveAt(0);
            Destroy(a);
            Destroy(b);
        }
    }

    void doubleTapDetection(Touch touch){
        TapCount += 1;
        if (TapCount == 1) {
            NewTime = Time.time + MaxDubbleTapTime;
        }else if(TapCount == 2 && Time.time <= NewTime){
            is_doubletap = true;
            TapCount = 0;
        }else{
            TapCount = 1;
            NewTime = Time.time + MaxDubbleTapTime;
        }
    }

}
