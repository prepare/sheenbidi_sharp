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

namespace SheenBidi.Internal
{
    internal class StatusStack
    {
        private class List
        {
            internal const int Length = 16;
            internal const int MaxIndex = (Length - 1);

            internal byte[] embeddingLevel = new byte[Length];
            internal CharType[] overrideStatus = new CharType[Length];
            internal bool[] isolateStatus = new bool[Length];

            internal List previous;
            internal List next;
        }

        private const int MaxElements = Level.MaxValue + 2;

        private readonly List firstList = new List();
        private List peekList;
        private int peekTop;
        private int size;

        internal StatusStack()
        {
            Reset();
        }

        internal void Reset()
        {
            peekList = firstList;
            peekTop = 0;
            size = 0;
        }

        internal int Count
        {
            get { return size; }
        }

        internal bool IsEmpty
        {
            get { return (size == 0); }
        }

        internal byte EmbeddingLevel
        {
            get { return peekList.embeddingLevel[peekTop]; }
        }

        internal CharType OverrideStatus
        {
            get { return peekList.overrideStatus[peekTop]; }
        }

        internal bool IsolateStatus
        {
            get { return peekList.isolateStatus[peekTop]; }
        }

        internal byte EvenLevel
        {
            get { return (byte)((EmbeddingLevel + 2) & ~1); }
        }

        internal byte OddLevel
        {
            get { return (byte)((EmbeddingLevel + 1) | 1); }
        }

        internal void Clear()
        {
            size = 0;
        }

        internal void Push(byte embeddingLevel, CharType overrideStatus, bool isolateStatus)
        {
#if DEBUG
            if (size == MaxElements)
                throw (new InvalidOperationException("The stack is full."));
#endif

            if (peekTop != List.MaxIndex)
            {
                ++peekTop;
            }
            else
            {
                if (peekList.next == null)
                {
                    List list = new List();
                    list.previous = peekList;
                    list.next = null;

                    peekList.next = list;
                    peekList = list;
                }
                else
                {
                    peekList = peekList.next;
                }

                peekTop = 0;
            }
            ++size;

            peekList.embeddingLevel[peekTop] = embeddingLevel;
            peekList.overrideStatus[peekTop] = overrideStatus;
            peekList.isolateStatus[peekTop] = isolateStatus;
        }

        internal void Pop()
        {
#if DEBUG
            if (size == 0)
                throw (new InvalidOperationException("The stack is empty."));
#endif

            if (peekTop != 0)
            {
                --peekTop;
            }
            else
            {
                peekList = peekList.previous;
                peekTop = List.MaxIndex;
            }
            --size;
        }
    }
}
