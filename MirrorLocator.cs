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
using SheenBidi.Data;

namespace SheenBidi
{
    public class MirrorLocator : IEnumerable<MirrorAgent>
    {
        private Line _line;

        private sealed class MirrorEnumerator : IEnumerator<MirrorAgent>
        {
            private string _text;
            private List<Run> _runs;

            private MirrorAgent _agent;
            private int _runIndex;
            private int _charIndex;

            internal MirrorEnumerator(Line line)
            {
                _text = line.Text;
                _runs = line.Runs;

                _agent = new MirrorAgent();
                _runIndex = 0;
                _charIndex = -1;
            }

            MirrorAgent IEnumerator<MirrorAgent>.Current
            {
                get { return _agent; }
            }

            object IEnumerator.Current
            {
                get { return _agent; }
            }

            bool IEnumerator.MoveNext()
            {
                int runCount = _runs.Count;
                for (; _runIndex < runCount; _runIndex++)
                {
                    Run run = _runs[_runIndex];

                    if ((run.level & 1) != 0)
                    {
                        int index = _charIndex;
                        int limit = run.offset + run.length;

                        if (index == -1)
                            index = run.offset;

                        for (; index < limit; index++)
                        {
                            int mirror = PairingLookup.DetermineMirror(_text[index]);

                            if (mirror != 0)
                            {
                                _charIndex = index + 1;
                                _agent.index = index;
                                _agent.mirror = mirror;

                                return true;
                            }
                        }
                    }

                    _charIndex = -1;
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

        public MirrorLocator()
        {
        }

        public void LoadLine(Line line)
        {
            _line = line;
        }

        public IEnumerator<MirrorAgent> GetEnumerator()
        {
            return (new MirrorEnumerator(_line));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
