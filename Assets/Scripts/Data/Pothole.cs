using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pothole : MonoBehaviour
{
    private float _angerPerRound;
    public bool isPatched;
    public float patchMoneyCost;
    public Sprite potholeSprite;
    public Sprite patchedPotholeSprite;
    private PlaythroughStatistics _stats;
    private BalanceParameters _parameters;
    private int _durability;
    public ContextMenuControler contextMenuControler;
    public Road roadSegment;

    void Start()
    {
        this._parameters = FindObjectOfType<BalanceParameters>();
        this._stats = FindObjectOfType<PlaythroughStatistics>();
        this._angerPerRound = Random.value * _parameters.maximumPotholeAngerPerRound;
        this.patchMoneyCost = _parameters.patchMoneyCost;
        this.isPatched = false;
        this._durability = -1;
        RenderNormal();
    }

    public float getAngerCausedPerRound()
    {
        return this.isPatched ? 0 : this._angerPerRound;
    }

    private void Patch()
    {
        this.isPatched = true;
        InitializeDurability();
        RenderPatch();
    }

    public void TryPatch()
    {
        if (canAffordPatch())
        {
            DeductCost();
            Patch();
        }
        else
        {
            Debug.Log("Cannot patch pothole: insufficient funds.  Have " + this._stats.currentBudget + ", need " + this.patchMoneyCost);
        }
    }
    
    public void Unpatch()
    {
        this.isPatched = false;
        _durability = -1;
        RenderNormal();
    }

    public void NotifyRoundEnded()
    {
        if (this.isPatched)
        {
            this._durability--;
            if(this._durability <= 0){
                Unpatch();
            }
        }
    }

    private void InitializeDurability()
    {
        this._durability = Random.Range(_parameters.minimumPotholePatchDuration, _parameters.maximumPotholePatchDuration + 1);
    }

    private void RenderNormal()
    {
        Texture2D tex = Colorize(potholeSprite);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), potholeSprite.pixelsPerUnit);
        this.GetComponent<SpriteRenderer>().sprite = s;
    }
    
    private void RenderPatch()
    {
        Texture2D tex = Colorize(patchedPotholeSprite);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), patchedPotholeSprite.pixelsPerUnit);
        
        this.GetComponent<SpriteRenderer>().sprite = s;
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        string contextText = "Annoyance: " + _angerPerRound.ToString("#,#");
        if (isPatched)
            contextText += "\nDurability: " + _durability.ToString("#,#");

        contextMenuControler.Open(contextText, transform.position, this);
    }

    private float DeductCost()
    {
        return this._stats.currentBudget -= this.patchMoneyCost;
    }

    private bool canAffordPatch()
    {
        return this._stats.currentBudget >= this.patchMoneyCost;
    }

    private Texture2D Colorize(Sprite sprite)
    {
        Texture2D original = sprite.texture;
        Texture2D tex = new Texture2D(original.width, original.height);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color sourcePixel = original.GetPixel(x, y);
                if (sourcePixel.r > 0 && sourcePixel.b < 1)
                {
                    Color c = Color.Lerp(Color.yellow, Color.red, _angerPerRound / 3);
                    //c.b = sourcePixel.b;
                    tex.SetPixel(x, y, c);
                } else
                {
                    tex.SetPixel(x, y, sourcePixel);
                }
            }
        }
        tex.Apply();
        return tex;
    }
}
