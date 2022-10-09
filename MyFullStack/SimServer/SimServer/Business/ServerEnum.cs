using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum RegisterType
{
    Phone,
    Mail,
}

public enum LoginType
{
    Phone,
    Mail,
    WX,
    QQ,
    Token,
}

public enum RegisterResult
{
    Success,
    Failed,
    AlreadyExist,
    WrongCode,
    Forbidden,
}

public enum LoginResult
{
    Success,
    Failed,
    WrongPwd,
    UserNotExist,
    TimeoutToken,
}

