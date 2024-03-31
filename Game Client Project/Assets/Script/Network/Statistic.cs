using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Statistic : MonoBehaviour
{
    public TMP_Text simulatedLatency;
    public TMP_Text dropPacketChange;
    public TMP_Text brandWidthIn;
    public TMP_Text brandWidthOut;
    public TMP_Text roundTripTime;

    void Update()
    {
        simulatedLatency.text = "simulated lantency:" + (NetworkClient.Instance as NetworkClient).GetSimulatedLatency().ToString("0.0");
        dropPacketChange.text = "drop packet change:" + (NetworkClient.Instance as NetworkClient).GetDropPacketChance().ToString("0.00");
        brandWidthIn.text = "bytes recv per second:" + (NetworkClient.Instance as NetworkClient).GetBytesReceivedPerSecond().GetValue().ToString("0");
        brandWidthOut.text = "bytes sent per second:" + (NetworkClient.Instance as NetworkClient).GetBytesSentPerSecond().GetValue().ToString("0");
        roundTripTime.text = "RTT:" + ((NetworkClient.Instance as NetworkClient).GetAvgRoundTripTime().GetValue() * 1000f).ToString("0") ;
    }
}
