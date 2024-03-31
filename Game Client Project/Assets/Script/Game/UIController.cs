using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIController : MonoBehaviour
{
    string Name;
    string Ip;
    string Port;

    [Header("Statistics")]
    public TMP_InputField nameInputField;
    public TMP_InputField ipInputField;
    public TMP_InputField portInputField;

    public TMP_Text namePlaceHolder;
    public TMP_Text ipPlaceHolder;
    public TMP_Text portPlaceHolder;

    public GameObject LoginPanel;
    public GameObject StatisticPanel;
    public GameObject ScoreBoardPanel;

    [Header("Player Score")]
    public TMP_Text playerScore;
    List<TMP_Text> playersInScoreBoard = new List<TMP_Text>();

    private void Start()
    {
        nameInputField.onValueChanged.AddListener(delegate { Name = nameInputField.text; });
        ipInputField.onValueChanged.AddListener(delegate { Ip = ipInputField.text; });
        portInputField.onValueChanged.AddListener(delegate { Port = portInputField.text; });

        playersInScoreBoard.Add(playerScore);
    }

    private void Update()
    {
        var entities = ScoreBoardManager.Instance.GetEntries();
        int textsCount = playersInScoreBoard.Count;
        int playersCount = entities.Count;
        if(textsCount < playersCount)
        {
            while(playersInScoreBoard.Count < entities.Count)
            {
                ClonePanel();
            }
        }
        if(textsCount >= playersCount)
        {
            for (int i = 0; i < textsCount; i++)
            {
                playersInScoreBoard[i].gameObject.SetActive(i <= playersCount - 1);
                if(i <= playersCount - 1)
                {
                    playersInScoreBoard[i].text = entities[i].GetFormattedNameScore();
                }
            }
        }
    }

    public void OnConnectButtonClicked()
    {
     
        Name = (Name == null || Name.Length == 0) ? namePlaceHolder.text : Name;
        Ip = (Ip == null || Ip.Length == 0) ? ipPlaceHolder.text : Ip;
        Port = (Port == null || Port.Length == 0) ? portPlaceHolder.text : Port;

        SaveInformation();

        (NetworkClient.Instance as NetworkClient).Init(Name, Ip, Convert.ToInt32(Port));
        LoginPanelVisible(false);
        Debug.Log("Start to connect");
    }

    public void LoginPanelVisible(bool Visibility)
    {
        LoginPanel.SetActive(Visibility);

        if (Visibility) { RetreiveInformation(); }
    }

    private void SaveInformation()
    {
        PlayerPrefs.SetString("Name", Name);
        PlayerPrefs.SetString("IP", Ip);
        PlayerPrefs.SetString("Port", Port);
    }

    private void RetreiveInformation()
    {
        PlayerPrefs.GetString("Name", Name);
        PlayerPrefs.SetString("IP", Ip);
        PlayerPrefs.SetString("Port", Port);
    }

    public void ClonePanel()
    {
        TMP_Text go = Instantiate(playerScore);
        go.transform.SetParent(ScoreBoardPanel.transform);

        playersInScoreBoard.Add(go);
    }
}
