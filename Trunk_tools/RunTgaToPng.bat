@echo off
chcp 65001 > nul

set /p input_folder="请输入目标文件夹路径: "

cd TgaToPng

python tgaTopng.py "%input_folder%"

echo 等待30秒钟，按任意键提前继续...
timeout /t 30
exit
