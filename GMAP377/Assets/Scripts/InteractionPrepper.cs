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
            {
                SelectObject(hit.collider.gameObject);
            }

            if (highlightedObject.CompareTag("Screen"))
            {
                highlightedObject.transform.parent.GetComponent<ScreenScript>().ScreenMouseOver(hit.textureCoord);
            }

            if (Input.GetMouseButtonDown(0))
            {
                switch (highlightedObject.tag)
                {
                    case "Screen":
                        ScreenScript screen = highlightedObject.transform.parent.GetComponent<ScreenScript>();
                        highlightedObject.SendMessage("Down",screen.mouseOver);
                        break;
                    default:
                        Debug.Log($"Interacted With {highlightedObject}");
                        break;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ScreenScript screen = highlightedObject.transform.parent.GetComponent<ScreenScript>();
                highlightedObject.SendMessage("Up",screen.mouseOver);
            }
        }
        else
        {
            DeselectObject();
        }
    }

    void SelectObject(GameObject newHighlight)
    {
        if (highlightedObject != null)
        {
            DeselectObject();
        }

        highlightedObject = newHighlight;
        nativeMat = highlightedObject.GetComponent<MeshRenderer>().material;
        if (selectMat != null && !highlightedObject.CompareTag("Screen"))
        {
            highlightedObject.GetComponent<MeshRenderer>().material = selectMat;
        }
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
