using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform[] cameraPositions = new Transform[4];
    int initialCameraIndex = 0;
    void Awake(){
        mainCamera = Camera.main;
        setCameraTransform(initialCameraIndex);
    }

    void setCamera(){
        if(mainCamera == null) mainCamera = Camera.main;
        initialCameraIndex++;
        initialCameraIndex = initialCameraIndex%4;
        setCameraTransform(initialCameraIndex);
    }

    public void setCameraTransform(int index){
        Debug.Log("Set camera transform : ID = " + index.ToString());
        mainCamera.transform.position = cameraPositions[index].position;
        mainCamera.transform.rotation = cameraPositions[index].rotation;
    }
}
