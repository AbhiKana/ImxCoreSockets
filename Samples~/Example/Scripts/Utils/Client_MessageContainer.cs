using System.Collections.Generic;

[System.Serializable]
public class Client_MessageContainer
{
    public int SrNo;
    public string IP;
    public string Message;
}

public class MessageList
{
    public List<Client_MessageContainer> messages = new List<Client_MessageContainer>();
}

