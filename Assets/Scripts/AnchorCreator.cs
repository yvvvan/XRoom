
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System;
using System.Collections;
using System.Collections.Generic;


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
    // The ARAnchorManager handles the processing of all anchors and updates their position and rotation.
    ARRaycastManager m_RaycastManager;
    // The ARRaycastManager allows us to perform raycasts so that we know where to place an anchor.
    ARAnchorManager m_AnchorManager;
    // The ARPlaneManager detects surfaces we can place our objects on.
    ARPlaneManager m_PlaneManager;

    // a list of objects's anchor
    List<ARAnchor> AnchorPoints; 
    // all hit point alone the ray of tap
    static List<ARRaycastHit> s_Hits;

    // list of prefab
    public GameObject[] Prefabs;
    // index of next prefab
    private int dropIndex; 
    // a list of dropped objects
    private List<GameObject> GameObjects; 
    // current object 
    private GameObject currentObj;
    // index of current object
    private int itemIndex;
   
    // resizing original distance 
    private float fingerdistance;
    private float sizeScale;

    // double tap 
    private int tapCount;
    private float maxDoubleTapTime;
    private float newTime;
    private bool isDoubleTap;

    // rotation button
    public Button rotationX;
    public Button rotationY;
    public Button rotationZ;
    private float rotationSpeed;
    private bool isRotateX;
    private bool isRotateY;
    private bool isRotateZ;
    private bool isButton;

    // control button
    public Button previousObj; // get previous obj
    public Button deleteObj; // delete current obj
    public Button nextPrefab; // get next prefab
    public Text currentObjText; 
    public Text nextPrefabText;
    public bool useCursor = false;

    public Toggle isShow;
    private bool wasShow;
    public Canvas UI;
    public Slider s1;
    public Slider s2;
    public Slider s3;
    public Button resetSize;
    public Slider s4;
    public Slider s5;
    public Slider s6;
    private float movingSpeed;

    // Removes all the anchors that have been created.
    public void RemoveAllAnchors()  {
        foreach (var anchor in AnchorPoints){
            Destroy(anchor);
        }
        foreach (var gObject in GameObjects){
            Destroy(gObject);
        }
        dropIndex = 0;
    }

    // rotation
    void RotationX()  {
        isRotateX = !isRotateX;
        tapCount = 0;
        isButton = true;
    }

    void RotationY()  {
        isRotateY = !isRotateY;
        tapCount = 0;
        isButton = true;
    }

    void RotationZ() {
        isRotateZ = !isRotateZ;
        tapCount = 0;
        isButton = true;
    }

    public bool Rotation()  {
        if (!isRotateX && !isRotateY && !isRotateZ)
            return false;
        if (currentObj != null) {
            if(isRotateX){
                currentObj.transform.Rotate(rotationSpeed, 0.0f, 0.0f, Space.Self);
            } 
            if (isRotateY) {
                currentObj.transform.Rotate(0.0f, rotationSpeed, 0.0f, Space.Self);
            } 
            if (isRotateZ) {
                currentObj.transform.Rotate(0.0f, 0.0f, rotationSpeed, Space.Self);
            }
        }
        return true;
    }

    // control
    // set a already dropped previous object as current obj
    public void setPrevious(){
        itemIndex -= 1;
        if (itemIndex < -1){
            itemIndex = GameObjects.Count-1;
        }
        if (itemIndex >= 0 ){
            currentObj = GameObjects[itemIndex];
        }
        tapCount = 0;
        isButton = true;
    }

    // delete the current obj
    public void deleteCurrent(){
        if (GameObjects.Count>0) {
            var a = AnchorPoints[itemIndex];
            var b = GameObjects[itemIndex];
            AnchorPoints.RemoveAt(itemIndex);
            GameObjects.RemoveAt(itemIndex);
            Destroy(a);
            Destroy(b);
            if (GameObjects.Count>=itemIndex+1){
                // don't change index
            }
            else {
                // set to the last obj
                itemIndex = GameObjects.Count -1;
            }
            if (itemIndex >= 0 ){
                currentObj = GameObjects[itemIndex];
            }    
        }
        tapCount = 0;
        isButton = true;
    }

    // set the next to-drop obj
    public void setNext(){
        dropIndex += 1;
        if (dropIndex >= Prefabs.Length){
            dropIndex = 0;
        }
        tapCount = 0;
        isButton = true;
    }

    void changeSize(){
        if (currentObj!=null){
            var scaleChange = new Vector3(1+s1.value, 1+s2.value, 1+s3.value);
            currentObj.transform.localScale = scaleChange*sizeScale;
        }
        tapCount = 0;
        isButton = true;
    }

    void clearSize(){
        if (currentObj!=null){
            var scaleChange = new Vector3(1, 1, 1);
            currentObj.transform.localScale = scaleChange;
        }
        tapCount = 0;
        isButton = true;
    }

    void sliderMoving(){
        if (currentObj!=null){
            var movingTo = new Vector3(s4.value, s5.value, s6.value);
            currentObj.transform.position += movingTo * movingSpeed;
        }
    }

    void SetAllPlanesActive(bool value)  {
        foreach (var plane in m_PlaneManager.trackables)
            plane.gameObject.SetActive(value);
    }

    void Awake()
    {
        // set basic component
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_AnchorManager = GetComponent<ARAnchorManager>();
        m_PlaneManager = GetComponent<ARPlaneManager>();
        // hit ray
        s_Hits = new List<ARRaycastHit>();
        // used anchors
        AnchorPoints = new List<ARAnchor>();
        // used obj
        GameObjects = new List<GameObject>();

        // which obj to drop
        dropIndex = 0;
        // current obj in GameObjects list
        itemIndex = -1;

        // finger distance by resizing
        fingerdistance = 0;
        sizeScale = 1;
        
        // double tap
        isDoubleTap = false;
        maxDoubleTapTime = 0.3f;
        newTime = Time.time;

        // rotation
        rotationSpeed = 2.5f;
        rotationX.onClick.AddListener(RotationX);
        rotationY.onClick.AddListener(RotationY);
        rotationZ.onClick.AddListener(RotationZ);
        isRotateX = false;
        isRotateY = false;
        isRotateZ = false;

        // controls
        previousObj.onClick.AddListener(setPrevious);
        deleteObj.onClick.AddListener(deleteCurrent);
        nextPrefab.onClick.AddListener(setNext);

        isButton = false;

        wasShow = isShow.isOn;

        s1.onValueChanged.AddListener(delegate {changeSize(); });
        s2.onValueChanged.AddListener(delegate {changeSize(); });
        s3.onValueChanged.AddListener(delegate {changeSize(); });
        resetSize.onClick.AddListener(clearSize);

        movingSpeed = 0.002f;
    }

    

    void Update()
    {   
        
        if(wasShow!=isShow.isOn){
            wasShow = isShow.isOn;
            UI.enabled = wasShow;
            SetAllPlanesActive(wasShow);
            m_PlaneManager.enabled = wasShow;
        }

        // update Text Label
        if(currentObj==null){
            currentObjText.text = "No Object";
        } else {
            currentObjText.text = currentObj.name;
        }
        nextPrefabText.text = Prefabs[dropIndex].name;

        // 
        sliderMoving();

        // update Double Tap Verification
        // reset finger distance (improve resizing)
        if (Time.time > newTime) {
            tapCount = 0;
            isDoubleTap = false;
            fingerdistance = 0;
        }
        
        // if rotation, only rotation (cannot do any other maneuvers)
        if (Rotation() || isButton){
            isButton = false;
            return;
        }

        // no tap
        if (Input.touchCount == 0) {
             return;
        }

        // get touch
        var touch = Input.GetTouch(0);

        // tap in the untouchable area
        if (isShow.isOn && touch.position.x < 560 ){
             //Debug.Log("in");
             return;
        }
        //Debug.Log(touch.position);
            
        // three-fingers-tap: reset
        if (Input.touchCount > 2) {
            RemoveAllAnchors();
            Awake();
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
            sizeScale = currentObj.transform.localScale.x;
        }

        // single-finger-tap
        if (Input.touchCount == 1) {

            // double tap detection
            doubleTapDetection(touch);

            bool hasTapAnchor = m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.All);

            
            // one tap : drag
            if (hasTapAnchor && !isDoubleTap){
                if (touch.phase != TouchPhase.Moved)
                    return;
                var hitPose = s_Hits[0].pose;
                currentObj.transform.position = hitPose.position;
            }

            // double tap: drop    
            if (isDoubleTap){     
                ARAnchor anchor;
                isDoubleTap = false;
                if (touch.phase != TouchPhase.Began)
                    return;       
                if (useCursor){
                    // replace with screen center
                    var screenPosition = Camera.current.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
                    debugText.text = String.Format("{0}",screenPosition);                    
                    hasTapAnchor = true;                    
                    m_RaycastManager.Raycast(screenPosition, s_Hits, TrackableType.All);
                }
                else if (hasTapAnchor && !useCursor){
                    // do noting
                } else {
                    // quit
                    return;
                }
                if (hasTapAnchor) {
                    var hitPose = s_Hits[0].pose;
                    var hitTrackableId = s_Hits[0].trackableId;
                    var hitPlane = m_PlaneManager.GetPlane(hitTrackableId);
                    anchor = m_AnchorManager.AttachAnchor(hitPlane, hitPose);
                    currentObj = Instantiate(Prefabs[dropIndex], anchor.transform);
                    dropIndex += 1;
                    if (dropIndex == Prefabs.Length) {
                        dropIndex =0;
                    }
                    if (currentObj != null) {
                    // if (anchor == null) {
                    //     Debug.Log("Error creating anchor.");
                    // }
                    // else {
                        // Stores the anchor so that it may be removed later.
                        AnchorPoints.Add(anchor);
                        GameObjects.Add(currentObj);
                        itemIndex = AnchorPoints.Count -1 ;
                    }
                }
            }
        }
    }

    void doubleTapDetection(Touch touch){
        tapCount += 1;
        if (tapCount == 1) {
            newTime = Time.time + maxDoubleTapTime;
        } else if(tapCount == 2 && Time.time <= newTime){
            isDoubleTap = true;
            tapCount = 0;
        } else{
            tapCount = 1;
            newTime = Time.time + maxDoubleTapTime;
        }
    }

    public Text debugText;
}
