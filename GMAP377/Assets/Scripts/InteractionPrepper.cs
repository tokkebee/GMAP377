using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class InteractionPrepper : MonoBehaviour
{
    private BasicMovement movement;
    public GameObject highlightedObject;
    [SerializeField] private float selectDist;
    [SerializeField] private Material selectMat;
    [SerializeField] private Camera cam;
    [SerializeField] private Vector3 lookDir;
    [SerializeField] private List<string> selectableTags;
    private Material nativeMat;
    void Start()
    {
        Application.targetFrameRate = 60;
        movement = GetComponent<BasicMovement>();
        cam = movement.playerCam;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        RaycastHit hit;
        lookDir = cam.gameObject.transform.forward;
        Debug.DrawRay(cam.transform.position, lookDir*selectDist);
        
        if (Physics.Raycast(cam.transform.position, lookDir, out hit, selectDist) && 
            selectableTags.Contains(hit.collider.tag))
        {
            if (hit.collider.gameObject != highlightedObject)
                SelectObject(hit.collider.gameObject);

            if (highlightedObject.CompareTag("Screen"))
                highlightedObject.transform.parent.GetComponent<ScreenScript>().ScreenMouseOver(hit.textureCoord);

            if (Mouse.current.leftButton.wasPressedThisFrame) //Left click down
            {
                if (highlightedObject.TryGetComponent(out InteractBase interact))
                    interact.Click();
                Debug.Log($"Interacted With {highlightedObject}");
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame) //Left Click Up
            {
                if (highlightedObject.TryGetComponent(out InteractBase interact))
                    interact.Release();
            }
        }
        else DeselectObject();
    }

    void SelectObject(GameObject newHighlight)
    {
        if (highlightedObject != null) DeselectObject();
        highlightedObject = newHighlight;
        nativeMat = highlightedObject.GetComponent<MeshRenderer>().material;
        if (selectMat != null && !highlightedObject.CompareTag("Screen"))
            highlightedObject.GetComponent<MeshRenderer>().material = selectMat;
    }

    void DeselectObject()
    {
        if (highlightedObject != null)
        {
            highlightedObject.GetComponent<MeshRenderer>().material = nativeMat;
            highlightedObject = null;
        }
    }
}
