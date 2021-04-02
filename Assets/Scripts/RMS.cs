using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RMS : MonoBehaviour
{
    FMOD.Studio.EventInstance instance;

    [FMODUnity.EventRef]
    public string fmodEvent;

    FMOD.DSP dsp = new FMOD.DSP();
    FMOD.DSP_METERING_INFO meterInfo = new FMOD.DSP_METERING_INFO();
    FMOD.ChannelGroup channelGroup;
    bool loaded;

    [SerializeField]
    private TMPro.TMP_Text text;

    float GetRMS()
    {
        float rms = 0f;

        dsp.getMeteringInfo(IntPtr.Zero, out meterInfo);
        for (int i = 0; i < meterInfo.numchannels; i++)
        {
            rms += meterInfo.rmslevel[i] * meterInfo.rmslevel[i];
        }

        rms = Mathf.Sqrt(rms / (float)meterInfo.numchannels);

        float dB = rms > 0 ? 20.0f * Mathf.Log10(rms * Mathf.Sqrt(2.0f)) : -80.0f;
        if (dB > 10.0f) dB = 10.0f;
        return dB;
    }

    void Start()
    {
        instance = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        instance.start();
        StartCoroutine(GetChannelGroup());
        InvokeRepeating("DisplayRMS", 0.5f, 0.3f);
    }

    IEnumerator GetChannelGroup()
    {
        if (instance.isValid())
        {
            while (instance.getChannelGroup(out channelGroup) != FMOD.RESULT.OK)
            {
                yield return new WaitForEndOfFrame();
                loaded = false;
            }

            channelGroup.getDSP(0, out dsp);
            dsp.setMeteringEnabled(false, true);

            loaded = true;
        }
        else
        {
            Debug.Log("Instanz gibt es nicht");
            yield return null;
        }
    }

    void DisplayRMS()
    {
        if (loaded)
        {
            text.SetText("RMS: " + GetRMS().ToString("F2") + " dB");
        }
    }
}
