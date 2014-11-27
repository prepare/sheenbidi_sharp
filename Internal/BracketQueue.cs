// Copyright (C) 2014 Muhammad Tayyab Akram
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace SheenBidi.Internal
{
    internal class BracketQueue
    {
        private class List
        {
            internal const int Length = 8;
            internal const int MaxIndex = (Length - 1);

            internal readonly BracketPair[] bracketPairs = new BracketPair[Length];

            internal List previous;
            internal List next;
        }

        private List frontList;
        private int frontTop;

        private List rearList;
        private int rearTop;

        private CharType direction;
        private bool shouldDequeue;
        private int size;

        internal BracketQueue(CharType direction)
        {
            Clear(direction);
        }

        internal int Count
        {
            get { return size; }
        }

        internal bool IsEmpty
        {
            get { return (size == 0); }
        }

        internal bool ShouldDequeue
        {
            get { return shouldDequeue; }
        }

        internal void Clear(CharType direction)
        {
            this.frontList = new List();
            this.frontTop = 0;

            this.rearList = frontList;
            this.rearTop = -1;

            this.direction = direction;
            this.shouldDequeue = false;
            this.size = 0;
        }

        internal void Enqueue(BracketPair bracketPair)
        {
            if (rearTop == List.MaxIndex)
            {
                List list = rearList.next;
                if (list == null)
                {
                    list = new List();
                    list.previous = rearList;
                    list.next = null;

                    rearList.next = list;
                }

                rearList = list;
                rearTop = 0;
            }
            else
            {
                ++rearTop;
            }

            rearList.bracketPairs[rearTop] = bracketPair;
            ++size;
        }

        internal void Dequeue()
        {
#if DEBUG
            if (this.IsEmpty)
                throw (new InvalidOperationException("The queue is empty."));
#endif

            if (frontTop == List.MaxIndex)
            {
                if (frontList == rearList)
                    rearTop = -1;
                else
                    frontList = frontList.next;

                frontTop = 0;
            }
            else
            {
                ++frontTop;
            }

            --size;
        }

        internal BracketPair Peek()
        {
            return frontList.bracketPairs[frontTop];
        }

        internal void SetStrongType(CharType strongType)
        {
            List list = rearList;
            int top = rearTop;

            for (; ; )
            {
                int limit = (list == frontList ? frontTop : 0);

                do
                {
                    BracketPair pair = list.bracketPairs[top];
                    if (pair.closingLink == null && pair.strongType != direction)
                    {
                        pair.strongType = strongType;
                    }
                } while (top-- > limit);

                if (list == frontList)
                    break;

                list = list.previous;
                top = List.MaxIndex;
            };
        }

        internal void ClosePair(BidiLink closingLink, char bracket)
        {
            List list = rearList;
            int top = rearTop;

            for (; ; )
            {
                bool isFrontList = (list == frontList);
                int limit = (isFrontList ? frontTop : 0);

                do
                {
                    BracketPair pair = list.bracketPairs[top];
                    if (pair.openingLink != null
                        && pair.closingLink == null
                        && pair.bracket == bracket)
                    {
                        pair.closingLink = closingLink;
                        InvalidatePairs(list, top);

                        if (isFrontList && top == frontTop)
                            shouldDequeue = true;

                        return;
                    }
                } while (top-- > limit);

                if (isFrontList)
                    break;

                list = list.previous;
                top = List.MaxIndex;
            };
        }

        private void InvalidatePairs(List list, int top)
        {
            do
            {
                int limit = (list == rearList ? rearTop : List.MaxIndex);

                while (++top <= limit)
                {
                    BracketPair pair = list.bracketPairs[top];
                    if (pair.openingLink != null
                        && pair.closingLink == null)
                    {
                        pair.openingLink = null;
                    }
                };

                list = list.next;
                top = 0;
            } while (list != null);
        }
    }
}
