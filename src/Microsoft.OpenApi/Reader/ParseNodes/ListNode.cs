// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Microsoft.OpenApi.Reader
{
    internal class ListNode : ParseNode, IEnumerable<ParseNode>
    {
        private readonly JsonArray _nodeList;

        public ListNode(ParsingContext context, JsonArray jsonArray) : base(
            context, jsonArray)
        {
            _nodeList = jsonArray;
        }

        public override List<T> CreateList<T>(Func<MapNode, OpenApiDocument, T> map, OpenApiDocument hostDocument)
        {
            if (_nodeList == null)
            {
                throw new OpenApiReaderException($"Expected list while parsing {typeof(T).Name}");
            }

            var list = new List<T>(_nodeList.Count);
            foreach (var item in _nodeList)
            {
                if (item is JsonObject jsonObject)
                {
                    var value = map(new MapNode(Context, jsonObject), hostDocument);
                    if (value != null)
                        list.Add(value);
                }
            }
            return list;
        }

        public override List<JsonNode> CreateListOfAny()
        {
            var list = new List<JsonNode>(_nodeList.Count);
            foreach (var item in _nodeList)
            {
                if (item is not null)
                {
                    var any = Create(Context, item).CreateAny();
                    if (any != null)
                        list.Add(any);
                }
            }
            return list;
        }

        public override List<T> CreateSimpleList<T>(Func<ValueNode, OpenApiDocument?, T> map, OpenApiDocument openApiDocument)
        {
            if (_nodeList == null)
            {
                throw new OpenApiReaderException($"Expected list while parsing {typeof(T).Name}");
            }

            var list = new List<T>(_nodeList.Count);
            foreach (var item in _nodeList)
            {
                if (item is not null)
                    list.Add(map(new ValueNode(Context, item), openApiDocument));
            }
            return list;
        }

        public IEnumerator<ParseNode> GetEnumerator()
        {
            foreach (var item in _nodeList)
            {
                if (item is not null)
                    yield return Create(Context, item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Create a <see cref="JsonArray"/>
        /// </summary>
        /// <returns>The created Any object.</returns>
        public override JsonNode CreateAny()
        {
            return _nodeList;
        }
    }
}
