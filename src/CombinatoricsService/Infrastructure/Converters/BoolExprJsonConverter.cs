using System.Text.Json;
using System.Text.Json.Serialization;
using FluentResults;
using Raijin.CombinatoricsService.Application.Parsing;
using Raijin.CombinatoricsService.Domain.BooleanExpressions;

namespace Raijin.CombinatoricsService.Infrastructure.Converters;

public sealed class BoolExprJsonConverter(IBoolExprParser parser)
    : JsonConverter<BoolExpr>
{
    public override BoolExpr Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? exprString = reader.GetString();
        
        if (exprString == null)
            throw new JsonException("Expression string is null.");

        Result<BoolExpr> boolExprResult = parser.Parse(exprString);
        
        if (boolExprResult.IsFailed)
            throw new JsonException(string.Join("; ", boolExprResult.Errors.Select(e => e.Message)));

        return boolExprResult.Value;
    }

    public override void Write(Utf8JsonWriter writer, BoolExpr value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}