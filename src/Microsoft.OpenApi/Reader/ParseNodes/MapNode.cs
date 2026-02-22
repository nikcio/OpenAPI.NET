// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.OpenApi.Reader
{
    /// <summary>
    /// Abstraction of a Map to isolate semantic parsing from details of JSON DOM
    /// </summary>
    internal class MapNode : ParseNode, IEnumerable<PropertyNode>
    {
        private readonly JsonObject _node;

        public MapNode(ParsingContext context, JsonNode node) : base(
            context, node)
        {
            if (node is not JsonObject mapNode)
            {
                throw new OpenApiReaderException("Expected map.", Context);
            }

            _node = mapNode;
        }

        public override Dictionary<string, T> CreateMap<T>(Func<MapNode, OpenApiDocument, T> map, OpenApiDocument hostDocument)
        {
            var jsonMap = _node ?? throw new OpenApiReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var result = new Dictionary<string, T>(jsonMap.Count);
            foreach (var n in jsonMap)
            {
                var key = n.Key;
                T value;
                try
                {
                    Context.StartObject(key);
                    value = n.Value is JsonObject jsonObject
                      ? map(new MapNode(Context, jsonObject), hostDocument)
                      : default!;
                }
                finally
                {
                    Context.EndObject();
                }
                result[key] = value;
            }
            return result;
        }

        public override Dictionary<string, T> CreateSimpleMap<T>(Func<ValueNode, T> map)
        {
            var jsonMap = _node ?? throw new OpenApiReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var result = new Dictionary<string, T>(jsonMap.Count);
            foreach (var n in jsonMap)
            {
                var key = n.Key;
                try
                {
                    Context.StartObject(key);
                    JsonValue valueNode = n.Value is JsonValue value ? value
                    : throw new OpenApiReaderException($"Expected scalar while parsing {typeof(T).Name}", Context);

                    result[key] = map(new ValueNode(Context, valueNode));
                }
                finally
                {
                    Context.EndObject();
                }
            }
            return result;
        }

        public override Dictionary<string, HashSet<T>> CreateArrayMap<T>(Func<ValueNode, OpenApiDocument?, T> map, OpenApiDocument? openApiDocument)
        {
            var jsonMap = _node ?? throw new OpenApiReaderException($"Expected map while parsing {typeof(T).Name}", Context);
            var result = new Dictionary<string, HashSet<T>>(jsonMap.Count);
            foreach (var n in jsonMap)
            {
                var key = n.Key;
                try
                {
                    Context.StartObject(key);
                    JsonArray arrayNode = n.Value is JsonArray value
                        ? value
                        : throw new OpenApiReaderException($"Expected array while parsing {typeof(T).Name}", Context);

                    var values = new HashSet<T>();
                    foreach (var item in arrayNode)
                    {
                        if (item is not null)
                            values.Add(map(new ValueNode(Context, item), openApiDocument));
                    }

                    result[key] = values;
                }
                finally
                {
                    Context.EndObject();
                }
            }
            return result;
        }

        public IEnumerator<PropertyNode> GetEnumerator()
        {
            foreach (var kvp in _node)
            {
                yield return new PropertyNode(Context, kvp.Key, kvp.Value ?? JsonNullSentinel.JsonNull);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string GetRaw()
        {
            var x = JsonSerializer.Serialize(_node, SourceGenerationContext.Default.JsonObject);
            return x;
        }

        public string? GetReferencePointer()
        {
            if (!_node.TryGetPropertyValue("$ref", out JsonNode? refNode))
            {
                return null;
            }

            return refNode?.GetScalarValue();
        }

        public string? GetJsonSchemaIdentifier()
        {
            if (!_node.TryGetPropertyValue("$id", out JsonNode? idNode))
            {
                return null;
            }

            return idNode?.GetScalarValue();
        }

        public string? GetSummaryValue()
        {
            if (!_node.TryGetPropertyValue("summary", out JsonNode? summaryNode))
            {
                return null;
            }

            return summaryNode?.GetScalarValue();
        }

        public string? GetDescriptionValue()
        {
            if (!_node.TryGetPropertyValue("description", out JsonNode? descriptionNode))
            {
                return null;
            }

            return descriptionNode?.GetScalarValue();
        }

        public string? GetScalarValue(ValueNode key)
        {
            var keyValue = key.GetScalarValue();
            if (keyValue is not null)
            {
                var scalarNode = _node[keyValue] is JsonValue jsonValue
                        ? jsonValue
                        : throw new OpenApiReaderException($"Expected scalar while parsing {key.GetScalarValue()}", Context);

                return Convert.ToString(scalarNode?.GetValue<object>(), CultureInfo.InvariantCulture);
            }
            return null;
        }

        /// <summary>
        /// Create an <see cref="JsonNodeExtension"/>
        /// </summary>
        /// <returns>The created Json object.</returns>
        public override JsonNode CreateAny()
        {
            return _node;
        }
    }

    [JsonSerializable(typeof(JsonObject))]
    internal partial class SourceGenerationContext : JsonSerializerContext { }
}
