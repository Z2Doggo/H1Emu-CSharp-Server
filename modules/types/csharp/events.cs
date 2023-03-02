// EventEmitter from Node.Js ported/translated to C# by @doggo_wuz_here on twitter! :^)

namespace CS_Modules.Events
{
    public interface IEventEmitterOptions
    {
        bool? CaptureRejections { get; set; }
    }

    public interface INodeEventTarget
    {
        void Once(string eventName, Action<object[]> listener);
    }

    public interface IDOMEventTarget
    {
        void AddEventListener(string EventName, Action<object[]> Listener, bool Once, dynamic? Opts = null);
    }
}