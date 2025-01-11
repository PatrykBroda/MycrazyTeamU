using GFFAddons;
using Mirror;
using SQLite;
using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace uSurvival
{
    //for database
    public class banned_accounts
    {
        [PrimaryKey] // important for performance: O(log n) instead of O(n)
        public string account { get; set; }
        public bool banned { get; set; }
        public int failedLogin { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public string bannedBy { get; set; }
        public string reason { get; set; }
    }

    //for Game Control Panel
    public partial struct BannedAccounts
    {
        public string startDate;
        public string endDate;
        public string account;
        public string bannedBy;
        public string reasonBan;
    }

    public enum WorkMode { server, client, serverAndClient };
    public enum RegistrationError { none, accountLengthIsNotCorrect, passwordLengthIsNotCorrect, loginIsNotFree, emailIsNotFree };
    public enum LoginError { none, outdated, invalidAccountOrPassword, banned, alreadyLoggedIn };

    public partial class NetworkManagerSurvival
    {
        [Header("GFF Login Extended")]
        public int accountMinLength = 4;
        public int accountMaxLength = 16;
        public int passwordMinLength = 4;

        [Header("Settings : eMail")]
        [Tooltip("If enabled = email is used")] public bool verificationRequired;
        public bool emailRequiredForRegistration;
        [SerializeField] private string smtpServer = "smtp.gmail.com";
        [SerializeField] private int smtpPort = 25;
        [SerializeField] private string emailAdminLogin;
        [SerializeField] private string emailAdminPassword;

        [Header("If the player entered the password incorrectly")]
        [SerializeField] private bool useBanSystemByFailedLogins;
        [SerializeField] private int failedLoginAmount = 5;
        [HideInInspector] public int failedLogins = 0;
        [SerializeField] private int banTimeHours = 24;

        //Send Message to Email
        public async void SendMessageVerificationCode(string account, int code, string emailRecipient)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailAdminLogin);
                mail.To.Add(emailRecipient);
                mail.Subject = "Access to uMMORPG";
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                mail.Body = "Hello " + account + "\n \n" + "Your activation code is :  " + code;

                SmtpClient client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = smtpPort;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(emailAdminLogin, emailAdminPassword);

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                client.Send(mail);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            await Task.Yield();
        }
        public async void SendMessageRecovery(string account, int code, string emailRecipient)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(emailAdminLogin);
                mail.To.Add(emailRecipient);
                mail.Subject = "Restore access to uMMORPG";
                mail.BodyEncoding = System.Text.Encoding.UTF8;

                mail.Body = "To change the password for the account " + account + " , enter the Code " + code;

                SmtpClient client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = smtpPort;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(emailAdminLogin, emailAdminPassword);

                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

                client.Send(mail);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.ToString());
            }

            await Task.Yield();
        }

        public void OnStartServer_Registration()
        {
            NetworkServer.RegisterHandler<LoginExtendedMsg>(TryLogin, false);
            NetworkServer.RegisterHandler<RegistrationMsg>(AccountRegistration, false);
            NetworkServer.RegisterHandler<UpdateVerificationMsg>(UpdateVerification, false);
            NetworkServer.RegisterHandler<AccountRecoveryMsg>(AccountRecovery, false);
            NetworkServer.RegisterHandler<SetNewPasswordForAccountMsg>(SetNewPasswordForAccount, false);
        }

        private void TryLogin(NetworkConnection conn, LoginExtendedMsg message)
        {
            //correct version?
            if (message.version == Application.version)
            {
                Database.accounts accountInfo = Database.singleton.LoadAccountInfo(message.account);
                //validate account ?
                if (accountInfo != null)
                {
                    //account is banned ?
                    if (accountInfo.banned) SendLoginReplyMessage(conn, LoginError.banned, "", 0);
                    else if (CheckBannedAccount(message.account, conn) == false)
                    {
                        string hash = Utils.PBKDF2Hash(message.password, "at_least_16_byte" + message.account);

                        //check password
                        if (CheckPassword(message.account, accountInfo.password, hash, conn))
                        {
                            // not in lobby and not in world yet?
                            if (!lobby.ContainsValue(message.account) || !Player.onlinePlayers.Values.Any(p => p.account == message.account))
                                SendLoginReplyMessage(conn, LoginError.none, "", accountInfo.verification);
                            else SendLoginReplyMessage(conn, LoginError.alreadyLoggedIn, "", 0);
                        }
                        else SendLoginReplyMessage(conn, LoginError.invalidAccountOrPassword, "", 0);
                    }
                }
                else SendLoginReplyMessage(conn, LoginError.invalidAccountOrPassword, "", 0);
            }
            else SendLoginReplyMessage(conn, LoginError.outdated, "", 0);
        }

        private void AccountRegistration(NetworkConnection conn, RegistrationMsg message)
        {
            //check account length
            if (message.account.Length >= accountMinLength)
            {
                //check password length
                if (message.password.Length >= passwordMinLength)
                {
                    //account name is free ?
                    if (Database.singleton.CheckAccountAvailable(message.account) == false)
                    {
                        //email is free ?
                        if (!emailRequiredForRegistration || Database.singleton.CheckEmailAvailable(message.email) == false)
                        {
                            SendRegistrationReplyMessage(conn, RegistrationError.none);

                            int verificationCode = 0;
                            if (verificationRequired)
                            {
                                verificationCode = UnityEngine.Random.Range(10000, 99999);
                                SendMessageVerificationCode(message.account, verificationCode, message.email);
                            }

                            Database.singleton.Registration(message.account, message.password, message.email, verificationCode);
                        }
                        else SendRegistrationReplyMessage(conn, RegistrationError.emailIsNotFree);
                    }
                    else SendRegistrationReplyMessage(conn, RegistrationError.loginIsNotFree);
                }
                else SendRegistrationReplyMessage(conn, RegistrationError.passwordLengthIsNotCorrect);
            }
            else SendRegistrationReplyMessage(conn, RegistrationError.accountLengthIsNotCorrect);
        }

        private void UpdateVerification(NetworkConnection conn, UpdateVerificationMsg message)
        {
            Database.accounts accountInfo = Database.singleton.LoadAccountInfo(message.account);

            if (accountInfo != null)
            {
                if (!message.resend)
                {
                    if (message.verification == accountInfo.verification)
                    {
                        Database.singleton.UpdateVerification(message.account, 0);

                        //send reply to client
                        UpdateVerificationMsgtoClient reply = new UpdateVerificationMsgtoClient { verificationSucces = true };
                        conn.Send(reply);
                    }
                    else
                    {
                        //send reply to client
                        UpdateVerificationMsgtoClient reply = new UpdateVerificationMsgtoClient { verificationSucces = false };
                        conn.Send(reply);
                    }
                }
                else
                {
                    int temp = UnityEngine.Random.Range(10000, 99999);
                    Database.singleton.UpdateVerification(message.account, temp);

                    //send email
                    SendMessageVerificationCode(message.account, temp, accountInfo.email);
                }
            }
        }

        private void AccountRecovery(NetworkConnection conn, AccountRecoveryMsg message)
        {
            string account = Database.singleton.LoadAccountNameByEmail(message.email);

            //if email exists
            if (account != null)
            {
                //send reply to client
                AccountRecoveryMsgtoClient reply = new AccountRecoveryMsgtoClient { findEmail = true };
                conn.Send(reply);

                int tempCode = UnityEngine.Random.Range(10000, 99999);
                Database.singleton.UpdateRecoveryCode(account, tempCode);

                SendMessageRecovery(account, tempCode, message.email);
            }
            else
            {
                //send reply to client
                AccountRecoveryMsgtoClient reply = new AccountRecoveryMsgtoClient { findEmail = false };
                conn.Send(reply);
            }
        }

        private void SetNewPasswordForAccount(NetworkConnection conn, SetNewPasswordForAccountMsg message)
        {
            Database.accounts accountInfo = Database.singleton.LoadAccountInfoByEmail(message.email);

            //if email exists
            if (accountInfo != null)
            {
                if (message.code == accountInfo.recoveryCode)
                {
                    Database.singleton.UpdateAccountPassword(accountInfo.name, message.password);
                    //Database.singleton.UpdateAccountBanStateExtended(accountInfo.name, 0, DateTime.Now, "", "", false);

                    //send reply to client
                    SetNewPasswordForAccountMsgtoClient reply = new SetNewPasswordForAccountMsgtoClient { result = true };
                    conn.Send(reply);
                }
                else
                {
                    //send reply to client
                    SetNewPasswordForAccountMsgtoClient reply = new SetNewPasswordForAccountMsgtoClient { result = false };
                    conn.Send(reply);
                }
            }
        }

        private bool CheckPassword(string account, string passwordInDatabase, string tryPassword, NetworkConnection conn)
        {
            if (passwordInDatabase == tryPassword)
            {
                if (useBanSystemByFailedLogins && failedLogins > 0)
                {
                    //reset the number failed logins
                    Database.singleton.UpdateAccountBanStateExtended(account, 0, DateTime.Now, "", "", false);
                }

                return true;
            }
            else
            {
                if (useBanSystemByFailedLogins)
                {
                    if (failedLogins + 1 >= failedLoginAmount)
                    {
                        //Database.singleton.UpdateAccountBanStateOriginal(account, true);
                        Database.singleton.UpdateAccountBanStateExtended(account, failedLoginAmount, DateTime.Now.AddHours(banTimeHours), "system", "failed login", true);

                        //send reply to client
                        TryLoginMsgtoClient messageReply = new TryLoginMsgtoClient { error = LoginError.banned, endBanTime = DateTime.Now.AddHours(banTimeHours).ToShortTimeString(), verification = 0 };
                        conn.Send(messageReply);
                    }
                    else
                    {
                        Database.singleton.UpdateAccountBanStateExtended(account, failedLogins + 1, DateTime.Now, "", "", false);

                        //send reply to client
                        TryLoginMsgtoClient messageReply = new TryLoginMsgtoClient { error = LoginError.banned, endBanTime = "", verification = 0 };
                        conn.Send(messageReply);
                    }
                }

                return false;
            }
        }

        private bool CheckBannedAccount(string account, NetworkConnection conn)
        {
            //load account ban info
            banned_accounts bannedInfo = Database.singleton.LoadBannedAccount(account);

            if (bannedInfo == null) return false;
            else
            {
                failedLogins = bannedInfo.failedLogin;
                if (bannedInfo.banned == false) return false;
                else
                {
                    if (bannedInfo.endDate <= DateTime.Now)
                    {
                        Database.singleton.DeleteAccountFromBannedAccounts(account);

                        return false;
                    }
                    else
                    {
                        string dateEnd = bannedInfo.endDate.ToLongDateString() + " : " + bannedInfo.endDate.ToShortTimeString();
                        SendLoginReplyMessage(conn, LoginError.banned, dateEnd, 0);

                        return true;
                    }
                }
            }
        }

        private void SendRegistrationReplyMessage(NetworkConnection conn, RegistrationError reply)
        {
            RegistrationReplyMsgtoClient message = new RegistrationReplyMsgtoClient { reply = reply };
            conn.Send(message);
        }

        public void SendLoginReplyMessage(NetworkConnection conn, LoginError error, string dateEnd, int verification)
        {
            TryLoginMsgtoClient reply = new TryLoginMsgtoClient { error = error, endBanTime = dateEnd, verification = verification };
            conn.Send(reply);
        }

        public void Restart()
        {
            StopClient();
            state = NetworkState.Offline;
            Invoke("StartClient", 0);
        }

        public void LogOut()
        {
            PlayerPrefs.SetInt("RememberMe", 0);
            PlayerPrefs.SetString("LastLogin", "");
            PlayerPrefs.SetString("LastPass", "");

            // inputs
            ((NetworkAuthenticatorSurvival)authenticator).loginAccount = "";
            ((NetworkAuthenticatorSurvival)authenticator).loginPassword = "";

            StopClient();
            state = NetworkState.Offline;
            UILoginExtendedAutorization.singleton.Show();
            UILoginExtendedAutorization.singleton.ClientRegisterHandles();
            UILoginExtendedAutorization.singleton.ClientRegisterHandlesVkPlay();
        }

        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            System.Net.NetworkInformation.Ping pinger = null;
            try
            {
                pinger = new System.Net.NetworkInformation.Ping();
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }
            return pingable;
        }

        //for vk
        private IEnumerator GetRequest(string uri, NetworkConnection conn, string account)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Reply: " + pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("Reply: " + pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:

                        string temp = webRequest.downloadHandler.text;
                        if (temp.IndexOf("ok") > 0)
                        {
                            //validate account ?
                            if (!Database.singleton.CheckAccountAvailable(account))
                                Database.singleton.AddNewAccointForVK(account, "987654321");

                            TryLoginViaVKMsgtoClient reply = new TryLoginViaVKMsgtoClient { error = LoginError.none };
                            conn.Send(reply);
                        }
                        if (temp.IndexOf("gas_invalid_sign") > 0)
                        {
                            //Debug.Log("Неправильная подпись");
                        }
                        else if (temp.IndexOf("gas_invalid_user") > 0)
                        {
                            //Debug.Log("Юзер не найден");
                        }
                        else if (temp.IndexOf("gas_otp_error") > 0)
                        {
                            //Debug.Log("Параметр hash невалидный");
                        }
                        break;
                }
            }
        }
    }
}

