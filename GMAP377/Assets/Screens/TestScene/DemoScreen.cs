using System;
using UnityEngine;

//https://www.edraflame.com/blog/tutorial-unity-custom-script-templates/
//New scripts can be created using this template by right-clicking in project,
//and click on Create/GMAP377/New UI Script.

public class DemoScreen : MonoBehaviour
{//Add this script to the screen game object that is being targeted in physics raycasting

    private ScreenScript _screen; //Main Screen Script
    private RectTransform _mouseFollow; //Where the mouse is in canvas space

    private void Start()
    {
        _screen = transform.parent.GetComponent<ScreenScript>();
        _mouseFollow = _screen.mouseFollow;
    }

    public void Down(GameObject over = null)
    {//Left Click Down
        if (over != null)
        {
            over.transform.parent = _mouseFollow;
        }
    }

    public void Up(GameObject over = null)
    {
        if (over != null)
        {
            over.transform.parent = _screen.screenCanvas.transform;
            _mouseFollow.SetAsLastSibling(); 
        }
    }
}
