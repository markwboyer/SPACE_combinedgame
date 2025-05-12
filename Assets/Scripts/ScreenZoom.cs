using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenZoom : MonoBehaviour
{
    public InputAction zoomPFD = new InputAction(type: InputActionType.Button);
    public InputAction zoomRot = new InputAction(type: InputActionType.Button);

    private bool isPFDZoom = false;
    private bool isRotated = false;

    private Vector3 originalPosition;
    private Vector3 originalRotation;

    private void OnEnable() 
    {
        zoomPFD.Enable();
        zoomRot.Enable();
    }

    private void OnDisable()
    {
        zoomPFD.Disable();
        zoomRot.Disable();
    }

    void Start()
    {
        originalPosition = this.transform.localPosition;
        originalRotation = this.transform.localEulerAngles;
        Debug.Log("Original position: " + originalPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (zoomPFD.triggered)
        {
            if (!isPFDZoom && !isRotated)
            {
                //StartCoroutine(LerpFromTo(this.transform.localPosition, new Vector3(this.transform.localPosition.x  + 0.3f, this.transform.localPosition.y -0.2f, this.transform.localPosition.z + 0.0625f), 0.25f));
                StartCoroutine(LerpFromTo(this.transform.localPosition, new Vector3(this.transform.localPosition.x + 0.5f, this.transform.localPosition.y - 0.22f, this.transform.localPosition.z + 0.35f), 0.25f));
                isPFDZoom = !isPFDZoom;
            }
            else if (!isPFDZoom && isRotated)
            {
                // We do not want the subject to be able to rotate the camera and zoom into the PFD at the same time
            }
            else
            {
                StartCoroutine(LerpFromTo(this.transform.localPosition, originalPosition, 0.25f));
                isPFDZoom = !isPFDZoom;
            }
        }
        if (zoomRot.triggered)
        {
            if(!isRotated && !isPFDZoom)
            {
                // -65 y
                StartCoroutine(LerpFromToRot(this.transform.localEulerAngles, new Vector3(this.transform.localEulerAngles.x, this.transform.localPosition.y -65f, this.transform.localEulerAngles.z), 0.25f));
                isRotated = !isRotated;
            }
            else if (!isRotated && isPFDZoom)
            {
                // Same reason as above
            }
            else
            {
                Vector3 modOriginal = new Vector3(this.transform.localEulerAngles.x, this.transform.localEulerAngles.y + 65f, this.transform.localEulerAngles.z);
                StartCoroutine(LerpFromToRot(this.transform.localEulerAngles, modOriginal, 0.25f));
                isRotated = !isRotated;
            }
        }
    }

    // Honestly the most helpful co-routine ever invented
    // I adapted it to use localPosition instead of global, but otherwise it is unchanged.
    // Credits: https://forum.unity.com/threads/moving-the-camera-smoothly.464545/
    // User: StarManta
    IEnumerator LerpFromTo(Vector3 pos1, Vector3 pos2, float duration) 
    {
        for (float t=0f; t<duration; t += Time.deltaTime) 
        {
            transform.localPosition = Vector3.Lerp(pos1, pos2, t / duration);
            yield return 0;
        }
        transform.localPosition = pos2;
        //Debug.Log("Finished lerping!");
    }

    // Modified version to support rotation lerping
    IEnumerator LerpFromToRot(Vector3 pos1, Vector3 pos2, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            transform.localEulerAngles = Vector3.Lerp(pos1, pos2, t / duration);
            yield return 0;
        }
        transform.localEulerAngles = pos2;
        //Debug.Log("Finished lerping!");
    }
}
