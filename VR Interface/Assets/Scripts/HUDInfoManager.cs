using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;

public class HUDInfoManager : MonoBehaviour
{
    public TMP_Text TextIPAddress;

    void Start()
    {
        TextIPAddress.SetText(GetLocalIPAddress());
    }

    public string GetLocalIPAddress()
    // Source: https://discussions.unity.com/t/get-the-device-ip-address-from-unity/235351/4
    {
        string ip_address = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                ip_address = ip.ToString();
            }
        }

        if (string.IsNullOrEmpty(ip_address))
        {
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }
        else
        {
            return ip_address;
        }
    }
}
