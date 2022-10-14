using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public string IP = "127.0.0.1";
    public string PORT = "8011";
    public string Message = "这是1条测试消息!!";
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!NetManager.Instance.IsConnect())
        {
            return;
        }
        
        NetManager.Instance.Update();

        if (Input.GetKeyDown(KeyCode.A))
        {
            SendSocketTest();
        }

        if (Input.GetKeyDown(KeyCode.R)) 
        {
            ProtocolMgr.Register(RegisterType.Phone, "16619759365", "Dong", "123456",(res) =>
            {
                if (res == RegisterResult.AlreadyExist)
                {
                    Debug.LogError("该手机号已经注册过了");
                }
                else if (res == RegisterResult.WrongCode) 
                {
                    Debug.LogError("验证码错误");
                }
                else if (res == RegisterResult.Forbidden)
                {
                    Debug.LogError("改账户禁止铸错，联系客服！");
                }
                else if (res == RegisterResult.Success)
                {
                    Debug.Log("注册成功");
                }
            });
        }

        if (Input.GetKeyDown(KeyCode.L)) 
        {
            ProtocolMgr.Login( LoginType.Phone, "16619759365", "Dong",(res,restoken)=> 
            {
                if (res == LoginResult.Success)
                {
                    Debug.Log("登录成功");
                }
                else if (res == LoginResult.Failed) 
                {
                    Debug.LogError("登录失败");
                }
                else if (res == LoginResult.WrongPwd)
                {
                    Debug.LogError("密码错误");
                }
                else if (res == LoginResult.UserNotExist)
                {
                    Debug.LogError("用户不存在");
                }
            });
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
            StartCoroutine(NetManager.Instance.CheckNet());
        }
        
        if (GUI.Button(new Rect(50, 360, 250, 100), "发送Socket消息"))
        {
            if (NetManager.Instance.IsConnect())
            {
                SendSocketTest();
            }
        }

        GUI.TextArea(new Rect(400, 100, 500, 300), @"请执行以下步骤：
1.默认链接地址:127.0.0.1:8011
2.连接Socket
3.发送Socket消息
4.按下键盘T键:
发送一条测试消息给服务端
5.按下键盘R键:
发送一条注册消息给服务端
6.按下键盘L键:
发送一条登录消息给服务端
", GetGUIStyle(20,Color.red));
    }
    
    private GUIStyle GetGUIStyle(int fontSize,Color color)
    {
        GUIStyle style = GUI.skin.textArea;
        style.normal.textColor = color;
        style.fontSize = fontSize;
        return style;
    }

    private void SendSocketTest()
    {
        MsgTest msg = new MsgTest();
        msg.ReqContent = Message;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgTest, (resmsg) =>
        {
            Debug.Log("测试回调：" + ((MsgTest)resmsg).RecContent);
        });
    }

    private void OnApplicationQuit()
    {
        NetManager.Instance.Close();
    }
}
