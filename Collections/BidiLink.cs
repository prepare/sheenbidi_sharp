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

using SheenBidi.Data;

namespace SheenBidi.Collections
{
    internal class BidiLink
    {
        public int offset;
        public int length;
        public CharType type;
        public byte level;
        private BidiLink _next;

        public BidiLink Next
        {
            get { return _next; }
        }

        public void AbandonNext()
        {
            _next = _next._next;
        }

        public void MergeNext()
        {
            BidiLink firstNext = _next;
            BidiLink secondNext = firstNext._next;
            _next = secondNext;
            length += firstNext.length;
        }

        public void ReplaceNext(BidiLink next)
        {
            _next = next;
        }
    }
}
