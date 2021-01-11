using System;
using Newtonsoft.Json;

namespace TG_Bot.Helpers
{
    public class EventsJsonConverter:JsonConverter
    {
        ///Для конвертации событий нужно дописать парсер
        /// https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm
        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}