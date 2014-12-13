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
    public class RunAdapter : IEnumerable<RunAgent>
    {
        private Line _line;

        private sealed class RunEnumerator : IEnumerator<RunAgent>
        {
            private RunAgent _agent;

            private List<Run> _runs;
            private int _index;

            internal RunEnumerator(Line line)
            {
                _agent = new RunAgent();

                _runs = line.Runs;
                _index = 0;
            }

            RunAgent IEnumerator<RunAgent>.Current
            {
                get { return _agent; }
            }

            object IEnumerator.Current
            {
                get { return _agent; }
            }

            bool IEnumerator.MoveNext()
            {
                if (_index < _runs.Count)
                {
                    Run run = _runs[_index];
                    _agent.offset = run.offset;
                    _agent.length = run.length;
                    _agent.level = run.level;
                    ++_index;

                    return true;
                }

                return false;
            }

            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }
        }

        public RunAdapter()
        {
        }

        public void LoadLine(Line line)
        {
            _line = line;
        }

        public IEnumerator<RunAgent> GetEnumerator()
        {
            return (new RunEnumerator(_line));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
