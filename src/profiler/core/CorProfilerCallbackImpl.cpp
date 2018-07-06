//
// Created by liuhaoyang on 2018/6/1.
//

#include "CorProfilerCallbackImpl.h"


CorProfilerCallbackImpl::CorProfilerCallbackImpl()
        : m_referenceCount(1)
{
}

CorProfilerCallbackImpl::~CorProfilerCallbackImpl()
{
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::QueryInterface(REFIID riId, void **ppvObject) {
    if (riId == IID__ICorProfilerCallback3 ||
        riId == IID__ICorProfilerCallback2 ||
        riId == IID__ICorProfilerCallback ||
        riId == IID__IUnknown) {

        *ppvObject = this;
        this->AddRef();

        return S_OK;
    }

    *ppvObject = NULL;
    return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE CorProfilerCallbackImpl::AddRef(void)
{
    return __sync_fetch_and_add(&m_referenceCount, 1) + 1;
}

ULONG STDMETHODCALLTYPE CorProfilerCallbackImpl::Release(void)
{
    LONG result = __sync_fetch_and_sub(&m_referenceCount, 1) - 1;
    if (result == 0)
    {
        delete this;
    }

    return result;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::Initialize(IUnknown *pICorProfilerInfoUnk)
{
    HRESULT hr = pICorProfilerInfoUnk->QueryInterface(IID__ICorProfilerInfo3, (void **) &info);
    if (hr == S_OK && info != NULL)
    {
        info->SetEventMask(COR_PRF_MONITOR_JIT_COMPILATION | COR_PRF_MONITOR_ASSEMBLY_LOADS | COR_PRF_MONITOR_CLASS_LOADS);
        return S_OK;
    }

    return E_FAIL;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::Shutdown(void)
{
    info->Release();
    info = NULL;
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AppDomainCreationStarted(AppDomainID appDomainId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AppDomainShutdownStarted(AppDomainID appDomainId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AssemblyLoadStarted(AssemblyID assemblyId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AssemblyUnloadStarted(AssemblyID assemblyId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ModuleLoadStarted(ModuleID moduleId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ModuleUnloadStarted(ModuleID moduleId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ClassLoadStarted(ClassID classId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ClassLoadFinished(ClassID classId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ClassUnloadStarted(ClassID classId)
{
    //LogProfilerActivity("ClassUnloadStarted\n");
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ClassUnloadFinished(ClassID classId, HRESULT hrStatus)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::FunctionUnloadStarted(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock)
{
    //LogProfilerActivity("JITCompilationFinished\n");
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::JITCachedFunctionSearchStarted(FunctionID functionId, BOOL *pbUseCachedFunction)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::JITFunctionPitched(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::JITInlining(FunctionID callerId, FunctionID calleeId, BOOL *pfShouldInline)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ThreadCreated(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ThreadDestroyed(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingClientInvocationStarted(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingClientSendingMessage(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingClientReceivingReply(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingClientInvocationFinished(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingServerReceivingMessage(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingServerInvocationStarted(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingServerInvocationReturned(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RemotingServerSendingReply(GUID *pCookie, BOOL fIsAsync)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::UnmanagedToManagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ManagedToUnmanagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeSuspendFinished(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeSuspendAborted(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeResumeStarted(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeResumeFinished(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeThreadSuspended(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RuntimeThreadResumed(ThreadID threadId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::MovedReferences(ULONG cMovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ObjectAllocated(ObjectID objectId, ClassID classId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ObjectsAllocatedByClass(ULONG cClassCount, ClassID classIds[], ULONG cObjects[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID objectRefIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RootReferences(ULONG cRootRefs, ObjectID rootRefIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionThrown(ObjectID thrownObjectId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionSearchFunctionEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionSearchFunctionLeave(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionSearchFilterEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionSearchFilterLeave(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionSearchCatcherFound(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionOSHandlerEnter(UINT_PTR __unused)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionOSHandlerLeave(UINT_PTR __unused)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionUnwindFunctionEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionUnwindFunctionLeave(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionUnwindFinallyEnter(FunctionID functionId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionUnwindFinallyLeave(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionCatcherLeave(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::COMClassicVTableCreated(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable, ULONG cSlots)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::COMClassicVTableDestroyed(ClassID wrappedClassId, REFGUID implementedIID, void *pVTable)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionCLRCatcherFound(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ExceptionCLRCatcherExecute(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ThreadNameChanged(ThreadID threadId, ULONG cchName, _In_reads_opt_(cchName) WCHAR name[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::GarbageCollectionFinished(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[])
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::HandleCreated(GCHandleID handleId, ObjectID initialObjectId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::HandleDestroyed(GCHandleID handleId)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::InitializeForAttach(IUnknown *pCorProfilerInfoUnk, void *pvClientData, UINT cbClientData)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ProfilerAttachComplete(void)
{
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CorProfilerCallbackImpl::ProfilerDetachSucceeded(void)
{
    return S_OK;
}
