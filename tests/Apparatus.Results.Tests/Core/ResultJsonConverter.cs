namespace Apparatus.Results.Tests.Core;

public class ResultJsonConverter : WriteOnlyJsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override void Write(VerifyJsonWriter writer, object value)
    {
        var type = value.GetType();
        var isSuccessProperty = type.GetProperty("IsSuccess")!;
        var isErrorProperty = type.GetProperty("IsError")!;
        
        var isSuccess = (bool)isSuccessProperty.GetValue(value)!;
        var isError = (bool)isErrorProperty.GetValue(value)!;

        writer.WriteStartObject();
        
        writer.WritePropertyName("IsSuccess");
        writer.WriteValue(isSuccess);
        
        writer.WritePropertyName("IsError");
        writer.WriteValue(isError);

        if (isSuccess)
        {
            var valueProperty = type.GetProperty("Value")!;
            var val = valueProperty.GetValue(value);
            writer.WritePropertyName("Value");
            writer.Serialize(val);
        }
        else
        {
            var errorProperty = type.GetProperty("Error")!;
            var error = errorProperty.GetValue(value);
            writer.WritePropertyName("Error");
            writer.Serialize(error);
        }

        writer.WriteEndObject();
    }
}