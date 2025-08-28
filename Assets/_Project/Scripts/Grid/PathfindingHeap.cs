using System;
using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Binary heap implementation for efficient pathfinding priority queue
    /// Optimized for A* pathfinding where we need frequent min-extractions and updates
    /// </summary>
    public class PathfindingHeap<T> where T : class, IComparable<T>
    {
        private T[] items;
        private int currentItemCount;

        public int Count => currentItemCount;

        public PathfindingHeap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        /// <summary>
        /// Add item to the heap
        /// </summary>
        public void Add(T item)
        {
            if (item is GridNode node)
            {
                node.HeapIndex = currentItemCount;
            }
            
            items[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        /// <summary>
        /// Remove and return the first (highest priority) item
        /// </summary>
        public T RemoveFirst()
        {
            if (currentItemCount == 0)
                return null;

            T firstItem = items[0];
            currentItemCount--;
            
            items[0] = items[currentItemCount];
            if (items[0] is GridNode node)
            {
                node.HeapIndex = 0;
            }
            
            SortDown(items[0]);
            return firstItem;
        }

        /// <summary>
        /// Update item position in heap (call after changing its priority)
        /// </summary>
        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        /// <summary>
        /// Check if heap contains the specified item
        /// </summary>
        public bool Contains(T item)
        {
            if (item is GridNode node)
            {
                return node.HeapIndex < currentItemCount && 
                       items[node.HeapIndex] != null && 
                       items[node.HeapIndex].Equals(item);
            }
            
            // Fallback to linear search for non-GridNode items
            for (int i = 0; i < currentItemCount; i++)
            {
                if (items[i].Equals(item))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Clear all items from the heap
        /// </summary>
        public void Clear()
        {
            currentItemCount = 0;
            // Optional: clear references to help with garbage collection
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = null;
            }
        }

        /// <summary>
        /// Sort item up the heap towards root
        /// </summary>
        private void SortUp(T item)
        {
            if (!(item is GridNode node)) return;
            
            int parentIndex = (node.HeapIndex - 1) / 2;
            
            while (true)
            {
                if (parentIndex < 0) break;
                
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) >= 0) break;
                
                Swap(item, parentItem);
                parentIndex = (node.HeapIndex - 1) / 2;
            }
        }

        /// <summary>
        /// Sort item down the heap towards leaves
        /// </summary>
        private void SortDown(T item)
        {
            if (!(item is GridNode node)) return;
            
            while (true)
            {
                int childIndexLeft = node.HeapIndex * 2 + 1;
                int childIndexRight = node.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;
                    
                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) > 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }
                    
                    if (item.CompareTo(items[swapIndex]) > 0)
                    {
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Swap two items in the heap
        /// </summary>
        private void Swap(T itemA, T itemB)
        {
            if (!(itemA is GridNode nodeA) || !(itemB is GridNode nodeB)) return;
            
            items[nodeA.HeapIndex] = itemB;
            items[nodeB.HeapIndex] = itemA;
            
            int itemAIndex = nodeA.HeapIndex;
            nodeA.HeapIndex = nodeB.HeapIndex;
            nodeB.HeapIndex = itemAIndex;
        }

        /// <summary>
        /// Get heap statistics for debugging
        /// </summary>
        public HeapStatistics GetStatistics()
        {
            return new HeapStatistics
            {
                CurrentSize = currentItemCount,
                MaxSize = items.Length,
                MemoryUsage = items.Length * System.Runtime.InteropServices.Marshal.SizeOf(typeof(T))
            };
        }
    }

    /// <summary>
    /// Statistics for heap performance monitoring
    /// </summary>
    public struct HeapStatistics
    {
        public int CurrentSize;
        public int MaxSize;
        public int MemoryUsage;
        
        public float UsagePercentage => MaxSize > 0 ? (float)CurrentSize / MaxSize * 100f : 0f;
    }
}
