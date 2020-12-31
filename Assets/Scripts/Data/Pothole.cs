using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pothole : MonoBehaviour
{
    private float _angerPerRound;
    private bool _isPatched;
    private float _patchMoneyCost;
    public Sprite potholeSprite;
    public Sprite patchedPotholeSprite;
    private PlaythroughStatistics _stats;
    private BalanceParameters _parameters;
    private int _durability;

    void Start()
    {
        this._parameters = FindObjectOfType<BalanceParameters>();
        this._stats = FindObjectOfType<PlaythroughStatistics>();
        this._angerPerRound = Random.value * _parameters.maximumPotholeAngerPerRound;
        this._patchMoneyCost = _parameters.patchMoneyCost;
        this._isPatched = false;
        this._durability = -1;
        RenderNormal();
    }

    public float getAngerCausedPerRound()
    {
        return this._isPatched ? 0 : this._angerPerRound;
    }

    public void Patch()
    {
        this._isPatched = true;
        InitializeDurability();
        RenderPatch();
    }
    
    public void Unpatch()
    {
        this._isPatched = false;
        _durability = -1;
        RenderNormal();
    }

    public void NotifyRoundEnded()
    {
        if (this._isPatched)
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
        this.GetComponent<SpriteRenderer>().sprite = potholeSprite;
    }
    
    private void RenderPatch()
    {
        this.GetComponent<SpriteRenderer>().sprite = patchedPotholeSprite;
    }

    void OnMouseUpAsButton()
    {
        if (canAffordPatch())
        {
            DeductCost();
            Patch();
        }
        else
        {
            Debug.Log("Cannot patch pothole: insufficient funds.  Have " + this._stats.currentBudget + ", need " + this._patchMoneyCost);
        }
    }

    private float DeductCost()
    {
        return this._stats.currentBudget -= this._patchMoneyCost;
    }

    private bool canAffordPatch()
    {
        return this._stats.currentBudget >= this._patchMoneyCost;
    }
}
