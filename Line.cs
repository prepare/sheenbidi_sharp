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
using System.Collections.Generic;
using SheenBidi.Data;

namespace SheenBidi
{
    internal class Run
    {
        internal int offset;
        internal int length;
        internal byte level;

        internal Run(int offset, int length, byte level)
        {
            this.offset = offset;
            this.length = length;
            this.level = level;
        }
    }

    public class Line
    {
        #region Variables

        private string _text;
        private int _offset;
        private int _length;
        private List<Run> _runs = new List<Run>();

        #endregion Variables

        #region Properties

        public string Text
        {
            get { return _text; }
        }

        public int Offset
        {
            get { return _offset; }
        }

        public int Length
        {
            get { return _length; }
        }

        internal List<Run> Runs
        {
            get { return _runs; }
        }

        #endregion Properties

        #region Constructors

        public Line(Paragraph paragraph)
            : this(paragraph, 0, paragraph.Text.Length)
        {
        }

        public Line(Paragraph paragraph, int offset, int length)
        {
            _text = paragraph.Text;
            _offset = offset;
            _length = length;

            byte[] levels = new byte[length];
            Array.Copy(paragraph.Levels, offset, levels, 0, length);

            Initialize(paragraph.Types, levels, paragraph.BaseLevel);
        }

        public Line(string text)
            : this(text, BaseDirection.AutoLeftToRight)
        {
        }

        public Line(string text, BaseDirection direction)
        {
            if (string.IsNullOrEmpty(text))
                throw (new ArgumentException("Text is empty."));

            _text = text;
            _offset = 0;
            _length = text.Length;

            Paragraph paragraph = new Paragraph(text, direction);
            Initialize(paragraph.Types, paragraph.Levels, paragraph.BaseLevel);
        }

        private void Initialize(CharType[] types, byte[] levels, byte baseLevel)
        {
            ResetLevels(types, levels, baseLevel);
            byte maxLevel = DetermineRuns(levels);
            ReorderRuns(maxLevel);
        }

        #endregion Constructors

        #region Reset Levels

        private void ResetLevels(CharType[] types, byte[] levels, byte baseLevel)
        {
            bool reset = true;
            int resetLength = 0;

            for (int index = _length - 1; index >= 0; index--)
            {
                CharType type = types[index + _offset];

                switch (type)
                {
                    case CharType.B:
                    case CharType.S:
                        SetNewLevel(levels, index, resetLength + 1, baseLevel);
                        resetLength = 0;
                        reset = true;
                        break;

                    case CharType.LRE:
                    case CharType.RLE:
                    case CharType.LRO:
                    case CharType.RLO:
                    case CharType.PDF:
                    case CharType.BN:
                        ++resetLength;
                        break;

                    case CharType.WS:
                    case CharType.LRI:
                    case CharType.RLI:
                    case CharType.FSI:
                    case CharType.PDI:
                        if (reset)
                        {
                            SetNewLevel(levels, index, resetLength + 1, baseLevel);
                            resetLength = 0;
                        }
                        break;

                    default:
                        resetLength = 0;
                        reset = false;
                        break;
                }
            }
        }

        private void SetNewLevel(byte[] levels, int index, int length, byte newLevel)
        {
            int limitIndex = index + length;
            for (; index < limitIndex; index++)
            {
                levels[index] = newLevel;
            }
        }

        #endregion Reset Levels

        #region Determine Runs

        private byte DetermineRuns(byte[] levels)
        {
            Run priorRun = new Run(0, 0, levels[0]);
            _runs.Clear();
            _runs.Add(priorRun);

            byte maxLevel = 0;
            int length = levels.Length;

            for (int index = 0; index < length; index++)
            {
                byte level = levels[index];

                if (level > maxLevel)
                {
                    maxLevel = level;
                }

                if (level != priorRun.level)
                {
                    priorRun.length = index - priorRun.offset;

                    Run run = new Run(index, 0, level);
                    _runs.Add(run);

                    priorRun = run;
                }
            }

            priorRun.length = length - priorRun.offset;

            return maxLevel;
        }

        #endregion Determine Runs

        #region Reorder Runs

        private void ReorderRuns(byte maxLevel)
        {
            for (int newLevel = maxLevel; newLevel > 0; newLevel--)
            {
                for (int index = _runs.Count - 1; index >= 0; index--)
                {
                    if (_runs[index].level >= newLevel)
                    {
                        int reverseCount = 1;

                        for (; index > 0 && _runs[index - 1].level >= newLevel; --index)
                        {
                            ++reverseCount;
                        }

                        ReverseRuns(index, reverseCount);
                    }
                }
            }
        }

        private void ReverseRuns(int index, int count)
        {
            int halfCount = count / 2;
            int finalIndex = index + count - 1;

            for (int i = 0; i < halfCount; ++i)
            {
                int newIndex = index + i;
                int tieIndex = finalIndex - i;

                Run tempRun = _runs[newIndex];
                _runs[newIndex] = _runs[tieIndex];
                _runs[tieIndex] = tempRun;
            }
        }

        #endregion Reorder Runs
    }
}
