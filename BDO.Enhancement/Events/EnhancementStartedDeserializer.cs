using Newtonsoft.Json;
using ZES.Infrastructure.Serialization;

namespace BDO.Enhancement.Events
{
    public class EnhancementStartedDeserializer : EventDeserializerBase<EnhancementStarted>
    {
        public override void Switch(JsonTextReader reader, string currentProperty, EnhancementStarted e)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String when currentProperty == nameof(EnhancementStarted.EnchancementId):
                    e.EnchancementId = (string)reader.Value;
                    break;
            }
        }

        public override string EventType => nameof(EnhancementStarted);
    }
}