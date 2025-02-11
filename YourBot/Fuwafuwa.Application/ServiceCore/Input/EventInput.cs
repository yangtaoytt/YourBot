using Fuwafuwa.Core.Attributes.ServiceAttribute.Level0;
using Fuwafuwa.Core.Attributes.ServiceAttribute.Level1;
using Fuwafuwa.Core.Data.ServiceData.Level1;
using Fuwafuwa.Core.Data.SharedDataWrapper.Level2;
using Fuwafuwa.Core.Log;
using Fuwafuwa.Core.ServiceCore.Level3;
using Lagrange.Core.Event;
using YourBot.Fuwafuwa.Application.Attribute.Processor;
using YourBot.Fuwafuwa.Application.Data.ProcessorData;

namespace YourBot.Fuwafuwa.Application.ServiceCore.Input;

public class EventInput : IInputCore<NullSharedDataWrapper<object>, object> {
    public static IServiceAttribute<InputPackagedData> GetServiceAttribute() {
        return IInputAttribute.GetInstance();
    }

    public static NullSharedDataWrapper<object> Init(object initData) {
        return new NullSharedDataWrapper<object>(initData);
    }

    public static void Final(NullSharedDataWrapper<object> sharedData, Logger2Event? logger) { }


    public async Task<List<Certificate>> ProcessData(InputPackagedData data, NullSharedDataWrapper<object> sharedData,
        Logger2Event? logger) {
        await Task.CompletedTask;

        var eventBase = (EventBase)data.PackagedObject!;

        var qEventData = new EventData(eventBase);

        return [
            ReadQEventAttribute.GetInstance().GetCertificate(qEventData)
        ];
    }
}