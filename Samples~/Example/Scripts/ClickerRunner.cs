using System.Collections;
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
            Manager.SaveProgress();
            PlayerPrefs.Save();
        }
    }

    private void OnDestroy()
    {
        Manager.SaveProgress();
    }
}