using System;
using System.Collections.Generic;
using ConsoleApp3.model;
using DemoSession2.entity;
using DemoSession2.error;
using DemoSession2.untility;
using MySql.Data.MySqlClient;
using Transaction = DemoSession2.entity.Transaction;

namespace DemoSession2.model
{
    public class AccountModel
    {
        public Boolean Save(Account account)
        {
            DbConnection.Instance().OpenConnection(); // dam bao rang da ket noi den database thanh cong.
            var salt = Hash.RandomString(7); // tao muoi random
            account.Salt = salt; // dua muoi vao thuoc tinh cua accountde luu vao database.
            account.Password = Hash.GenerateSaltedSHA1(account.Password, account.Salt);
            var sqlQuery = "insert into `account`" +
                           "(`username`,`password`,`accountNumber`,`identityCard`,`balance`,`phone`,`fullName`,`email`,`salt`) values" +
                           "(@username,@password,@accountNumber,@identityCard,@balance,@phone,@fullName,@email,@salt)";

            MySqlCommand cmd = new MySqlCommand(sqlQuery, DbConnection.Instance().Connection);

            cmd.Parameters.AddWithValue("@username", account.Username);
            cmd.Parameters.AddWithValue("@password", account.Password);
            cmd.Parameters.AddWithValue("@accountNumber", account.AccountNumber);
            cmd.Parameters.AddWithValue("@identityCard", account.IdentityCard);
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@phone", account.Phone);
            cmd.Parameters.AddWithValue("@fullName", account.FullName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@salt", account.Salt);
            var result = cmd.ExecuteNonQuery();
            DbConnection.Instance().CloseConnection();
            return result == 1;
        }

        // ham tra ve 1 accountNumber
        public Account GetAccountByAccountNumber(string accountNumber)
        {
            DbConnection.Instance().OpenConnection();
            string queryString = "select * from `account` where accountNumber = @accountNumber ";
            MySqlCommand cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            MySqlDataReader reader = cmd.ExecuteReader();
            Account account = null;
            if (reader.Read())
            {
                var username = reader.GetString("username");
                var password = reader.GetString("password");
                var salt = reader.GetString("salt");
                var _accountNumber = reader.GetString("accountNumber");
                var identityCard = reader.GetString("identityCard");
                var balance = reader.GetDecimal("balance");
                var phone = reader.GetString("phone");
                var email = reader.GetString("email");
                var fullName = reader.GetString("fullName");
                var createdAt = reader.GetString("createdAt");
                var updatedAt = reader.GetString("updatedAt");
                var status = reader.GetInt32("status");

                account = new Account(username, password, salt, _accountNumber, identityCard, balance, phone, email,
                    fullName, createdAt, updatedAt, (ActiveStatus) status);
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }

// Ham nay tra ve 1 Account.
        public Account GetAccountByUserName(string username)
        {
            DbConnection.Instance().OpenConnection();
            string queryString = "select * from `account` where username = @username";
            MySqlCommand cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            MySqlDataReader reader = cmd.ExecuteReader();
            Account account = null;
            if (reader.Read())
            {
                var _username = reader.GetString("username");
                var password = reader.GetString("password");
                var salt = reader.GetString("salt");
                var accountNumber = reader.GetString("accountNumber");
                var identityCard = reader.GetString("identityCard");
                var balance = reader.GetDecimal("balance");
                var phone = reader.GetString("phone");
                var email = reader.GetString("email");
                var fullName = reader.GetString("fullName");
                var createdAt = reader.GetString("createdAt");
                var updatedAt = reader.GetString("updatedAt");
                var status = reader.GetInt32("status");

                account = new Account(_username, password, salt, accountNumber, identityCard, balance, phone, email,
                    fullName, createdAt, updatedAt, (ActiveStatus) status);
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }

        public Boolean
            CheckExistUserName(string username) // ham check theo username va gia tri tra ve la true hoac false.
        {
            return false;
        }

        public bool UpdateBalance(Account account, Transaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection();
            // tao transaction
            var transaction = DbConnection.Instance().Connection.BeginTransaction();

            try
            {
                // Lay thong tin so du
                var queryBalance = "select balance from `account` where username = @username and status = @status";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                queryBalanceCommand.Parameters.AddWithValue("@status", account.Status);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // thow loi neu k ton tai ban ghi
                if (!balanceReader.Read())
                {
                    throw new BankError("Invalid username");
                }

                // dam bao luon co ban ghi
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();
                //kiem tra kieu transaction chi chap nhan withdraw va deposit
                if (historyTransaction.Type != Transaction.TransactionType.DEPOSIT
                    && historyTransaction.Type != Transaction.TransactionType.WITHDRAW)
                {
                    throw new BankError("Số dư không đủ");
                }

                if (historyTransaction.Type == Transaction.TransactionType.WITHDRAW &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new BankError("Số dư không đủ!");
                }

                // cong tien vao tai khoan
                if (historyTransaction.Type != Transaction.TransactionType.DEPOSIT)
                {
                    currentBalance -= historyTransaction.Amount;
                }
                else
                {
                    currentBalance += historyTransaction.Amount;
                }

                // update so du vao database

                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `account` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();
                // luu thong tin transaction 
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transaction` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (BankError e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        // Ham Update balance Transfer
        public bool UpdateBalanceTransfer(Account account, Transaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection();
            var transaction = DbConnection.Instance().Connection.BeginTransaction();
            try
            {
                // 1.thực hiện trừ số tiền người chuyển.
                //1.1 Lấy thông tin số dư người chuyển.
                var queryBalanceUser = "select balance from `account` where username = @username and status = @status";
                MySqlCommand queryBalanceCommand =
                    new MySqlCommand(queryBalanceUser, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                queryBalanceCommand.Parameters.AddWithValue("@status", account.Status);
                var balanceReader = queryBalanceCommand.ExecuteReader();

                if (!balanceReader.Read())
                {
                    throw new BankError("Invalid username");
                }

                // dam bao luon co ban ghi
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();

                // kiểm tra số dư tài khoản có đủ thực hiện giao dịch k
                if (historyTransaction.Type == Transaction.TransactionType.TRANSFER &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new BankError("Số dư không đủ!");
                }

                // trừ tiền người gửi.
                if (historyTransaction.Type == Transaction.TransactionType.TRANSFER)
                {
                    currentBalance = currentBalance - historyTransaction.Amount;
                }

                // update so du vao vao tai khoan người chuyển.
                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `account` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 2. thực hiện cộng tiền cho người nhận 
                // 2.1 lấy thông tin so dư mới nhất của người nhận.
                var queryBalanceAccountReceiver =
                    "select balance from `account` where username = @username and status = @status";
                MySqlCommand queryBalanceCommandAccount =
                    new MySqlCommand(queryBalanceAccountReceiver, DbConnection.Instance().Connection);
                queryBalanceCommandAccount.Parameters.AddWithValue("@username", Program.curentAccountReceiver.Username);
                queryBalanceCommandAccount.Parameters.AddWithValue("@status", Program.curentAccountReceiver.Status);
                var balanceReaderAccountReceiver = queryBalanceCommandAccount.ExecuteReader();

                if (!balanceReaderAccountReceiver.Read())
                {
                    throw new BankError("Invalid username");
                }

                // dam bao luon co ban ghi
                var currentBalanceAccountReceiver = balanceReaderAccountReceiver.GetDecimal("balance");
                balanceReaderAccountReceiver.Close();
                // Cộng tiền người nhận.
                if (historyTransaction.Type == Transaction.TransactionType.TRANSFER)
                {
                    currentBalanceAccountReceiver = currentBalanceAccountReceiver + historyTransaction.Amount;
                }

                // update so du moi nhat cua nguoi nhận vào database
                var updateAccountNumber = 0;
                var queryUpdateBalanceReceiver =
                    "update `account` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalanceReceiver =
                    new MySqlCommand(queryUpdateBalanceReceiver, DbConnection.Instance().Connection);
                cmdUpdateAccountBalanceReceiver.Parameters.AddWithValue("@username", Program.curentAccountReceiver.Username);
                cmdUpdateAccountBalanceReceiver.Parameters.AddWithValue("@balance", currentBalanceAccountReceiver);
                updateAccountNumber = cmdUpdateAccountBalanceReceiver.ExecuteNonQuery();

                // luu thong tin transaction 
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transaction` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1 && updateAccountNumber == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (BankError e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        // Lịch sử giao dịch
//        public bool HistoryTransaction()
//        {
//            Console.WriteLine("Lua chon truy van lich su giao dich.");
//            Console.WriteLine("====================================");
//            Console.WriteLine("1. Lich su giao dich 10 ngay gan nhat.");
//            Console.WriteLine("2. Tim giao dich theo ngay");
//            Console.WriteLine("3. Thoat");
//            Console.WriteLine("====================================");
//            return false;
              
//        }
        public List<Transaction> GetListTransactionHistory(string senderAccountNumber)
        {
            DbConnection.Instance().OpenConnection();
            List<Transaction> listHistoryTransactions = new List<Transaction>();

            var queryHistoryTransaction =
                "select * from `transaction` where senderAccountNumber = @senderAccountNumber or receiverAccountNumber = @receiverAccountNumber and createdAt =@createdAt";
            MySqlCommand cmdHistoryTransaction = new MySqlCommand(queryHistoryTransaction,DbConnection.Instance().Connection);
            cmdHistoryTransaction.Parameters.AddWithValue("@receiverAccountNumber", senderAccountNumber);
            var historyTransaction = cmdHistoryTransaction.ExecuteReader();
                listHistoryTransactions.Add(new Transaction(historyTransaction.GetString("id"),historyTransaction.GetString("createdAt"),
                    historyTransaction.GetString("senderAccountNumber"),historyTransaction.GetString("receiverAccountNumber")));
            
            DbConnection.Instance().CloseConnection();
            return null;
        }
    }
}