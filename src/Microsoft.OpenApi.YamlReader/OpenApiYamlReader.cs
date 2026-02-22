// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Reader;
using SharpYaml;
using SharpYaml.Events;
using System;
using System.Text;

namespace Microsoft.OpenApi.YamlReader
{
    /// <summary>
    /// Reader for parsing YAML files into an OpenAPI document.
    /// </summary>
    public class OpenApiYamlReader : IOpenApiReader
    {
        private const int copyBufferSize = 4096;
        private static readonly OpenApiJsonReader _jsonReader = new();

        /// <inheritdoc/>
        public async Task<ReadResult> ReadAsync(Stream input,
                                                Uri location,
                                                OpenApiReaderSettings settings,
                                                CancellationToken cancellationToken = default)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (input is MemoryStream memoryStream)
            {
                return UpdateFormat(Read(memoryStream, location, settings));
            } 
            else 
            {
                using var preparedStream = new MemoryStream();
                await input.CopyToAsync(preparedStream, copyBufferSize, cancellationToken).ConfigureAwait(false);
                preparedStream.Position = 0;
                return UpdateFormat(Read(preparedStream, location, settings));
            }
        }

        /// <inheritdoc/>
        public ReadResult Read(MemoryStream input,
                               Uri location,
                               OpenApiReaderSettings settings)
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            if (settings is null) throw new ArgumentNullException(nameof(settings));
            JsonNode jsonNode;

            // Parse the YAML text in the stream into a sequence of JsonNodes
            try
            {
#if NET
// this represents net core, net5 and up
                using var stream = new StreamReader(input, default, true, -1, settings.LeaveStreamOpen);
#else
// the implementation differs and results in a null reference exception in NETFX
                using var stream = new StreamReader(input, Encoding.UTF8, true, 4096, settings.LeaveStreamOpen);
#endif
                jsonNode = LoadJsonNodesFromYamlDocument(stream);
            }
            catch (JsonException ex)
            {
                var diagnostic = new OpenApiDiagnostic();
                diagnostic.Errors.Add(new($"#line={ex.LineNumber}", ex.Message));
                diagnostic.Format = OpenApiConstants.Yaml;
                return new()
                {
                    Document = null,
                    Diagnostic = diagnostic,
                };
            }

            return UpdateFormat(Read(jsonNode, location, settings));
        }
        private static ReadResult UpdateFormat(ReadResult result)
        {
            result.Diagnostic ??= new OpenApiDiagnostic();
            result.Diagnostic.Format = OpenApiConstants.Yaml;
            return result;
        }

        /// <inheritdoc/>
        public static ReadResult Read(JsonNode jsonNode, Uri location, OpenApiReaderSettings settings)
        {
            return UpdateFormat(_jsonReader.Read(jsonNode, location, settings));
        }

        /// <inheritdoc/>
        public T? ReadFragment<T>(MemoryStream input,
                                 OpenApiSpecVersion version,
                                 OpenApiDocument openApiDocument,
                                 out OpenApiDiagnostic diagnostic,
                                 OpenApiReaderSettings? settings = null) where T : IOpenApiElement
        {
            if (input is null) throw new ArgumentNullException(nameof(input));
            JsonNode jsonNode;

            // Parse the YAML
            try
            {
                using var stream = new StreamReader(input);
                jsonNode = LoadJsonNodesFromYamlDocument(stream);
            }
            catch (JsonException ex)
            {
                diagnostic = new();
                diagnostic.Errors.Add(new($"#line={ex.LineNumber}", ex.Message));
                return default;
            }

            return ReadFragment<T>(jsonNode, version, openApiDocument, out diagnostic, settings);
        }

        /// <inheritdoc/>
        public static T? ReadFragment<T>(JsonNode input, OpenApiSpecVersion version, OpenApiDocument openApiDocument, out OpenApiDiagnostic diagnostic, OpenApiReaderSettings? settings = null) where T : IOpenApiElement
        {
            return _jsonReader.ReadFragment<T>(input, version, openApiDocument, out diagnostic, settings);
        }

        /// <summary>
        /// Helper method to turn streams into a sequence of JsonNodes.
        /// Uses SharpYaml's event-based parser to build JsonNode directly,
        /// avoiding the intermediate YamlNode DOM allocation.
        /// </summary>
        /// <param name="input">Stream containing YAML formatted text</param>
        /// <returns>A JsonNode representing the first YAML document</returns>
        static JsonNode LoadJsonNodesFromYamlDocument(TextReader input)
        {
            var parser = Parser.CreateParser(input);

            // Skip StreamStart
            if (!parser.MoveNext() || parser.Current is not StreamStart)
                throw new InvalidOperationException("Expected YAML stream start.");

            // Skip DocumentStart
            if (!parser.MoveNext() || parser.Current is not DocumentStart)
                throw new InvalidOperationException("Expected YAML document start.");

            // Parse the root node
            if (!parser.MoveNext())
                throw new InvalidOperationException("No documents found in the YAML stream.");

            return ConvertYamlEventToJsonNode(parser)
                   ?? throw new InvalidOperationException("No documents found in the YAML stream.");
        }

        private static readonly HashSet<string> YamlNullRepresentations = new(StringComparer.Ordinal)
        {
            "~",
            "null",
            "Null",
            "NULL"
        };

        /// <summary>
        /// Reads the current parser event and any child events, building a JsonNode.
        /// After returning, parser.Current is the last consumed event.
        /// </summary>
        private static JsonNode? ConvertYamlEventToJsonNode(IParser parser)
        {
            switch (parser.Current)
            {
                case Scalar scalar:
                    return ConvertScalarToJsonValue(scalar);

                case MappingStart:
                    return ConvertMappingToJsonObject(parser);

                case SequenceStart:
                    return ConvertSequenceToJsonArray(parser);

                default:
                    throw new InvalidOperationException(
                        $"Unexpected YAML event: {parser.Current?.GetType().Name}");
            }
        }

        private static JsonValue ConvertScalarToJsonValue(Scalar scalar)
        {
            var value = scalar.Value;

            if (scalar.Style == ScalarStyle.Plain)
            {
                // Check for null representations
                if (value is null || YamlNullRepresentations.Contains(value))
                    return (JsonValue)JsonNullSentinel.JsonNull.DeepClone();

                // Try numeric
                if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                    return JsonValue.Create(d);

                // Try boolean
                if (bool.TryParse(value, out var b))
                    return JsonValue.Create(b);
            }

            return JsonValue.Create(value);
        }

        private static JsonObject ConvertMappingToJsonObject(IParser parser)
        {
            // parser.Current is MappingStart
            var obj = new JsonObject();

            while (parser.MoveNext())
            {
                if (parser.Current is MappingEnd)
                    break;

                // Key must be a scalar
                if (parser.Current is not Scalar keyScalar)
                    throw new InvalidOperationException("Expected scalar key in YAML mapping.");

                var key = keyScalar.Value!;

                // Advance to the value event
                if (!parser.MoveNext())
                    throw new InvalidOperationException("Unexpected end of YAML stream while reading mapping value.");

                obj[key] = ConvertYamlEventToJsonNode(parser);
            }

            return obj;
        }

        private static JsonArray ConvertSequenceToJsonArray(IParser parser)
        {
            // parser.Current is SequenceStart
            var arr = new JsonArray();

            while (parser.MoveNext())
            {
                if (parser.Current is SequenceEnd)
                    break;

                arr.Add(ConvertYamlEventToJsonNode(parser));
            }

            return arr;
        }
    }
}
