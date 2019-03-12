$lineBreak = "`r`n"
$licenses = { /*
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
 
}
 
$noticeTemplate = $licenses.ToString()
$tokenToReplace = [regex]::Escape("[FileName]")

Function CreateFileSpecificNotice($sourcePath){
    $fileName = Split-Path $sourcePath -Leaf
    $fileSpecificNotice = $noticeTemplate -replace $tokenToReplace, $fileName
    return $fileSpecificNotice
}

Function SourceFileContainsNotice($sourcePath){
    $copyrightSnippet = [regex]::Escape("Licensed to the SkyAPM under one or more")

    $fileSpecificNotice = CreateFileSpecificNotice($sourcePath)
    $arrMatchResults = Get-Content $sourcePath | Select-String $copyrightSnippet

    if ($arrMatchResults -ne $null -and $arrMatchResults.count -gt 0){
        return $true 
    }
    else{ 
        return $false 
    }
}

Function AddHeaderToSourceFile($sourcePath) {
    # "Source path is: $sourcePath"
    
    $containsNotice = SourceFileContainsNotice($sourcePath)
    # "Contains notice: $containsNotice"

    if ($containsNotice){
        #"Source file already contains notice -- not adding"
    }
    else {
        #"Source file does not contain notice -- adding"
        $noticeToInsert = CreateFileSpecificNotice($sourcePath)

        $fileLines = (Get-Content $sourcePath) -join $lineBreak
    
        $content = $noticeToInsert + $fileLines + $lineBreak

        $content | Out-File $sourcePath -Encoding utf8

    }
}

$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$parent = (get-item $scriptPath).Parent.FullName
$startingPath = "$parent\src"
Get-ChildItem  $startingPath\*.cs -Recurse | Select FullName | Foreach-Object { AddHeaderToSourceFile($_.FullName)}
Get-ChildItem  $startingPath\*.fs -Recurse | Select FullName | Foreach-Object { AddHeaderToSourceFile($_.FullName)}
