using System.Security.Cryptography;

public class JsonFileEncryptor
{
    private const int Iterations = 10000; // Number of iterations for PBKDF2

    public static void EncryptJsonFile(string inputFile, string encryptedFile, string password)
    {
        try
        {
            // Generate a random salt
            byte[] salt;
            using (var rng = new RNGCryptoServiceProvider())
            {
                salt = new byte[16]; // 16 bytes salt
                rng.GetBytes(salt);
            }

            // Derive the AES key from the password using PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] key = pbkdf2.GetBytes(32); // 32 bytes for AES 256-bit key

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.GenerateIV();
                    byte[] iv = aesAlg.IV;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);

                    using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
                    {
                        using (FileStream fsOutput = new FileStream(encryptedFile, FileMode.Create))
                        {
                            fsOutput.Write(salt, 0, salt.Length); // Write the salt to the beginning of the encrypted file
                            fsOutput.Write(iv, 0, iv.Length); // Write the IV to the encrypted file

                            using (CryptoStream csEncrypt = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write))
                            {
                                fsInput.CopyTo(csEncrypt);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}