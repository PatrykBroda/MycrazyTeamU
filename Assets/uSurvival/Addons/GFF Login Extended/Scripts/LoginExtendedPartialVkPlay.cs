using Mirror;
using UnityEngine;
using uSurvival;

public struct LoginExtendedViaVKPlayMsg : NetworkMessage
{
    public int id;
    public string OTPhash;
    public string version;
}
public struct TryLoginViaVKMsgtoClient : NetworkMessage
{
    public LoginError error;
}

namespace uSurvival
{
    public partial class NetworkManagerSurvival
    {
        public void OnStartServer_RegistrationVkPlay()
        {
            NetworkServer.RegisterHandler<LoginExtendedViaVKPlayMsg>(TryLogin, false);
        }

        private void TryLogin(NetworkConnectionToClient conn, LoginExtendedViaVKPlayMsg message)
        {
            Debug.Log("Try login via vk " + conn.address);

            //correct version?
            if (message.version == Application.version)
            {
                int uid = message.id;
                string hash = message.OTPhash;
                string sign = string.Concat("appid=21095hash=", hash, "ip=", conn.address, "uid=", uid, "1ut5Z5tOEVVeQ7rQ");
                string md5sign = CommandLineReaderVK.CreateMD5(sign);
                string url = string.Concat("https://vkplay.ru/app/21095/gas?uid=", uid, "&hash=", hash, "&ip=", conn.address, "&sign=", md5sign);
                StartCoroutine(GetRequest(url, conn, uid.ToString()));
            }
            else
            {
                TryLoginViaVKMsgtoClient reply = new TryLoginViaVKMsgtoClient { error = LoginError.outdated };
                conn.Send(reply);
            }
        }
    }
}

namespace GFFAddons
{
    public partial class UILoginExtendedAutorization
    {
        public void ClientRegisterHandlesVkPlay()
        {
            NetworkClient.RegisterHandler<TryLoginViaVKMsgtoClient>(ReceiveMsgTryLoginViaVk, false);
        }

        private void ReceiveMsgTryLoginViaVk(TryLoginViaVKMsgtoClient message)
        {
            if (message.error == LoginError.none)
            {
                CancelInvoke();

                // inputs
                auth.loginAccount = vkId.ToString();
                auth.loginPassword = "987654321";

                if (NetworkClient.active)
                {
                    NetworkClient.Disconnect();
                    NetworkClient.Shutdown();
                }

                manager.StartClient();
            }
            else textDebug.text = "Авторизация не Успешна / " + message.error;
        }
    }
}