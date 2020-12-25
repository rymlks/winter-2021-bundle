using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PotholeController : MonoBehaviour
{
    public int minimumNewHolesPerRound;
    public int maximumNewHolesPerRound;
    public float maximumPotholeAngerPerRound;
    public GameObject potholePrefab;
    private GameObject _potholeParent;

    private float WIDTH_OF_PLAY_AREA = 20f;
    private float HEIGHT_OF_PLAY_AREA = 9.5f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (potholePrefab == null)
        {
            potholePrefab = Resources.Load<GameObject>("Prefabs/Pothole");
        }

        if (_potholeParent == null)
        {
            _potholeParent = new GameObject("Potholes");
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (roundHasEndedThisFrame())
        {
            createAngerDueToExistingPotholes();
            createNewPotholes();
        }
    }

    private void createAngerDueToExistingPotholes()
    {
        FindObjectOfType<PlaythroughStatistics>().currentAnger += getTotalAngerFromExistingPotholes();
    }

    private float getTotalAngerFromExistingPotholes()
    {
        return _potholeParent.GetComponentsInChildren<Pothole>().Sum(hole => hole.getAngerCausedPerRound());
    }

    private void createNewPotholes()
    {
        int newPotholesThisRound = new System.Random().Next(minimumNewHolesPerRound, maximumNewHolesPerRound + 1);    //Sysrand is exclusive of the maximum
        for (int i = 0; i < newPotholesThisRound; i++)
        {
            GameObject.Instantiate(potholePrefab, generatePotholeLocation(), Quaternion.identity, _potholeParent.transform);
        }
    }

    private Vector2 generatePotholeLocation()
    {
        //integrate the real map when we have one
        //for now, anyplace the pothole is fully visible to the camera is a valid place for it to appear
        float x = Random.value * WIDTH_OF_PLAY_AREA;
        float z = Random.value * HEIGHT_OF_PLAY_AREA;
        return new Vector2((-WIDTH_OF_PLAY_AREA / 2f) + x,(-HEIGHT_OF_PLAY_AREA / 2f) + z);
    }

    private bool roundHasEndedThisFrame()
    {
        //integrate the real round system when we have one
        //for now, rounds can be faked as ending by the player pressing 'R'
        return Input.GetKeyUp(KeyCode.R);
    }
}
