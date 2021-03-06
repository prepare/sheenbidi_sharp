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

namespace SheenBidi.Collections
{
    internal class BidiChain
    {
        private BidiLink _roller;
        private BidiLink _last;

        public BidiLink RollerLink
        {
            get { return _roller; }
        }

        public BidiLink LastLink
        {
            get { return _last; }
        }

        public BidiChain()
        {
            _roller = new BidiLink();
            _last = _roller;
        }

        public void AddLink(BidiLink link)
        {
            link.ReplaceNext(_roller);
            _last.ReplaceNext(link);
            _last = link;
        }
    }
}
