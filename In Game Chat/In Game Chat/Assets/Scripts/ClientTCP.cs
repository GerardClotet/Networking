using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class Message
{
    public string prefix;
    public string name_;
    public string message;
    public string finalofmsg;
    public List<string> current_users = new List<string>();
    public int n_users = 0;
}

public class ClientTCP : ClientBase
{
    #region Client_Socket_OBJ
    public class ClientOBJ
    {
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket socket = null;

        public bool endC = false;
    }
    #endregion

    #region Class_Variables
    [SerializeField]
    InputField inputField_text;

    string msg_to_send = string.Empty;
    string client_name = string.Empty;

    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    IPEndPoint ipep;


    private static ManualResetEvent recieveDone = new ManualResetEvent(false);
    private static ManualResetEvent connectDone = new ManualResetEvent(false);

    Dictionary<int, string> commands = new Dictionary<int, string>()
    {
        {1, "/ban"},
        {2, "/color_list"},
        {3, "/list"},
        {4,"/private" },
        {5,"/changename"},
        {6,"/mute" }
    };
    #endregion

    #region MainThread
    // Start is called before the first frame update
    public void Start() //We should create the several clients from here
    {
        GetComponent<ClientProgram>().closingAppEvent.AddListener(ExitClient);
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 26012);
    }

    public void StartClient()
    {
        //Only for testing
        msg_to_send = "connected";

        StartThreadingFunction(Client);
        
    }


    // Update is called once per frame
    void Update()
    {
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc = functionsToRunInMainThread.Peek();
            functionsToRunInMainThread.Dequeue();

            //Now run it;
            someFunc?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S))
            client.Close();
    }

    public void SetClientName(string s)
    {
        client_name = s;
    }
    public void ExitClient()
    {


        try
        {
            //Disconnecting
            Debug.Log("Disconnecting From server");
            Action Disconnecting = () => { logControl.LogText("Disconnecting from server", Color.black); };
            QueueMainThreadFunction(Disconnecting);
            client.Shutdown(SocketShutdown.Both);
        }
        catch (SystemException e)
        {
            Debug.LogWarning("Couldn't shutdown the server, socket already closed " + e);
        }


        try
        {
            client.Close();
        }
        catch (SystemException e)
        {
            Debug.Log("Couldn't Close socket" + e);
        }
        Action CloseSocket = () => { logControl.LogText("Socket Closed", Color.black); };
        QueueMainThreadFunction(CloseSocket);
    }
    #endregion

    #region ClientHandler
    void Client()
    {

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

        byte[] data = new byte[1024];


        //Send Connect Message to server
        client.BeginConnect(ipep, new AsyncCallback(ConnectCallback), client);
        
        connectDone.WaitOne();
        try { Send(client, msg_to_send,"MSG"); }
        catch (SystemException e)
        {
            Debug.LogWarning(e);
        }



        //TODO EXIT LOOP
        ClientOBJ obj = new ClientOBJ();
        obj.socket = client;

        //Recieve loop
        while (!obj.endC)
        {
            //starting point once the message is recieved
            recieveDone.Reset();

            if (obj.endC)
                break;

            client.BeginReceive(obj.buffer, 0, ClientOBJ.BufferSize, 0,
                new AsyncCallback(ReadCallback), obj);



            //Until the recieved isnt resolved the loop will stop here
            recieveDone.WaitOne();

        }

        Debug.Log("Exit recieving loop");
        try
        {
            client.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException e)
        {
            Debug.Log("Couldnt shutdown server" + e);
        }
        
        client.Close();


    }
    #endregion

    #region Callbacks
    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client_r = (Socket)ar.AsyncState;
            //HERE IS THE ERROR, WE MUST PROPERLY ENDCONNECTION FOR NOW IF WE USE THIS METHOD SERVER WILL ONLY ACCEPT ONE CLIENT,
            //Now we send with the connect request the user name data
            //byte[] byteData = Encoding.ASCII.GetBytes(client_name);
            //client_r.BeginSend(byteData, 0, byteData.Length, 0,
            //new AsyncCallback(SendCallback), client_r);

            client.EndConnect(ar);

            //Now the connect request ends
            Debug.Log("Socket connected to " + client_r.RemoteEndPoint.ToString());


            connectDone.Set();
        }
        catch(Exception e)
        {
            Action Errorconection = () => { logControl.LogText("Couldn't connect to server " + e, Color.black); };
            QueueMainThreadFunction(Errorconection);
            Debug.Log(e);
        }
    }
    void ReadCallback(IAsyncResult ar)
    {
        string content = string.Empty;

        ClientOBJ state = (ClientOBJ)ar.AsyncState;

    
        Socket handler = state.socket;
        int bytesRead = 0 ;

        bytesRead = handler.EndReceive(ar); //Peta AQUIII


        if (bytesRead >0)
        {
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

            if (content.IndexOf("__END") > -1)
            {
                Message msg = Deserialize(state.buffer);

                //handle incoming commands from other users
                if(msg.prefix != "MSG")
                {
                    CommandHandler(msg);
                    recieveDone.Set();
                }
                string s = msg.name_ + ": " + msg.message;

                Action RecieveMsg = () => { logControl.LogText(s, Color.black); };
                QueueMainThreadFunction(RecieveMsg);

                recieveDone.Set();
            }
            else
            {
                handler.BeginReceive(state.buffer, 0, ClientOBJ.BufferSize, 0, new AsyncCallback(ReadCallback), state);

            }

        }

        else //Server is disconnected
        {
            Action ServerDisconnect = () => { logControl.LogText("The server has shutet down", Color.black); };
            QueueMainThreadFunction(ServerDisconnect);
            state.endC = true;


            Debug.Log("Exit recieving loop");
            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException e)
            {
                Debug.Log("Couldnt shutdown server" + e);
            }

            client.Close();
        }
    }

    private void SendCallback(IAsyncResult ar)
    {
        // Retrieve the socket from the state object.  
        Socket client = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.  
        int bytesSent = client.EndSend(ar);
        Debug.Log(bytesSent + "bytes sent to server");
    }
    #endregion

    #region Serialize Functions
    byte[] Serialize(string message,string prefix)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(prefix);
        writer.Write(client_name);
        writer.Write(message);
        //temporal
        writer.Write(1);
        for(int i= 0; i<1; i++)
        {
            writer.Write(client_name);
        }
        writer.Write("__END"); //To check if the message is fully read
        byte[] b = stream.GetBuffer();
       // writer.Close();
        stream.Close();
        GC.SuppressFinalize(stream);
        return b;
    }

    Message Deserialize(byte[] data)
    {
        Message msg = new Message();
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        msg.prefix = reader.ReadString();
        msg.name_ = reader.ReadString();
        msg.message = reader.ReadString();
        msg.n_users = reader.ReadInt32();
        for(int i =0;i< msg.n_users;i++)
        {
            msg.current_users.Add(reader.ReadString());
        }

        //UpdateUsersLog
        Action UpdateUsersLog = () => {CheckUsersLog(msg.current_users);};
        QueueMainThreadFunction(UpdateUsersLog);

        msg.finalofmsg = reader.ReadString();

        Debug.Log(msg.n_users);

        //reader.Close();
        stream.Close();
        //GC.SuppressFinalize(stream);
        return msg;
    }

    void CheckUsersLog(List<string> current_active_users_list)
    {

        //list where the elements of users_log.secondaryList aren't in the current_active_users_list 
        List<string> to_delete_users = users_log.secondaryList.Except(current_active_users_list).ToList();

        //Old users are deleted to the users log
        foreach (string s in to_delete_users)
        {
            users_log.DeleteItem(s);
            Debug.Log("Deleted user " + s);
        }

        //list where the elements of the first list aren't in the second
        List<string> firstNotSecond = current_active_users_list.Except(users_log.secondaryList).ToList();

        //New users are added to the users log
        foreach (string s in firstNotSecond)
        {
            users_log.LogText(s, Color.black);
            Debug.Log("Added user " + s);

        }


        //users_log.textItems

    }
    #endregion

    #region SendMessage

    public void NewMessageToSend(string s)
    {
        //Check what is it

        //check here https://www.delftstack.com/howto/csharp/csharp-find-in-string/ for a more polite way

        string tmp = string.Empty;

        //if (s.StartsWith("/"))
        //{
        //    //  s.StartsWith
        //    //Analyze string & check if there are some prefixes there
        //    for (int i = 0; i < s.Length; i++)
        //    {
        //        tmp += s[i];

        //        switch (s)
        //        {
        //            case "/help":
        //                HelpCommand();
        //                return;
        //            case "/ban":
        //                //BanCommand();
        //                return;
        //            case "/color_list":
        //                return;
        //            case "/whisper":
        //                return;
        //            case "/changeName":
        //                return;
        //                //DO STUFF
        //        }
        //    }
        //}


        if(s.StartsWith("/help"))
        {
            HelpCommand();
            inputField_text.Select();
            inputField_text.text = "";
            return;
        }
        if(s.StartsWith("/ban"))
        {
            BanCommand(s);
            inputField_text.Select();
            inputField_text.text = "";
            return;
        }
        if (s.StartsWith("/private"))
        {
            WhisperCommand(s);
            inputField_text.Select();
            inputField_text.text = "";
            return;
        }




        Action MsgSended = () => { logControl.LogText(client_name + ": " + s, Color.black); };
        QueueMainThreadFunction(MsgSended);
        Send(client, s,"MSG");
        inputField_text.Select();
        inputField_text.text = "";
    }

    private void Send(Socket client, string msg, string prefix)
    {
        try
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Serialize(msg, prefix);
            // Begin sending the data to the remote device.  
            // client.Send(byteData);
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }
        catch (SystemException e)
        {
            Debug.Log(e);
            Action ConnectionLost = () => { logControl.LogText("Conection with the server Lost, please restart client", Color.grey); };
            QueueMainThreadFunction(ConnectionLost);
        }
    }

    #endregion

    #region Commands
    private void CommandHandler(Message msg)
    {
        switch (msg.prefix)
        {
            case "WHISPER":
                string s = msg.name_ + ": (whisper) " + msg.message;

                Action RecieveMsg = () => { logControl.LogText(s, Color.grey); };
                QueueMainThreadFunction(RecieveMsg);
                break;
        }
    }
    private void WhisperCommand(string s)
    {
        //divide the string from spaces into substrings
        string[] words = s.Split(' ');      

        if (words.Length >= 3)
        {
            //Maybe while iteratin this the handler threads modifys the list and has some excection
            for (int i = 0; i < users_log.secondaryList.Count; i++)
            {
                if (users_log.secondaryList[i] == words[1])
                {
                    string whisper_string= string.Empty;
                    for (int j =1; j < words.Length; j++)
                    {
                        whisper_string += words[j] + ' ';
                    }
                    //Send whisper message to server
                    Send(client, whisper_string, "WHISPER");

                }
            }
        }
        Action WHS_Msg = () => { logControl.LogText(s, Color.grey); };
        QueueMainThreadFunction(WHS_Msg);
    }

    void BanCommand(string s)
    {
        //divide the string from spaces into substrings
        string[] words = s.Split(' ');

        if (words.Length >= 2)
        {
            //Maybe while iteratin this the handler threads modifys the list and has some excection
            for (int i = 0; i < users_log.secondaryList.Count; i++)
            {
                if (users_log.secondaryList[i] == words[1])
                {
                    //Send bann message to server
                    Send(client, words[1], "BAN");
                }
            }
        }
    }

    void HelpCommand()
    {
        foreach(KeyValuePair<int,string> c in commands)
        {
            Action Commandhelper = () => { logControl.LogText(c.Value, Color.grey); };
            QueueMainThreadFunction(Commandhelper);
        }
    }
    #endregion



}
