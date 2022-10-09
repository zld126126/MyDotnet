using ProtoBuf;

[ProtoContract]
public class MsgSecret : MsgBase 
{
    //每一个协议类必然包含构造函数来确定当前协议类型，并且都有ProtoType进行序列化标记
    public MsgSecret() 
    {
        ProtoType = ProtocolEnum.MsgSecret;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)]
    public string Srcret;
}

[ProtoContract]
public class MsgPing : MsgBase
{
    public MsgPing()
    {
        ProtoType = ProtocolEnum.MsgPing;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }
}

[ProtoContract]
public class MsgTest : MsgBase
{
    public MsgTest()
    {
        ProtoType = ProtocolEnum.MsgTest;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)]
    public string ReqContent { get; set; }

    [ProtoMember(3)]
    public string RecContent { get; set; }
}