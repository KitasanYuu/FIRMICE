@echo off
chcp 65001 > nul
setlocal

cd CSVGen

python CSVMaker.py

echo 等待30秒钟，按任意键提前继续...
timeout /t 30
exit