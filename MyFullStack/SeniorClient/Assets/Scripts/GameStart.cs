using Newtonsoft.Json;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private bool isConnect = false;
    public string IP = "127.0.0.1";
    public string PORT = "10188";
    public string Message = "我是Hello!";
    
    // Start is called before the first frame update
    void Awake()
    {
        EventAdd();
    }

    // Update is called once per frame
    void Update()
    {
        if (isConnect)
        {
            NetManager.Instance.Update();
            
            if (Input.GetKeyDown(KeyCode.A))
            {
                string data = "我是1!-client";
                Debug.LogError("发送数据:"+data.ToString());
                NetManager.Instance.SendMessage(data);
            }

            if (Input.GetKeyDown(KeyCode.B)) 
            {
                MsgCSharp data = new MsgCSharp();
                data.ReqContent = "我是2!-client";
                Debug.LogError("发送数据:"+data.ToString());
                NetManager.Instance.SendMessage(data);
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                MsgLua lua = new MsgLua();
                lua.LuaJson = "{\"name\":\"lucy\",\"age\":18,\"love\":\"写代码\"}";
                NetManager.Instance.SendMessage(lua);
            }
        }
    }
    
    void OnGUI()
    {
        GUI.Label(new Rect(50, 50, 120, 30), "Ip:",GetGUIStyle(20,Color.green));
        IP = GUI.TextField(new Rect(180, 50, 120, 30), IP, 20,GetGUIStyle(20,Color.yellow));
        
        GUI.Label(new Rect(50, 100, 120, 30), "Port:",GetGUIStyle(20,Color.green));
        PORT = GUI.TextField(new Rect(180, 100, 120, 30), PORT,20,GetGUIStyle(20,Color.yellow));
        
        GUI.Label(new Rect(50, 150, 120, 30), "发送消息:",GetGUIStyle(20,Color.green));
        Message = GUI.TextField(new Rect(180, 150, 120, 60), Message,20,GetGUIStyle(20,Color.yellow));
        
        if (GUI.Button(new Rect(50, 240, 250, 100), "连接Socket"))
        {
            NetManager.Instance.Connect(IP, int.Parse(PORT));
        }
        
        if (GUI.Button(new Rect(50, 360, 250, 100), "发送Socket消息"))
        {
            if (isConnect)
            {
                Debug.LogError("发送数据:"+Message.ToString());
                NetManager.Instance.SendMessage(Message);
            }
        }

        GUI.TextArea(new Rect(400, 100, 500, 300), @"请执行以下步骤：
1.默认链接地址:127.0.0.1:10188
2.连接Socket
3.发送Socket消息
4.按下键盘A键:
发送一条string字符串消息给服务端
5.按下键盘B键:
发送一条proto包装消息给服务端
6.按下键盘C键:
发送一条json-string给服务端
", GetGUIStyle(20,Color.red));
    }

    private void OnApplicationQuit()
    {
        NetManager.Instance.Close();
        EventRemove();
    }

    private GUIStyle GetGUIStyle(int fontSize,Color color)
    {
        GUIStyle style = GUI.skin.textArea;
        style.normal.textColor = color;
        style.fontSize = fontSize;
        return style;
    }
    
    void EventAdd()
    {
        EventCenter.AddListener(EventDefine.SocketConnectSuccess, SocketConnectSuccess_CallBack);
        EventCenter.AddListener(EventDefine.SocketConnectFail, SocketConnectFail_CallBack);
        EventCenter.AddListener(EventDefine.SocketSendMessageSuccess, SocketSendMessageSuccess_CallBack);
        EventCenter.AddListener(EventDefine.SocketSendMessageFail, SocketSendMessageFail_CallBack);
        EventCenter.AddListener(EventDefine.SocketCloseSuccess, SocketCloseSuccess_CallBack);
        EventCenter.AddListener(EventDefine.SocketCloseFail, SocketCloseFail_CallBack);
    }
    
    void EventRemove()
    {
        EventCenter.RemoveListener(EventDefine.SocketConnectSuccess, SocketConnectSuccess_CallBack);
        EventCenter.RemoveListener(EventDefine.SocketConnectFail, SocketConnectFail_CallBack);
        EventCenter.RemoveListener(EventDefine.SocketSendMessageSuccess, SocketSendMessageSuccess_CallBack);
        EventCenter.RemoveListener(EventDefine.SocketSendMessageFail, SocketSendMessageFail_CallBack);
        EventCenter.RemoveListener(EventDefine.SocketCloseSuccess, SocketCloseSuccess_CallBack);
        EventCenter.RemoveListener(EventDefine.SocketCloseFail, SocketCloseFail_CallBack);
    }
    
    void SocketConnectSuccess_CallBack() {
        Debug.Log("连接Socket成功");
        isConnect = true;
    }
    
    void SocketConnectFail_CallBack() {
        Debug.Log("连接Socket失败");
        isConnect = false;
    }
    
    void SocketSendMessageSuccess_CallBack() {
        Debug.Log("发送Socket消息成功");
    }
    
    void SocketSendMessageFail_CallBack() {
        Debug.Log("发送Socket消息失败");
    }
    
    void SocketCloseSuccess_CallBack() {
        Debug.Log("关闭Socket成功");
        isConnect = false;
    }
    
    void SocketCloseFail_CallBack() {
        Debug.Log("关闭Socket失败");
    }
    
}
