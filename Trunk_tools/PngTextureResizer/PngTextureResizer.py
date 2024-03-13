import os
import sys
import time
from PIL import Image

def print_welcome_message():
    print("=====================================================================================================")
    pattern=[
                "██╗   ██╗██╗   ██╗██╗   ██╗    ████████╗ ██████╗  ██████╗ ██╗     ███████╗    ",
                "╚██╗ ██╔╝██║   ██║██║   ██║    ╚══██╔══╝██╔═══██╗██╔═══██╗██║     ██╔════╝    ",
                " ╚████╔╝ ██║   ██║██║   ██║       ██║   ██║   ██║██║   ██║██║     ███████╗    ",
                "  ╚██╔╝  ██║   ██║██║   ██║       ██║   ██║   ██║██║   ██║██║     ╚════██║    ",
                "   ██║   ╚██████╔╝╚██████╔╝       ██║   ╚██████╔╝╚██████╔╝███████╗███████║    ",
                "   ╚═╝    ╚═════╝  ╚═════╝        ╚═╝    ╚═════╝  ╚═════╝ ╚══════╝╚══════╝    ",
                "                                                                              ",
                "██████╗ ███╗   ██╗ ██████╗ ██████╗ ███████╗███████╗██╗███████╗███████╗██████╗ ",
                "██╔══██╗████╗  ██║██╔════╝ ██╔══██╗██╔════╝██╔════╝██║╚══███╔╝██╔════╝██╔══██╗",
                "██████╔╝██╔██╗ ██║██║  ███╗██████╔╝█████╗  ███████╗██║  ███╔╝ █████╗  ██████╔╝",
                "██╔═══╝ ██║╚██╗██║██║   ██║██╔══██╗██╔══╝  ╚════██║██║ ███╔╝  ██╔══╝  ██╔══██╗",
                "██║     ██║ ╚████║╚██████╔╝██║  ██║███████╗███████║██║███████╗███████╗██║  ██║",
                "╚═╝     ╚═╝  ╚═══╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝╚══════╝╚══════╝╚═╝  ╚═╝",                             
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

def confirm_resize(folder_path, target_size):
    """
    显示文件夹内所有PNG图片的当前尺寸，并请求用户确认是否继续。
    """
    print(f"将要处理的文件夹：{folder_path}")

    png_files = []
    traverse_folders = input("是否遍历当前文件夹及其所有子文件夹？(y/n): ").lower() == 'y'
    print("=====================================================================================================")

    if traverse_folders:
        for subdir, _, files in os.walk(folder_path):
            for file in files:
                if file.lower().endswith('.png'):
                    png_files.append(os.path.join(subdir, file))
    else:
        for filename in os.listdir(folder_path):
            if filename.lower().endswith('.png'):
                png_files.append(os.path.join(folder_path, filename))

    if not png_files:
        print("=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=")
        print("没有找到可处理的PNG文件。请检查文件夹路径或确认文件夹中是否包含PNG文件。")
        print("=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=")

        return False, False

    for filename in png_files:
        with Image.open(filename) as image:
            print(f"{os.path.relpath(filename, folder_path)}: 当前尺寸为 {image.size[0]}x{image.size[1]}")

    print("=====================================================================================================")
    print(f"共有{len(png_files)}个待处理文件。")
    print(f"所有PNG图片将被调整到的目标尺寸：{target_size}x{target_size}")
    print("=====================================================================================================")

    confirm_resize_choice = input("确认要继续吗？(y/n): ").lower() == 'y'
    
    return confirm_resize_choice, traverse_folders
    
def resize_image(input_image_path, size):
    start_time = time.time()  # 开始时间
    original_size = os.path.getsize(input_image_path) / 1024  # 原始大小，转换为KB
    
    with Image.open(input_image_path) as image:
        if image.size[0] == size or image.size[1] == size:
            print(f"已跳过{os.path.basename(input_image_path)}:{original_size:.2f}KB -> {original_size:.2f}KB | 耗时：{0:.2f}秒")
            print(f"跳过原因：原始尺寸 {image.size[0]}x{image.size[1]} 等于目标尺寸 {size}x{size}")
            return original_size, original_size, 0  # 未调整大小，处理时间为0

        if image.size[0] < size or image.size[1] < size:
            print("=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=")
            print(f"已跳过{os.path.basename(input_image_path)}:{original_size:.2f}KB -> {original_size:.2f}KB | 耗时：{0:.2f}秒")
            print(f"跳过原因：原始尺寸 {image.size[0]}x{image.size[1]} 小于目标尺寸 {size}x{size}")
            print("=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=")
            return original_size, original_size, 0  # 未调整大小，处理时间为0
        
        resized_image = image.resize((size, size), Image.LANCZOS)
        resized_image.save(input_image_path, "PNG")
    
    new_size = os.path.getsize(input_image_path) / 1024  # 调整后的大小，转换为KB
    processing_time = time.time() - start_time  # 处理时间
    
    return original_size, new_size, processing_time

def resize_images_in_folder(folder_path, size):
    confirmation, traverse_folders = confirm_resize(folder_path, size)

    if not confirmation:
        print("操作已取消。")
        return
    
    png_files = []

    if traverse_folders:
        for subdir, _, files in os.walk(folder_path):
            for file in files:
                if file.lower().endswith('.png'):
                    png_files.append(os.path.join(subdir, file))
    else:
        for filename in os.listdir(folder_path):
            if filename.lower().endswith('.png'):
                png_files.append(os.path.join(folder_path, filename))

    total_files = len(png_files)

    if total_files == 0:
        print("=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=")
        print("没有找到可处理的PNG文件。请检查文件夹路径或确认文件夹中是否包含PNG文件。")
        print("=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=!=")
        return
    
    print("=====================================================================================================")
    print("=====================================================================================================")
    print(f"开始处理{total_files}个文件...\n")
    total_start_time = time.time()
    
    processed_files = 0  # 实际处理的文件数
    skipped_files = 0 # 跳过的文件数

    for i, input_image_path in enumerate(png_files, start=1):
        original_size, new_size, processing_time = resize_image(input_image_path, size)
        if processing_time > 0:  # 如果处理时间大于0，则表示文件被处理了
            print(f"已处理{os.path.relpath(input_image_path, folder_path)}: {original_size:.2f}KB -> {new_size:.2f}KB | 耗时：{processing_time:.2f}秒")
            processed_files += 1
        else:
            skipped_files += 1
        print_progress_bar(i, total_files, prefix='总进度', suffix='完成', length=40)
    
    total_time = time.time() - total_start_time

    print("")
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

    print(f"处理完成。总共处理了{total_files}个文件中的{processed_files}个文件，跳过了{skipped_files}个文件，总耗时：{total_time:.2f}秒。")

if __name__ == "__main__":
    print_welcome_message()

    folder_path = input("目标文件夹路径：")
    size = int(input("目标文件尺寸："))
    print("=====================================================================================================")

    resize_images_in_folder(folder_path, size)
