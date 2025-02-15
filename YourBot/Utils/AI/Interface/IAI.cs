using Lagrange.Core.Message;

namespace YourBot.AI.Interface;

// ReSharper disable InconsistentNaming
public interface IAI {
    // ReSharper restore InconsistentNaming
    public Task<JudgeFriendlyResult> JudgeFriendly(List<IMessageEntity> messageEntities);
}