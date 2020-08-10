using Newtonsoft.Json;
using ZES.Infrastructure.Serialization;

namespace BDO.Enhancement.Events
{
    public class EnhancementFailedDeserializer : EventDeserializerBase<EnhancementFailed>
    {
        public override void Switch(JsonTextReader reader, string currentProperty, EnhancementFailed e)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String when currentProperty == nameof(EnhancementFailed.Id):
                    e.Id = (string) reader.Value;
                    break;
            }
        }

        public override string EventType => nameof(EnhancementFailed);
    }
}