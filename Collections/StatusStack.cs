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
using SheenBidi.Data;

namespace SheenBidi.Collections
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

        private readonly List _firstList = new List();
        private List _peekList;
        private int _peekTop;
        private int _size;

        internal StatusStack()
        {
            Reset();
        }

        internal void Reset()
        {
            _peekList = _firstList;
            _peekTop = 0;
            _size = 0;
        }

        internal int Count
        {
            get { return _size; }
        }

        internal bool IsEmpty
        {
            get { return (_size == 0); }
        }

        internal byte EmbeddingLevel
        {
            get { return _peekList.embeddingLevel[_peekTop]; }
        }

        internal CharType OverrideStatus
        {
            get { return _peekList.overrideStatus[_peekTop]; }
        }

        internal bool IsolateStatus
        {
            get { return _peekList.isolateStatus[_peekTop]; }
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
            _size = 0;
        }

        internal void Push(byte embeddingLevel, CharType overrideStatus, bool isolateStatus)
        {
#if DEBUG
            if (_size == MaxElements)
                throw (new InvalidOperationException("The stack is full."));
#endif

            if (_peekTop != List.MaxIndex)
            {
                ++_peekTop;
            }
            else
            {
                if (_peekList.next == null)
                {
                    List list = new List();
                    list.previous = _peekList;
                    list.next = null;

                    _peekList.next = list;
                    _peekList = list;
                }
                else
                {
                    _peekList = _peekList.next;
                }

                _peekTop = 0;
            }
            ++_size;

            _peekList.embeddingLevel[_peekTop] = embeddingLevel;
            _peekList.overrideStatus[_peekTop] = overrideStatus;
            _peekList.isolateStatus[_peekTop] = isolateStatus;
        }

        internal void Pop()
        {
#if DEBUG
            if (_size == 0)
                throw (new InvalidOperationException("The stack is empty."));
#endif

            if (_peekTop != 0)
            {
                --_peekTop;
            }
            else
            {
                _peekList = _peekList.previous;
                _peekTop = List.MaxIndex;
            }
            --_size;
        }
    }
}
