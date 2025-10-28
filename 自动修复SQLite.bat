@echo off
chcp 65001 >nul
echo ========================================
echo 自动修复 SQLite.Interop.dll 缺失问题
echo ========================================
echo.

set "PROJECT_DIR=%~dp0写入文件备注"
set "OUTPUT_DIR=%PROJECT_DIR%\bin\Debug"

echo 当前目录: %~dp0
echo 项目目录: %PROJECT_DIR%
echo 输出目录: %OUTPUT_DIR%
echo.

REM 检查输出目录是否存在
if not exist "%OUTPUT_DIR%" (
    echo [错误] 输出目录不存在: %OUTPUT_DIR%
    echo 请先编译项目！
    pause
    exit /b 1
)

REM 创建 x86 和 x64 目录
echo [步骤 1] 创建平台目录...
if not exist "%OUTPUT_DIR%\x86" mkdir "%OUTPUT_DIR%\x86"
if not exist "%OUTPUT_DIR%\x64" mkdir "%OUTPUT_DIR%\x64"
echo   ✓ x86 目录: %OUTPUT_DIR%\x86
echo   ✓ x64 目录: %OUTPUT_DIR%\x64
echo.

REM 尝试从 NuGet 包中复制 SQLite.Interop.dll
echo [步骤 2] 查找 SQLite.Interop.dll...

set "FOUND=0"
set "NUGET_CACHE=%USERPROFILE%\.nuget\packages\system.data.sqlite.core"

if exist "%NUGET_CACHE%" (
    echo   检查 NuGet 缓存: %NUGET_CACHE%
    
    for /d %%V in ("%NUGET_CACHE%\*") do (
        set "VERSION_DIR=%%V"
        
        REM 检查 x86 版本
        if exist "%%V\build\net46\x86\SQLite.Interop.dll" (
            echo   找到 x86 版本: %%V\build\net46\x86\
            copy /Y "%%V\build\net46\x86\SQLite.Interop.dll" "%OUTPUT_DIR%\x86\" >nul
            set "FOUND=1"
        ) else if exist "%%V\build\net48\x86\SQLite.Interop.dll" (
            echo   找到 x86 版本: %%V\build\net48\x86\
            copy /Y "%%V\build\net48\x86\SQLite.Interop.dll" "%OUTPUT_DIR%\x86\" >nul
            set "FOUND=1"
        ) else if exist "%%V\runtimes\win-x86\native\netstandard2.0\SQLite.Interop.dll" (
            echo   找到 x86 版本: %%V\runtimes\win-x86\native\
            copy /Y "%%V\runtimes\win-x86\native\netstandard2.0\SQLite.Interop.dll" "%OUTPUT_DIR%\x86\" >nul
            set "FOUND=1"
        )
        
        REM 检查 x64 版本
        if exist "%%V\build\net46\x64\SQLite.Interop.dll" (
            echo   找到 x64 版本: %%V\build\net46\x64\
            copy /Y "%%V\build\net46\x64\SQLite.Interop.dll" "%OUTPUT_DIR%\x64\" >nul
            set "FOUND=1"
        ) else if exist "%%V\build\net48\x64\SQLite.Interop.dll" (
            echo   找到 x64 版本: %%V\build\net48\x64\
            copy /Y "%%V\build\net48\x64\SQLite.Interop.dll" "%OUTPUT_DIR%\x64\" >nul
            set "FOUND=1"
        ) else if exist "%%V\runtimes\win-x64\native\netstandard2.0\SQLite.Interop.dll" (
            echo   找到 x64 版本: %%V\runtimes\win-x64\native\
            copy /Y "%%V\runtimes\win-x64\native\netstandard2.0\SQLite.Interop.dll" "%OUTPUT_DIR%\x64\" >nul
            set "FOUND=1"
        )
    )
)

echo.
echo [步骤 3] 验证结果...
set "SUCCESS=1"

if exist "%OUTPUT_DIR%\x86\SQLite.Interop.dll" (
    echo   ✓ x86\SQLite.Interop.dll 存在
) else (
    echo   ✗ x86\SQLite.Interop.dll 不存在
    set "SUCCESS=0"
)

if exist "%OUTPUT_DIR%\x64\SQLite.Interop.dll" (
    echo   ✓ x64\SQLite.Interop.dll 存在
) else (
    echo   ✗ x64\SQLite.Interop.dll 不存在
    set "SUCCESS=0"
)

echo.
echo ========================================
if "%SUCCESS%"=="1" (
    echo [成功] SQLite.Interop.dll 已成功放置！
    echo.
    echo 请重新运行程序并点击"历史诊断"按钮验证。
) else (
    echo [失败] 未能自动修复！
    echo.
    echo 可能的原因:
    echo   1. NuGet 包未正确安装
    echo   2. 包版本不匹配
    echo.
    echo 请手动执行以下操作：
    echo   1. 在 Visual Studio 中重新安装 System.Data.SQLite.Core
    echo   2. 或参考"修复SQLite缺失DLL.md"中的手动方案
)
echo ========================================
echo.
pause

