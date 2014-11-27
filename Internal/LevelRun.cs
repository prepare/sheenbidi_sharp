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

namespace SheenBidi.Internal
{
    internal class LevelRun
    {
        private enum Kind : byte
        {
            Simple = 0,
            Isolate = 1,
            Partial = 2,
            Terminating = 4,
            Attached = 8,
        }

        private enum Extrema : byte
        {
            None = 0,
            SOR_L = 1,
            SOR_R = 2,
            EOR_L = 4,
            EOR_R = 8,
        }

        internal readonly BidiLink firstLink;
        internal readonly BidiLink lastLink;
        internal readonly BidiLink subsequentLink;
        private Extrema extrema;
        private Kind kind;
        private LevelRun next;

        internal CharType SOR
        {
            get
            {
                if ((extrema & Extrema.SOR_L) != 0)
                    return CharType.L;

                return CharType.R;
            }
        }

        internal CharType EOR
        {
            get
            {
                if ((extrema & Extrema.EOR_L) != 0)
                    return CharType.L;

                return CharType.R;
            }
        }

        internal byte Level
        {
            get { return firstLink.level; }
        }

        internal LevelRun Next
        {
            get { return next; }
        }

        internal bool IsSimple
        {
            get { return (kind == Kind.Simple); }
        }

        internal bool IsIsolateInitiator
        {
            get { return ((kind & Kind.Isolate) != 0); }
        }

        internal bool IsIsolateTerminator
        {
            get { return ((kind & Kind.Terminating) != 0); }
        }

        internal bool IsPartialIsolate
        {
            get { return ((kind & Kind.Partial) != 0); }
        }

        internal bool IsAttachedTerminator
        {
            get { return ((kind & Kind.Attached) != 0); }
        }

        internal LevelRun(BidiLink firstLink, BidiLink lastLink, CharType sor, CharType eor)
        {
            this.firstLink = firstLink;
            this.lastLink = lastLink;

            switch (sor)
            {
                case CharType.L:
                    this.extrema |= Extrema.SOR_L;
                    break;

                case CharType.R:
                    this.extrema |= Extrema.SOR_R;
                    break;

#if DEBUG
                default:
                    throw (new ArgumentException("The type of SOR should be either L or R."));
#endif
            }

            switch (eor)
            {
                case CharType.L:
                    this.extrema |= Extrema.EOR_L;
                    break;

                case CharType.R:
                    this.extrema |= Extrema.EOR_R;
                    break;

#if DEBUG
                default:
                    throw (new ArgumentException("The type of EOR should be either L or R."));
#endif
            }

            // An isolating run ends at an isolating initiator.
            switch (lastLink.type)
            {
                case CharType.LRI:
                case CharType.RLI:
                case CharType.FSI:
                    this.kind |= Kind.Isolate | Kind.Partial;
                    break;
            }

            // A terminating run starts with a PDI.
            if (firstLink.type == CharType.PDI)
                this.kind |= Kind.Terminating;

            this.subsequentLink = lastLink.Next;
        }

        internal void AttachLevelRun(LevelRun levelRun)
        {
#if DEBUG
            if (this.Level != levelRun.Level)
                throw (new ArgumentException("Cannot attach a level run of different level."));

            if (this.IsSimple)
                throw (new ArgumentException("Cannot attach another level run to a default run."));

            if (!this.IsPartialIsolate && this.IsIsolateInitiator)
                throw (new ArgumentException("Cannot attach another level run to a complete isolating run."));

            if (this.IsIsolateInitiator && !levelRun.IsIsolateTerminator)
                throw (new ArgumentException("Cannot attach a non terminating level run to an initiating level run."));
#endif

            if (levelRun.IsIsolateTerminator)
                levelRun.kind |= Kind.Attached;

            if (this.IsIsolateInitiator)
                kind &= ~Kind.Partial;

            next = levelRun;
        }
    }
}
