namespace YourBot.Utils.Command;

public class InvalidCommandException : Exception {
    public InvalidCommandException() : base("No more command found") { }
}