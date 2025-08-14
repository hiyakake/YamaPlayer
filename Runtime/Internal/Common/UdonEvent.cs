
using UdonSharp;

namespace Yamadev.YamaStream
{
    public class UdonEvent: UdonSharpBehaviour
    {
        public static UdonEvent Empty()
        {
            object[] result = new object[] { null, null };
            return (UdonEvent)(object)result;
        }

        public static UdonEvent New(UdonSharpBehaviour udon, string callback)
        {
            // 0. UdonSharpBehaviour: udon
            // 1. string: callback event name
            object[] result = new object[] { udon, callback };
            return (UdonEvent)(object)result;
        }

        // Overload: pass a single variable before invoking the event
        public static UdonEvent New(UdonSharpBehaviour udon, string callback, string variableName, object variable)
        {
            object[] result = new object[] { udon, callback, variableName, variable };
            return (UdonEvent)(object)result;
        }

        // Overload: pass multiple variables (name/value pairs) before invoking the event
        public static UdonEvent New(UdonSharpBehaviour udon, string callback, string[] variableNames, object[] variables)
        {
            int count = variableNames == null ? 0 : variableNames.Length;
            object[] result = new object[2 + count * 2];
            result[0] = udon;
            result[1] = callback;
            for (int i = 0; i < count; i++)
            {
                result[2 + i * 2] = variableNames[i];
                result[2 + i * 2 + 1] = variables[i];
            }
            return (UdonEvent)(object)result;
        }
    }

    public static class UdonEventExtentions
    {
        public static void Invoke(this UdonEvent _udonEvent) 
        {
            object[] raw = (object[])(object)_udonEvent;
            UdonSharpBehaviour udon = (UdonSharpBehaviour)raw[0];
            string callback = (string)raw[1];
            // Apply optional variable assignments (name/value pairs)
            for (int i = 2; i + 1 < raw.Length; i += 2)
            {
                string name = raw[i] as string;
                object value = raw[i + 1];
                if (!string.IsNullOrEmpty(name) && udon != null)
                    udon.SetProgramVariable(name, value);
            }
            if (udon != null) udon.SendCustomEvent(callback);
        }
    }
}