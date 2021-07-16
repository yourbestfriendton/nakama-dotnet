
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

using Nakama;

namespace NakamaSync
{
    internal class SharedGuestEgress : ISyncService
    {
        public SyncErrorHandler ErrorHandler { get; set; }
        public ILogger Logger { get; set; }

        private readonly VarKeys _keys;
        private readonly EnvelopeBuilder _builder;

        public SharedGuestEgress(VarKeys keys, EnvelopeBuilder builder)
        {
            _keys = keys;
            _builder = builder;
        }

        public void HandleLocalSharedVarChanged<T>(string key, T newValue, SharedVarAccessor<T> accessor)
        {
            var status = _keys.GetValidationStatus(key);

            if (status == ValidationStatus.Validated)
            {
                status = ValidationStatus.Pending;
                _keys.SetValidationStatus(key, status);
            }

            _keys.IncrementLockVersion(key);
            var newSyncedValue = new SharedValue<T>(key, newValue, _keys.GetLockVersion(key), status);

            _builder.AddSharedVar(accessor, newSyncedValue);
            _builder.SendEnvelope();
        }
    }
}
