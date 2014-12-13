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
            public const int Length = 16;
            public const int MaxIndex = (Length - 1);

            public readonly byte[] EmbeddingLevel = new byte[Length];
            public readonly CharType[] OverrideStatus = new CharType[Length];
            public readonly bool[] IsolateStatus = new bool[Length];

            public List previous;
            public List next;
        }

        private const int MaxElements = Level.MaxValue + 2;

        private readonly List _firstList = new List();
        private List _peekList;
        private int _peekTop;
        private int _size;

        public StatusStack()
        {
            Reset();
        }

        public void Reset()
        {
            _peekList = _firstList;
            _peekTop = 0;
            _size = 0;
        }

        public int Count
        {
            get { return _size; }
        }

        public bool IsEmpty
        {
            get { return (_size == 0); }
        }

        public byte EmbeddingLevel
        {
            get { return _peekList.EmbeddingLevel[_peekTop]; }
        }

        public CharType OverrideStatus
        {
            get { return _peekList.OverrideStatus[_peekTop]; }
        }

        public bool IsolateStatus
        {
            get { return _peekList.IsolateStatus[_peekTop]; }
        }

        public byte EvenLevel
        {
            get { return (byte)((EmbeddingLevel + 2) & ~1); }
        }

        public byte OddLevel
        {
            get { return (byte)((EmbeddingLevel + 1) | 1); }
        }

        public void Clear()
        {
            _size = 0;
        }

        public void Push(byte embeddingLevel, CharType overrideStatus, bool isolateStatus)
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

            _peekList.EmbeddingLevel[_peekTop] = embeddingLevel;
            _peekList.OverrideStatus[_peekTop] = overrideStatus;
            _peekList.IsolateStatus[_peekTop] = isolateStatus;
        }

        public void Pop()
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
