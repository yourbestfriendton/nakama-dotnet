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
using Nakama;

namespace NakamaSync
{
    internal class SharedVarEgress : ISyncService
    {
        public SyncErrorHandler ErrorHandler { get; set; }
        public ILogger Logger { get; set; }

        private PresenceTracker _presenceTracker;
        private HostTracker _hostTracker;
        private SharedVarGuestEgress _sharedVarGuestEgress;
        private SharedVarHostEgress _sharedHostEgress;

        public SharedVarEgress(SharedVarGuestEgress sharedVarGuestEgress, SharedVarHostEgress sharedHostEgress, PresenceTracker presenceTracker, HostTracker hostTracker)
        {
            _sharedVarGuestEgress = sharedVarGuestEgress;
            _sharedHostEgress = sharedHostEgress;
            _presenceTracker = presenceTracker;
            _hostTracker = hostTracker;
        }

        public void Subscribe(SharedVarRegistry registry, HandshakeRequester requester)
        {
            requester.OnHandshakeSuccess += () =>
            {
                // now that we have initial store loaded,
                // listen for user modifications to sync vars.
                Subscribe(registry);
            };
        }

        public void Subscribe(SharedVarRegistry registry)
        {
            Subscribe(registry.SharedBools, values => values.SharedBools);
            Subscribe(registry.SharedFloats, values => values.SharedFloats);
            Subscribe(registry.SharedInts,  values => values.SharedInts);
            Subscribe(registry.SharedStrings, values => values.SharedStrings);
        }

        private void Subscribe<T>(Dictionary<string, SharedVar<T>> vars, SharedVarAccessor<T> accessor)
        {
            foreach (var kvp in vars)
            {
                Logger?.DebugFormat($"Subscribing to shared variable with key {kvp.Key}");
                vars[kvp.Key].OnValueChanged += (evt) => HandleLocalSharedVarChanged(kvp.Key, evt, accessor);
            }
        }

        private void HandleLocalSharedVarChanged<T>(string key, ISharedVarEvent<T> evt, SharedVarAccessor<T> accessor)
        {
            if (evt.Source.UserId != _presenceTracker.GetSelf().UserId)
            {
                // ingress should only send out changes initated by self.
                return;
            }

            bool isHost = _hostTracker.IsSelfHost();

            Logger?.DebugFormat($"Local shared variable changed. Key: {key}, OldValue: {evt.ValueChange.OldValue}, Value: {evt.ValueChange.NewValue}");

            if (isHost)
            {
                _sharedHostEgress.HandleLocalSharedVarChanged(key, evt.Var, evt.ValueChange.NewValue, accessor);
            }
            else
            {
                _sharedVarGuestEgress.HandleLocalSharedVarChanged(key, evt.Var, evt.ValueChange.NewValue, accessor);
            }
        }
    }
}