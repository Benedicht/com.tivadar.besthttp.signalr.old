using Best.HTTP.JSON;
using System.Collections.Generic;

namespace Best.SignalR.JsonEncoders
{
    public sealed class DefaultJsonEncoder : IJsonEncoder
    {
        public string Encode(object obj)
        {
            return Json.Encode(obj);
        }

        public IDictionary<string, object> DecodeMessage(string json)
        {
            bool ok = false;
            IDictionary<string, object> result = Json.Decode(json, ref ok) as IDictionary<string, object>;
            return ok ? result : null;
        }
    }
}
