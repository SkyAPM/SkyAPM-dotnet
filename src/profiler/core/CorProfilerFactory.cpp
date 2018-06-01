//
// Created by liuhaoyang on 2018/6/1.
//

#include "CorProfilerFactory.h"

CorProfilerFactory::CorProfilerFactory()
        : m_referenceCount(1)
{
}

CorProfilerFactory::~CorProfilerFactory()
{
}

HRESULT STDMETHODCALLTYPE CorProfilerFactory::QueryInterface(REFIID riid, void **ppvObject)
{
    if (riid == IID__IUnknown || riid == IID__IClassFactory)
    {
        *ppvObject = this;
        this->AddRef();

        return S_OK;
    }

    *ppvObject = NULL;
    return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE CorProfilerFactory::AddRef(void)
{
    return __sync_fetch_and_add(&m_referenceCount, 1) + 1;
}

ULONG STDMETHODCALLTYPE CorProfilerFactory::Release(void)
{
    LONG result = __sync_fetch_and_sub(&m_referenceCount, 1) - 1;
    if (result == 0)
    {
        delete this;
    }

    return result;
}

HRESULT STDMETHODCALLTYPE CorProfilerFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject)
{
    if (riid== IID__ICorProfilerCallback || riid == IID__ICorProfilerCallback2 || riid == IID__ICorProfilerCallback3)
    {
        if (ppvObject != NULL)
            *ppvObject = new CorProfiler();

        return S_OK;
    }

    return E_NOINTERFACE;
}

HRESULT STDMETHODCALLTYPE CorProfilerFactory::LockServer(BOOL fLock)
{
    return S_OK;
}