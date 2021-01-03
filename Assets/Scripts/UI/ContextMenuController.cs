using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuController : MonoBehaviour
{
    public Canvas mainCanvas;
    public Text messageBox;
    public GameObject buttonsPanel;
    public GameObject selectRoadButton;

    private Vector3 worldPosition;
    private string message = "";
    private Vector2 gridCellSize = new Vector2(130, 20);

    private Road targetRoad;
    private Pothole targetPothole;

    private int _buttonFontSize = 14;

    // Update is called once per frame
    void Update()
    {
        if (worldPosition != null)
        {
            transform.position = Camera.main.WorldToScreenPoint(worldPosition);
        }
    }

    public void Open(Pothole target)
    {
        gameObject.SetActive(true);
        Deselect();

        selectRoadButton.SetActive(true);
        targetRoad = null;
        targetPothole = target;
        string text = target.GetContextMessage();
        worldPosition = target.transform.position;
        message = text;
        messageBox.text = message;

        if (!target.isPatched)
        {
            buttonsPanel.SetActive(true);
            buttonsPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            buttonsPanel.GetComponentInChildren<Button>().onClick.AddListener(target.TryPatch);
            buttonsPanel.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "$" + target.patchMoneyCost.ToString("#,#");
            if (!target.CanAffordPatch())
                buttonsPanel.GetComponentInChildren<Button>().interactable = false;
            else
                buttonsPanel.GetComponentInChildren<Button>().interactable = true;

            buttonsPanel.GetComponentInChildren<Button>().onClick.AddListener(Close);
        } else
        {
            buttonsPanel.SetActive(false);
        }
        SetScale();
    }

    public void Open(Road target)
    {
        gameObject.SetActive(true);
        Deselect();

        selectRoadButton.SetActive(false);
        targetRoad = target;
        targetPothole = null;
        string text = target.GetContextMessage();
        worldPosition = target.MidPoint();
        message = text;
        messageBox.text = message;

        buttonsPanel.SetActive(true);
        buttonsPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        buttonsPanel.GetComponentInChildren<Button>().onClick.AddListener(target.TryRePave);
        buttonsPanel.GetComponentInChildren<Button>().GetComponentInChildren<Text>().text = "$" + target.GetRePaveCost().ToString("#,#");
        if (!target.CanAffordRePave())
            buttonsPanel.GetComponentInChildren<Button>().interactable = false;
        else
            buttonsPanel.GetComponentInChildren<Button>().interactable = true;

        buttonsPanel.GetComponentInChildren<Button>().onClick.AddListener(Close);

        SetScale();
    }

    public void SetScale()
    {
        // Find minimum width
        float minWidth;
        if (buttonsPanel.activeSelf)
            minWidth = gridCellSize.x * UIController.UIScale * 2 + _buttonFontSize;
        else
            minWidth = 36;

        // Set font size
        messageBox.fontSize = (int)(17 * UIController.UIScale);

        // Adjust buttons
        buttonsPanel.GetComponent<GridLayoutGroup>().cellSize = new Vector2(gridCellSize.x * UIController.UIScale, gridCellSize.y * UIController.UIScale);
        float buttonsHeight = 0;
        int i = 0;
        foreach (Text subText in buttonsPanel.GetComponentsInChildren<Text>())
        {
            if (subText.gameObject.activeSelf)
            {
                i++;
                if (i%2==0)
                    buttonsHeight += (gridCellSize.y * UIController.UIScale) + 1;
                subText.fontSize = (int)(14 * UIController.UIScale);
            }
        }
        buttonsPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, buttonsHeight);
        selectRoadButton.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 18 * UIController.UIScale);

        RectTransform rt = GetComponent<RectTransform>();
        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings generationSettings = messageBox.GetGenerationSettings(new Vector2(Mathf.Infinity, Mathf.Infinity));
        float width = Mathf.Max(Mathf.Round(textGen.GetPreferredWidth(message, generationSettings) / mainCanvas.scaleFactor + 40), minWidth);
        float height = textGen.GetPreferredHeight(message, generationSettings) / mainCanvas.scaleFactor + 1;
        messageBox.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        if (buttonsPanel.gameObject.activeSelf)
            height += buttonsHeight;
        if (selectRoadButton.activeSelf)
            height += selectRoadButton.GetComponent<RectTransform>().rect.height;
        height += 24;

        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    public void GoToRoad()
    {
        if (targetPothole != null)
        {
            targetPothole.roadSegment.DoClick();
        }
    }

    public void Close()
    {
        Deselect();
        gameObject.SetActive(false);
    }

    private void Deselect()
    {

        if (targetPothole != null)
        {
            targetPothole.Deselect();
        }
        if (targetRoad != null)
        {
            targetRoad.Deselect();
        }
    }
}
