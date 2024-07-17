using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net.Sockets;

public delegate void MainMenuChatMessageSent(string userName, string message);
public delegate void TwitchConnected(bool status);


public class TwitchChat : MonoBehaviour
{
    public static TwitchChat Instance;

    public static event MainMenuChatMessageSent OnChatMessageSent;
    public static event MainMenuChatMessageSent OnChatUserjoined;
    public static event TwitchConnected OnTwitchConnection;

    //[SerializeField] private TMP_InputField input;
    //[SerializeField] private Toggle checkBox;
    [SerializeField] private bool debug = false;

    //public UnityEvent<string, string> onChatMessage;

    public string username; // 1
    public string password;
    public string channelName;
    public List<string> availablePlayers = new List<string>();
    public List<string> lobby = new List<string>();
    public int maxPlayersInLobby = 20;
    public List<string> chatMessages = new List<string>();

    public bool isTwitchConnected = false;
    public bool remember = false;

    private TcpClient twitchClient; // 2
    private StreamReader reader; // 3

    private StreamWriter writer;

    private float reconnectTimer; // 4 
    private float reconnectAfter;

    private bool pressedConnect = false;
    private float pingCounter = 0f;
    private bool firstConnect = false;

    private RandomDiabloFacts randomFactsClass;
    private float randomFactsTimer = 60.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != null)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        for (int i = 0; i < 9; i++)
        {
            chatMessages.Add(" ");
        }
        DontDestroyOnLoad(this.gameObject);
        reconnectAfter = 60.0f;

        randomFactsClass = new RandomDiabloFacts();
    }

    public void GetViewerList()
    {
    }

    void Update()
    {
        //if (randomFactsTimer > 0.0f && twitchClient != null && twitchClient.Connected)
        //{
        //    randomFactsTimer -= Time.deltaTime;
        //    if (randomFactsTimer < 0)
        //    {
        //        WriteToChat("");
        //        randomFactsTimer = 300.0f;
        //    }
        //}
        if (twitchClient != null && !twitchClient.Connected && isTwitchConnected)
        {
            isTwitchConnected = false;
            OnTwitchConnection?.Invoke(false);
            print("123");
        }
        else if (twitchClient != null && twitchClient.Connected && !isTwitchConnected)
        {
            isTwitchConnected = true;
            OnTwitchConnection?.Invoke(true);
            print("456");

        }

        if (pressedConnect == false) return;

        pingCounter += Time.deltaTime;

        if (pingCounter > 60)
        {
            writer.WriteLine("Ping " + "irc.chat.twitch.tv");
            writer.Flush();
            pingCounter = 0;
        }

        if (!twitchClient.Connected)
        {
            Connect();
        }

        if (twitchClient.Available > 0)
        {
            //string message = reader.ReadLine();
            //print(message);
            if (firstConnect)
            {
                ReadChat();
                //print("reading");
            }
            firstConnect = true;
        }


    }

    public void ClearLobby()
    {
        lobby.Clear();
    }

    public void WriteToChat(string message)
    {
        // string message = randomFactsClass.randomFacts[Random.Range(0, randomFactsClass.randomFacts.Length)];
        string messageToSend = randomFactsClass.randomFacts[Random.Range(0, randomFactsClass.randomFacts.Length)]; ;
        //writer.WriteLine("PRIVMSG #" + "Diablo Bot" + " :" + messageToSend);
        writer.WriteLine("PRIVMSG #" + channelName + " :" + "Random DIablo 3 Facts: " + messageToSend);

        //writer.WriteLine($"PRIVMSG #{channelName} :{messageToSend}");
        print("PRIVMSG #" + channelName + " :" + messageToSend);
    }

    private void Connect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667); // 1
        reader = new StreamReader(twitchClient.GetStream()); // 2
        writer = new StreamWriter(twitchClient.GetStream());
        writer.WriteLine("PASS " + password); // 3
        writer.WriteLine("NICK " + username);
        // writer.WriteLine("USER " + username + " 8 *:" + username);
        writer.WriteLine("JOIN #" + channelName);
        print("######ChanelName######: " + channelName);
        writer.Flush();
        //print("Connect");
    }

    public ChatMessage ReadChat() // 1
    {
        string message = reader.ReadLine();

        print(message);

        if (message.Contains("PRIVMSG"))
        {
            // Get the username
            int splitPoint = message.IndexOf("!", 1); // 2
            string chatName = message.Substring(0, splitPoint);
            chatName = chatName.Substring(1);

            //Get the message
            splitPoint = message.IndexOf(":", 1);
            message = message.Substring(splitPoint + 1);
            ChatMessage chatMessage = new ChatMessage(); // 3
            chatMessage.user = chatName;
            chatMessage.message = message.ToLower();

            UpdateChat(chatName, chatMessage.user + ": " + chatMessage.message);
            OnChatMessageSent?.Invoke(chatName, message);

            CheckUserJoin(chatName, message);
            return chatMessage;
        }

        return null; // 4
    }

    private void CheckUserJoin(string userName, string message)
    {
        if (message == "!join")
        {
            if (!availablePlayers.Contains(userName))
            {
                availablePlayers.Add(userName);
                OnChatUserjoined?.Invoke(userName, message);
            }
        }
    }

    public void ConnectManually(string password)
    {
        this.password = password;
        pressedConnect = true;
        Connect();
    }

    public void SetPassword(string text)
    {
        this.password = text;
    }

    public void SetRememberOAuth(bool value)
    {
        remember = value;
    }

    public string GetRandomText()
    {
        string newMessage = "";
        string[] messages = {"Wuff, wuff, wuff...", "Miau, miau, miau...", "Muh, muh, muh...", " Kikerikiiiii...",
        "Iaaahh, Iaaahh, Iaaahhh..."};
        int index = Random.Range(0, messages.Length - 1);
        return messages[index];
    }

    public void UpdateChat(string chatName, string message)
    {
        string commandString = message.Split()[1].ToLower();
        string nameString = message.Split()[0].ToLower();
        string[] splitMessage = message.Split();
        if (commandString == "!animal")
        {
            if (chatMessages.Count >= 8)
            {
                chatMessages.RemoveAt(7);
            }
            chatMessages.Insert(0, nameString + " " + GetRandomText());
        }
        else if (commandString == "!join" && !availablePlayers.Contains(chatName))
        {
            if (chatMessages.Count >= 8)
            {
                chatMessages.RemoveAt(7);
            }
            chatMessages.Insert(0, nameString + " has joined!");
        }
    }

    public void GeneratePassword(string link)
    {
        Application.OpenURL(link);
    }

    public void AddToLobby(string userName)
    {
        if (!lobby.Contains(userName))
        {
            lobby.Add(userName);
        }
    }

    public void AddRandomPlayerToLobby()
    {
        string userName = availablePlayers[Random.Range(0, availablePlayers.Count - 1)];
        if (!lobby.Contains(userName) && lobby.Count < maxPlayersInLobby)
        {
            lobby.Add(userName);
        }
    }

    public void RemoveFromLobby(string userName)
    {
        if (lobby.Contains(userName))
        {
            lobby.Remove(userName);
        }
    }

    private void OnApplicationQuit()
    {
        reader?.Close();
        twitchClient?.Close();
    }

    public void testDisconnect()
    {
        twitchClient?.Close();
        pressedConnect = false;
    }


}
