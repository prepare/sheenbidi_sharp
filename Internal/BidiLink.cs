﻿// Copyright (C) 2014 Muhammad Tayyab Akram
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
    internal class BidiLink
    {
        internal int offset;
        internal int length;
        internal CharType type;
        internal byte level;
        private BidiLink next;

        internal BidiLink Next
        {
            get { return next; }
        }

        internal void AbandonNext()
        {
            this.next = this.next.next;
        }

        internal void MergeNext()
        {
            BidiLink firstNext = this.next;
            BidiLink secondNext = firstNext.next;
            this.next = secondNext;
            this.length += firstNext.length;
        }

        internal void ReplaceNext(BidiLink next)
        {
            this.next = next;
        }
    }
}
