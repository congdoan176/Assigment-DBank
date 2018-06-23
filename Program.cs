using System;
using DemoSession2.controller;
using DemoSession2.entity;
using DemoSession2.model;
using DemoSession2.untility;

namespace DemoSession2
// lam viec voi 
{
    class Program
    {
        public static Account currentLoggedIn;
        public static Account curentAccountReceiver;
        public static AccountController accountController = new AccountController();
        
        static void Main(string[] args)
        {
         GenerateMenu();
        }
        
        public static void GenerateMenu()
        {
            while (true)
            {
                if (currentLoggedIn == null)
                {
                    GeneraMenu();
                }
                else
                {
                    GenerateCustomerMenu();
                }
            }
        }
        
        private static void GenerateCustomerMenu()
        {
            while (true)
            {
                Console.WriteLine("---------SPRING HERO BANK---------");
                Console.WriteLine("Welcome back: " + currentLoggedIn.FullName);
                Console.WriteLine("1. Balance.");
                Console.WriteLine("2. Withdraw.");
                Console.WriteLine("3. Deposit.");
                Console.WriteLine("4. Transfer.");
                Console.WriteLine("5. History transaction.");
                Console.WriteLine("6. Exit.");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("Please enter your choice (1|2|3|4|5|6): ");
                var choice = Utility.GetInt32Number();
                switch (choice)
                {
                    case 1:
                        accountController.CheckBalance();
                        break;
                    case 2:
                        accountController.Withdraw();
                        break;
                    case 3:
                        accountController.Deposit();
                        break;
                    case 4:
                        accountController.Transfer();
                        break;
                    case 5:
                        accountController.HistoryTransaction();
                        break;
                    case 6:
                        Console.WriteLine("See you later.");
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
        }

        private static void GeneraMenu()
        {
            while (true)
            {
                
                Console.WriteLine("---------Menu----------");
                Console.WriteLine("1. Register.");
                Console.WriteLine("2. Login.");
                Console.WriteLine("3. Exit.");
                Console.WriteLine("Please enter your choice: ");
                int choice = Int32.Parse(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        accountController.Register();
                        break;
                    case 2:
                        accountController.Login();
                        break;
                    case 3:
                        Console.WriteLine("Thoat chuong trinh");
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("This is not a valid choice, please enter your choice again");
                        break;
                }
                if (currentLoggedIn != null)
                {
                    break;
                }
            }
        }
    }
}