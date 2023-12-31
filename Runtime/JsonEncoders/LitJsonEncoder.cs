using System.Collections.Generic;

using Best.HTTP.JSON.LitJson;

namespace Best.SignalR.JsonEncoders
{
    public sealed class LitJsonEncoder : IJsonEncoder
    {
        public string Encode(object obj)
        {
            JsonWriter writer = new JsonWriter();
            JsonMapper.ToJson(obj, writer);

            return writer.ToString();
        }

        public IDictionary<string, object> DecodeMessage(string json)
        {
            JsonReader reader = new JsonReader(json);

            return JsonMapper.ToObject<Dictionary<string, object>>(reader);
        }
    }
}
