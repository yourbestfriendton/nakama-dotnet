/**
 * Copyright 2021 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using NakamaSync;

namespace Nakama.Tests
{
    public delegate string VarIdGenerator(string userId, string varName, int varIndex);

    public class SyncTestUserEnvironment
    {
        public List<SharedVar<bool>> SharedBools { get; }
        public List<SharedVar<float>> SharedFloats { get; }
        public List<SharedVar<int>> SharedInts { get; }
        public List<SharedVar<string>> SharedStrings { get; }

        public List<UserVar<bool>> UserBools { get; }
        public List<UserVar<float>> UserFloats { get; }
        public List<UserVar<int>> UserInts { get; }
        public List<UserVar<string>> UserStrings { get; }

        public SyncTestUserEnvironment(ISession session, VarRegistry registry, int numTestVars, VarIdGenerator keyGenerator)
        {
            SharedBools = new List<SharedVar<bool>>();
            SharedFloats = new List<SharedVar<float>>();
            SharedInts = new List<SharedVar<int>>();
            SharedStrings = new List<SharedVar<string>>();

            UserBools = new List<UserVar<bool>>();
            UserFloats = new List<UserVar<float>>();
            UserInts = new List<UserVar<int>>();
            UserStrings = new List<UserVar<string>>();

            for (int i = 0; i < numTestVars; i++)
            {
                var newSharedBool = new SharedVar<bool>();
                registry.SharedBools[keyGenerator(session.UserId, nameof(newSharedBool), i)] = newSharedBool;
                SharedBools.Add(newSharedBool);

                var newSharedFloat = new SharedVar<float>();
                registry.SharedFloats[keyGenerator(session.UserId, nameof(newSharedFloat), i)] = newSharedFloat;
                SharedFloats.Add(newSharedFloat);

                var newSharedInt = new SharedVar<int>();
                registry.SharedInts[keyGenerator(session.UserId, nameof(newSharedInt), i)] = newSharedInt;
                SharedInts.Add(newSharedInt);

                var newSharedString = new SharedVar<string>();
                registry.SharedStrings[keyGenerator(session.UserId, nameof(newSharedString), i)] = newSharedString;
                SharedStrings.Add(newSharedString);

                var newUserBool = new UserVar<bool>();
                registry.UserBools[keyGenerator(session.UserId, nameof(newUserBool), i)] = newUserBool;
                UserBools.Add(newUserBool);

                var newUserFloat = new UserVar<float>();
                registry.UserFloats[keyGenerator(session.UserId, nameof(newUserFloat), i)] = newUserFloat;
                UserFloats.Add(newUserFloat);

                var newUserInt = new UserVar<int>();
                registry.UserInts[keyGenerator(session.UserId, nameof(newUserInt), i)] = newUserInt;
                UserInts.Add(newUserInt);

                var newUserString = new UserVar<string>();
                registry.UserStrings[keyGenerator(session.UserId, nameof(newUserString), i)] = newUserString;
                UserStrings.Add(newUserString);
            }
        }

        public static string DefaultVarIdGenerator(string userId, string varName, int varIndex)
        {
            return varName + varIndex.ToString();
        }
    }
}
