﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Heap
{
    public class FibonacciHeap<T> : IFibonacciHeap<T>
    {
        private List<FibHeapNode<T>> Roots;
        private FibHeapNode<T> Min;

        public FibonacciHeap()
        {
            Roots = new List<FibHeapNode<T>>();
            Min = default(FibHeapNode<T>);
        }

        // check for emptiness
        public bool Empty()
        {
            return (Roots.Count() > 0) ? false : true;
        }

        // insert an element
        public FibHeapHandle<T> Insert(T Element, double Key)
        {
            // create new tree
            var newTree = new FibHeapNode<T>(Element, Key);

            AddToRoot(newTree);
            ConsolidateTrees();

            var handle = new FibHeapHandle<T>(newTree, this);
            newTree.HandleTo = handle;
            return handle;
        }

        // get min element and update heap
        public T DeleteMin()
        {
            T minElement = Min.Element;

            // merge children of min into roots list
            foreach (var child in Min.Children) AddToRoot(child);

            Roots.Remove(Min);
            UpdateMin();
            ConsolidateTrees();
            if (Min.HandleTo != null) Min.HandleTo.ValidHandle = false;
            return minElement;
        }

        public void UpdateKey(FibHeapHandle<T> handle, double newKey)
        {
            if (handle.ValidHandle && handle.ParentHeap == this)
            {
                if (newKey < handle.Element.Key) DecreaseKey(handle.Element, newKey);
                else IncreaseKey(handle.Element, newKey);
            }
        }

        // scan roots for min element
        private void UpdateMin()
        {
            if (Roots.Count > 0)
            {
                Min = Roots.First();
                foreach (var root in Roots)
                {
                    if (root.Key < Min.Key) Min = root;
                }
            }
        }

        // merge two trees
        private void Link(FibHeapNode<T> tree1, FibHeapNode<T> tree2)
        {
            if (tree1.Key > tree2.Key)
            {
                Link(tree2, tree1);
            }
            else
            {
                tree1.Rank += tree2.Rank + 1;
                tree1.Children.Add(tree2);
                tree2.Parent = tree1;
            }
        }

        // link any root trees of same rank
        private void ConsolidateTrees()
        {
            while (InnerConsolidateTrees()) ;
        }

        // having inner function prevents stack from overflowing with too many recursions
        private bool InnerConsolidateTrees()
        {
            bool linkNeeded = false;

            var enumerator1 = Roots.GetEnumerator();
            FibHeapNode<T> tree1 = null, tree2 = null;

            while (enumerator1.MoveNext() && !linkNeeded)
            {
                var enumerator2 = enumerator1;
                while (enumerator2.MoveNext() && !linkNeeded)
                {
                    if (enumerator1.Current.Rank == enumerator2.Current.Rank)
                    {
                        linkNeeded = true;
                        if (enumerator1.Current.Key < enumerator2.Current.Key)
                        {
                            tree1 = enumerator1.Current;
                            tree2 = enumerator2.Current;
                        }
                        else
                        {
                            tree1 = enumerator2.Current;
                            tree2 = enumerator1.Current;
                        }
                    }
                }
            }

            if (linkNeeded)
            {
                Roots.Remove(tree2);
                Link(tree1, tree2);
            }

            return linkNeeded;
        }

        // prune the tree
        private void CutOrMark(FibHeapNode<T> Tree)
        {
            if (Tree.Marked == true)
            {
                Tree.Marked = false;
                Tree.Parent.Children.Remove(Tree);
                Tree.Parent.Rank -= Tree.Rank;
                Roots.Add(Tree);
                if (Tree.Key < Min.Key) UpdateMin();
                ConsolidateTrees();
                CutOrMark(Tree.Parent);
            }
            else
            {
                Tree.Marked = true;
            }
        }

        // decrease key value and updates heap
        private void DecreaseKey(FibHeapNode<T> Tree, double newKey)
        {
            if (Tree.HandleTo.ValidHandle)
            {
                if (Tree.Parent == null)
                { // element is a root
                    Tree.Key = newKey;
                    if (newKey < Min.Key) UpdateMin();
                }
                else if (Tree.Parent.Key < newKey)
                { // heap order not violated
                    Tree.Key = newKey;
                }
                else
                { // MAYDAY MAYDAY, HEAP ORDER HAS BEEN VIOLATED, I REPEAT, HEAP ORDER HAS BEEN VIOLATED
                    Tree.Key = newKey;
                    Tree.Marked = true;
                    CutOrMark(Tree);
                }
            }
        }

        // increases key value and updates heap
        private void IncreaseKey(FibHeapNode<T> Tree, double newKey)
        {
            if (Tree.HandleTo.ValidHandle)
            {
                Tree.Key = newKey;

                var HeapViolators = from child in Tree.Children
                                    where child.Key < newKey
                                    select child;

                foreach (var child in HeapViolators)
                {
                    Tree.Children.Remove(child);
                    Tree.Rank -= child.Rank;
                    AddToRoot(child);
                }

                UpdateMin();
                ConsolidateTrees();
            }
        }

        private void AddToRoot(FibHeapNode<T> Tree)
        {
            Tree.Marked = false;
            if (Roots.Count() > 0)
            {
                if (Tree.Key < Min.Key) Min = Tree;
            }
            else
            {
                Min = Tree;
            }

            Roots.Add(Tree);
        }
    }
}
