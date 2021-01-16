using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class StatisticsUIController : MonoBehaviour
{

    public GameObject movingAngerIconImage;
    public GameObject angerThermometer;
    public GameObject laborMeter;
    public GameObject moneyPanel;
    public GameObject retireButton;
    public Text roundDisplay;

    public GameObject moneyPrefab;
    public Texture2D thermometerTexture;
    public Texture2D moneyTexture;
    public Texture2D shovelTexture;
    public Color thermometerColor;

    private Text budgetText;
    private Text angerText;
    private Text laborText;
    private PlaythroughStatistics statsModel;
    private GameManager gameManager;

    private float angerInitialPosition;
    private List<GameObject> moneyInstances;

    private Texture2D _shovelTexture;
    private Texture2D _thermometerTexture;
    private float _previousLabor;

    // Start is called before the first frame update
    void Start()
    {
        budgetText = GameObject.Find("BudgetText").GetComponent<Text>();
        angerText = GameObject.Find("AngerText").GetComponent<Text>();
        laborText = GameObject.Find("LaborText").GetComponent<Text>();
        statsModel = GameObject.FindObjectOfType<PlaythroughStatistics>();
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();

        angerInitialPosition = movingAngerIconImage.GetComponent<RectTransform>().localPosition.y;
        moneyInstances = new List<GameObject>();
        retireButton.SetActive(false);

        _shovelTexture = new Texture2D(shovelTexture.width, shovelTexture.height);
        _shovelTexture.wrapMode = TextureWrapMode.Clamp;
        _thermometerTexture = new Texture2D(thermometerTexture.width, thermometerTexture.height);
        _thermometerTexture.wrapMode = TextureWrapMode.Clamp;
        _previousLabor = statsModel.currentLabor;
    }

    // Update is called once per frame
    void Update()
    {
        
        budgetText.text = statsModel.currentBudget.ToString("#,0");
        angerText.text = statsModel.currentAnger.ToString("#,0");
        roundDisplay.text = "Month: " + GetMonth(gameManager.currentRound+1) + "\nYear: " + (gameManager.currentYear+1);

        UpdateAngerMeter();
        UpdateBudgetMeter();
        UpdateLaborMeter();
        _previousLabor = statsModel.currentLabor;
    }

    private string GetMonth(int month)
    {
        DateTime date = new DateTime(2020, month, 1);
        return date.ToString("MMMM");
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

        StartCoroutine(UpdateThermometerTexture(angerPercent));
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
                moneyObj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                moneyObj.GetComponent<RectTransform>().localPosition = new Vector3(Mathf.Sin(i) * 0.5f, i * moneyPanel.GetComponent<RectTransform>().rect.height / numberOfMoneys, 0);
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

    private void UpdateLaborMeter()
    {
        laborText.text = string.Format("Labor: {0:#,0} Hours", Mathf.Floor(statsModel.currentLabor));
        if (_previousLabor != statsModel.currentLabor)
        {
            StartCoroutine(UpdateShovelTexture());
        }
    }

    private IEnumerator UpdateThermometerTexture(float percentFilled)
    {
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
                float pixelAlpha = thermometerTexture.GetPixel(x, y).a;
                if (y < colorHeight && x > leftMostBlackPixel && x < rightMostBlackPixel && pixelAlpha < 1)
                {
                    //Color color = new Color(thermometerColor.r * (1- pixelAlpha), thermometerColor.g * (1 - pixelAlpha), thermometerColor.b * (1 - pixelAlpha), 1);
                    _thermometerTexture.SetPixel(x, y, thermometerColor);
                } else
                {
                    _thermometerTexture.SetPixel(x, y, thermometerTexture.GetPixel(x, y));
                }
            }

            if (y % 100 == 0) yield return null;
        }
        _thermometerTexture.Apply();

        angerThermometer.GetComponent<RawImage>().texture = _thermometerTexture;
    }

    private IEnumerator UpdateShovelTexture()
    {
        float percentFilled = statsModel.currentLabor / statsModel.maxLabor;

        //texture.alphaIsTransparency = true;
        for (int y = 0; y < _shovelTexture.height; y++)
        {
            for (int x = 0; x < _shovelTexture.width; x++)
            {
                if (shovelTexture.GetPixel(x, y).a == 1 && (float) y / _shovelTexture.height > percentFilled)
                {
                    _shovelTexture.SetPixel(x, y, Color.black);
                } else
                {
                    _shovelTexture.SetPixel(x, y, shovelTexture.GetPixel(x, y));
                }
            }

            if (y % 100 == 0) yield return null;
        }
        _shovelTexture.Apply();

        laborMeter.GetComponent<RawImage>().texture = _shovelTexture;
    }
}
