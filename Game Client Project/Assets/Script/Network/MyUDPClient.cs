using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
using System.Net.NetworkInformation;

public struct UdpState
{
    public UdpClient u;
    public IPEndPoint e;
    public int bytesLength;
}

public class MyUDPClient
{
    private byte[] receiveBuffer;
    private byte[] sendBuffer;
    //private int receiveByteLength;

    UdpClient udpClient;
    IPEndPoint endPoint;

    UdpState s = new UdpState();

    public static bool messageReceived = false;
    public static bool messageSent = false;

    public delegate void DelegateSSLSessionMsgEvent(byte[] msg);
    public DelegateSSLSessionMsgEvent OnMsgParse;

    public MyUDPClient(string host, int port, byte[] receiveBuffer, byte[] sendBuffer)
    {  
        IPAddress ipAddress = Dns.GetHostEntry(host).AddressList[1];
        this.udpClient = new UdpClient();
        this.endPoint = new IPEndPoint(ipAddress, port);
        this.receiveBuffer = receiveBuffer;
        this.sendBuffer = sendBuffer;
        //this.receiveByteLength = receiveByteLength;
    }

    public void Connect()
    {
        try
        {
            udpClient.Connect(endPoint);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            udpClient.Close();
        }
    }

    public void SendCallback(IAsyncResult ar)
    {
        UdpClient u = (UdpClient)ar.AsyncState;
        int bytesSent = u.EndSend(ar);
        Console.WriteLine($"number of bytes sent: {bytesSent}");
        messageSent = true;
    }

    public int Send(byte[] sendBytes, int bytesLength)
    {
        try
        {
            udpClient.BeginSend(sendBytes, sendBytes.Length, SendCallback, udpClient);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            udpClient.Close();
        }
        return bytesLength;
    }

    public void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            if(receiveBytes.Length > 0)
            {
                OnMsgParse.Invoke(receiveBytes);
            }
            
            messageReceived = true;
            Receive();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            udpClient.Close();
        }
    }

    public void Receive()
    {
        try
        {
            s.e = endPoint;
            s.u = udpClient;
            udpClient.BeginReceive(ReceiveCallback, s);
        }
        catch
        {
            udpClient.Close();
        }
    }

    public void Close()
    {
        udpClient.Close();
    }

    public void Dispose() { }

    public void Test()
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
            Debug.LogException(e);
        }
    }
}
