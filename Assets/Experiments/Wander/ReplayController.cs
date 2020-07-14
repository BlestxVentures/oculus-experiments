using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UltimateReplay;

public class ReplayController : MonoBehaviour
{
    public void StartRecording()
    {
        Debug.Log("--- Start Recording ---");
        ReplayManager.BeginRecording(true);
    }

    public void StopRecording()
    {
        Debug.Log("--- Stop Recording ---");
        ReplayManager.StopRecording();
    }

    public void StartPlayback()
    {
        ReplayManager.BeginPlayback();
    }
}
