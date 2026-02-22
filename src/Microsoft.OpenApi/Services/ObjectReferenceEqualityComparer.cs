// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.OpenApi
{
    /// <summary>
    /// An equality comparer that uses reference identity (object.ReferenceEquals)
    /// instead of the default Equals/GetHashCode. Works on all target frameworks
    /// including netstandard2.0.
    /// </summary>
    internal sealed class ObjectReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ObjectReferenceEqualityComparer<T> Default = new();

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
