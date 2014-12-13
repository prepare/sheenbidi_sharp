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

        private List _frontList;
        private int _frontTop;

        private List _rearList;
        private int _rearTop;

        private CharType _direction;
        private bool _shouldDequeue;
        private int _size;

        internal BracketQueue(CharType direction)
        {
            Clear(direction);
        }

        internal int Count
        {
            get { return _size; }
        }

        internal bool IsEmpty
        {
            get { return (_size == 0); }
        }

        internal bool ShouldDequeue
        {
            get { return _shouldDequeue; }
        }

        internal void Clear(CharType direction)
        {
            _frontList = new List();
            _frontTop = 0;

            _rearList = _frontList;
            _rearTop = -1;

            _direction = direction;
            _shouldDequeue = false;
            _size = 0;
        }

        internal void Enqueue(BracketPair bracketPair)
        {
            if (_rearTop == List.MaxIndex)
            {
                List list = _rearList.next;
                if (list == null)
                {
                    list = new List();
                    list.previous = _rearList;
                    list.next = null;

                    _rearList.next = list;
                }

                _rearList = list;
                _rearTop = 0;
            }
            else
            {
                ++_rearTop;
            }

            _rearList.bracketPairs[_rearTop] = bracketPair;
            ++_size;
        }

        internal void Dequeue()
        {
#if DEBUG
            if (this.IsEmpty)
                throw (new InvalidOperationException("The queue is empty."));
#endif

            if (_frontTop == List.MaxIndex)
            {
                if (_frontList == _rearList)
                    _rearTop = -1;
                else
                    _frontList = _frontList.next;

                _frontTop = 0;
            }
            else
            {
                ++_frontTop;
            }

            --_size;
        }

        internal BracketPair Peek()
        {
            return _frontList.bracketPairs[_frontTop];
        }

        internal void SetStrongType(CharType strongType)
        {
            List list = _rearList;
            int top = _rearTop;

            for (; ; )
            {
                int limit = (list == _frontList ? _frontTop : 0);

                do
                {
                    BracketPair pair = list.bracketPairs[top];
                    if (pair.closingLink == null && pair.strongType != _direction)
                    {
                        pair.strongType = strongType;
                    }
                } while (top-- > limit);

                if (list == _frontList)
                    break;

                list = list.previous;
                top = List.MaxIndex;
            };
        }

        internal void ClosePair(BidiLink closingLink, char bracket)
        {
            List list = _rearList;
            int top = _rearTop;

            for (; ; )
            {
                bool isFrontList = (list == _frontList);
                int limit = (isFrontList ? _frontTop : 0);

                do
                {
                    BracketPair pair = list.bracketPairs[top];
                    if (pair.openingLink != null
                        && pair.closingLink == null
                        && pair.bracket == bracket)
                    {
                        pair.closingLink = closingLink;
                        InvalidatePairs(list, top);

                        if (isFrontList && top == _frontTop)
                            _shouldDequeue = true;

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
                int limit = (list == _rearList ? _rearTop : List.MaxIndex);

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
