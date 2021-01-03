﻿using System.Collections;
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
    public BalanceParameters balanceParameters;
    private int _durability;
    public ContextMenuController contextMenuControler;
    public Road roadSegment;

    private bool _selected = false;
    private Vector3 _initialScale;

    void Start()
    {
        _initialScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
        this._stats = FindObjectOfType<PlaythroughStatistics>();
        this._angerPerRound = Random.value * balanceParameters.maximumPotholeAngerPerRound;
        this.patchMoneyCost = balanceParameters.patchMoneyCost;
        this.isPatched = false;
        this._durability = -1;
        RenderNormal();
    }

    public void Update()
    {
        if (_selected)
        {
            float scale = 1.0f + Mathf.Sin(Time.time * 5) * 0.05f;
            gameObject.transform.localScale = new Vector3(_initialScale.x * scale, _initialScale.y * scale, 1);
        }
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        contextMenuControler.Open(this);
        Select();
    }

    public void Select()
    {
        _selected = true;
        if (isPatched)
        {
            RenderPatch(Color.blue);
        } else
        {
            RenderNormal(Color.blue);
        }
        GetComponent<PolygonCollider2D>().enabled = false;
    }

    public void Deselect()
    {
        _selected = false;
        if (isPatched)
        {
            RenderPatch();
        }
        else
        {
            RenderNormal();
        }
        GetComponent<PolygonCollider2D>().enabled = true;
        transform.localScale = _initialScale;
    }

    public float getAngerCausedPerRound()
    {
        return this.isPatched ? 0 : this._angerPerRound;
    }

    public void Degrade()
    {
        if(isPatched) 
        {
            this._durability--;
            if (this._durability <= 0)
            {
                Unpatch();
            }
        }
        else
        {
            this._angerPerRound += Random.value * balanceParameters.maximumPotholeAngerPerRound;
            RenderNormal();
        }
    }

    private void Patch()
    {
        this.isPatched = true;
        InitializeDurability();
        RenderPatch();
    }

    public void TryPatch()
    {
        if (CanAffordPatch())
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
            } else
            {
                RenderPatch();
            }
        }
    }

    private void InitializeDurability()
    {
        this._durability = Random.Range(balanceParameters.minimumPotholePatchDuration, balanceParameters.maximumPotholePatchDuration + 1);
    }

    private void RenderNormal()
    {
        Texture2D tex = Colorize(potholeSprite, _angerPerRound / 3);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), potholeSprite.pixelsPerUnit);
        Render(s);
    }
    private void RenderNormal(Color color)
    {
        Texture2D tex = Colorize(potholeSprite, color);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), potholeSprite.pixelsPerUnit);
        Render(s);
    }

    private void RenderPatch()
    {
        Texture2D tex = Colorize(patchedPotholeSprite, 1.0f / _durability);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), patchedPotholeSprite.pixelsPerUnit);
        Render(s);
    }

    private void RenderPatch(Color color)
    {
        Texture2D tex = Colorize(patchedPotholeSprite, color);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), patchedPotholeSprite.pixelsPerUnit);
        Render(s);
    }

    private void Render(Sprite s)
    {
        GetComponent<SpriteRenderer>().sprite = s;
        DestroyImmediate(GetComponent<PolygonCollider2D>());
        gameObject.AddComponent<PolygonCollider2D>();
    }

    public string GetContextMessage()
    {
        string message = "Pothole on " + roadSegment.roadName;
        message += "\nRoad Segment ID: " + roadSegment.ID;
        message += "\nAnnoyance: " + _angerPerRound.ToString("#,#");
        if (isPatched)
            message += "\nDurability: " + _durability.ToString("#,#");
        return message;
    }

    private float DeductCost()
    {
        return this._stats.currentBudget -= this.patchMoneyCost;
    }

    public bool CanAffordPatch()
    {
        return this._stats.currentBudget >= this.patchMoneyCost;
    }

    private Texture2D Colorize(Sprite sprite, float percent)
    {
        return Colorize(sprite, Color.Lerp(Color.yellow, Color.red, percent));
    }

    private Texture2D Colorize(Sprite sprite, Color color)
    {
        Texture2D original = sprite.texture;
        Texture2D tex = new Texture2D(original.width, original.height);
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                Color sourcePixel = original.GetPixel(x, y);
                if (sourcePixel.r > 0 && sourcePixel.b < sourcePixel.r)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, sourcePixel);
                }
            }
        }
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();
        return tex;
    }
}
