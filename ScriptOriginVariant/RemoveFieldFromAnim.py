import sys
import os

def remove_field_from_anim_file(input_file_path, field_to_remove):
    # 生成一个临时文件路径
    temp_file_path = input_file_path + ".temp"

    with open(input_file_path, 'r') as input_file, open(temp_file_path, 'w') as temp_file:
        for line in input_file:
            if line.startswith("    path:"):
                # 使用字符串的replace方法删除指定字段
                modified_line = line.replace(field_to_remove, "")
                temp_file.write(modified_line)
            else:
                temp_file.write(line)

    # 关闭文件后，将临时文件内容覆盖原文件
    os.replace(temp_file_path, input_file_path)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python script.py input_file field_to_remove")
        sys.exit(1)

    input_file_path = sys.argv[1]
    field_to_remove = sys.argv[2]

    remove_field_from_anim_file(input_file_path, field_to_remove)

    print("处理完成！")

