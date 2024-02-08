using System.Security.Cryptography;
using System.Text;

public class JsonFileDecryptor
{
    private const int Iterations = 10000; // Number of iterations for PBKDF2
    private const int SaltSize = 16;
    public static string DecryptJsonFile(string encryptedFile, string password)
    {
        try
        {
            byte[] salt = new byte[SaltSize];
            byte[] iv = new byte[16]; // 16 bytes for AES IV

            // Read the salt and IV from the encrypted file
            using (FileStream fsInput = new FileStream(encryptedFile, FileMode.Open))
            {
                fsInput.Read(salt, 0, SaltSize);
                fsInput.Read(iv, 0, iv.Length);
            }

            // Derive the AES key from the password and salt using PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] key = pbkdf2.GetBytes(32); // 32 bytes for AES 256-bit key

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        using (FileStream fsInput = new FileStream(encryptedFile, FileMode.Open))
                        {
                            fsInput.Position = SaltSize + iv.Length; // Skip the salt and IV
                            using (CryptoStream csDecrypt = new CryptoStream(fsInput, decryptor, CryptoStreamMode.Read))
                            {
                                csDecrypt.CopyTo(msOutput);
                            }
                        }

                        // Convert the decrypted data to a string
                        string decryptedJson = Encoding.UTF8.GetString(msOutput.ToArray());

                        // Write the decrypted JSON data to a file
                        string outputFile = Path.ChangeExtension(encryptedFile, "json");
                        File.WriteAllText(outputFile, decryptedJson);

                        return decryptedJson;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            return null;
        }
    }
}