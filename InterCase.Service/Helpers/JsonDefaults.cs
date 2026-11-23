using System.Text.Json;
using System.Text.Json.Serialization;

namespace InterCase.Service.Helpers
{
    public static class JsonDefaults
    {
        public static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
