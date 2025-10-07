using PurrNet;
using PurrNet.Modules;
using PurrNet.Packing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class MenuManager : NetworkBehaviour
{
    [SerializeField] TMP_Text playerCountTxt;
    [SerializeField] TMP_Text connectionStatusTxt;
    [SerializeField] GameObject hostBtn, joinBtn, startBtn;
    [SerializeField] Button nicknameConfirmation, host, join, start;
    [SerializeField] TMP_InputField nicknameField;

    string nicknameStr;

    public void OnNicknameValueChange(string str)
    {
        if(str.IsNullOrEmpty())
        {
            if(nicknameConfirmation.interactable)
                nicknameConfirmation.interactable = false;
        }
        else
        {
            nicknameStr = str;

            if(!nicknameConfirmation.interactable)
                nicknameConfirmation.interactable = true;
        }
    }

    public void NicknameConfirmation()
    {
        nicknameConfirmation.interactable = false;
        nicknameField.interactable = false;

        host.interactable = true;
        join.interactable = true;
        start.interactable = true;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();

        SavePlayerNickName(networkManager.localPlayer.id.value, nicknameStr);
        Debug.Log($"{networkManager.localPlayer.id}");

        if (isServer)
        {
            hostBtn.SetActive(false);
            joinBtn.SetActive(false);
            startBtn.SetActive(true);

            playerCountTxt.text = "1 Player Connected";
            connectionStatusTxt.text = $"Host {networkManager.serverState.ToString()}";

            return;
        }

        hostBtn.SetActive(false);
        joinBtn.SetActive(false);
        UpdatePlayerCount();
        connectionStatusTxt.text = $"{networkManager.clientState.ToString()}, Wait for host to start";
    }

    [ServerRpc]
    private void SavePlayerNickName(ulong id, string nickname)
    {
        GameData.Instance.nicknames.Add(id, nickname);
        GameData.Instance.scores.Add(id, 0);
    }

    [ObserversRpc]
    private void UpdatePlayerCount()
    {
        playerCountTxt.text = $"{networkManager.playerCount} Players Connected";
    }
}
