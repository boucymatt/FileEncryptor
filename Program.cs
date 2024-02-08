using System.Security.Cryptography;
using System.Text;

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
class Program
{
    static void Main(string[] args)
    {
        bool exitRequested = false;

        do
        {
            Console.WriteLine("Choose an option:");
            Console.WriteLine("1. Encrypt JSON file");
            Console.WriteLine("2. Decrypt JSON file");
            Console.WriteLine("3. Exit");
            Console.Write("Enter your choice (1, 2, or 3): ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    EncryptJsonFile();
                    break;
                case "2":
                    DecryptJsonFile();
                    break;
                case "3":
                    exitRequested = true;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter 1, 2, or 3.");
                    break;
            }
        } while (!exitRequested);

    }
    static void EncryptJsonFile()
    {
        string inputFile = ReadJsonName();
        string trimmedName = Path.GetFileNameWithoutExtension(inputFile); // Remove the ".json" extension

        string password = ReadPassword();
        string encryptedFile = trimmedName + ".dat";

        JsonFileEncryptor.EncryptJsonFile(inputFile, encryptedFile, password);
        Console.WriteLine("Encryption complete.");
    }
    static void DecryptJsonFile()
    {
        string encryptedFile = ReadDatName();

        string password = ReadPassword();

        string decryptedJson = JsonFileDecryptor.DecryptJsonFile(encryptedFile, password);
        if (decryptedJson != null)
        {
            Console.WriteLine("Decrypted JSON:");
            Console.WriteLine(decryptedJson);
        }
        else
        {
            Console.WriteLine("Failed to decrypt JSON file.");
        }
    }
    private static string ReadPassword()
    {
        Console.Write("Enter password: ");
        string password = "";
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else
            {
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[0..^1];
                    Console.Write("\b \b");
                }
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine(); // Move to the next line after password input
        return password;
    }
    private static string ReadDatName()
    {
        Console.Write("[Encrypt] File Name: ");
        string fileName = Console.ReadLine();
        return fileName;
    }
    private static string ReadJsonName()
    {
        Console.Write("[Json] File Name: ");
        string fileName = Console.ReadLine();
        return fileName;
    }
}