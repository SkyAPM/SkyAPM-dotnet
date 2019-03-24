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

#ifndef CLR_PROFILER_UTIL_H_
#define CLR_PROFILER_UTIL_H_

#include <string>
#include <vector>
#include <cor.h>

#ifdef _WIN32

#include <io.h>
#include <direct.h>  
#include <process.h>

#else

#include <unistd.h> 
#include <sys/types.h>  
#include <sys/stat.h>
#include <fstream>

#endif

namespace clrprofiler {

	// WCHAR in windows is wchar_t , in linux is char16_t
	typedef std::basic_string<WCHAR> WSTRING;
	typedef std::basic_stringstream<WCHAR> WSTRINGSTREAM;

#ifdef _WIN32
#define W(str)  L##str
#else
#define W(str)  u##str
#endif

	std::string ToString(const WSTRING& wstr);
	WSTRING ToWString(const std::string& str);

    const WSTRING CORECLR_PROFILER_HOME = W("CORECLR_PROFILER_HOME");
    const WSTRING COR_PROFILER_HOME = W("COR_PROFILER_HOME");

    void SetClrProfilerFlag(bool flag);
    WSTRING GetClrProfilerHome();

#ifdef _WIN32
    const auto PathSeparator = W("\\");
#else
    const auto PathSeparator = W("/");
#endif

    inline int GetPID() {
#ifdef _WIN32
        return _getpid();
#else
        return getpid();
#endif
    } 

    static bool CheckDir(const char* dir)
    {
#ifdef _WIN32  
        if (_access(dir, 0) == -1)
#else 
        if (access(dir, 0) == -1)
#endif
        {
#ifdef _WIN32  
            int flag = _mkdir(dir);
#else 
            int flag = mkdir(dir, 0777);
#endif  
            return (flag == 0);
        }
        return true;
    };

    // WSTRING Split
    std::vector<WSTRING> Split(const WSTRING &s, const WCHAR wchar);

    // WSTRING Trim
    WSTRING Trim(const WSTRING &str);

    // GetEnvironmentValue
    WSTRING GetEnvironmentValue(const WSTRING &name);

    //HexStr
    WSTRING HexStr(const unsigned char *data, int len);

    //HexToBytes
    std::vector<BYTE> HexToBytes(const std::string& hex);
}
#endif
