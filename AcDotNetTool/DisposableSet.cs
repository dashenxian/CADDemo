﻿using System;
using System.Collections.Generic;
using System.Linq;
using AcDotNetTool;
using AcDotNetTool.Extensions;

namespace AcDotNetTool
{
    /// <summary>
    /// Describes a set of disposable values.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class DisposableSet<T> : HashSet<T>, IDisposableCollection<T>
        where T : IDisposable
    {
        /// <summary>
        /// Creates a new empty instance using the default comparer.
        /// </summary>
        public DisposableSet()
        { }

        /// <summary>
        /// Creates a new instance using the default comparer by copying the sequence items.
        /// </summary>
        /// <param name="collection">Sequence whose elements are copied into the new set.</param>
        public DisposableSet(IEnumerable<T> collection)
            : base(collection)
        { }

        /// <summary>
        /// Creates a new empty instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">IEqualityComparer&lt;T&gt; implementation.</param>
        public DisposableSet(IEqualityComparer<T> comparer)
            : base(comparer)
        { }

        /// <summary>
        /// Creates a new instance using the specified comparer by copying the sequence items.
        /// </summary>
        /// <param name="collection">Sequence whose elements are copied into the new set.</param>
        /// <param name="comparer">IEqualityComparer&lt;T&gt; implementation.</param>
        public DisposableSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : base(collection, comparer)
        { }

        /// <summary>
        /// Disposes of all items.
        /// </summary>
        public void Dispose()
        {
            if (0 < Count)
            {
                Exception last = null;
                var list = this.ToList();
                Clear();
                foreach (T item in list)
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Dispose();
                        }
                        catch (Exception ex)
                        {
                            last = last ?? ex;
                        }
                    }
                }
                if (last != null)
                    throw last;
            }
        }

        /// <summary>
        /// Adds items to the active instance.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            Assert.IsNotNull(items, nameof(items));
            UnionWith(items);
        }

        /// <summary>
        /// Removes items from the active instance.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        /// <returns>The sequence of effectively removed items.</returns>
        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            Assert.IsNotNull(items, nameof(items));
            ExceptWith(items);
            return items;
        }
    }
}
