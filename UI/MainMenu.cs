using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject availablePlayersParent;
    [SerializeField] private GameObject chosenPlayers;
    [SerializeField] private GameObject availablePlayersPrefab;
    [SerializeField] private string testName;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text userNotFoundText;
    [SerializeField] private List<TMP_Text> chatUiText = new List<TMP_Text>();
    [SerializeField] private Image connectedImage;
    [SerializeField] private Image NotConnectedImage;
    [SerializeField] private EventSystem eSystem;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private Toggle checkBox;

    private List<GameObject> availableObjs = new List<GameObject>();
    private List<GameObject> chosenObjs = new List<GameObject>();


    private TwitchChat twitchChat;

    private RandomDiabloFacts randomFactsClass;
    private float randomFactsTimer = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        TwitchChat.OnChatMessageSent += OnChatMessage;
        TwitchChat.OnTwitchConnection += SetConnectedImage;
        twitchChat = TwitchChat.Instance;

        if (twitchChat == null && !twitchChat.remember) return;
        checkBox.isOn = twitchChat.remember;
        inputPassword.text = twitchChat.password;
        if (twitchChat != null && twitchChat.isTwitchConnected)
        {
            connectedImage.gameObject.SetActive(true);
            NotConnectedImage.gameObject.SetActive(false);
        }
        else if (twitchChat != null && !twitchChat.isTwitchConnected)
        {
            connectedImage.gameObject.SetActive(false);
            NotConnectedImage.gameObject.SetActive(true);
        }



        randomFactsClass = new RandomDiabloFacts();
    }

    private void Update()
    {
       
    }

    public void OnChatMessage(string userName, string message)
    {
        string commandString = message.Split()[0].ToLower();
        string[] splitMessage = message.Split();
        if (commandString == "!animal")
        {
            print("test working");
        }
        for (int i = 0; i < twitchChat.chatMessages.Count; i++)
        {
            chatUiText[i].text = twitchChat.chatMessages[i];
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void SetConnectedImage(bool status)
    {
        if (status)
        {
            connectedImage.gameObject.SetActive(true);
            NotConnectedImage.gameObject.SetActive(false);
        }
        else
        {
            connectedImage.gameObject.SetActive(false);
            NotConnectedImage.gameObject.SetActive(true);
        }
    }

    public void SetInputFocused()
    {
        eSystem.SetSelectedGameObject(input.gameObject);
    }

    public void SetPlayerNotFoundText()
    {
        foreach (var obj in availableObjs)
        {
            if (obj.name == input.text)
            {
                userNotFoundText.gameObject.SetActive(false);
                return;
            }
        }
        userNotFoundText.gameObject.SetActive(true);
    }

    public void CloseSearchUser()
    {
        input.text = string.Empty;
        userNotFoundText.gameObject.SetActive(false);
    }

    public void SetPassword()
    {
        twitchChat.SetPassword(inputPassword.text);
    }

    public void SetRememberOAuth(bool value)
    {
        twitchChat.remember = checkBox.isOn;
        //if (twitchChat.remember)
        //{
        //    twitchChat.SetRememberOAuth(false);
        //}
        //else
        //{
        //    twitchChat.SetRememberOAuth(true);
        //}
    }

    public void ConnectManually()
    {
        twitchChat.ConnectManually(inputPassword.text);
    }

    public void GetViewerList()
    {
        twitchChat.GetViewerList();
    }

    public void WriteToChat()
    {
        string message = randomFactsClass.randomFacts[Random.Range(0, randomFactsClass.randomFacts.Length)];
        twitchChat.WriteToChat(message);
    }

    private void OnEnable()
    {
        TwitchChat.Instance.ClearLobby();
    }

    private void OnDestroy()
    {
        TwitchChat.OnChatMessageSent -= OnChatMessage;
        TwitchChat.OnTwitchConnection -= SetConnectedImage;
    }
}
