using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScreenScript : MonoBehaviour
{
    public RectTransform mouseFollow;
    public Camera outputCamera;
    public GameObject mouseOver;
    public Canvas screenCanvas;
    public MeshRenderer screenObj;
    private EventSystem _eventSystem;
    private Vector2 _screenDimensions;
    private GraphicRaycaster _screenRay;
    private Transform _player;
    [SerializeField] private Material onMat;
    [SerializeField] private Material offMat;
    [SerializeField] private float onDistance;

    private void Start()
    {
        RectTransform screenRect = screenCanvas.gameObject.GetComponent<RectTransform>();
        float x = screenRect.rect.width;
        float y = screenRect.rect.height;
        _screenDimensions = new Vector2(x, y);
        _eventSystem = FindAnyObjectByType<EventSystem>();
        _screenRay = screenCanvas.GetComponent<GraphicRaycaster>();
        _player = FindAnyObjectByType<BasicMovement>().transform;
        if (screenObj == null)
        {
            screenObj = transform.GetChild(0).GetComponent<MeshRenderer>();
        }

        if (onMat == null)
        {
            onMat = screenObj.material;
        }
    }
    


    private void FixedUpdate()
    {
        if (Vector3.Distance(_player.position, screenObj.transform.position) < onDistance)
        {
            if (screenObj.material != onMat)
            {
                screenObj.material = onMat;
            }
        }
        else
        {
            if (screenObj.material != offMat)
            {
                screenObj.material = offMat;
            }
        }
    }
    

    public void ScreenMouseOver(Vector2 screenCoordinate) 
    {
        //Updates Mouse Follow object. Coordinate components must come in as float between 0 and 1
        float x = Mathf.Lerp(-_screenDimensions.x/2, _screenDimensions.x/2, screenCoordinate.x);
        float y = Mathf.Lerp(-_screenDimensions.y/2, _screenDimensions.y/2, screenCoordinate.y);
        mouseFollow.anchoredPosition = new Vector2(x,y);
        //Raycasts, gets item.
        Vector2 mouseFollowPosition = new(screenCoordinate.x * Screen.width, screenCoordinate.y * Screen.height);
        PointerEventData pointerData = new PointerEventData(_eventSystem){position = mouseFollowPosition};
        List<RaycastResult> coveredUI = new List<RaycastResult>();
        _screenRay.Raycast(pointerData, coveredUI);
        if (coveredUI.Count > 0)
        {
            mouseOver = coveredUI[0].gameObject;
        }
    }
}
