using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pothole : MonoBehaviour
{
    public bool isPatched;
    public bool underConstruction;
    public float patchMoneyCost;
    public Sprite potholeSprite;
    public Sprite patchedPotholeSprite;
    public Sprite constructionSprite;

    public UnityEngine.Material angerParticle;
    public UnityEngine.Material laborParticle;

    private PlaythroughStatistics _stats;
    public BalanceParameters balanceParameters;
    private int _durability;
    private int _constructionTime;
    public ContextMenuController contextMenuControler;
    public Road roadSegment;

    private ParticleSystem _particleSystem;
    private ParticleSystemRenderer _particleSystemRenderer;

    private bool _selected = false;
    private Vector3 _initialScale;
    private float _currentLaborCost = -1;
    private float _angerPerRound;
    private float _constructionAngerPerRound = -1;

    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        _initialScale = new Vector3(transform.localScale.x, transform.localScale.y, 1);
        this._stats = FindObjectOfType<PlaythroughStatistics>();
        this.isPatched = false;
        this.underConstruction = false;
        this._durability = -1;
        this._constructionTime = -1;
        ApplyPotholeAnger();
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
        if (underConstruction)
        {
            RenderConstruction(Color.blue);
        }
        else if (isPatched)
        {
            RenderPatch(Color.blue);
        }
        else
        {
            RenderNormal(Color.blue);
        }
        GetComponent<PolygonCollider2D>().enabled = false;
    }

    public void FauxSelect()
    {
        _selected = true;
        if (underConstruction)
        {
            RenderConstruction(Color.blue);
        }
        else if (isPatched)
        {
            RenderPatch(Color.blue);
        }
        else
        {
            RenderNormal(Color.blue);
        }
    }

    public void Deselect()
    {
        _selected = false;
        if (underConstruction)
        {
            RenderConstruction();
        }
        else if (isPatched)
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
        if (underConstruction)
        {
            PlayAngerParticle();
            return GetConstructionAnger();
        } else if (isPatched)
        {
            return 0;
        } else
        {
            PlayAngerParticle();
            return _angerPerRound;
        }
    }

    private void ApplyPotholeAnger()
    {
        _angerPerRound += Random.Range(0, balanceParameters.potholeAngerPerCar) * (float)roadSegment.trafficRate;

    }

    private void PlayAngerParticle()
    {
        if (_particleSystemRenderer == null || _particleSystem == null)
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        }
        _particleSystemRenderer.material = angerParticle;
        _particleSystem.Play();
    }

    private void PlayLaborParticle()
    {
        if (_particleSystemRenderer == null || _particleSystem == null)
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        }
        _particleSystemRenderer.material = laborParticle;
        _particleSystem.Play();
    }

    private void ApplyConstructionAnger()
    {
        _constructionAngerPerRound = Random.Range(0, balanceParameters.potholeConstructionAngerPerCar);// * (float)roadSegment.trafficRate;
    }

    private float GetConstructionAnger()
    {
        return _constructionAngerPerRound;
    }


    public void Degrade()
    {
        if(isPatched && !underConstruction) 
        {
            this._durability--;
            if (this._durability <= 0)
            {
                Unpatch();
            }
        }
        else if (!isPatched)
        {
            ApplyPotholeAnger();
            RenderNormal();
        }
    }

    private void Patch(int durability, int time)
    {
        isPatched = true;
        InitializeDurability(durability);

        if (time > 0)
        {
            Debug.Log("construction");
            underConstruction = true;
            InitializeConstructionTime(time);
            RenderConstruction();
            ApplyConstructionAnger();
        } else
        {
            RenderPatch();
        }
    }

    public void TryPatch(float cost, int durability, float labor, int time)
    {
        if (CanAffordPatch(cost, labor))
        {
            DeductCost(cost, labor);
            Patch(durability, time);
        }
        else
        {
            Debug.Log("Cannot patch pothole: insufficient funds.  Have " + this._stats.currentBudget + ", need " + cost);
        }
    }

    public void Unpatch()
    {
        this.isPatched = false;
        _durability = -1;
        ApplyPotholeAnger();
        RenderNormal();
    }

    public void NotifyRoundEnded()
    {
        if (this.underConstruction)
        {
            _constructionTime--;
            if (_constructionTime <= 0)
            {
                underConstruction = false;
                RenderPatch();
            }
            else
            {
                RenderConstruction();
                _stats.currentLabor -= _currentLaborCost;

                PlayLaborParticle();
            }

        } 
        else if (this.isPatched && Random.value < 0.5f)
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

    private void InitializeDurability(int durability)
    {
        this._durability = Random.Range(balanceParameters.minimumPotholePatchDuration, durability);
    }

    private void InitializeConstructionTime(int time)
    {
        this._constructionTime = time;
    }

    private void RenderNormal()
    {
        Texture2D tex = Colorize(potholeSprite, _angerPerRound / (balanceParameters.maxAnger * 0.1f));
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

    private void RenderConstruction()
    {
        Render(constructionSprite);
    }

    private void RenderConstruction(Color color)
    {
        Texture2D tex = Colorize(constructionSprite, color);
        Sprite s = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), constructionSprite.pixelsPerUnit);
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
        string message = ""; 
        if (underConstruction)
        {
            message += "Construction on " + roadSegment.roadName;
            message += string.Format("\n<color=#ff2222><b>Annoyance: {0:#,0}</b></color>", GetConstructionAnger());
            message += string.Format("\nLabor cost per month: {0:#,0} hrs.", _currentLaborCost);
            message += "\nRemaining construction time: " + _constructionTime.ToString("#,0");
        }
        else if (isPatched)
        {
            message += "(Patched) Pothole on " + roadSegment.roadName;
            message += "\nDurability: " + _durability.ToString("#,0");
        }
        else
        {
            message += "Pothole on " + roadSegment.roadName;
            message += string.Format("\n<color=#ff2222><b>Annoyance: {0:#,0}</b></color>", _angerPerRound);
        }
        message += "\nRoad Segment ID: " + roadSegment.ID;
        return message;
    }

    private void DeductCost(float cost, float labor)
    {
        this._stats.currentBudget -= cost;
        this._stats.currentLabor -= labor;
        _currentLaborCost = labor;
    }

    public bool CanAffordPatch(float cost, float labor)
    {
        return this._stats.currentBudget >= cost && this._stats.currentLabor >= labor;
    }

    public List<ContextMenuController.ContextMenuOption> GetRepairOptions()
    {
        List<ContextMenuController.ContextMenuOption> options = new List<ContextMenuController.ContextMenuOption>();
        foreach (BalanceParameters.RepairOption option in balanceParameters.potholeRepairs)
        {
            if (option.compatibleRoadMaterials.Contains(roadSegment.material))
            {
                options.Add(CreateOption(option));
            }
        }
        return options;
    }

    private ContextMenuController.ContextMenuOption CreateOption(BalanceParameters.RepairOption option)
    {
        return new ContextMenuController.ContextMenuOption(option.description, option.cost, option.labor, delegate { TryPatch(option.cost, option.durability, option.labor, option.time); });
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
