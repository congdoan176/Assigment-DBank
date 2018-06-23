using System;

namespace DemoSession2.error
{
    public class BankError: Exception
    {
        public BankError(string message) : base(message)
        {
            
        }
    }
}