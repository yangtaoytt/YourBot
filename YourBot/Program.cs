namespace YourBot;

public static class Program {
    public static async Task Main(string[] args) {
        var botApplication = new BotApplication("MainConfig.json");

        Console.CancelKeyPress += (sender, e) => {
            Console.WriteLine("Eixt...");
            e.Cancel = true;
            botApplication.Dispose();
            Task.Delay(1000).Wait();
            Environment.Exit(0);
        };

        await botApplication.Run();
    }
}