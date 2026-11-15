using BankApi.Repositories.Interfaces;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

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

        // Generate a unique number by checking the repository
        public static async Task<string> GenerateUniqueAccountNumberAsync(IAccountRepository accountRepo)
        {
            string accountNumber;
            bool exists;
            int attempts = 0;

            do
            {
                accountNumber = GenerateRandom10DigitNumber();
                exists = await accountRepo.AccountExistsAsync(accountNumber);
                attempts++;

                if (attempts > 10)
                    throw new InvalidOperationException("Unable to generate unique account number after multiple attempts.");
            }
            while (exists);

            return accountNumber;
        }
    }
}
