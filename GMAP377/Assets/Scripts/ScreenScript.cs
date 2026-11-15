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
    private GraphicRaycaster _screenRay;
    private Transform _player;
    [SerializeField] private Material onMat, offMat;
    [SerializeField] private float onDistance;
    [SerializeField] private RenderTexture screenTexture;
    [SerializeField] private RectTransform screenRect;//UI Canvas  
    
    private void Start()
    {
        CreateScreenMat();
        screenRect = screenCanvas.gameObject.GetComponent<RectTransform>();
        float x = screenRect.rect.width;
        float y = screenRect.rect.height;
        _eventSystem = FindAnyObjectByType<EventSystem>();
        _screenRay = screenCanvas.GetComponent<GraphicRaycaster>();
        _player = FindAnyObjectByType<BasicMovement>().transform;
        if (screenObj == null) screenObj = transform.GetChild(0).GetComponent<MeshRenderer>();
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(_player.position, screenObj.transform.position) < onDistance)
        {
            if (screenObj.material != onMat)
            {
                screenObj.material = onMat;
                foreach (Transform child in transform)
                    child.gameObject.SetActive(true);
            }
        }
        else
        {
            if (screenObj.material != offMat)
            {
                screenObj.material = offMat;
                foreach (Transform screenChild in transform)
                    if (screenChild!=screenObj.transform)
                        screenChild.gameObject.SetActive(false);
            }
        }
    }
    
    private void CreateScreenMat()
    {
        screenTexture = new RenderTexture(1920, 1080, 16);
        screenTexture.Create();
        outputCamera.targetTexture = screenTexture;
        var screenMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        screenMat.SetTexture("_BaseMap", screenTexture);
        screenMat.EnableKeyword("_EMISSION");
        screenMat.SetTexture("_EmissionMap", screenTexture);
        screenMat.SetColor("_EmissionColor", Color.white);
        screenMat.SetFloat("_Smoothness", 0.9f);

        onMat = screenMat;
    }
    
    public void ScreenMouseOver(Vector2 screenCoordinate)
    {
        screenCoordinate = new Vector2(Mathf.Clamp01(screenCoordinate.x), Mathf.Clamp01(screenCoordinate.y));
        Vector2 size = screenRect.rect.size;
        Vector2 localPosition = new Vector2((screenCoordinate.x - screenRect.pivot.x) * size.x,
            (screenCoordinate.y - screenRect.pivot.y) * size.y);
        mouseFollow.anchoredPosition = localPosition;
        Vector2 pointerPosition = new Vector2(screenCoordinate.x * screenTexture.width,
            screenCoordinate.y * screenTexture.height);
        var pointerData = new PointerEventData(_eventSystem) { position = pointerPosition };
        var results = new List<RaycastResult>();
        _screenRay.Raycast(pointerData, results);
        mouseOver = results.Count > 0 ? results[0].gameObject : null;
    }
}
