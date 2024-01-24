import sys
import os

def process_anim_file(input_file_path, additional_path):
    # 生成一个临时文件路径
    temp_file_path = input_file_path + ".temp"

    with open(input_file_path, 'r') as input_file, open(temp_file_path, 'w') as temp_file:
        for line in input_file:
            if line.startswith("    path:"):
                # 使用字符串的strip方法去除原始路径后的空格，然后拼接新路径并在之间加一个空格
                original_path = line[len("    path:"):].strip()
                new_line = f"    path: {additional_path}{original_path}\n"
                temp_file.write(new_line)
            else:
                temp_file.write(line)

    # 关闭文件后，将临时文件内容覆盖原文件
    os.replace(temp_file_path, input_file_path)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python script.py input_file additional_path")
        sys.exit(1)

    input_file_path = sys.argv[1]
    additional_path = sys.argv[2]

    process_anim_file(input_file_path, additional_path)

    print("处理完成！")
