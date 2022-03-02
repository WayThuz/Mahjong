using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMove : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform[] cameraPositions = new Transform[4];
    [SerializeField] private Transform tableCenter;
    [SerializeField] private Vector3 playerPositionOffset = new Vector3(0, 345, 0);
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
        if(mainCamera == null) mainCamera = Camera.main;
        mainCamera.transform.position = cameraPositions[index].position;
        mainCamera.transform.rotation = cameraPositions[index].rotation;
        mainCamera.transform.LookAt(tableCenter);
    }

    public void setPlayerPositionToCamera(Transform playerTr){
        playerTr.position = mainCamera.transform.position - playerPositionOffset;
        playerTr.rotation = mainCamera.transform.rotation;
    }
}
