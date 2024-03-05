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

def print_progress_bar(iteration, total, prefix='', suffix='', length=50, fill='█'):
    """
    在控制台打印进度条。
    """
    percent = "{0:.1f}".format(100 * (iteration / float(total)))
    filled_length = int(length * iteration // total)
    bar = fill * filled_length + '-' * (length - filled_length)
    print(f'\r{prefix} |{bar}| {percent}% {suffix}', end="\r")
    if iteration == total: 
        print()

def tga_to_png(input_folder, keep_original=False, traverse_folder=False):
    # 确保输入文件夹存在
    if not os.path.exists(input_folder):
        print("输入文件夹不存在")
        return

    # 获取输入文件夹的绝对路径
    input_folder = os.path.abspath(input_folder)

    # 是否遍历文件夹
    traverse_folder = input("是否遍历文件夹？(y/n): ").lower() == 'y'

    # 统计需要处理的文件数量
    num_files_to_process = 0
    if traverse_folder:
        for root, _, files in os.walk(input_folder):
            for filename in files:
                if filename.lower().endswith('.tga'):
                    num_files_to_process += 1
    else:
        for filename in os.listdir(input_folder):
            if filename.lower().endswith('.tga'):
                num_files_to_process += 1
    # 显示需要处理的文件数量，并等待用户确认
    print(f"共有 {num_files_to_process} 个TGA文件需要转换。")
    # 删除原始文件
    if not keep_original:
        confirm_delete = input("是否删除原始TGA文件和对应的.meta文件？(y/n): ")
        if confirm_delete.lower() == 'y':
            delete_original = True
        else:
            delete_original = False

    confirm = input("是否要继续转换？(y/n): ")
    if confirm.lower() != 'y':
        print("用户取消了转换。")
        return
    print("=====================================================================================================")
    print("=====================================================================================================")
    # 获取开始时间
    start_time = time.time()
    files_processed = 0

    # 存储需要删除的文件路径
    files_to_delete = []

    # 遍历输入文件夹中的所有文件
    if traverse_folder:
        for root, _, files in os.walk(input_folder):
            for i, filename in enumerate(files, 1):
                # 检查文件是否以 .tga 扩展名结尾
                if filename.lower().endswith('.tga'):
                    singletime = time.time()
                    input_path = os.path.join(root, filename)
                    output_folder = os.path.dirname(input_path)  # 输出文件夹即为输入文件所在目录
                    output_path = os.path.join(output_folder, os.path.splitext(filename)[0] + '.png')

                    # 检查目标文件夹中是否已经存在同名的 PNG 文件
                    if os.path.exists(output_path):
                        print(f"警告：{filename} 已存在转换后PNG文件，跳过转换。")
                        continue

                    # 打开TGA文件并保存为PNG格式
                    with Image.open(input_path) as img:
                        img.save(output_path, 'PNG')

                    # 显示处理完成的文件名、大小和用时
                    input_size = os.path.getsize(input_path) / 1024  # 将字节转换为KB
                    output_size = os.path.getsize(output_path) / 1024  # 将字节转换为KB
                    elapsed_time = time.time() - singletime
                    print(f'转换完成：{filename} (大小：{input_size:.2f} KB -> {output_size:.2f} KB，耗时：{elapsed_time:.2f} 秒)')
                    
                    # 更新已处理的文件数量
                    files_processed += 1
                    print_progress_bar(files_processed, num_files_to_process, prefix='进度:', suffix='完成', length=50)

        print("=====================================================================================================")
        print("=====================================================================================================")
    # 删除原始文件
    
    if delete_original:
        for file_to_delete in files_to_delete:
            meta_file = file_to_delete + '.meta'
            os.remove(file_to_delete)
            os.remove(meta_file)
            print(f'已删除：{file_to_delete}')
            print(f'已删除：{meta_file}')

    # 显示总体处理时间
    total_time = time.time() - start_time
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
    print(f'\n所有文件已处理完成,处理了{num_files_to_process}中的{files_processed}个文件，总耗时：{total_time:.2f} 秒')

if __name__ == "__main__":
    print_welcome_message()
    input_folder = input("目标文件夹路径：")
    tga_to_png(input_folder, keep_original=False)
