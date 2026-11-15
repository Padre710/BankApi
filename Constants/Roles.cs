namespace BankApi.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
        public const string Teller = "Teller";
        public const string AdminOrTeller = Admin + "," + Teller;
        public const string AllRoles = Admin + "," + Customer + "," + Teller;
    }

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string CustomerOnly = "CustomerOnly";
        public const string TellerOrAdmin = "TellerOrAdmin";
    }
}
