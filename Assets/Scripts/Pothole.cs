using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pothole : MonoBehaviour
{
    private float _angerPerRound;
    private bool _isPatched;
    private float _timeLastPatched;
    private float _patchMoneyCost;
    public Sprite potholeSprite;
    public Sprite patchedPotholeSprite;
    private PlaythroughStatistics _stats;
    
    void Start()
    {
        this._stats = FindObjectOfType<PlaythroughStatistics>();
        this._angerPerRound = Random.value * FindObjectOfType<BalanceParameters>().maximumPotholeAngerPerRound;
        this._patchMoneyCost = FindObjectOfType<BalanceParameters>().patchMoneyCost;
        this._isPatched = false;
        this._timeLastPatched = -1f;
        RenderNormal();
    }

    

    public float getAngerCausedPerRound()
    {
        return this._isPatched ? 0 : this._angerPerRound;
    }

    public void Patch()
    {
        this._isPatched = true;
        this._timeLastPatched = Time.time;
        RenderPatch();
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
