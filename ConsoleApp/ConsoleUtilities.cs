namespace ConsoleApp;

public class ConsoleUtilities
{
    public static string GetUserStringInput(string prompt = "", bool allowEmpty = true)
    {
        string? userChoice = null;
        do
        {
            if (!string.IsNullOrWhiteSpace(prompt)) Console.Write(prompt);
            userChoice = Console.ReadLine();
            if (userChoice == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(userChoice) && !allowEmpty)
            {
                Console.WriteLine("Input cannot be empty");
            }
        
        } while (string.IsNullOrWhiteSpace(userChoice) && !allowEmpty);
    
    
        return userChoice ?? string.Empty;
    }
}