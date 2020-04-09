using System;
using System.Collections;
using System.Collections.Generic;
using uClicker;
using UnityEngine;

public class ClickerRunner : MonoBehaviour
{
    public ClickerManager Manager;

    // Use this for initialization
    IEnumerator Start()
    {
        Manager.LoadProgress();
        while (Application.isPlaying)
        {
            yield return new WaitForSecondsRealtime(1);
            Manager.Tick();
        }
    }

    private void OnDestroy()
    {
        Manager.SaveProgress();
    }
}