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
    internal class RunQueue
    {
        private class List
        {
            internal const int Length = 8;
            internal const int MaxIndex = (Length - 1);

            internal readonly LevelRun[] levelRuns = new LevelRun[Length];

            internal List previous;
            internal List next;
        }

        private List frontList;
        private int frontTop;

        private List rearList;
        private int rearTop;

        private List isolatingList;
        private int isolatingTop;

        private int size;

        internal RunQueue()
        {
            this.frontList = new List();
            this.frontTop = 0;

            this.rearList = this.frontList;
            this.rearTop = -1;

            this.isolatingList = null;
            this.isolatingTop = -1;

            this.size = 0;
        }

        internal bool IsEmpty
        {
            get { return (size == 0); }
        }

        internal int Count
        {
            get { return size; }
        }

        internal bool ShouldDequeue
        {
            get { return (isolatingTop == -1); }
        }

        internal void Enqueue(LevelRun levelRun)
        {
            if (rearTop == List.MaxIndex)
            {
                List list = rearList.next;
                if (list == null)
                {
                    list = new List();
                    list.previous = rearList;
                    list.next = null;

                    rearList.next = list;
                }

                rearList = list;
                rearTop = 0;
            }
            else
            {
                ++rearTop;
            }

            ++size;
            rearList.levelRuns[rearTop] = levelRun;

            // Complete the latest isolating run with this terminating run.
            if (isolatingTop != -1 && levelRun.IsIsolateTerminator)
            {
                LevelRun incompleteRun = isolatingList.levelRuns[isolatingTop];
                incompleteRun.AttachLevelRun(levelRun);
                FindPreviousIncompleteRun();
            }

            // Save the location of the isolating run.
            if (levelRun.IsIsolateInitiator)
            {
                isolatingList = rearList;
                isolatingTop = rearTop;
            }
        }

        internal void Dequeue()
        {
            if (frontTop == List.MaxIndex)
            {
                if (frontList == rearList)
                    rearTop = -1;
                else
                    frontList = frontList.next;

                frontTop = 0;
            }
            else
            {
                ++frontTop;
            }

            --size;
        }

        internal LevelRun Peek()
        {
            return frontList.levelRuns[frontTop];
        }

        private void FindPreviousIncompleteRun()
        {
            List list = isolatingList;
            int top = isolatingTop;

            do
            {
                int limit = (list == frontList ? frontTop : 0);

                do
                {
                    LevelRun levelRun = list.levelRuns[top];
                    if (levelRun.IsPartialIsolate)
                    {
                        isolatingList = list;
                        isolatingTop = top;
                        return;
                    }
                } while (top-- > limit);

                list = list.previous;
                top = List.MaxIndex;
            } while (list != null);

            isolatingList = null;
            isolatingTop = -1;
        }
    }
}
