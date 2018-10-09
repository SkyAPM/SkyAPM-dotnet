//
// Created by liuhaoyang on 2018/7/8.
//

#include "FunctionInfo.h"
//#include "utilcode.h"

FunctionInfo *FunctionInfo::CreateFunctionInfo(ICorProfilerInfo *profilerInfo, FunctionID functionID)
{
    ClassID classID = 0;
    ModuleID moduleID = 0;
    mdToken tkMethod = 0;
    Check(profilerInfo->GetFunctionInfo(functionID, &classID, &moduleID, &tkMethod));

    WCHAR moduleName[MAX_LENGTH];
    AssemblyID assemblyID;
    Check(profilerInfo->GetModuleInfo(moduleID, NULL, MAX_LENGTH, 0, moduleName, &assemblyID));

    WCHAR assemblyName[MAX_LENGTH];
    Check(profilerInfo->GetAssemblyInfo(assemblyID, MAX_LENGTH, 0, assemblyName, NULL, NULL));

    if(wcscmp(assemblyName, W("Logger.Core")) == 0 || PAL_wcscmp(assemblyName, W("mscorlib")) == 0 || PAL_wcscmp(assemblyName, W("lkw_bhsn")) == 0 || PAL_wcscmp(assemblyName, W("System")) == 0)
    {
        return NULL;
    }

    IMetaDataImport* metaDataImport = NULL;
    mdToken token = NULL;
    Check(profilerInfo->GetTokenAndMetaDataFromFunction(functionID, IID_IMetaDataImport, (LPUNKNOWN *) &metaDataImport, &token));

    mdTypeDef classTypeDef;
    WCHAR functionName[MAX_LENGTH];
    WCHAR className[MAX_LENGTH];
    PCCOR_SIGNATURE signatureBlob;
    ULONG signatureBlobLength;
    DWORD methodAttributes = 0;
    Check(metaDataImport->GetMethodProps(token, &classTypeDef, functionName, MAX_LENGTH, 0, &methodAttributes, &signatureBlob, &signatureBlobLength, NULL, NULL));
    Check(metaDataImport->GetTypeDefProps(classTypeDef, className, MAX_LENGTH, 0, NULL, NULL));
    metaDataImport->Release();

    ULONG callConvension = IMAGE_CEE_CS_CALLCONV_MAX;
    signatureBlob += CorSigUncompressData(signatureBlob, &callConvension);

    ULONG argumentCount;
    signatureBlob += CorSigUncompressData(signatureBlob, &argumentCount);

    LPWSTR returnType = new WCHAR[MAX_LENGTH];
    returnType[0] = '\0';
    signatureBlob = ParseSignature(metaDataImport, signatureBlob, returnType);

    WCHAR signatureText[MAX_LENGTH] = W("");

    for ( ULONG i = 0; (signatureBlob != NULL) && (i < argumentCount); ++i )
    {
        LPWSTR parameters = new WCHAR[MAX_LENGTH];
        parameters[0] = '\0';
        signatureBlob = ParseSignature(metaDataImport, signatureBlob, parameters);

        if ( signatureBlob != NULL )
        {
            PAL_wcscat(signatureText, W("|"));
            PAL_wcscat(signatureText, parameters);
        }
    }

    FunctionInfo* result = new FunctionInfo(functionID, classID, moduleID, tkMethod, functionName, className, assemblyName, signatureText);
    return result;
}

