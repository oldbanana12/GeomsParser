@echo off
pushd "%~dp0"

powershell Compress-7Zip "x64\Release" -ArchiveFileName "SampleX64.zip" -Format Zip

:exit
popd
@echo on
