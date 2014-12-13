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
using SheenBidi.Internal;

namespace SheenBidi
{
    public class MirrorLocator : IEnumerable<MirrorAgent>
    {
        private Line line;

        private sealed class MirrorEnumerator : IEnumerator<MirrorAgent>
        {
            private string text;
            private List<Run> runs;

            private MirrorAgent agent;
            private int runIndex;
            private int charIndex;

            internal MirrorEnumerator(Line line)
            {
                text = line.Text;
                runs = line.Runs;

                agent = new MirrorAgent();
                runIndex = 0;
                charIndex = -1;
            }

            MirrorAgent IEnumerator<MirrorAgent>.Current
            {
                get { return agent; }
            }

            object IEnumerator.Current
            {
                get { return agent; }
            }

            bool IEnumerator.MoveNext()
            {
                int runCount = runs.Count;
                for (; runIndex < runCount; runIndex++)
                {
                    Run run = runs[runIndex];

                    if ((run.level & 1) != 0)
                    {
                        int index = charIndex;
                        int limit = run.offset + run.length;

                        if (index == -1)
                            index = run.offset;

                        for (; index < limit; index++)
                        {
                            int mirror = PairingLookup.DetermineMirror(text[index]);

                            if (mirror != 0)
                            {
                                charIndex = index + 1;
                                agent.index = index;
                                agent.mirror = mirror;

                                return true;
                            }
                        }
                    }

                    charIndex = -1;
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
            this.line = line;
        }

        public IEnumerator<MirrorAgent> GetEnumerator()
        {
            return (new MirrorEnumerator(line));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
