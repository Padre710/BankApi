namespace BankApi.Constants
{
    public class Roles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string Teller = "Teller";
        public const string AdminOrTeller = Admin + "," + Teller;
        public const string AllRoles = Admin + "," + Customer + "," + Teller;
    }
}
