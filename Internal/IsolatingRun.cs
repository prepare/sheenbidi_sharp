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
    internal class IsolatingRun
    {
        private BracketQueue bracketQueue;

        private BidiLink roller;
        private LevelRun baseLevelRun;
        private LevelRun lastLevelRun;

        private CharType sos;
        private CharType eos;
        private byte level;

        private string text;
        private byte paragraphLevel;

        internal string Text
        {
            get { return text; }
            set { text = value; }
        }

        internal byte ParagraphLevel
        {
            get { return paragraphLevel; }
            set { paragraphLevel = value; }
        }

        internal LevelRun BaseLevelRun
        {
            get { return baseLevelRun; }
            set { baseLevelRun = value; }
        }

        internal IsolatingRun()
        {
            bracketQueue = new BracketQueue(CharType.L);
            roller = new BidiLink();
        }

        internal void Resolve()
        {
            // Attach level run links to form isolating run.
            AttachLevelRunLinks();
            // Save last subsequent link.
            BidiLink subsequentLink = lastLevelRun.subsequentLink;

            // Rules W1-W7
            BidiLink finalLink = ResolveWeakTypes();
            // Rule N0
            ResolveBrackets();
            // Rules N1, N2
            ResolveNeutrals();
            // Rules I1, I2
            ResolveImplicitLevels();

            // Re-attach original links.
            AttachOriginalLinks();
            // Attach last subsequent link with new final link.
            finalLink.ReplaceNext(subsequentLink);
        }

        private void AttachLevelRunLinks()
        {
            LevelRun current;
            LevelRun next;

            roller.ReplaceNext(baseLevelRun.firstLink);

            for (current = baseLevelRun; (next = current.Next) != null; current = next)
            {
                current.lastLink.ReplaceNext(next.firstLink);
            }
            current.lastLink.ReplaceNext(roller);

            lastLevelRun = current;
            level = baseLevelRun.Level;
            sos = baseLevelRun.SOR;

            if (!baseLevelRun.IsPartialIsolate)
            {
                eos = current.EOR;
            }
            else
            {
                byte eosLevel = Math.Max(level, paragraphLevel);
                eos = Level.LevelToEmbeddingType(eosLevel);
            }
        }

        private void AttachOriginalLinks()
        {
            for (LevelRun current = baseLevelRun; current != null; current = current.Next)
            {
                current.lastLink.ReplaceNext(current.subsequentLink);
            };
        }

        #region Resolve Weak Types

        private BidiLink ResolveWeakTypes()
        {
            BidiLink priorLink = roller;

            CharType w1PriorType = sos;
            CharType w2StrongType = sos;

            for (BidiLink link = roller.Next; link != roller; link = link.Next)
            {
                CharType type = link.type;

                // Rule W1
                if (type == CharType.NSM)
                {
                    // Change the 'type' variable as well because it can be EN on which
                    // W2 depends.
                    switch (w1PriorType)
                    {
                        case CharType.LRI:
                        case CharType.RLI:
                        case CharType.FSI:
                        case CharType.PDI:
                            link.type = type = CharType.ON;
                            break;

                        default:
                            link.type = type = w1PriorType;
                            break;
                    }
                }
                w1PriorType = type;

                // Rule W2
                if (type == CharType.EN)
                {
                    if (w2StrongType == CharType.AL)
                        link.type = CharType.AN;
                }
                // Rule W3
                // Note: It is safe to apply W3 in 'else-if' statement because it only
                //       depends on type AL. Even if W2 changes EN to AN, there won't
                //       be any harm.
                else if (type == CharType.AL)
                {
                    link.type = CharType.R;
                }

                // Save the strong type for W2.
                switch (type)
                {
                    case CharType.L:
                    case CharType.AL:
                    case CharType.R:
                        w2StrongType = type;
                        break;
                }

                if (type != CharType.ON && priorLink.type == type)
                {
                    priorLink.MergeNext();
                }
                else
                {
                    priorLink = link;
                }
            }

            priorLink = roller;
            CharType w4PriorType = sos;
            CharType w5PriorType = sos;
            CharType w7StrongType = sos;

            for (BidiLink link = roller.Next; link != roller; link = link.Next)
            {
                CharType type = link.type;
                CharType nextType = link.Next.type;

                // Rule W4
                if (link.length == 1
                    && (type == CharType.ES || type == CharType.CS)
                    && (w4PriorType == CharType.EN || w4PriorType == CharType.AN)
                    && (w4PriorType == nextType)
                    && (w4PriorType == CharType.EN || type == CharType.CS))
                {
                    // Change the current type as well because it can be EN on which W7
                    // depends.
                    link.type = type = w4PriorType;
                }
                w4PriorType = type;

                // Rule W5
                if (type == CharType.ET
                    && (w5PriorType == CharType.EN || nextType == CharType.EN))
                {
                    // Change the current type as well because it is EN on which W7
                    // depends.
                    link.type = type = CharType.EN;
                }
                w5PriorType = type;

                switch (type)
                {
                    // Rule W6
                    case CharType.ET:
                    case CharType.CS:
                    case CharType.ES:
                        link.type = CharType.ON;
                        break;

                    // Rule W7
                    // Note: W7 is expected to be applied after W6. However this is not the
                    //       case here. The reason is that W6 can only create the type ON
                    //       which is not tested in W7 by any means. So it won't affect the
                    //       algorithm.
                    case CharType.EN:
                        if (w7StrongType == CharType.L)
                        {
                            link.type = CharType.L;
                        }
                        break;

                    // Save the strong type for W7.
                    // Note: The strong type is expected to be saved after applying W7
                    //       because W7 itself creates a strong type. However the strong type
                    //       being saved here is based on the type after W5.
                    //       This won't effect the algorithm because a single link contains
                    //       all consecutive EN types. This means that even if W7 creates a
                    //       strong type, it will be saved in next iteration.
                    case CharType.L:
                    case CharType.R:
                        w7StrongType = type;
                        break;
                }

                if (type != CharType.ON && priorLink.type == type)
                {
                    priorLink.MergeNext();
                }
                else
                {
                    priorLink = link;
                }
            }

            return priorLink;
        }

        #endregion Resolve Weak Types

        #region Resolve Brackets

        private void ResolveBrackets()
        {
            BidiLink priorStrongLink = null;
            bracketQueue.Clear(Level.LevelToEmbeddingType(level));

            for (BidiLink link = roller.Next; link != roller; link = link.Next)
            {
                CharType type = link.type;

                switch (type)
                {
                    case CharType.ON:
                        BracketType bracketType;

                        char ch = text[link.offset];
                        char bracketValue = (char)PairingLookup.DetermineBracketPair((int)ch, out bracketType);

                        switch (bracketType)
                        {
                            case BracketType.Open:
                                BracketPair bracketPair = new BracketPair()
                                {
                                    priorStrongLink = priorStrongLink,
                                    openingLink = link,
                                    bracket = ch
                                };
                                bracketQueue.Enqueue(bracketPair);
                                break;

                            case BracketType.Close:
                                if (!bracketQueue.IsEmpty)
                                {
                                    bracketQueue.ClosePair(link, bracketValue);

                                    if (bracketQueue.ShouldDequeue)
                                        ResolveAvailableBracketPairs();
                                }
                                break;
                        }
                        break;

                    case CharType.L:
                    case CharType.R:
                    case CharType.AL:
                    case CharType.EN:
                    case CharType.AN:
                        if (!bracketQueue.IsEmpty)
                        {
                            if (type == CharType.EN || type == CharType.AN)
                                type = CharType.R;

                            bracketQueue.SetStrongType(type);
                        }
                        priorStrongLink = link;
                        break;
                }
            }

            ResolveAvailableBracketPairs();
        }

        private void ResolveAvailableBracketPairs()
        {
            CharType embeddingDirection = Level.LevelToEmbeddingType(level);
            CharType oppositeDirection = Level.LevelToOppositeType(level);

            while (!bracketQueue.IsEmpty)
            {
                BracketPair pair = bracketQueue.Peek();

                if (pair.IsComplete)
                {
                    CharType innerStrongType = pair.strongType;
                    CharType pairType;

                    // Rule: N0.b
                    if (innerStrongType == embeddingDirection)
                    {
                        pairType = innerStrongType;
                    }
                    // Rule: N0.c
                    else if (innerStrongType == oppositeDirection)
                    {
                        BidiLink priorStrongLink;
                        CharType priorStrongType;

                        priorStrongLink = pair.priorStrongLink;

                        if (priorStrongLink != null)
                        {
                            priorStrongType = priorStrongLink.type;
                            if (priorStrongType == CharType.EN || priorStrongType == CharType.AN)
                                priorStrongType = CharType.R;

                            BidiLink link = priorStrongLink.Next;
                            BidiLink start = pair.openingLink;

                            while (link != start)
                            {
                                CharType type = link.type;
                                if (type == CharType.L || type == CharType.R)
                                    priorStrongType = type;

                                link = link.Next;
                            }
                        }
                        else
                        {
                            priorStrongType = sos;
                        }

                        // Rule: N0.c.1
                        if (priorStrongType == oppositeDirection)
                        {
                            pairType = oppositeDirection;
                        }
                        // Rule: N0.c.2
                        else
                        {
                            pairType = embeddingDirection;
                        }
                    }
                    // Rule: N0.d
                    else
                    {
                        pairType = CharType.Nil;
                    }

                    if (pairType != CharType.Nil)
                    {
                        // Do the substitution
                        pair.openingLink.type = pairType;
                        pair.closingLink.type = pairType;
                    }
                }

                bracketQueue.Dequeue();
            }
        }

        #endregion Resolve Brackets

        #region Resolve Neutrals

        private void ResolveNeutrals()
        {
            CharType strongType = sos;
            BidiLink neutralLink = null;

            for (BidiLink link = roller.Next; link != roller; link = link.Next)
            {
                CharType type = link.type;

#if DEBUG
                ValidateTypeForNeutralRules(type);
#endif

                switch (type)
                {
                    case CharType.L:
                        strongType = CharType.L;
                        break;

                    case CharType.R:
                    case CharType.EN:
                    case CharType.AN:
                        strongType = CharType.R;
                        break;

                    case CharType.B:
                    case CharType.S:
                    case CharType.WS:
                    case CharType.ON:
                    case CharType.LRI:
                    case CharType.RLI:
                    case CharType.FSI:
                    case CharType.PDI:
                        if (neutralLink == null)
                        {
                            neutralLink = link;
                        }

                        CharType nextType = link.Next.type;
                        switch (nextType)
                        {
                            case CharType.Nil:
                                nextType = eos;
                                break;

                            case CharType.EN:
                            case CharType.AN:
                                nextType = CharType.R;
                                break;
                        }

                        if (nextType == CharType.R || nextType == CharType.L)
                        {
                            // Rules N1, N2
                            CharType resolvedType = (strongType == nextType
                                                     ? strongType
                                                     : Level.LevelToEmbeddingType(level)
                                                    );

                            do
                            {
                                neutralLink.type = resolvedType;
                                neutralLink = neutralLink.Next;
                            } while (neutralLink != link.Next);

                            neutralLink = null;
                        }
                        break;
                }
            }
        }

        private static void ValidateTypeForNeutralRules(CharType type)
        {
            switch (type)
            {
                case CharType.L:
                case CharType.R:
                case CharType.EN:
                case CharType.AN:
                case CharType.B:
                case CharType.S:
                case CharType.WS:
                case CharType.ON:
                case CharType.LRI:
                case CharType.RLI:
                case CharType.FSI:
                case CharType.PDI:
                    break;

                default:
                    throw (new InvalidOperationException("The char type is invalid for neutral rules."));
            }
        }

        #endregion Resolve Neutrals

        #region Resolve Implicit Levels

        private void ResolveImplicitLevels()
        {
            if ((level & 1) == 0)
            {
                for (BidiLink link = roller.Next; link != roller; link = link.Next)
                {
                    CharType type = link.type;

#if DEBUG
                    ValidateTypeForImplicitRules(type);
#endif

                    // Rule I1
                    if (type == CharType.R)
                    {
                        link.level += 1;
                    }
                    else if (type != CharType.L)
                    {
                        link.level += 2;
                    }
                }
            }
            else
            {
                for (BidiLink link = roller.Next; link != roller; link = link.Next)
                {
                    CharType type = link.type;

#if DEBUG
                    ValidateTypeForImplicitRules(type);
#endif

                    // Rule I2
                    if (type != CharType.R)
                    {
                        link.level += 1;
                    }
                }
            }
        }

        private static void ValidateTypeForImplicitRules(CharType type)
        {
            switch (type)
            {
                case CharType.L:
                case CharType.R:
                case CharType.EN:
                case CharType.AN:
                    break;

                default:
                    throw (new InvalidOperationException("The char type is invalid for implicit rule."));
            }
        }

        #endregion Resolve Implicit Levels
    }
}
