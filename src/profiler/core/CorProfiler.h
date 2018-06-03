//
// Created by liuhaoyang on 2018/6/1.
//

#ifndef PROFILER_CORPROFILER_H
#define PROFILER_CORPROFILER_H

#include "CorProfilerCallbackImpl.h"

class CorProfiler : public CorProfilerCallbackImpl {
public:
    CorProfiler();
    ~CorProfiler();
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted(AssemblyID assemblyId) override;
    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock) override;
};


#endif //PROFILER_CORPROFILER_H
