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

    public GameObject labelPrefab;
    public GameObject buttonPrefab;

    private Vector3 worldPosition;
    private string message = "";
    private Vector2 gridCellSize = new Vector2(130, 20);

    private Road targetRoad;
    private Pothole targetPothole;

    private int _buttonFontSize = 14;
    private int numOptions = 0;

    public class ContextMenuOption
    {
        public string label;
        public float cost;
        public UnityEngine.Events.UnityAction callback;

        public ContextMenuOption(string label, float cost, UnityEngine.Events.UnityAction callback)
        {
            this.label = label;
            this.cost = cost;
            this.callback = callback;
        }
    }

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

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ContextMenuItem"))
            {
                Destroy(obj);
            }
            List<ContextMenuOption> options = target.GetRepairOptions();
            numOptions = options.Count;
            foreach(ContextMenuOption option in options)
            {
                GameObject label = Instantiate(labelPrefab, buttonsPanel.transform);
                label.GetComponent<Text>().text = option.label;
                GameObject button = Instantiate(buttonPrefab, buttonsPanel.transform);
                button.GetComponentInChildren<Text>().text = "$" + option.cost.ToString("#,#");
                button.GetComponent<Button>().onClick.AddListener(option.callback);
                button.GetComponent<Button>().onClick.AddListener(Close);

                if (!target.CanAffordPatch(option.cost))
                    button.GetComponent<Button>().interactable = false;
            }
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

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ContextMenuItem"))
        {
            Destroy(obj);
        }
        List<ContextMenuOption> options = target.GetRepairOptions();
        numOptions = options.Count;
        foreach (ContextMenuOption option in options)
        {
            GameObject label = Instantiate(labelPrefab, buttonsPanel.transform);
            label.GetComponent<Text>().text = option.label;
            GameObject button = Instantiate(buttonPrefab, buttonsPanel.transform);
            button.GetComponentInChildren<Text>().text = "$" + option.cost.ToString("#,#");
            button.GetComponent<Button>().onClick.AddListener(option.callback);
            button.GetComponent<Button>().onClick.AddListener(Close);

            if (!target.CanAffordRePave(option.cost))
                button.GetComponent<Button>().interactable = false;
        }

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
        float buttonsHeight = gridCellSize.y * UIController.UIScale * (numOptions + 1) + 4;
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
