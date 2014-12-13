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
using System.Collections;
using System.Collections.Generic;

namespace SheenBidi
{
    public class RunAdapter
    {
        private Line _line;

        private RunAgent _agent;
        private int _index;

        public RunAgent Agent
        {
            get { return _agent; }
        }

        public RunAdapter()
        {
            _agent = new RunAgent();
            Reset();
        }

        public void LoadLine(Line line)
        {
            _line = line;
            Reset();
        }

        public bool MoveNext()
        {
            if (_index < _line.Runs.Count)
            {
                Line.Run run = _line.Runs[_index];
                _agent.offset = run.offset;
                _agent.length = run.length;
                _agent.level = run.level;
                ++_index;

                return true;
            }

            Reset();
            return false;
        }

        public void Reset()
        {
            _index = 0;
            _agent.offset = -1;
            _agent.length = 0;
            _agent.level = byte.MaxValue;
        }
    }
}
