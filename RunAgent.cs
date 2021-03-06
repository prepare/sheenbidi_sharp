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

namespace SheenBidi
{
    public class RunAgent
    {
        internal int offset;
        internal int length;
        internal byte level;

        public int Offset
        {
            get { return offset; }
        }

        public int Length
        {
            get { return length; }
        }

        public byte Level
        {
            get { return level; }
        }

        public bool IsRightToLeft
        {
            get { return ((level & 1) != 0); }
        }
    }
}
