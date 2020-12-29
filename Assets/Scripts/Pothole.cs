using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pothole : MonoBehaviour
{
    private float _angerPerRound;
    private bool _isPatched;
    private float _timeLastPatched;
    public Sprite potholeSprite;
    public Sprite patchedPotholeSprite;
    
    void Start()
    {
        this._angerPerRound = Random.value * FindObjectOfType<BalanceParameters>().maximumPotholeAngerPerRound;
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
        this.Patch();
    }
}
