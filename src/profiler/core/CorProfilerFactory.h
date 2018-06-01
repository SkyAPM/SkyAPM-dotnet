//
// Created by liuhaoyang on 2018/6/1.
//

#ifndef PROFILER_CORPROFILERFACTORY_H
#define PROFILER_CORPROFILERFACTORY_H

#include <cor.h>
#include <corprof.h>
#include "GuidDefine.h"
#include "CorProfiler.h"

class CorProfilerFactory : IClassFactory {

public:
    CorProfilerFactory();
    virtual ~CorProfilerFactory();
    virtual HRESULT STDMETHODCALLTYPE QueryInterface(REFIID riid, void **ppvObject) override;
    virtual ULONG STDMETHODCALLTYPE AddRef(void) override;
    virtual ULONG STDMETHODCALLTYPE Release(void) override;
    virtual HRESULT STDMETHODCALLTYPE CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject) override;
    virtual HRESULT STDMETHODCALLTYPE LockServer(BOOL fLock) override;

private:
    LONG m_referenceCount;
};


#endif //PROFILER_CORPROFILERFACTORY_H
