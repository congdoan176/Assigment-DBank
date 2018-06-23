using System;
using System.Collections.Generic;
using DemoSession2.entity;
using DemoSession2.model;
using DemoSession2.untility;

namespace DemoSession2.controller
{
    public class AccountController
    {
        private static AccountModel _accountModel = new AccountModel();

        public void Register()
        {
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            Console.WriteLine("Confirm Password: ");
            var cpassword = Console.ReadLine();
            Console.WriteLine("Identity Card: ");
            var identityCard = Console.ReadLine();
            Console.WriteLine("Full Name: ");
            var fullName = Console.ReadLine();
            Console.WriteLine("Email: ");
            var email = Console.ReadLine();
            Console.WriteLine("Phone: ");
            var phone = Console.ReadLine();
            var account = new Account(username, password, cpassword, identityCard, phone, email, fullName);
            var errors = account.CheckValid();
    
            if (errors.Count == 0)
            {
                _accountModel.Save(account);
                Console.WriteLine("Dang ky thanh cong!");
                Console.ReadLine();
            }
            else
            {
                Console.Error.WriteLine("Vui long sua cac loi va thu lai.");
                foreach (var messagErrorsValue in errors.Values)
                {
                    Console.Error.WriteLine(messagErrorsValue);
                }

                Console.ReadLine();
            }
        }

        public Boolean Login()
        {
            Console.WriteLine("Username:");
            string username = Console.ReadLine();
            Console.WriteLine("Password:");
            string password = Console.ReadLine();
            var account = new Account(username,password);
            var errors = account.ValidLogin();

            if (errors.Count > 0)
            {
                Console.WriteLine("Vui long nhap thong tin .");
                foreach (var messagErrorsValue in errors.Values)
                {
                    Console.Error.WriteLine(messagErrorsValue);
                }
                Console.ReadLine();
                return false;
            }
            account = _accountModel.GetAccountByUserName(username);
            if (account == null)
            {
                Console.WriteLine("Sai thong tin dang nhap.");
                return false;
            }
            var hashPassword = Hash.GenerateSaltedSHA1(password, account.Salt);
            if (account.Password != hashPassword)
            {
                Console.WriteLine("khong dung ten dang nhap hoac mat khau");
                return false;
            }

            // dang nhap thanh cong , luu thong tin dang nhap ra currentLoggedIn;
            Program.currentLoggedIn = account;
            return true;
            
        }

        public void CheckBalance()
        {
            Program.currentLoggedIn = _accountModel.GetAccountByUserName(Program.currentLoggedIn.Username);
            Console.WriteLine("Account Information");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Full name: " + Program.currentLoggedIn.FullName);
            Console.WriteLine("Account number: " + Program.currentLoggedIn.AccountNumber);
            Console.WriteLine("Balance: " + Program.currentLoggedIn.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }
        public void Withdraw()
        {
            Console.WriteLine("Withdraw.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Vui long nhap so tien can rut: ");
            var amount = Utility.GetUnsignDecimalNumber();
            var content = Console.ReadLine();
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.WITHDRAW,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedIn.AccountNumber,
                Status = Transaction.ActiveStatus.DONE
            };
            if (_accountModel.UpdateBalance(Program.currentLoggedIn, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedIn = _accountModel.GetAccountByUserName(Program.currentLoggedIn.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedIn.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }
        
        public void Deposit()
        {
            Console.WriteLine("Deposit.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Vui long nhap so tien gui: ");
            var amount = Utility.GetUnsignDecimalNumber();
            Console.WriteLine("Vui long nhap noi dung tin: ");
            var content = Console.ReadLine();
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.DEPOSIT,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedIn.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedIn.AccountNumber,
                Status = Transaction.ActiveStatus.DONE
            };
            if (_accountModel.UpdateBalance(Program.currentLoggedIn, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedIn = _accountModel.GetAccountByUserName(Program.currentLoggedIn.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedIn.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }

        public void Transfer()
        {
            Console.WriteLine("Transfer.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Vui long nhap so tai khoan can chuyen: ");
            var accountNumber = Console.ReadLine();

            Program.curentAccountReceiver = _accountModel.GetAccountByAccountNumber(accountNumber);
            if (Program.curentAccountReceiver == null)
            {
                Console.WriteLine("So tai khoan chua dung");
                return;
            }

            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Ban dang thuc hien Giao dich ");
            Console.WriteLine("So Tai Khoan : " + Program.curentAccountReceiver.AccountNumber);
            Console.WriteLine("Ten Tai khoan: " + Program.curentAccountReceiver.FullName);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Vui long nhap so tien can chuyen: ");
            var amount = Utility.GetUnsignDecimalNumber();
            Console.WriteLine("Vui long nhap noi dung tin: ");
            var content = Console.ReadLine();
            Console.WriteLine("=======================================================");
            var historyTransaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = Transaction.TransactionType.TRANSFER,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedIn.AccountNumber,
                ReceiverAccountNumber = accountNumber,
                Status = Transaction.ActiveStatus.DONE
            };
            if (_accountModel.UpdateBalanceTransfer(Program.currentLoggedIn, historyTransaction))
            {
                Console.WriteLine("Giao Dich Thanh Cong.");
                Program.currentLoggedIn = _accountModel.GetAccountByUserName(Program.currentLoggedIn.Username);
                Console.WriteLine("Ban Vua Giao Dich Thanh Cong .");
                Console.WriteLine("So Du Hien Tai Sau Giao Dich: "+ Program.currentLoggedIn.Balance);
                Console.WriteLine("Vui Long An Enter Tiep Tuc Su Dung Dich Vu!");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }   
        }

        public void HistoryTransaction()
        {
            List<Transaction> listTransactions = new List<Transaction>();

            listTransactions = _accountModel.GetListTransactionHistory(Program.currentLoggedIn.AccountNumber);

            Console.WriteLine("Lich Su Giao dich" + listTransactions);
        }
    }
}