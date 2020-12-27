using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Utils : MonoBehaviour
{
    public static float Sigmoid(double value)
    {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }
}
