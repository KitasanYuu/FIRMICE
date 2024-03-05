@echo off
chcp 65001 > nul

set /p input_folder="请输入目标文件夹路径: "

cd TgaToPng

python tgaTopng.py "%input_folder%"
pause
