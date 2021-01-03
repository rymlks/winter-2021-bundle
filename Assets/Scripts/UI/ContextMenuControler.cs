using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuControler : MonoBehaviour
{
    public Canvas mainCanvas;
    public Text messageBox;
    public GameObject buttonsPanel;

    private Vector3 worldPosition;
    private string message = "";
    private Vector2 gridCellSize = new Vector2(130, 20);

    // Update is called once per frame
    void Update()
    {
        if (worldPosition != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        }
    }

    public void Open(string text, Vector3 location, Pothole target)
    {
        gameObject.SetActive(true);
        worldPosition = location;
        message = text;
        messageBox.text = message;

        if (!target.isPatched)
        {
            buttonsPanel.SetActive(true);
            buttonsPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            buttonsPanel.GetComponentInChildren<Button>().onClick.AddListener(target.TryPatch);
            buttonsPanel.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "$" + target.patchMoneyCost.ToString("#,#");

            buttonsPanel.GetComponentInChildren<Button>().onClick.AddListener(Close);
        } else
        {
            buttonsPanel.SetActive(false);
        }
        SetScale();
    }

    public void SetScale()
    {
        float minWidth;
        if (buttonsPanel.activeSelf)
            minWidth = gridCellSize.x * UIController.UIScale * 2 + 14;
        else
            minWidth = 36;
        messageBox.fontSize = (int)(17 * UIController.UIScale);

        //buttonsPanel.gameObject.GetComponent<RectTransform>().localScale = new Vector3(UIController.UIScale, UIController.UIScale, 0);
        buttonsPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(gridCellSize.x * UIController.UIScale, gridCellSize.y * UIController.UIScale);

        int buttonsHeight = 0;
        //int buttonsWidth = (int)(gridCellSize.x * UIController.UIScale * 2) + 1;

        int i = 0;
        foreach (Text subText in buttonsPanel.GetComponentsInChildren<Text>())
        {
            if (subText.gameObject.activeSelf)
            {
                i++;
                if (i%2==0)
                    buttonsHeight += (int)(gridCellSize.y * UIController.UIScale) + 1;
                subText.fontSize = (int)(14 * UIController.UIScale);
            }
        }
        //buttonsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, buttonsWidth);
        buttonsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonsHeight);

        RectTransform rt = GetComponent<RectTransform>();
        TextGenerator textGen = new TextGenerator();
        //TextGenerationSettings generationSettings = messageBox.GetGenerationSettings(messageBox.GetComponent<RectTransform>().rect.size);
        TextGenerationSettings generationSettings = messageBox.GetGenerationSettings(new Vector2(Mathf.Infinity, Mathf.Infinity));
        float width = Mathf.Max(Mathf.Round(textGen.GetPreferredWidth(message, generationSettings) / mainCanvas.scaleFactor + 40), minWidth);
        float height = textGen.GetPreferredHeight(message, generationSettings) / mainCanvas.scaleFactor + 1;
        messageBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        if (buttonsPanel.gameObject.activeSelf)
            height += buttonsHeight;
        height += 40;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
