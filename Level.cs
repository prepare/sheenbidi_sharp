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

using SheenBidi.Data;

namespace SheenBidi
{
    internal static class Level
    {
        internal const byte MaxValue = 125;
        internal const byte MinValue = 0;

        internal static CharType LevelToEmbeddingType(byte level)
        {
            if ((level & 1) == 0)
                return CharType.L;

            return CharType.R;
        }

        internal static CharType LevelToOppositeType(byte level)
        {
            if ((level & 1) == 0)
                return CharType.R;

            return CharType.L;
        }
    }
}