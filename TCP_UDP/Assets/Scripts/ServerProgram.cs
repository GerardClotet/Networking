using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
public class ServerProgram : MonoBehaviour
{

    protected Socket _socket;
    protected IPEndPoint ipep;

    protected Queue<Action> functionsToRunInMainThread = new Queue<Action>();
    protected string CurrentLog;
    [SerializeField]
    List<GameObject> UI_to_hide;

    [SerializeField]
    GameObject starterPanel;

    [SerializeField]
    protected TextLogControl logControl;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(false);
        }
    } 


    public void StartUDPServer()
    {
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ServerUDP>().enabled = true;

    }
    public void StartTCPServer()
    {
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ServerTCP>().enabled = true;

    }

    protected void startThreadingFunction(Action someFunction)
    {
        Thread t = new Thread(someFunction.Invoke);
        t.Start();

    }

    public void QueueMainThreadFunction(Action someFunction)
    {
        //We need to make sure that some function is running from the main Thread

        //someFunction(); //This isn't okay, if we're in a child thread
        functionsToRunInMainThread.Enqueue(someFunction);
    }
}
