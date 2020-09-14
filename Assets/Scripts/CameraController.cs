using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // target to orbit around
    public Transform target;
    public float speed;
    public float cameraXAngle;
    public float cameraHeight;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool enableRotate;
    private bool resetComplete;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        transform.LookAt(target);
        enableRotate = true;
        resetComplete = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (enableRotate)
        {
            transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        }
        else
        {
            //play animation to move camera to battlefield and stay there
            FocusOnMap();
            if (!resetComplete)
            {
                ResetPosition();
                TopDownPosition();
                resetComplete = true;
            }
            
        }
    }

    // Plays animation to move camera at center of battlefield
    // Reset to position then allow to move to battlefield position, which will be a Vector3 most likely
    private void FocusOnMap()
    {
        // Animation Controller over here
    }

    private void ResetPosition()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        //transform.position = Vector3.Lerp(transform.position, target.position, 1.0f * Time.deltaTime);
    }

    private void TopDownPosition()
    {
        Vector3 rotationVector = new Vector3(cameraXAngle, 0, 0);
  
        transform.position = new Vector3(-0.6f, cameraHeight, -155.0f);
        transform.rotation = Quaternion.Euler(rotationVector);
    }

    public void ToggleRotate(bool value)
    {
        enableRotate = value;
    }
}
