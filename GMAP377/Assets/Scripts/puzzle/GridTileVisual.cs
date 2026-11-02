using TMPro;
using UnityEngine;


public class GridTileVisual : MonoBehaviour
{
    private static readonly string[] Operations = { "+", "-", "*", "/" };

    [SerializeField] private Color exitColor = new Color(1f, 0.4f, 0.4f);
    [SerializeField] private Color operatorColor = new Color(1f, 0.85f, 0.3f);

    public void SetupTile(string value, bool isExit)
    {
        var text = GetComponent<TMP_Text>();

        if (isExit)
        {
            text.text = "EXIT";
            text.color = exitColor;
            text.enableAutoSizing = true;   
            text.fontSizeMax = text.fontSize;
            text.fontSizeMin = 1f;
            return;
        }

        text.text = value;
        if (System.Array.IndexOf(Operations, value) >= 0) text.color = operatorColor;
    }
}
