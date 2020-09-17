/**
 * Copyright 2020 The Nakama Authors
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

namespace Nakama.Tests.Api
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;
    using Xunit;

    // NOTE: Requires Lua modules from server repo.

    public class HttpErrorTest
    {
        private IClient _client;

        // ReSharper disable RedundantArgumentDefaultValue

        public HttpErrorTest()
        {
            _client = new Client("http", "127.0.0.1", 7350, "defaultkey");
        }

        [Fact]
        public async Task BadLuaRpcReturnsErrorMessageAndDict()
        {
            var session = await _client.AuthenticateCustomAsync($"{Guid.NewGuid()}");
            const string funcid = "clientrpc.rpc_error";

            var exception = await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid));
            await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid));
            Assert.NotNull(exception.Message);
            Assert.NotEmpty(exception.Message);
            Assert.NotNull(exception.Data);
            Assert.NotEmpty(exception.Data);
            Assert.True(exception.Data is IDictionary);
            Assert.True(exception.Data.Contains("Type"));
            Assert.True(exception.Data.Contains("Object"));
            Assert.True(exception.Data.Contains("StackTrace"));
            Assert.True(exception.Data.Contains("Cause"));
        }

        [Fact]
        public async Task BadGoRpcReturnsErrorMessageAndEmptyDict()
        {
            var session = await _client.AuthenticateCustomAsync($"{Guid.NewGuid()}");
            const string funcid = "clientrpc.rpc_error_go";

            var exception = await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid));
            await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid));
            Assert.NotNull(exception.Message);
            Assert.NotEmpty(exception.Message);
            Assert.Empty(exception.Data);
        }

        /*
        Make RPC calls to storage API as an example to test error format in Lua and Go runtimes.
        */

        [Fact]
        public async Task BadGoStorageRpcReturnsErrorMessageAndEmptyDict()
        {
            var session = await _client.AuthenticateCustomAsync("user_rpc_error_storage_go");
            const string funcid = "clientrpc.rpc_error_storage_go";

            var exception = await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid, session.UserId));
            await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid));
            Assert.NotNull(exception.Message);
            Assert.NotEmpty(exception.Message);
            // go runtime returns an empty object
            Assert.Empty(exception.Data);
        }

        [Fact]
        public async Task BadLuaStorageRpcReturnsErrorMessageAndStringNotDict()
        {
            var session = await _client.AuthenticateCustomAsync("user_rpc_error_storage_lua");
            const string funcid = "clientrpc.rpc_storage_error";

            var exception = await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid, session.UserId));
            await Assert.ThrowsAsync<ApiResponseException>(() => _client.RpcAsync(session, funcid));
            Assert.NotNull(exception.Message);
            Assert.NotEmpty(exception.Message);
             //lua runtime differs from go runtime in returning error as string.
            Assert.True(exception.Data.Contains("error"));
            Assert.NotNull(exception.Data["error"] as string);
            Assert.NotEmpty(exception.Data["error"] as string);

        }
    }
}
