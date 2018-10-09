//
// Created by liuhaoyang on 2018/7/8.
//

#ifndef PROFILER_FUNCTIONINFO_H
#define PROFILER_FUNCTIONINFO_H

#include "../CorProfilerStd.h"

class FunctionInfo {
public:
    static FunctionInfo *CreateFunctionInfo(ICorProfilerInfo *profilerInfo, FunctionID functionID);
    ~FunctionInfo(void);

    FunctionID GetFunctionID();
    ClassID GetClassID();
    ModuleID GetModuleID();
    mdToken GetToken();
    LPWSTR GetClassName();
    LPWSTR GetFunctionName();
    LPWSTR GetAssemblyName();
    LPWSTR GetSignatureText();

private:
    FunctionInfo(FunctionID functionID, ClassID classID, ModuleID moduleID, mdToken token, LPWSTR functionName, LPWSTR className, LPWSTR assemblyName, LPWSTR signatureText);
    static PCCOR_SIGNATURE ParseSignature( IMetaDataImport *pMDImport, PCCOR_SIGNATURE signature, WCHAR* szBuffer);
    FunctionID functionID;
    ClassID classID;
    ModuleID moduleID;
    mdToken token;
    LPWSTR className;
    LPWSTR functionName;
    LPWSTR assemblyName;
    LPWSTR signatureText;
};


#endif //PROFILER_FUNCTIONINFO_H
