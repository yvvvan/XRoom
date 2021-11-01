using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.IO;

public class ObjectDetecting : MonoBehaviour
{
    public ARCameraManager cameraManager;
    public Text abc;
    public Text efg;
    public RawImage image2;
    public RawImage image3;
    int iRound = 0;

    public static DirectoryInfo SafeCreateDirectory(string path)
    {
        //Generate if you don't check if the directory exists
        if (Directory.Exists(path))
        {
            return null;
        }
        return Directory.CreateDirectory(path);
    }

    void Update() {
        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;
        
        efg.text = String.Format("{0}", iRound);
        iRound += 1;

        // Consider each image plane.
        for (int planeIndex = 0; planeIndex < image.planeCount; ++planeIndex)
        {
            // Log information about the image plane.
            var plane = image.GetPlane(planeIndex);
            //Debug.LogFormat("Plane {0}:\n\tsize: {1}\n\trowStride: {2}\n\tpixelStride: {3}", planeIndex, plane.data.Length, plane.rowStride, plane.pixelStride);
            //abc.text = String.Format("{0}", BitConverter.ToString(pngBytes) );
            //abc.text = String.Format("Plane {0}:\n\tsize: {1}\n\trowStride: {2}\n\tpixelStride: {3} \n\n size: {4}", planeIndex, pngBytes[0], plane.rowStride, plane.pixelStride,byteArray[0] );


            // Texture2D target = new Texture2D(2, 2, TextureFormat.R16, false);
            // target.LoadImage(plane.data.GetSubArray(0,400).ToArray());
            // image2.texture = target;

            // Texture2D target2 = new Texture2D(2, 2);
            // target2.LoadImage(byteArray);
            // image3.texture = target2;

            //Data storage  
            // SafeCreateDirectory(Application.persistentDataPath + "/" + "test");
            // string json = JsonUtility.ToJson(plane.data);
            // StreamWriter Writer = new StreamWriter(Application.persistentDataPath + "/" + "test" + "/date.json");
            // Writer.Write(json);
            // Writer.Flush();
            // Writer.Close();
            // Do something with the data.
            // MyComputerVisionAlgorithm(plane.data);
            return;
        }

        // Dispose the XRCpuImage to avoid resource leaks.
        image.Dispose();
    }
}

