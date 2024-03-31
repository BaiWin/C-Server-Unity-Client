using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class TestOne : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UdpClient udpClient = new UdpClient(11001);
        try
        {
            udpClient.Connect("172.16.1.198", 11000);

            // Sends a message to the host to which you have connected.
            Byte[] sendBytes = Encoding.ASCII.GetBytes("Is anybody there?");

            udpClient.Send(sendBytes, sendBytes.Length);


            //IPEndPoint object will allow us to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            // Blocks until a message returns on this socket from a remote host.
            Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes);

            // Uses the IPEndPoint object to determine which of these two hosts responded.
            Debug.Log("This is the message you received " +
                                         returnData.ToString());
            Debug.Log("This message was sent from " +
                                        RemoteIpEndPoint.Address.ToString() +
                                        " on their port number " +
                                        RemoteIpEndPoint.Port.ToString());

            udpClient.Close();
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
