using System;

namespace uSurvival
{
    public partial class Database
    {
        public bool CheckAccountAvailable(string account)
        {
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", account) != null;
        }

        public bool CheckEmailAvailable(string email)
        {
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE email=?", email) != null;
        }

        public void Registration(string account, string password, string email, int verification)
        {
            string hash = Utils.PBKDF2Hash(password, "at_least_16_byte" + account);

            connection.Insert(new accounts
            {
                name = account,
                password = hash,
                created = DateTime.UtcNow,
                lastlogin = DateTime.UtcNow
                //email = email,
                //verification = verification
            });
        }

        public void UpdateVerification(string account, int code)
        {
            connection.Execute("UPDATE accounts SET verification=? WHERE name=?", code, account);
        }

        public void UpdateRecoveryCode(string account, int code)
        {
            connection.Execute("UPDATE accounts SET recoveryCode=? WHERE name=?", code, account);
        }

        public void UpdateAccountPassword(string account, string password)
        {
            string hash = Utils.PBKDF2Hash(password, "at_least_16_byte" + account);
            connection.Execute("UPDATE accounts SET password=? WHERE name=?", hash, account);
        }

        public string LoadAccountNameByEmail(string email)
        {
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE email=?", email).name;
        }

        public accounts LoadAccountInfoByEmail(string email)
        {
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE email=?", email);
        }

        public accounts LoadAccountInfo(string accountname)
        {
            return connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE name=?", accountname);
        }

        public banned_accounts LoadBannedAccount(string account)
        {
            return connection.FindWithQuery<banned_accounts>("SELECT * FROM banned_accounts WHERE account =?", account);
        }

        public void UpdateAccountBanStateExtended(string account, int logins, DateTime timeEnd, string bannedBy, string reason, bool banned)
        {
            connection.InsertOrReplace(new banned_accounts
            {
                account = account,
                failedLogin = logins,
                startDate = DateTime.Now,
                endDate = timeEnd,
                bannedBy = bannedBy,
                reason = reason,
                banned = banned
            });
        }

        public void DeleteAccountFromBannedAccounts(string account)
        {
            connection.Execute("DELETE FROM banned_accounts WHERE account=?", account);
        }

        public void AddNewAccointForVK(string account, string password)
        {
            string hash = Utils.PBKDF2Hash(password, "at_least_16_byte" + account);
            connection.Insert(new accounts { name = account, password = hash, created = DateTime.UtcNow, lastlogin = DateTime.Now, banned = false });
        }
    }
}