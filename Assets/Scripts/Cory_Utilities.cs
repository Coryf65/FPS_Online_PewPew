using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Cory_Utilities
{
    /// <summary>
    ///  Jason Story's EaseIn
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float EaseInSine(float start, float end, float value)
    {
        end -= start;

        return -end * Mathf.Cos(value * (Mathf.PI * 0.5f)) + end + start;
    }

    /// <summary>
    ///  Jason Story's EaseOut
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float EaseOutSine(float start, float end, float value)
    {
        end -= start;

        return end * Mathf.Sin(value * (Mathf.PI * 0.5f)) + start;
    }

    /// <summary>
    ///  Jason Story's EaseInOut
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float EaseInOutSine(float start, float end, float value)
    {
        end -= start;

        return -end * 0.5f * (Mathf.Cos(Mathf.PI * value) - 1);
    }
}
