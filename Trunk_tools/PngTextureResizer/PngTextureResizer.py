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

    for filename in os.listdir(folder_path):
        if filename.lower().endswith('.png'):
            with Image.open(os.path.join(folder_path, filename)) as image:
                print(f"{filename}: 当前尺寸为 {image.size[0]}x{image.size[1]}")
    
    print(f"所有PNG图片将被调整到的目标尺寸：{target_size}x{target_size}")
    print("=====================================================================================================")

    return input("\n确认要继续吗？(y/n): ").lower() == 'y'
    
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
    if not confirm_resize(folder_path, size):
        print("操作已取消。")
        return
    
    png_files = [f for f in os.listdir(folder_path) if f.lower().endswith('.png')]
    total_files = len(png_files)
    
    if total_files == 0:
        print("没有找到可处理的PNG文件。")
        return
    
    print("=====================================================================================================")
    print("=====================================================================================================")
    print(f"\n开始处理{total_files}个文件...\n")
    total_start_time = time.time()
    
    processed_files = 0  # 实际处理的文件数

    for i, filename in enumerate(png_files, start=1):
        input_image_path = os.path.join(folder_path, filename)
        original_size, new_size, processing_time = resize_image(input_image_path, size)
        if processing_time > 0:  # 如果处理时间大于0，则表示文件被处理了
            print(f"已处理{filename}: {original_size:.2f}KB -> {new_size:.2f}KB | 耗时：{processing_time:.2f}秒")
            processed_files += 1
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

    print(f"\n处理完成。总共处理了{total_files}个文件中的{processed_files}个文件，总耗时：{total_time:.2f}秒。")

if __name__ == "__main__":
    print_welcome_message()

    if len(sys.argv) != 3:
        print("Usage: python test.py <folder_path> <size>")
        sys.exit(1)

    folder_path = sys.argv[1]
    size = int(sys.argv[2])

    resize_images_in_folder(folder_path, size)
