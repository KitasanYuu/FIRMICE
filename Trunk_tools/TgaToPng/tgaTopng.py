from PIL import Image
import os
import sys
import time

def print_welcome_message():
    print("=====================================================================================================")
    pattern=[
                "██╗   ██╗██╗   ██╗██╗   ██╗████████╗ ██████╗  ██████╗ ██╗     ███████╗ ",
                "╚██╗ ██╔╝██║   ██║██║   ██║╚══██╔══╝██╔═══██╗██╔═══██╗██║     ██╔════╝ ",
                " ╚████╔╝ ██║   ██║██║   ██║   ██║   ██║   ██║██║   ██║██║     ███████╗ ",
                "  ╚██╔╝  ██║   ██║██║   ██║   ██║   ██║   ██║██║   ██║██║     ╚════██║ ",
                "   ██║   ╚██████╔╝╚██████╔╝   ██║   ╚██████╔╝╚██████╔╝███████╗███████║ ",
                "   ╚═╝    ╚═════╝  ╚═════╝    ╚═╝    ╚═════╝  ╚═════╝ ╚══════╝╚══════╝ ",
                "                                                                       ",
                "████████╗ ██████╗  █████╗ ████████╗ ██████╗ ██████╗ ███╗   ██╗ ██████╗ ",
                "╚══██╔══╝██╔════╝ ██╔══██╗╚══██╔══╝██╔═══██╗██╔══██╗████╗  ██║██╔════╝ ",
                "   ██║   ██║  ███╗███████║   ██║   ██║   ██║██████╔╝██╔██╗ ██║██║  ███╗",
                "   ██║   ██║   ██║██╔══██║   ██║   ██║   ██║██╔═══╝ ██║╚██╗██║██║   ██║",
                "   ██║   ╚██████╔╝██║  ██║   ██║   ╚██████╔╝██║     ██║ ╚████║╚██████╔╝",
                "   ╚═╝    ╚═════╝ ╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝     ╚═╝  ╚═══╝ ╚═════╝ ",                               
            ]
    for line in pattern:
        print(line)
    print("=====================================================================================================")

def tga_to_png(input_folder):
    # 确保输入文件夹存在
    if not os.path.exists(input_folder):
        print("输入文件夹不存在")
        return

    # 获取输入文件夹的绝对路径
    input_folder = os.path.abspath(input_folder)

    # 输出文件夹即为输入文件夹
    output_folder = input_folder

    # 统计需要处理的文件数量
    num_files_to_process = 0
    for filename in os.listdir(input_folder):
        if filename.lower().endswith('.tga'):
            num_files_to_process += 1

    # 显示需要处理的文件数量，并等待用户确认
    print(f"共有 {num_files_to_process} 个TGA文件需要转换。")
    confirm = input("是否要继续转换？(y/n): ")
    if confirm.lower() != 'y':
        print("用户取消了转换。")
        return

    # 获取开始时间
    start_time = time.time()
    files_processed = 0

    # 遍历输入文件夹中的所有文件
    for filename in os.listdir(input_folder):
        # 检查文件是否以 .tga 扩展名结尾
        if filename.lower().endswith('.tga'):
            input_path = os.path.join(input_folder, filename)
            output_path = os.path.join(output_folder, os.path.splitext(filename)[0] + '.png')

            # 检查目标文件夹中是否已经存在同名的 PNG 文件
            if os.path.exists(output_path):
                print(f"警告：{filename} 已存在转换后PNG文件，跳过转换。")
                continue

            # 打开TGA文件并保存为PNG格式
            with Image.open(input_path) as img:
                img.save(output_path, 'PNG')

            # 统计处理成功的文件数量
            files_processed += 1

            # 显示处理完成的文件名、大小和用时
            input_size = os.path.getsize(input_path) / 1024  # 将字节转换为KB
            output_size = os.path.getsize(output_path) / 1024  # 将字节转换为KB
            elapsed_time = time.time() - start_time
            print(f'转换完成：{filename} (大小：{input_size:.2f} KB -> {output_size:.2f} KB，用时：{elapsed_time:.2f} 秒)')

    # 显示处理统计信息
    pattern = [
                "===================================================================================================================",
                "██╗      ██████╗ ██╗   ██╗██╗██╗     ██████╗     ███████╗██╗   ██╗ ██████╗ ██████╗███████╗███████╗███████╗      ██╗",
                "██║      ██╔══██╗██║   ██║██║██║     ██╔══██╗    ██╔════╝██║   ██║██╔════╝██╔════╝██╔════╝██╔════╝██╔════╝      ██║",
                "██║█████╗██████╔╝██║   ██║██║██║     ██║  ██║    ███████╗██║   ██║██║     ██║     █████╗  ███████╗███████╗█████╗██║",
                "╚═╝╚════╝██╔══██╗██║   ██║██║██║     ██║  ██║    ╚════██║██║   ██║██║     ██║     ██╔══╝  ╚════██║╚════██║╚════╝╚═╝",
                "██╗      ██████╔╝╚██████╔╝██║███████╗██████╔╝    ███████║╚██████╔╝╚██████╗╚██████╗███████╗███████║███████║      ██╗",
                "╚═╝      ╚═════╝  ╚═════╝ ╚═╝╚══════╝╚═════╝     ╚══════╝ ╚═════╝  ╚═════╝ ╚═════╝╚══════╝╚══════╝╚══════╝      ╚═╝",
                "==================================================================================================================="
                ]
    for line in pattern:
        print(line)
    print(f'总共处理了 {files_processed} 个文件中的 {num_files_to_process} 个文件')

if __name__ == "__main__":
    print_welcome_message()
    
    # 检查是否提供了输入文件夹作为命令行参数
    if len(sys.argv) != 2:
        print("Usage: python script.py input_folder")
        sys.exit(1)

    input_folder = sys.argv[1]
    tga_to_png(input_folder)
