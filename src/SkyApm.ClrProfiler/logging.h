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

#ifndef CLR_PROFILER_LOGGING_H_
#define CLR_PROFILER_LOGGING_H_

#include "singleton.h"
#include "util.h"
#include <memory>
#include <spdlog/spdlog.h>
#include <spdlog/sinks/rotating_file_sink.h>
#include <iostream>

namespace clrprofiler {

    class CLogger : public Singleton<CLogger>
    {
        friend class Singleton<CLogger>;
    private:
        static WSTRING GetLogPath()
        {
            WSTRING log_path;
            auto home = GetEnvironmentValue(GetClrProfilerHome());
            if(!home.empty()) {
                log_path = home + PathSeparator + W("logs");
            }
            else {
                log_path = W("logs");
            }
            return log_path;
        }

        CLogger() {

            spdlog::set_error_handler([](const std::string& msg)
            {
                std::cerr << "Logger Handler: " << msg << std::endl;
            });

            spdlog::flush_every(std::chrono::seconds(5));

            const WSTRING log_path = GetLogPath();

            CheckDir(ToString(log_path).c_str());

            const auto log_name = log_path + PathSeparator + W("trace") + ToWString(std::to_string(GetPID())) + W(".log");
            m_fileout = spdlog::rotating_logger_mt("Logger", ToString(log_name), 1024 * 1024 * 10, 3);

            m_fileout->set_level(spdlog::level::info);

            m_fileout->set_pattern("[%Y-%m-%d %T.%e] [%l] [thread %t] %v");

            m_fileout->flush_on(spdlog::level::err);
        };

        ~CLogger()
        {
            spdlog::drop_all();
        };

    public:
        std::shared_ptr<spdlog::logger> m_fileout;
    };

#define Info( ... )                               \
    {                                                 \
        CLogger::Instance()->m_fileout->info(__VA_ARGS__);  \
    }

#define Warn( ... )                               \
    {                                                 \
        CLogger::Instance()->m_fileout->warn(__VA_ARGS__);  \
    }

#define Error( ... )                               \
    {                                                 \
        CLogger::Instance()->m_fileout->error(__VA_ARGS__);   \
    }
}  // namespace SkyApm.ClrProfiler

#endif  // CLR_PROFILER_LOGGING_H_
