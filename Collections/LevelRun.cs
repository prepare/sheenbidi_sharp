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

using System;
using SheenBidi.Data;

namespace SheenBidi.Collections
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

        public readonly BidiLink firstLink;
        public readonly BidiLink lastLink;
        public readonly BidiLink subsequentLink;
        private Extrema _extrema;
        private Kind _kind;
        private LevelRun _next;

        public CharType SOR
        {
            get
            {
                if ((_extrema & Extrema.SOR_L) != 0)
                    return CharType.L;

                return CharType.R;
            }
        }

        public CharType EOR
        {
            get
            {
                if ((_extrema & Extrema.EOR_L) != 0)
                    return CharType.L;

                return CharType.R;
            }
        }

        public byte Level
        {
            get { return firstLink.level; }
        }

        public LevelRun Next
        {
            get { return _next; }
        }

        public bool IsSimple
        {
            get { return (_kind == Kind.Simple); }
        }

        public bool IsIsolateInitiator
        {
            get { return ((_kind & Kind.Isolate) != 0); }
        }

        public bool IsIsolateTerminator
        {
            get { return ((_kind & Kind.Terminating) != 0); }
        }

        public bool IsPartialIsolate
        {
            get { return ((_kind & Kind.Partial) != 0); }
        }

        public bool IsAttachedTerminator
        {
            get { return ((_kind & Kind.Attached) != 0); }
        }

        public LevelRun(BidiLink firstLink, BidiLink lastLink, CharType sor, CharType eor)
        {
            this.firstLink = firstLink;
            this.lastLink = lastLink;

            switch (sor)
            {
                case CharType.L:
                    _extrema |= Extrema.SOR_L;
                    break;

                case CharType.R:
                    _extrema |= Extrema.SOR_R;
                    break;

#if DEBUG
                default:
                    throw (new ArgumentException("The type of SOR should be either L or R."));
#endif
            }

            switch (eor)
            {
                case CharType.L:
                    _extrema |= Extrema.EOR_L;
                    break;

                case CharType.R:
                    _extrema |= Extrema.EOR_R;
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
                    _kind |= Kind.Isolate | Kind.Partial;
                    break;
            }

            // A terminating run starts with a PDI.
            if (firstLink.type == CharType.PDI)
                _kind |= Kind.Terminating;

            this.subsequentLink = lastLink.Next;
        }

        public void AttachLevelRun(LevelRun levelRun)
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
                levelRun._kind |= Kind.Attached;

            if (this.IsIsolateInitiator)
                _kind &= ~Kind.Partial;

            _next = levelRun;
        }
    }
}
