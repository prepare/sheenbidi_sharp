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
        private Line line;

        private sealed class RunEnumerator : IEnumerator<RunAgent>
        {
            private RunAgent agent;

            private List<Run> runs;
            private int index;

            internal RunEnumerator(Line line)
            {
                agent = new RunAgent();

                runs = line.Runs;
                index = 0;
            }

            RunAgent IEnumerator<RunAgent>.Current
            {
                get { return agent; }
            }

            object IEnumerator.Current
            {
                get { return agent; }
            }

            bool IEnumerator.MoveNext()
            {
                if (index < runs.Count)
                {
                    Run run = runs[index];
                    agent.offset = run.offset;
                    agent.length = run.length;
                    agent.level = run.level;
                    ++index;

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
            this.line = line;
        }

        public IEnumerator<RunAgent> GetEnumerator()
        {
            return (new RunEnumerator(line));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
