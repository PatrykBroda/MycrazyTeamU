using Mirror;
using uSurvival;

// client to server ////////////////////////////////////////////////////////////
public struct LoginExtendedMsg : NetworkMessage
{
    public string account;
    public string password;
    public string version;
}
public struct RegistrationMsg : NetworkMessage
{
    public string account;
    public string password;
    public string email;
}
public struct UpdateVerificationMsg : NetworkMessage
{
    public string account;
    public int verification;
    public bool resend;
}
public struct AccountRecoveryMsg : NetworkMessage { public string email; }
public struct SetNewPasswordForAccountMsg : NetworkMessage { public int code; public string email; public string password; }

// server to client ////////////////////////////////////////////////////////////
public struct RegistrationReplyMsgtoClient : NetworkMessage { public RegistrationError reply; }
public struct TryLoginMsgtoClient : NetworkMessage
{
    public LoginError error;
    public int verification;

    //for banned system
    public string endBanTime;
}
public struct UpdateVerificationMsgtoClient : NetworkMessage { public bool verificationSucces; }
public struct AccountRecoveryMsgtoClient : NetworkMessage { public bool findEmail; }
public struct SetNewPasswordForAccountMsgtoClient : NetworkMessage { public bool result; }