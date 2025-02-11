namespace YourBot.AI.Interface;

public class JudgeFriendlyResult {
    public JudgeFriendlyResult(bool isFriendly, string suggestion) {
        IsFriendly = isFriendly;
        Suggestion = suggestion;
    }

    public bool IsFriendly { get; set; }
    public string Suggestion { get; set; }
}