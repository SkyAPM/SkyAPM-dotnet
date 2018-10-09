cmake_minimum_required(VERSION 3.10)
project(profiler)

set(PROFILER_CMALE_BUILD_TYPE "Debug")

if(CMAKE_BUILD_TYPE STREQUAL DEBUG OR CMAKE_BUILD_TYPE STREQUAL Debug OR CMAKE_BUILD_TYPE STREQUAL debug)
    set(PROFILER_CMALE_BUILD_TYPE "Debug")
endif()

if(CMAKE_BUILD_TYPE STREQUAL RELEASE OR CMAKE_BUILD_TYPE STREQUAL Release OR CMAKE_BUILD_TYPE STREQUAL release)
    set(PROFILER_CMALE_BUILD_TYPE "Release")
endif()

MESSAGE(STATUS profiler_build_type=${CMAKE_BUILD_TYPE})

if(CMAKE_SYSTEM_NAME STREQUAL Darwin)
    set(PROFILER_CMAKE_BUILD_PLATFORM "OSX")
    set(PROFILER_CMAKE_BUILD_ARCH "x64")
endif(CMAKE_SYSTEM_NAME STREQUAL Darwin)

message(STATUS profiler_build_platform=${PROFILER_CMAKE_BUILD_PLATFORM})
message(STATUS profiler_build_arch=${PROFILER_CMAKE_BUILD_ARCH})

set(CMAKE_CXX_STANDARD 11)
set(CMAKE_SHARED_LIBRARY_PREFIX "skywalking-")
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_DEBUG ${PROJECT_SOURCE_DIR}/../../dist/${PROFILER_CMALE_BUILD_TYPE}/${PROFILER_CMAKE_BUILD_PLATFORM}_${PROFILER_CMAKE_BUILD_ARCH})
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY_RELEASE ${PROJECT_SOURCE_DIR}/../../dist/${PROFILER_CMALE_BUILD_TYPE}/${PROFILER_CMAKE_BUILD_PLATFORM}_${PROFILER_CMAKE_BUILD_ARCH})

set(CMAKE_CXX_FLAGS_DEBUG "$ENV{CXXFLAGS} -O0 -Wall -g -ggdb ")
set(CMAKE_CXX_FLAGS_RELEASE "$ENV{CXXFLAGS} -O3 -Wall")



include_directories(${PROJECT_SOURCE_DIR}/../coreclr/src/pal/inc/rt)
include_directories(${PROJECT_SOURCE_DIR}/../coreclr/src/pal/prebuilt/inc)
include_directories(${PROJECT_SOURCE_DIR}/../coreclr/src/pal/inc)
include_directories(${PROJECT_SOURCE_DIR}/../coreclr/src/inc)
include_directories(${PROJECT_SOURCE_DIR}/../coreclr/bin/Product/${PROFILER_CMAKE_BUILD_PLATFORM}.${PROFILER_CMAKE_BUILD_ARCH}.${PROFILER_CMALE_BUILD_TYPE}/inc)

add_compile_options(-fPIC -lexpat -fms-extensions -DPLATFORM_UNIX -DBIT64 -std=c++11)



#add_definitions(-DPAL_STDCPP_COMPAT=0)
#set(CLR_CMAKE_PLATFORM_UNIX 1)
#
#set(CLR_CMAKE_PLATFORM_UNIX_AMD64 1)
#set(CLR_CMAKE_PLATFORM_DARWIN 1)

add_library(profiler SHARED
        core/CorProfilerCallbackImpl.cpp core/CorProfilerCallbackImpl.h
        core/CorProfilerFactory.cpp core/CorProfilerFactory.h
        core/GuidDefine.h
        core/CorProfiler.cpp core/CorProfiler.h
        Main.cpp
        function/FunctionInfo.cpp function/FunctionInfo.h CorProfilerStd.h CorProfilerStd.cpp)