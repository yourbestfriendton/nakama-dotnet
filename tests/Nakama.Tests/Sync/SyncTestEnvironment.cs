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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NakamaSync;

namespace Nakama.Tests.Sync
{
    public delegate string UserIdGenerator(int userIndex);
    public delegate string VarIdGenerator(string userId, string varName, int varIndex);
    public delegate string RpcIdGenerator(int rpcIndex);

    /// <summary>
    // A test environment within which multiple users sync their vars with one another.
    /// </summary>
    public class SyncTestEnvironment
    {
        private const int _RAND_GUEST_SEED = 1;

        public int CreatorIndex { get; }

        private readonly Random _randomGuestGenerator = new Random(_RAND_GUEST_SEED);
        private readonly List<SyncTestUserEnvironment> _syncTestUserEnvironments = new List<SyncTestUserEnvironment>();

       public SyncTestEnvironment(
            SyncOpcodes opcodes,
            int numClients,
            int numSharedVars,
            int creatorIndex,
            UserIdGenerator userIdGenerator = null,
            VarIdGenerator varIdGenerator = null)
        {
            CreatorIndex = creatorIndex;
            userIdGenerator = userIdGenerator ?? DefaultUserIdGenerator;
            varIdGenerator = varIdGenerator ?? DefaultVarIdGenerator;

            for (int i = 0; i < numClients; i++)
            {
                string userId = userIdGenerator(i);
                var env = new SyncTestUserEnvironment(userId, opcodes, varIdGenerator, numSharedVars);
                _syncTestUserEnvironments.Add(env);
            }
        }

        public SyncTestEnvironment(
            SyncOpcodes opcodes,
            int numClients,
            int creatorIndex,
            UserIdGenerator userIdGenerator = null)
        {
            CreatorIndex = creatorIndex;
            userIdGenerator = userIdGenerator ?? DefaultUserIdGenerator;

            for (int i = 0; i < numClients; i++)
            {
                string userId = userIdGenerator(i);
                var env = new SyncTestUserEnvironment(userId, opcodes);
                _syncTestUserEnvironments.Add(env);
            }
        }

        public SyncTestEnvironment(
            SyncOpcodes opcodes,
            int numClients,
            int numOtherVarCollections,
            int numOtherVarsPerCollection,
            int creatorIndex,
            UserIdGenerator userIdGenerator = null,
            VarIdGenerator varIdGenerator = null)
        {
            CreatorIndex = creatorIndex;
            userIdGenerator = userIdGenerator ?? DefaultUserIdGenerator;
            varIdGenerator = varIdGenerator ?? DefaultVarIdGenerator;

            for (int i = 0; i < numClients; i++)
            {
                string userId = userIdGenerator(i);
                var env = new SyncTestUserEnvironment(userId, opcodes, varIdGenerator, numOtherVarCollections, numOtherVarsPerCollection);
                _syncTestUserEnvironments.Add(env);
            }
        }

        public void StartViaMatchmaker(SyncErrorHandler errorHandler = null)
        {
            var matchmakerTasks = new List<Task>();

            for (int i = 0; i < _syncTestUserEnvironments.Count; i++)
            {
                var matchmakerTask = _syncTestUserEnvironments[i].StartMatchViaMatchmaker(_syncTestUserEnvironments.Count, errorHandler ?? DefaultErrorHandler());
                matchmakerTasks.Add(matchmakerTask);
            }

            Task.WaitAll(matchmakerTasks.ToArray());
        }

        public async Task Start(SyncErrorHandler errorHandler = null)
        {
            var match = await _syncTestUserEnvironments[CreatorIndex].CreateMatch(errorHandler ?? DefaultErrorHandler());

            for (int i = 0; i < _syncTestUserEnvironments.Count; i++)
            {
                if (i == CreatorIndex)
                {
                    continue;
                }

                await _syncTestUserEnvironments[i].JoinMatch(match.Id, errorHandler ?? DefaultErrorHandler());
            }
        }

        public async Task StartViaName(string name, SyncErrorHandler errorHandler = null)
        {
            var match = await _syncTestUserEnvironments[CreatorIndex].CreateMatch(name, errorHandler ?? DefaultErrorHandler());

            for (int i = 0; i < _syncTestUserEnvironments.Count; i++)
            {
                if (i == CreatorIndex)
                {
                    continue;
                }

                await _syncTestUserEnvironments[i].CreateMatch(name, errorHandler ?? DefaultErrorHandler());
            }
        }

        public void Dispose()
        {
            var disposeTasks = new List<Task>();

            foreach (SyncTestUserEnvironment userEnv in _syncTestUserEnvironments)
            {
                disposeTasks.Add(userEnv.Dispose());
            }

            Task.WaitAll(disposeTasks.ToArray());
        }

        public List<SyncTestUserEnvironment> GetAllEnvs()
        {
            return new List<SyncTestUserEnvironment>(_syncTestUserEnvironments);
        }

        public SyncTestUserEnvironment GetUserEnv(IUserPresence clientPresence)
        {
            return _syncTestUserEnvironments.First(env => env.Self.UserId == clientPresence.UserId);
        }

        public SyncTestUserEnvironment GetCreator()
        {
            return _syncTestUserEnvironments[CreatorIndex];
        }

        public IUserPresence GetCreatorPresence()
        {
            return _syncTestUserEnvironments[CreatorIndex].Self;
        }

        public IUserPresence GetRandomNonCreatorPresence()
        {
            List<IUserPresence> guests = GetNonCreatorPresences();
            int randGuestIndex = _randomGuestGenerator.Next(guests.Count);
            return guests[randGuestIndex];
        }

        public SyncTestUserEnvironment GetTestEnvironment(IUserPresence presence)
        {
            for (int i = 0; i < _syncTestUserEnvironments.Count; i++)
            {
                var testEnvironment = _syncTestUserEnvironments[i];

                if (testEnvironment.Self.UserId == presence.UserId)
                {
                    return testEnvironment;
                }
            }

            throw new InvalidOperationException($"Could not obtain guest environment with presence {presence.UserId}.");
        }

        private List<IUserPresence> GetNonCreatorPresences()
        {
            var guests = new List<IUserPresence>();

            for (int i = 0; i < _syncTestUserEnvironments.Count; i++)
            {
                if (i == CreatorIndex)
                {
                    continue;
                }

                guests.Add(_syncTestUserEnvironments[i].Self);
            }

            return guests;
        }

        private SyncErrorHandler DefaultErrorHandler()
        {
            return e => new StdoutLogger().ErrorFormat($"{e.Message}{e.StackTrace}");
        }

        private static string DefaultVarIdGenerator(string userId, string varName, int varIndex)
        {
            return varName + varIndex.ToString();
        }

        private static string DefaultUserIdGenerator(int userIndex)
        {
            return Guid.NewGuid().ToString();
        }

        private static string DefaultRpcIdGenerator(int rpcIndex)
        {
            return $"rpc_{rpcIndex}";
        }
    }
}