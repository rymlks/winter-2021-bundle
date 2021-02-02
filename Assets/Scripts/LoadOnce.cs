using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadOnce : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Music");
        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            transform.parent = null;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
