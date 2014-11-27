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
    internal partial class CharTypeLookup
    {
        internal static CharType DetermineCharType(int unicode)
        {
            if (unicode <= 0x10FFFD)
            {
                return (CharType)PrimaryData[
                                  MainIndexes[
                                   BranchIndexes[
                                         unicode / 0x2800
                                   ] + ((unicode % 0x2800) / 0x100)
                                  ] + (unicode % 0x100)
                                 ];
            }

            return CharType.L;
        }
    }
}
