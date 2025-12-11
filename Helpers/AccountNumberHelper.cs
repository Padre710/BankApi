using BankApi.Db;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BankApi.Helpers
{
    public class AccountNumberHelper
    {
        // Generate a random 10-digit number
        private static string GenerateRandom10DigitNumber()
        {
            Span<byte> bytes = stackalloc byte[8];
            RandomNumberGenerator.Fill(bytes);
            ulong value = BitConverter.ToUInt64(bytes);
            long number = (long)(value % 1_000_000_0000L);
            return number.ToString("D10");
        }

        // Generate a unique number by checking the database
        public static async Task<string> GenerateUniqueAccountNumberAsync(AppDbContext context)
        {
            string accountNumber;
            bool exists;
            int attempts = 0;

            do
            {
                accountNumber = GenerateRandom10DigitNumber();
                exists = await context.Accounts.AnyAsync(a => a.AccountNumber == accountNumber);
                attempts++;

                if (attempts > 10)
                    throw new Exception("Unable to generate unique account number after multiple attempts.");
            }
            while (exists);

            return accountNumber;
        }
    }
}
