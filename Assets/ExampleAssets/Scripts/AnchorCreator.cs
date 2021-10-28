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
    private int index=0;

    bool dragging;

    // Removes all the anchors that have been created.
    public void RemoveAllAnchors()
    {
        foreach (var anchor in m_AnchorPoints)
        {
            Destroy(anchor);
        }
        m_AnchorPoints.Clear();
    }

    // On Awake(), we obtains a reference to all the required components.
    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();
        m_AnchorPoints = new List<ARAnchor>();
        dragging = false;
    }


    void Update()
    {
        // no tap
        if (Input.touchCount == 0) {
             return;
         }

        var touch = Input.GetTouch(0);
       
        // double-fingers-tap: drop
        if (Input.touchCount > 1) {
            if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                if (touch.phase != TouchPhase.Began)
                    return;
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
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
                }
            }
        }

        // single-finger-tap: drag
        if (Input.touchCount == 1) {
            if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                var hitPose = s_Hits[0].pose;
                currentObj.transform.position = hitPose.position;
            }
        }

    }

    


}
