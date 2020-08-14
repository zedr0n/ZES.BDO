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
                case JsonToken.String when currentProperty == nameof(EnhancementStarted.Item):
                    e.Item = (string)reader.Value;
                    break;
                case JsonToken.Integer when currentProperty == nameof(EnhancementStarted.Grade):
                    e.Grade = (int)(long)reader.Value;
                    break;
                case JsonToken.Integer when currentProperty == nameof(EnhancementStarted.Failstack):
                    e.Failstack = (int)(long)reader.Value;
                    break;
            }
        }

        public override string EventType => nameof(EnhancementStarted);
    }
}