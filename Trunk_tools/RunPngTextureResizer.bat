@echo off
chcp 65001 > nul

cd PngTextureResizer

python PngTextureResizer.py

echo 等待30秒钟，按任意键提前继续...
timeout /t 30
exit