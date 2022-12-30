using Fluent.Localization.Languages;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using Contract;
using System.Collections.Immutable;

namespace BatchRename
{
    /// <summary>
    /// This class just use for deserialization IRule from json
    /// </summary>
    public class RuleJObj
    {
        [JsonProperty]
        public string RuleType { get; set; }

        [JsonProperty]
        public bool HasParameter { get; set; }
    }

    /// <summary>
    /// This class just use for deserialization IRule from json
    /// </summary>
    public class RuleWithParametterJObj: RuleJObj
    {
        [JsonProperty]
        public ImmutableList<string> Keys { get; set; }

        [JsonProperty]
        public List<string> Values { get; set; }

        [JsonProperty]
        public string Errors { get; set; }
    }
    public class RuleObjectConverter : JsonCreationConverter<RuleJObj>
    {
        protected override RuleJObj? Create(Type objectType, JObject jObject)
        {
            // This is the important part - we can query what json properties are present
            // to figure out what type of object to construct and populate
            if (!FieldExists("HasParameter", jObject))
            {
                return null;
            }

            if ((bool)jObject["HasParameter"] == false)
            {
                return new RuleJObj();
            }
            else
            {
                return new RuleWithParametterJObj();
            }
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // We don't deal with writing JSON content, and generally Newtonsoft would make a good job of
            // serializing these type of objects without having to use a custom writer anyway
        }
    }

    // Generic converter class - could combine with above class if you're only dealing
    // with one inheritance chain, but this way it's reusable
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            T target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }
    }

    public class IRuleConverter : JsonConverter
    {
        private RuleJObj Create(Type objectType, JObject jObject)
        {
            if (!FieldExists("HasParameter", jObject))
            {
                return null;
            }

            if ((bool)jObject["HasParameter"] == false)
            {
                return new RuleJObj();
            }
            else
            {
                return new RuleWithParametterJObj();
            }
        }
        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var jObject = JObject.Load(reader);
            RuleJObj target = Create(objectType, jObject);
            serializer.Populate(jObject.CreateReader(), target);

            var rule = RuleFactory.Instance().ParseRuleFromJObj(target);

            return rule;
        }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => false;
    }
}
