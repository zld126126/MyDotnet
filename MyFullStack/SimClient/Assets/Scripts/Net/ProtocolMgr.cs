using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//所有的协议收发的一个单独类
public class ProtocolMgr
{
    /// <summary>
    /// 链接服务器的第一个请求
    /// </summary>
    public static void SecretRequest() 
    {
        MsgSecret msg = new MsgSecret();
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgSecret, (resmsg) =>
        {
            NetManager.Instance.SetKey(((MsgSecret)resmsg).Srcret);
            Debug.Log("获取密钥：" + ((MsgSecret)resmsg).Srcret);
        });
    }

    /// <summary>
    /// 注册协议提交
    /// </summary>
    /// <param name="registerType"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="code"></param>
    /// <param name="callback"></param>
    public static void Register(RegisterType registerType, string userName, string password, string code, Action<RegisterResult> callback) 
    {
        MsgRegister msg = new MsgRegister();
        msg.RegisterType = registerType;
        msg.Account = userName;
        msg.Password = password;
        msg.Code = code;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgRegister, (resmsg) =>
        {
            MsgRegister msgRegister = (MsgRegister)resmsg;
            callback(msgRegister.Result);
        });
    }

    /// <summary>
    /// 登录协议的提交
    /// </summary>
    /// <param name="loginType"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="callback"></param>
    public static void Login(LoginType loginType,string userName,string password, Action<LoginResult,string> callback) 
    {
        MsgLogin msg = new MsgLogin();
        msg.Account = userName;
        msg.Password = password;
        msg.LoginType = loginType;
        NetManager.Instance.SendMessage(msg);
        NetManager.Instance.AddProtoListener(ProtocolEnum.MsgLogin,(resmsg)=> 
        {
            MsgLogin msgLogin = (MsgLogin)resmsg;
            callback(msgLogin.Result, msgLogin.Token);
        });
    }
}
