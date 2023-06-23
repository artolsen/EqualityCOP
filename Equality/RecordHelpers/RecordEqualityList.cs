using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Equality.RecordHelpers
{
    [JsonConverter(typeof(RecordEqualityListConverterFactory))]
    public class RecordEqualityList<T> : List<T>
    {
        private readonly bool _requireMathcingOrder;
        public RecordEqualityList(bool requireMatchingOrder = false) => _requireMathcingOrder = requireMatchingOrder;

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (!(other is IEnumerable<T> enumerable))
                return false;
            if (!_requireMathcingOrder)
                return enumerable.ScrambledEquals(this);
            return enumerable.SequenceEqual(this);
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var item in this)
            {
                hashCode ^= item.GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(RecordEqualityList<T> req1, RecordEqualityList<T> req2)
        {
            return req1.Except(req2).Count() == 0;
        }

        public static bool operator !=(RecordEqualityList<T> req1, RecordEqualityList<T> req2)
        {
            return !(req1 == req2);
        }
    }

    public class RecordEqualityListConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(RecordEqualityList<>);
        }
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var elementType = typeToConvert.GetGenericArguments()[0];
            var listType = typeof(RecordEqualityListConverter<>);
            var converter = (JsonConverter)Activator.CreateInstance(listType.MakeGenericType(elementType), BindingFlags.Instance | BindingFlags.Public,
                binder: null, args: null, culture: null)!;
            return converter;
        }

        public class RecordEqualityListConverter<T> : JsonConverter<RecordEqualityList<T>>
        {            
            public override RecordEqualityList<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    throw new JsonException();
                }
                reader.Read();
                RecordEqualityList<T> elements = new();
                while (reader.TokenType != JsonTokenType.EndArray)
                {
                    var value = JsonSerializer.Deserialize<T>(ref reader, options);
                    if (value is not null)
                    {
                        elements.Add(value);
                    }
                    reader.Read();
                }
                
                return elements;
            }
            
            public override void Write(Utf8JsonWriter writer, RecordEqualityList<T> value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value.AsEnumerable(), options);
            }
        }
    }

    static class EnumerableExtensions
    {

        /// <summary>
        /// Returns true if both enumerables contain the same items, regardless of order. O(N*Log(N))
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool ScrambledEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var source = first.GetItemsWithCounts();

            foreach (var item in second)
            {
                if (!source.TryGetValue(item, out var count))
                    return false;
                count -= 1;
                source[item] = count;
                if (count < 0)
                    return false;
            }

            return source.Values.All(c => c == 0);
        }


        public static Dictionary<T, int> GetItemsWithCounts<T>(this IEnumerable<T> enumerable)
        {
            var itemsWithCounts = new Dictionary<T, int>();
            foreach (var item in enumerable)
            {
                if (!itemsWithCounts.TryGetValue(item, out var count))
                {
                    count = 0;
                }

                count++;
                itemsWithCounts[item] = count;
            }

            return itemsWithCounts;
        }

    }
}
