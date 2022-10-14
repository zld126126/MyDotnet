using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class MsgCSharp : MsgBase
{
    public MsgCSharp()
    {
        ProtoType = ProtocolEnum.MsgCSharp;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)]
    public string ReqContent { get; set; }

    [ProtoMember(3)]
    public string RecContent { get; set; }
}

[ProtoContract]
public class MsgLua : MsgBase
{
    public MsgLua()
    {
        ProtoType = ProtocolEnum.MsgLua;
    }

    [ProtoMember(1)]
    public override ProtocolEnum ProtoType { get; set; }

    [ProtoMember(2)] public string LuaJson { get; set; }
}