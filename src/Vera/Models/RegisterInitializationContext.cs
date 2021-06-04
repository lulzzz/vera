namespace Vera.Models
{
    public class RegisterInitializationContext
    {
        public Account Account { get; }
        public Supplier Supplier { get; }
        public Register Register { get; }
        
        public RegisterInitializationContext(Account account, Supplier supplier, Register register)
        {
            Account = account;
            Supplier = supplier;
            Register = register;
        }
       
    }
}