PCCOR_SIGNATURE FunctionInfo::ParseSignature( IMetaDataImport *metaDataImport, PCCOR_SIGNATURE signature, LPWSTR signatureText)
{
    COR_SIGNATURE corSignature = *signature++;

    switch (corSignature)
    {
        case ELEMENT_TYPE_VOID:
            PAL_wcscat(signatureText, W("void"));
            break;
        case ELEMENT_TYPE_BOOLEAN:
            PAL_wcscat(signatureText, W("bool"));
            break;
        case ELEMENT_TYPE_CHAR:
            PAL_wcscat(signatureText, W("wchar"));
            break;
        case ELEMENT_TYPE_I1:
            PAL_wcscat(signatureText, W("int8"));
            break;
        case ELEMENT_TYPE_U1:
            PAL_wcscat(signatureText, W("unsigned int8"));
            break;
        case ELEMENT_TYPE_I2:
            PAL_wcscat(signatureText, W("int16"));
            break;
        case ELEMENT_TYPE_U2:
            PAL_wcscat(signatureText, W("unsigned int16"));
            break;
        case ELEMENT_TYPE_I4:
            PAL_wcscat(signatureText, W("int32"));
            break;
        case ELEMENT_TYPE_U4:
            PAL_wcscat(signatureText, W("unsigned int32"));
            break;
        case ELEMENT_TYPE_I8:
            PAL_wcscat(signatureText, W("int64"));
            break;
        case ELEMENT_TYPE_U8:
            PAL_wcscat(signatureText, W("unsigned int64"));
            break;
        case ELEMENT_TYPE_R4:
            PAL_wcscat(signatureText, W("float32"));
            break;
        case ELEMENT_TYPE_R8:
            PAL_wcscat(signatureText, W("float64"));
            break;
        case ELEMENT_TYPE_STRING:
            PAL_wcscat(signatureText, W("String"));
            break;
        case ELEMENT_TYPE_VAR:
            PAL_wcscat(signatureText, W("class variable(unsigned int8)"));
            break;
        case ELEMENT_TYPE_MVAR:
            PAL_wcscat(signatureText, W("method variable(unsigned int8)"));
            break;
        case ELEMENT_TYPE_TYPEDBYREF:
            PAL_wcscat(signatureText, W("refany"));
            break;
        case ELEMENT_TYPE_I:
            PAL_wcscat(signatureText, W("int"));
            break;
        case ELEMENT_TYPE_U:
            PAL_wcscat(signatureText, W("unsigned int"));
            break;
        case ELEMENT_TYPE_OBJECT:
            PAL_wcscat(signatureText, W("Object"));
            break;
        case ELEMENT_TYPE_SZARRAY:
            signature = ParseSignature(metaDataImport, signature, signatureText);
            PAL_wcscat(signatureText, W("[]"));
            break;
        case ELEMENT_TYPE_PINNED:
            signature = ParseSignature(metaDataImport, signature, signatureText);
            PAL_wcscat(signatureText, W("pinned"));
            break;
        case ELEMENT_TYPE_PTR:
            signature = ParseSignature(metaDataImport, signature, signatureText);
            PAL_wcscat(signatureText, W("*"));
            break;
        case ELEMENT_TYPE_BYREF:
            signature = ParseSignature(metaDataImport, signature, signatureText);
            PAL_wcscat(signatureText, W("&"));
            break;
        case ELEMENT_TYPE_VALUETYPE:
        case ELEMENT_TYPE_CLASS:
        case ELEMENT_TYPE_CMOD_REQD:
        case ELEMENT_TYPE_CMOD_OPT:
        {
            mdToken	token;
            signature += CorSigUncompressToken( signature, &token );

            WCHAR className[ MAX_LENGTH ];
            if ( TypeFromToken( token ) == mdtTypeRef )
            {
                Check(metaDataImport->GetTypeRefProps(token, NULL, className, MAX_LENGTH, NULL));
            }
            else
            {
                Check(metaDataImport->GetTypeDefProps(token, className, MAX_LENGTH, NULL, NULL, NULL ));
            }

            PAL_wcscat(signatureText, className);
        }
            break;
        case ELEMENT_TYPE_GENERICINST:
        {
            signature = ParseSignature(metaDataImport, signature, signatureText);

            PAL_wcscat(signatureText, W("<"));
            ULONG arguments = CorSigUncompressData(signature);
            for (ULONG i = 0; i < arguments; ++i)
            {
                if(i != 0)
                {
                    PAL_wcscat(signatureText, W(", "));
                }

                signature = ParseSignature(metaDataImport, signature, signatureText);
            }
            PAL_wcscat(signatureText, W(">"));
        }
            break;
        case ELEMENT_TYPE_ARRAY:
        {
            signature = ParseSignature(metaDataImport, signature, signatureText);
            ULONG rank = CorSigUncompressData(signature);
            if ( rank == 0 )
            {
                PAL_wcscat(signatureText, W("[?]"));
            }
            else
            {
                ULONG arraysize = (sizeof(ULONG) * 2 * rank);
                ULONG *lower = (ULONG *)alloca(arraysize);
                memset(lower, 0, arraysize);
                ULONG *sizes = &lower[rank];

                ULONG numsizes = CorSigUncompressData(signature);
                for (ULONG i = 0; i < numsizes && i < rank; i++)
                {
                    sizes[i] = CorSigUncompressData(signature);
                }

                ULONG numlower = CorSigUncompressData(signature);
                for (ULONG i = 0; i < numlower && i < rank; i++)
                {
                    lower[i] = CorSigUncompressData( signature );
                }

                PAL_wcscat(signatureText, W("["));
                for (ULONG i = 0; i < rank; ++i)
                {
                    if (i > 0)
                    {
                        PAL_wcscat(signatureText, W(","));
                    }

                    if (lower[i] == 0)
                    {
                        if(sizes[i] != 0)
                        {
                            WCHAR *size = new WCHAR[MAX_LENGTH];
                            size[0] = '\0';
                            PAL_wcscat(signatureText, size);
                        }
                    }
                    else
                    {
                        WCHAR *low = new WCHAR[MAX_LENGTH];
                        low[0] = '\0';
                        PAL_wcscat(signatureText, low);
                        PAL_wcscat(signatureText, W("..."));

                        if (sizes[i] != 0)
                        {
                            WCHAR *size = new WCHAR[MAX_LENGTH];
                            size[0] = '\0';
                            PAL_wcscat(signatureText, size);
                        }
                    }
                }
                PAL_wcscat(signatureText, W("]"));
            }
        }
            break;
        default:
        case ELEMENT_TYPE_END:
        case ELEMENT_TYPE_SENTINEL:
            WCHAR *elementType = new WCHAR[MAX_LENGTH];
            elementType[0] = '\0';
            PAL_wcscat(signatureText, elementType);
            break;
    }

    return signature;
}

