using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TeamManiacs.Core.Convertors
{
    public class JsonConverters
    {
        


    }

    public class BoolConvertor : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringVal = reader.GetString();
                if (bool.TryParse(stringVal, out bool value))
                {
                    return value;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                int? intVal = reader.GetInt32();
                if (intVal >= 1)
                    return true;
                return false;
            }
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {

            writer.WriteStringValue(value.ToString());
           // throw new NotImplementedException();
        }


    }


    public class Int32Convertor : JsonConverter<int>

    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? stringVal = reader.GetString();
                if (int.TryParse(stringVal, out int value))
                {
                    return value;
                }
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
            //throw new NotImplementedException();
        }
    }
}
