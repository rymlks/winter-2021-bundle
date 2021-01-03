using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public GameObject toolTipPanel;
    public GameObject pausePanel;
    public GameObject UIScalePanel;
    public Canvas mainCanvas;

    public GameObject contextMenu;

    public Texture2D toolTipTexture;

    public static string roadNameToolTipText = "";
    [Range(0.5f, 5.0f)]
    public static float UIScale;

    private bool paused = false;

    void Start()
    {
        toolTipPanel.SetActive(false);
        pausePanel.SetActive(false);
        UIScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        if (roadNameToolTipText != "")
        {
            ShowRoadSign(roadNameToolTipText);
        }
        else
        {
            HideRoadSign();
        }
    }

    public void Pause()
    {
        paused = true;
        pausePanel.SetActive(true);
        roadNameToolTipText = "";
    }

    public void UnPause()
    {
        paused = false;
        pausePanel.SetActive(false);
        roadNameToolTipText = "";
    }

    public void TogglePause()
    {
        paused = !paused;
        pausePanel.SetActive(paused);
        roadNameToolTipText = "";
    }

    public void UpdateUIScale(System.Single scale)
    {
        if (scale == 0)
            return;

        UIScale = scale;
        RectTransform rt = UIScalePanel.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100 * UIScale);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 24 * UIScale);
        Text guiText = UIScalePanel.GetComponentInChildren<Text>();
        guiText.rectTransform.localScale = new Vector3(UIScale, UIScale, 1.0f);

        contextMenu.GetComponent<ContextMenuControler>().SetScale();
    }

    public void HideRoadSign()
    {
        toolTipPanel.SetActive(false);
    }

    private void ShowRoadSign(string text)
    {
        // Make it visible
        toolTipPanel.SetActive(true);

        // Move it to the correct location
        RectTransform rt = toolTipPanel.GetComponent<RectTransform>();
        float x = (Input.mousePosition.x / mainCanvas.scaleFactor) + 3;
        float y = (Input.mousePosition.y / mainCanvas.scaleFactor) - 3;
        rt.anchoredPosition = new Vector2(x, y);

        // Get the correct size based on the text
        Text guiText = toolTipPanel.GetComponentInChildren<Text>();
        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings generationSettings = guiText.GetGenerationSettings(rt.rect.size);
        generationSettings.fontSize = 12;
        int width = (int)Mathf.Round(textGen.GetPreferredWidth(roadNameToolTipText, generationSettings) / mainCanvas.scaleFactor + 27 );
        //int height = (int)Mathf.Round(textGen.GetPreferredHeight(roadNameToolTipText, generationSettings) + 6 * UIScale);
        int height = 24;
        guiText.fontSize = (int)(12 * UIScale);
        guiText.text = text;
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * UIScale);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * UIScale);
        //guiText.rectTransform.localScale = new Vector3(UIScale, UIScale, 1.0f);

        // Update the background image to scale properly
        Texture bg = FrameTex(width, height);
        toolTipPanel.GetComponent<RawImage>().texture = bg;
    }

    /**
     * Get a Texture2D object that is a horizontally scaled version of the toolTipTexture. 
     * This preserves the size of the left and right borders
     */
    Texture2D FrameTex(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        //texture.alphaIsTransparency = true;
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                int pixX;
                // Keep left and right border pixels identical, including the rivet
                if (i <= 10)
                {
                    pixX = i;
                } else if (i >= width - 10)
                {
                    pixX = 100 - (width - i);
                } else // Copy the middle pixels using the 11th column in the source image
                {
                    pixX = 11;
                }

                texture.SetPixel(i, j, toolTipTexture.GetPixel(pixX, j));
            }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
}
