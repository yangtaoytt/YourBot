using Fuwafuwa.Core.Attributes.ServiceAttribute.Level1;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.Attribute.Processor;

public class ReadGroupQMessageAttribute : IProcessorAttribute<ReadGroupQMessageAttribute, MessageData>;