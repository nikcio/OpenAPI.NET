// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Microsoft.OpenApi.Reader
{
    internal class PropertyNode : ParseNode
    {
        private ParseNode? _value;

        public PropertyNode(ParsingContext context, string name, JsonNode node) : base(
            context, node)
        {
            Name = name;
        }

        public string Name { get; set; }

        public ParseNode Value
        {
            get => _value ??= Create(Context, JsonNode);
            set => _value = value;
        }

        public void ParseField<T>(
            T parentInstance,
            Dictionary<string, Action<T, ParseNode, OpenApiDocument>> fixedFields,
            Dictionary<Func<string, bool>, Action<T, string, ParseNode, OpenApiDocument>> patternFields,
            OpenApiDocument hostDocument)
        {
            if (fixedFields.TryGetValue(Name, out var fixedFieldMap))
            {
                try
                {
                    Context.StartObject(Name);
                    fixedFieldMap(parentInstance, Value, hostDocument);
                }
                catch (OpenApiReaderException ex)
                {
                    Context.Diagnostic.Errors.Add(new(ex));
                }
                catch (OpenApiException ex)
                {
                    ex.Pointer = Context.GetLocation();
                    Context.Diagnostic.Errors.Add(new(ex));
                }
                finally
                {
                    Context.EndObject();
                }
            }
            else
            {
                Action<T, string, ParseNode, OpenApiDocument>? map = null;
                foreach (var patternField in patternFields)
                {
                    if (patternField.Key(Name))
                    {
                        map = patternField.Value;
                        break;
                    }
                }
                if (map != null)
                {
                    try
                    {
                        Context.StartObject(Name);
                        map(parentInstance, Name, Value, hostDocument);
                    }
                    catch (OpenApiReaderException ex)
                    {
                        Context.Diagnostic.Errors.Add(new(ex));
                    }
                    catch (OpenApiException ex)
                    {
                        ex.Pointer = Context.GetLocation();
                        Context.Diagnostic.Errors.Add(new(ex));
                    }
                    finally
                    {
                        Context.EndObject();
                    }
                }
                else
                {
                    var error = new OpenApiError("", $"{Name} is not a valid property at {Context.GetLocation()}");
                    if ("$schema".Equals(Name, StringComparison.OrdinalIgnoreCase))
                    {
                        Context.Diagnostic.Warnings.Add(error);
                    }
                    else
                    {
                        Context.Diagnostic.Errors.Add(error);
                    }
                }
            }
        }

        public override JsonNode CreateAny()
        {
            throw new NotImplementedException();
        }
    }
}