FunctionInfo::FunctionInfo(FunctionID functionID, ClassID classID, ModuleID moduleID, mdToken token, LPWSTR functionName, LPWSTR className, LPWSTR assemblyName, LPWSTR signatureText)
{
    FunctionInfo::functionID = functionID;
    FunctionInfo::classID = classID;
    FunctionInfo::moduleID = moduleID;
    FunctionInfo::token = token;
    FunctionInfo::functionName = functionName;
    FunctionInfo::className = className;
    FunctionInfo::assemblyName = assemblyName;
    FunctionInfo::signatureText = signatureText;
}

FunctionInfo::~FunctionInfo(void)
{
    free(functionName);
    free(className);
    free(assemblyName);
    free(signatureText);
}

FunctionID FunctionInfo::GetFunctionID()
{
    return functionID;
}

ClassID FunctionInfo::GetClassID()
{
    return classID;
}

ModuleID FunctionInfo::GetModuleID()
{
    return moduleID;
}

mdToken FunctionInfo::GetToken()
{
    return token;
}

LPWSTR FunctionInfo::GetClassName()
{
    return className;
}

LPWSTR FunctionInfo::GetFunctionName()
{
    return functionName;
}

LPWSTR FunctionInfo::GetAssemblyName()
{
    return assemblyName;
}

LPWSTR FunctionInfo::GetSignatureText()
{
    return signatureText;
}