/*
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

#ifndef CLR_PROFILER_TRACECONFIG_LOADER_H_
#define CLR_PROFILER_TRACECONFIG_LOADER_H_

#include <string>
#include <vector>
#include "util.h"
#include <cor.h>

namespace clrprofiler {

    struct TraceMethod
    {
        WSTRING methodName;
        WSTRING paramsName;
        TraceMethod() : methodName(W("")), paramsName(W("")) {}
        TraceMethod(WSTRING methodName, WSTRING paramsName) : methodName(methodName), paramsName(paramsName) {}   
    };

    struct TraceAssembly
    {
        WSTRING assemblyName;
        WSTRING className;
        std::vector<TraceMethod> methods;
    };

    struct Version {
        unsigned short major = 1;
        unsigned short minor = 0;
        unsigned short build = 0;
        unsigned short revision = 0;
        Version() {}
        Version(std::vector<WSTRING> vector)
        {
            major = GetIndexNum(vector, 0);
            minor = GetIndexNum(vector, 1);
            build = GetIndexNum(vector, 2);
            revision = GetIndexNum(vector, 3);
        }

    private:
        unsigned short GetIndexNum(std::vector<WSTRING> vector, unsigned index) const
        {
            const auto size = vector.size();
            if (size > index)
                return (unsigned short)strtol(ToString(vector[index]).c_str(), NULL, 10);
            return 0;
        }
    };

    struct ManagedAssembly
    {
        std::vector<BYTE> publicKey;
        ASSEMBLYMETADATA assemblyMetaData{};
    };

    struct TraceConfig
    {
        std::vector<TraceAssembly> traceAssemblies;
        ManagedAssembly managedAssembly{};
    };

    TraceConfig LoadTraceConfig(const WSTRING& traceHomePath);

}

#endif
