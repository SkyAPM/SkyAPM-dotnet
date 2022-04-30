﻿/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.Extensions.DependencyInjection;
using SkyApm.Diagnostics.Grpc.Client;
using SkyApm.Diagnostics.Grpc.Server;
using SkyApm.Utilities.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.Grpc
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddGrpc(this SkyApmExtensions extensions)
        {
            extensions.Services.AddSingleton<ClientDiagnosticProcessor>();
            extensions.Services.AddSingleton<ClientDiagnosticInterceptor>();
            extensions.Services.AddSingleton<ServerDiagnosticProcessor>();
            extensions.Services.AddSingleton<ServerDiagnosticInterceptor>();
            return extensions;
        }
    }
}
