using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StatisticsUIController : MonoBehaviour
{

    public GameObject movingAngerIconImage;
    public GameObject angerThermometer;
    public GameObject moneyPanel;
    public GameObject retireButton;

    public GameObject moneyPrefab;
    public Texture2D thermometerTexture;
    public Texture2D moneyTexture;
    public Color thermometerColor;

    private Text budgetText;
    private Text angerText;
    private PlaythroughStatistics statsModel;

    private float angerInitialPosition;
    private float moneyAspectRatio;
    private List<GameObject> moneyInstances;

    // Start is called before the first frame update
    void Start()
    {
        budgetText = GameObject.Find("BudgetText").GetComponent<Text>();
        angerText = GameObject.Find("AngerText").GetComponent<Text>();
        statsModel = GameObject.FindObjectOfType<PlaythroughStatistics>();

        angerInitialPosition = movingAngerIconImage.GetComponent<RectTransform>().localPosition.y;
        moneyAspectRatio = (float)moneyTexture.height / (float)moneyTexture.width;
        moneyInstances = new List<GameObject>();
        retireButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        budgetText.text = statsModel.currentBudget.ToString("F0");
        angerText.text = statsModel.currentAnger.ToString("F0");

        UpdateAngerMeter();
        UpdateBudgetMeter();
    }


    private void UpdateAngerMeter()
    {
        float angerPercent = Mathf.Min(statsModel.currentAnger / statsModel.maxAnger, 1.0f);

        // Update the icon
        float scaleFactor = 1.0f + Mathf.Sin(Time.time * 4) * 0.25f;
        float rotation = (Mathf.Sin(Time.time * 2) - Time.time * 2) * 10.0f;
        RectTransform rect = movingAngerIconImage.GetComponent<RectTransform>();

        rect.localScale = new Vector3(scaleFactor, scaleFactor, 1.0f);
        rect.rotation = Quaternion.Euler(0, 0, rotation);
        rect.localPosition = new Vector3(rect.localPosition.x, angerInitialPosition * (1 - angerPercent), 0.0f);

        UpdateThermometerTexture(angerPercent);
    }
    private void UpdateBudgetMeter()
    {
        float numberOfMoneys = 100;
        //RectTransform budgetRect = moneyPanel.GetComponent<RectTransform>();
        for (int i=0; i < numberOfMoneys; i++)
        {
            if (i <= (statsModel.currentBudget / statsModel.maxBudget * numberOfMoneys) && moneyInstances.Count < i)
            {
                GameObject moneyObj = Instantiate(moneyPrefab);
                moneyInstances.Add(moneyObj);
                moneyObj.GetComponent<RectTransform>().SetParent(moneyPanel.GetComponent<RectTransform>());
                moneyObj.GetComponent<RectTransform>().localPosition = new Vector3(0, i * moneyPanel.GetComponent<RectTransform>().rect.height / numberOfMoneys, 0);
            } else if (i > (statsModel.currentBudget / statsModel.maxBudget * 100) && moneyInstances.Count >= i)
            {
                for (int j = moneyInstances.Count - 1; moneyInstances.Count >= i; j--)
                {
                    Destroy(moneyInstances[j]);
                    moneyInstances.RemoveAt(j);
                }
            }
        }

        retireButton.SetActive(statsModel.currentBudget >= statsModel.maxBudget);
    }

    private void UpdateThermometerTexture(float percentFilled)
    {
        Texture2D texture = new Texture2D(thermometerTexture.width, thermometerTexture.height, TextureFormat.ARGB32, false);
        int heightOfBulb = (int)((thermometerTexture.height / angerThermometer.GetComponent<RectTransform>().rect.height) * angerInitialPosition) + thermometerTexture.height;

        int colorHeight = (int)((thermometerTexture.height - heightOfBulb) * percentFilled) + heightOfBulb;

        //texture.alphaIsTransparency = true;
        for (int y = 0; y < thermometerTexture.height; y++)
        {
            int leftMostBlackPixel = thermometerTexture.width - 1;
            int rightMostBlackPixel = 0;
            for (int x = 0; x < thermometerTexture.width; x++)
            {
                if (thermometerTexture.GetPixel(x, y) == Color.black)
                {
                    if (leftMostBlackPixel == thermometerTexture.width - 1) leftMostBlackPixel = x;
                    rightMostBlackPixel = x;
                }
            }
            for (int x = 0; x < thermometerTexture.width; x++)
            {
                if (y < colorHeight && x > leftMostBlackPixel && x < rightMostBlackPixel && thermometerTexture.GetPixel(x, y).a < 1)
                {
                    texture.SetPixel(x, y, thermometerColor);
                } else
                {
                    texture.SetPixel(x, y, thermometerTexture.GetPixel(x, y));
                }
            }
        }
        texture.Apply();

        angerThermometer.GetComponent<RawImage>().texture = texture;
    }
}
