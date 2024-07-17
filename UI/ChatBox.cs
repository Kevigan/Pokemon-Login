using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBox : MonoBehaviour
{
    public int maxMessages = 25;

    [SerializeField] List<Message> messageList = new List<Message>();
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToChat("You pressed space key!");
        }
    }

    public void SendMessageToChat(string text)
    {
        if(messageList.Count >= maxMessages)
        {
            messageList.Remove(messageList[0]);
            print("Space");
        }
        Message newMessage = new Message();

        newMessage.text = text;

        messageList.Add(newMessage);
    }
}

[System.Serializable]
public class Message
{
    public string text;
}
