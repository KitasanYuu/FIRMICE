@echo off
chcp 65001 > nul

set /p input_folder="请输入目标文件夹路径: "
set /p Resize="请输入目标尺寸: "

cd PngTextureResizer

python PngTextureResizer.py "%input_folder%" %Resize%

echo 等待30秒钟，按任意键提前继续...
timeout /t 30
exit