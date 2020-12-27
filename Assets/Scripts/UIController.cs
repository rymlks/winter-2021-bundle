using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public GameObject toolTipPanel;
    public GameObject pausePanel;
    public Texture2D toolTipTexture;
    public static string roadNameToolTipText = "";

    [Range(0.5f, 5.0f)]
    public float UIScale;
    private bool paused = false;

    void Start()
    {
        toolTipPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        if (paused)
        {
            return;
        }
        // Render road name tooltip
        if (roadNameToolTipText != "")
        {
            // Make it visible
            toolTipPanel.SetActive(true);

            // Move it to the correct location
            RectTransform rt = toolTipPanel.GetComponent<RectTransform>();
            var x = Input.mousePosition.x;
            var y = Input.mousePosition.y;
            rt.anchoredPosition = new Vector2(x, y);

            // Get the correct size based on the text
            Text guiText = toolTipPanel.GetComponentInChildren<Text>();
            TextGenerator textGen = new TextGenerator();
            TextGenerationSettings generationSettings = guiText.GetGenerationSettings(toolTipPanel.GetComponent<RectTransform>().rect.size);
            int width = (int)Mathf.Round(textGen.GetPreferredWidth(roadNameToolTipText, generationSettings) + 24);
            int height = toolTipTexture.height;
            guiText.text = roadNameToolTipText;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width * UIScale);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height * UIScale);
            guiText.rectTransform.localScale = new Vector3(UIScale, UIScale, 1.0f);

            // Update the background image to scale properly
            Texture bg = FrameTex(width, height);
            toolTipPanel.GetComponent<RawImage>().texture = bg;
        }
        else
        {
            toolTipPanel.SetActive(false);
        }
    }

    public void Pause()
    {
        paused = true;
        pausePanel.SetActive(true);
    }

    public void UnPause()
    {
        paused = false;
        pausePanel.SetActive(false);
    }

    public void TogglePause()
    {
        paused = !paused;
        pausePanel.SetActive(paused);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    /**
     * Get a Texture2D object that is a horizontally scaled version of the toolTipTexture. 
     * This preserves the size of the left and right borders
     */
    Texture2D FrameTex(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.alphaIsTransparency = true;
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                // Keep left and right border pixels identical, including the rivet
                if (i <= 10)
                {
                    texture.SetPixel(i, j, toolTipTexture.GetPixel(i, j));
                } else if (i >= width - 10)
                {
                    int fromRight = width - i;
                    texture.SetPixel(i, j, toolTipTexture.GetPixel(toolTipTexture.width - fromRight, j));
                } else // Copy the middle pixels using the 11th column in the source image
                {
                    texture.SetPixel(i, j, toolTipTexture.GetPixel(11, j));
                }
            }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }
}
