
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