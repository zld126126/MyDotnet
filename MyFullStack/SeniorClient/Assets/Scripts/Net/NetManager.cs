using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class NetManager : Singleton<NetManager>
{
    private Socket m_Socket;
    private ByteArray m_ReadBuff;
    private string m_Ip;
    private int m_Port;

    //链接状态
    private bool m_Connecting = false;
    private bool m_Closing = false;

    private Thread m_MsgThread;

    private Queue<ByteArray> m_WriteQueue;

    private List<MsgBase> m_MsgList;
    private List<MsgBase> m_UnityMsgList;
    //消息列表长度
    private int m_MsgCount = 0;
    
    public delegate void ProtoListener(MsgBase msg);
    private Dictionary<ProtocolEnum, ProtoListener> m_ProtoDic = new Dictionary<ProtocolEnum, ProtoListener>();

    private bool m_Diaoxian = false;
    //是否链接成功过（只要链接成功过就是true，再也不会变成false）
    private bool m_IsConnectSuccessed = false;
    private bool m_ReConnect = false;

    private NetworkReachability m_CurNetWork = NetworkReachability.NotReachable;

    public IEnumerator CheckNet()
    {
        m_CurNetWork = Application.internetReachability;
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (m_Socket == null && !m_Socket.Connected)
            {
                if (m_IsConnectSuccessed)
                {
                    if (m_CurNetWork != Application.internetReachability)
                    {
                        ReConnect();
                        m_CurNetWork = Application.internetReachability;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 一个协议希望只有一个监听
    /// </summary>
    /// <param name="protocolEnum"></param>
    /// <param name="listener"></param>
    public void AddProtoListener(ProtocolEnum protocolEnum,ProtoListener listener) 
    {
        m_ProtoDic[protocolEnum] = listener;
    }

    public void FirstProto(ProtocolEnum protocolEnum,MsgBase msgBase) 
    {
        if (m_ProtoDic.ContainsKey(protocolEnum))
        {
            m_ProtoDic[protocolEnum](msgBase);
        }
    }

    public void Update() 
    {
        if (m_Diaoxian && m_IsConnectSuccessed) 
        {
            //弹框，确定是否重连
            //重新链接
            ReConnect();
            //退出游戏
            m_Diaoxian = false;
        }

        //断开链接后，链接服务器之后自动登录
        if (m_Socket.Connected && m_ReConnect) 
        {
            //在本地保存了我们的账户和token，然后进行判断有无账户和token，

            //使用token登录
            //ProtocolMgr.Login( LoginType.Token, "username", "token",(res, restoken)=> 
            //{
            //    if (res == LoginResult.Success)
            //    {

            //    }
            //    else 
            //    {

            //    }
            //});
            m_ReConnect = false;
        }

        MsgUpdate();
    }

    void MsgUpdate()
    {
        if (m_Socket != null && m_Socket.Connected)
        {
            if (m_MsgCount == 0) return;
            MsgBase msgBase = null;
            lock (m_UnityMsgList) 
            {
                if (m_UnityMsgList.Count > 0) 
                {
                    msgBase = m_UnityMsgList[0];
                    m_UnityMsgList.RemoveAt(0);
                    m_MsgCount--;
                }
            }
            if (msgBase != null)
            {
                FirstProto(msgBase.ProtoType, msgBase);
            }
        }
    }

    void MsgThread()
    {
        while (m_Socket != null && m_Socket.Connected) 
        {
            if (m_MsgList.Count <= 0) continue;

            MsgBase msgBase = null;
            lock (m_MsgList) 
            {
                if (m_MsgList.Count > 0) 
                {
                    msgBase = m_MsgList[0];
                    m_MsgList.RemoveAt(0);
                }
            }

            if (msgBase != null)
            {
                lock (m_UnityMsgList) 
                {
                    m_UnityMsgList.Add(msgBase);
                }
            }
            else 
            {
                break;
            }
        }
    }

    /// <summary>
    /// 重连方法
    /// </summary>
    public void ReConnect() 
    {
        Connect(m_Ip, m_Port);
        m_ReConnect = true;
    }

    /// <summary>
    /// 链接服务器函数
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public void Connect(string ip, int port) 
    {
        if (m_Socket != null && m_Socket.Connected) 
        {
            Debug.LogError("链接失败，已经链接了！");
            return;
        }

        if (m_Connecting)
        {
            Debug.LogError("链接失败，正在链接中！");
            return;
        }
        InitState();
        m_Socket.NoDelay = true;
        m_Connecting = true;
        m_Socket.BeginConnect(ip, port, ConnectCallback, m_Socket);
        m_Ip = ip;
        m_Port = port;
    }

    /// <summary>
    /// 初始化状态
    /// </summary>
    void InitState() 
    {
        //初始化变量
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_ReadBuff = new ByteArray();
        m_WriteQueue = new Queue<ByteArray>();
        m_Connecting = false;
        m_Closing = false;
        m_MsgList = new List<MsgBase>();
        m_UnityMsgList = new List<MsgBase>();
        m_MsgCount = 0;
    }

    /// <summary>
    /// 链接回调
    /// </summary>
    /// <param name="ar"></param>
    void ConnectCallback(IAsyncResult ar) 
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            m_IsConnectSuccessed = true;
            m_MsgThread = new Thread(MsgThread);
            m_MsgThread.IsBackground = true;
            m_MsgThread.Start();
            m_Connecting = false;
            Debug.Log("Socket Connect Success");
            
            m_Socket.BeginReceive(m_ReadBuff.Bytes,m_ReadBuff.WriteIdx,m_ReadBuff.Remain,0, ReceiveCallBack, socket);
            EventCenter.Broadcast(EventDefine.SocketConnectSuccess);
        }
        catch (SocketException ex) 
        {
            Debug.LogError("Socket Connect fail:" + ex.ToString());
            m_Connecting = false;
            EventCenter.Broadcast(EventDefine.SocketConnectFail);
        }
    }


    /// <summary>
    /// 接受数据回调
    /// </summary>
    /// <param name="ar"></param>
    void ReceiveCallBack(IAsyncResult ar) 
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if (count <= 0) 
            {
                Close();
                //关闭链接
                return;
            }

            m_ReadBuff.WriteIdx += count;
            OnReceiveData();
            if (m_ReadBuff.Remain < 8) 
            {
                m_ReadBuff.MoveBytes();
                m_ReadBuff.ReSize(m_ReadBuff.Length * 2);
            }
            socket.BeginReceive(m_ReadBuff.Bytes, m_ReadBuff.WriteIdx, m_ReadBuff.Remain, 0, ReceiveCallBack, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError("Socket ReceiveCallBack fail:" + ex.ToString());
            Close();
        }
    }

    /// <summary>
    /// 对数据进行处理
    /// </summary>
    void OnReceiveData() 
    {
        if (m_ReadBuff.Length <= 2 || m_ReadBuff.ReadIdx < 0)
            return;
        
        int readIdx = m_ReadBuff.ReadIdx;
        byte[] bytes = m_ReadBuff.Bytes;
        
        //读取协议长度之后进行判断，如果消息长度小于读出来的消息长度，证明是没有一条完整的数据
        int bodyLength = BitConverter.ToUInt16(bytes, readIdx);
        if (m_ReadBuff.Length < bodyLength + 2) 
        {
            return;
        }

        string msg = Encoding.UTF8.GetString(m_ReadBuff.Bytes, 2, m_ReadBuff.WriteIdx);
        Debug.LogError("收到数据:"+msg);

        m_ReadBuff.ReadIdx += 2;
        int nameCount = 0;
        ProtocolEnum protocol = MsgBase.DecodeName(m_ReadBuff.Bytes, m_ReadBuff.ReadIdx, out nameCount);
        if (protocol == ProtocolEnum.None) 
        {
            Debug.LogError("OnReceiveData MsgBase.DecodeName fail");
            Close();
            return;
        }

        m_ReadBuff.ReadIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        try
        {
            MsgBase msgBase = MsgBase.Decode(protocol, m_ReadBuff.Bytes, m_ReadBuff.ReadIdx, bodyCount);
            if (msgBase == null) 
            {
                Debug.LogError("接受数据协议内容解析出错");
                Close();
                return;
            }
            m_ReadBuff.ReadIdx += bodyCount;
            m_ReadBuff.CheckAndMoveBytes();
            //协议具体的操作
            lock (m_MsgList) 
            {
                m_MsgList.Add(msgBase);
                Debug.Log(msgBase.ToString());
            }
            m_MsgCount++;
            //处理粘包
            if (m_ReadBuff.Length > 2) 
            {
                OnReceiveData();
            }
        }
        catch (Exception ex) 
        {
            Debug.LogError("Socket OnReceiveData error:" + ex.ToString());
            Close();
        }
    }

    /// <summary>
    /// 发送数据到服务器
    /// </summary>
    /// <param name="msg"></param>
    public void SendMessage(string msg)
    {
        if (m_Socket == null || !m_Socket.Connected) 
        {
            return;
        }

        if (m_Connecting) 
        {
            Debug.LogError("正在链接服务器中，无法发送消息！");
            return;
        }

        if (m_Closing) 
        {
            Debug.LogError("正在关闭链接中，无法发送消息!");
            return;
        }
        
        try
        {
            byte[] dataBuff = System.Text.Encoding.UTF8.GetBytes(msg);
            byte[] lenBuff = BitConverter.GetBytes((ushort) dataBuff.Length);
            if (!BitConverter.IsLittleEndian)
            {
                lenBuff.Reverse();
            }
            byte[] sendBytes = lenBuff.Concat(dataBuff).ToArray();
            ByteArray ba = new ByteArray(sendBytes);
            
            int count = 0;
            lock (m_WriteQueue) 
            {
                m_WriteQueue.Enqueue(ba);
                count = m_WriteQueue.Count;
            }

            if (count == 1) 
            {
                m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, m_Socket);
                EventCenter.Broadcast(EventDefine.SocketSendMessageSuccess);
            }
        }
        catch (Exception ex) 
        {
            Debug.LogError("SendMessage error:" + ex.ToString());
            EventCenter.Broadcast(EventDefine.SocketSendMessageFail);
            Close();
        }
    }
    
    /// <summary>
    /// 发送数据到服务器
    /// </summary>
    /// <param name="msgBase"></param>
    public void SendMessage(MsgBase msgBase) 
    {
        if (m_Socket == null || !m_Socket.Connected) 
        {
            return;
        }

        if (m_Connecting) 
        {
            Debug.LogError("正在链接服务器中，无法发送消息！");
            return;
        }

        if (m_Closing) 
        {
            Debug.LogError("正在关闭链接中，无法发送消息!");
            return;
        }

        try
        {
            byte[] nameBytes = MsgBase.EncodeName(msgBase);
            byte[] bodyBytes = MsgBase.Encode(msgBase);
            byte[] dataBuff = nameBytes.Concat(bodyBytes).ToArray();

            byte[] lenBuff = BitConverter.GetBytes((ushort) dataBuff.Length);
            if (!BitConverter.IsLittleEndian)
            {
                lenBuff.Reverse();
            }
            byte[] sendBytes = lenBuff.Concat(dataBuff).ToArray();
            ByteArray ba = new ByteArray(sendBytes);
            
            int count = 0;
            lock (m_WriteQueue) 
            {
                m_WriteQueue.Enqueue(ba);
                count = m_WriteQueue.Count;
            }

            if (count == 1) 
            {
                m_Socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, m_Socket);
            }
        }
        catch (Exception ex) 
        {
            Debug.LogError("SendMessage error:" + ex.ToString());
            Close();
        }
    }

    /// <summary>
    /// 发送结束回调
    /// </summary>
    /// <param name="ar"></param>
    void SendCallBack(IAsyncResult ar) 
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            if (socket == null || !socket.Connected) return;
            int count = socket.EndSend(ar);
            //判断是否发送完成
            ByteArray ba;
            lock (m_WriteQueue) 
            {
                ba = m_WriteQueue.First();
            }
            ba.ReadIdx += count;
            //代表发送完整
            if (ba.Length == 0) 
            {
                lock (m_WriteQueue) 
                {
                    m_WriteQueue.Dequeue();
                    if (m_WriteQueue.Count > 0)
                    {
                        ba = m_WriteQueue.First();
                    }
                    else 
                    {
                        ba = null;
                    }
                }
            }

            //发送不完整或发送完整且存在第二条数据
            if (ba != null)
            {
                socket.BeginSend(ba.Bytes, ba.ReadIdx, ba.Length, 0, SendCallBack, socket);
            }
            //确保关闭链接前，先把消息发送出去
            else if (m_Closing) 
            {
                RealClose();
            }
        }
        catch (SocketException ex) 
        {
            Debug.LogError("SendCallBack error:" + ex.ToString());
            Close();
        }
    }

    /// <summary>
    /// 关闭链接
    /// </summary>
    /// <param name="normal"></param>
    public void Close(bool normal = true) 
    {
        if (m_Socket == null || m_Connecting) 
        {
            return;
        }

        if (m_Connecting) return;

        if (m_WriteQueue.Count > 0)
        {
            m_Closing = true;
        }
        else 
        {
            RealClose(normal);
        }
    }

    void RealClose(bool normal = true) 
    {
        m_Socket.Close();
        m_Diaoxian = true;
        if (m_MsgThread != null && m_MsgThread.IsAlive)
        {
            m_MsgThread.Abort();
            m_MsgThread = null;
        }
        Debug.Log("Close Socket");
        EventCenter.Broadcast(EventDefine.SocketCloseSuccess);
    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}
