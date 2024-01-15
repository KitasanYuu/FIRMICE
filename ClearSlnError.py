import os
import re
import time
import sys
import configparser
from watchdog.observers import Observer
from watchdog.events import FileSystemEventHandler

last_remove_time = None;

def is_project_removed(solution_content, project_name):
    pattern = re.compile(rf'Project\(".*"\) = "{project_name}", "(.*)", ".*"\nEndProject')
    return not bool(pattern.search(solution_content))

def remove_projects_from_solution(solution_path, projects_to_remove):
    max_attempts = 5
    current_attempt = 1

    while current_attempt <= max_attempts:
        try:
            with open(solution_path, 'r', encoding='utf-8') as solution_file:
                solution_content = solution_file.read()

            # 检查是否目标已经被移除
            if all(is_project_removed(solution_content, project) for project in projects_to_remove):
                print("All target projects have already been removed. Exiting...")
                break

            for project_to_remove in projects_to_remove:
                # 构建用于匹配项目的正则表达式
                pattern = re.compile(rf'Project\(".*"\) = "{project_to_remove}", "(.*)", ".*"\nEndProject')

                # 查找并删除匹配的项目
                solution_content = pattern.sub('', solution_content)

            with open(solution_path, 'w', encoding='utf-8') as solution_file:
                solution_file.write(solution_content)

            break  # 操作成功，跳出循环
        except PermissionError as e:
            print(f"PermissionError: {e}. Retrying in 2 seconds...")
            time.sleep(2)
            current_attempt += 1
    else:
        print(f"Failed after {max_attempts} attempts. Could not modify {solution_path}.")

class SolutionHandler(FileSystemEventHandler):
    def __init__(self, solution_path, projects_to_remove, delay_seconds):
        self.solution_path = solution_path
        self.projects_to_remove = projects_to_remove
        self.delay_seconds = delay_seconds

    def on_modified(self, event):
        global last_remove_time

        # 检查距离上次移除操作是否已经过了指定的延迟时间
        if last_remove_time is not None and (time.time() - last_remove_time) < self.delay_seconds:
            return

        if event.src_path.endswith('.sln'):
            print(f"Solution file {event.src_path} has been modified. Removing projects in {self.delay_seconds} seconds...")
            
            # 延迟指定时间后执行移除操作
            time.sleep(self.delay_seconds)

            # 检查是否目标已经被移除
            with open(self.solution_path, 'r', encoding='utf-8') as solution_file:
                solution_content = solution_file.read()

            if all(is_project_removed(solution_content, project) for project in self.projects_to_remove):
                print("All target projects have already been removed. Exiting...")
                return

            remove_projects_from_solution(self.solution_path, self.projects_to_remove)
    
def read_config():
    config = configparser.ConfigParser()
    config_path = os.path.join(os.path.dirname(__file__), "CSEconfig.ini")
    
    # 如果配置文件存在，则读取配置
    if os.path.exists(config_path):
        
        # 检查整个配置文件是否为空
        if os.stat(config_path).st_size == 0:
            print("CSEconfig为空，进程退出...")
            sys.exit(0)

        # 检查是否存在 [Settings] 标签
        if 'Settings' not in config:
            print("为在CSEconfig中定位到[Settings]标签，进程退出...")
            sys.exit(0)

        # 检查是否为空列表，如果是则输出提示信息并结束进程
        if not projects_to_remove or (len(projects_to_remove) == 1 and not projects_to_remove[0]):
            print("检测到配置表，但移除项目为空，进程退出...")
            sys.exit(0)

        config.read(config_path)
        projects_to_remove = config.get("Settings", "ProjectsToRemove", fallback="").strip().split(',')
        print("检测到配置表，将根据配置表进行移除。")
    else:
         # 如果没有读取到配置文件，则创建一个空的配置文件并写入基础格式
        with open(config_path, 'w', encoding='utf-8') as config_file:
            config_file.write("[Settings]\nProjectsToRemove = lilToon.Editor.External, lilToon.Editor\nDelaySeconds = 5\n")
            print("未找到配置文件，进程退出...")
            print(f"未检测到配置文件，已自动创建: {config_path}")
        sys.exit(0)
    
    return projects_to_remove, delay_seconds

def print_configuration(projects_to_remove, delay_seconds):
    print(f"Delay: {delay_seconds} seconds")
    print("Projects to Remove:")
    for project in projects_to_remove:
        print(f"- {project}")

if __name__ == "__main__":
    script_directory = os.path.dirname(os.path.abspath(__file__))
    solution_path = os.path.join(script_directory, "FIRMICE.sln")

    # 读取配置文件
    projects_to_remove, delay_seconds = read_config()

    # 打印读取的配置信息
    print_configuration(projects_to_remove, delay_seconds)

    # 在启动时执行删除操作
    remove_projects_from_solution(solution_path, projects_to_remove)

    # 启动监视器
    event_handler = SolutionHandler(solution_path, projects_to_remove, delay_seconds)
    observer = Observer()
    observer.schedule(event_handler, path=script_directory, recursive=False)
    observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()

    observer.join()
