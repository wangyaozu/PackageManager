# -*- coding: utf-8 -*-
"""
发布脚本（Python）：
- 从 AssemblyInfo.cs 解析版本（取前三段），生成版本标签 vX.Y.Z。
- 通过 FTP/FTPS 在远端 /PackageManager 下创建对应版本目录并上传 PackageManager.exe。
- 将 UpdateSummary.txt 上传到 /UpdateSummary/UpdateSummary.txt。

说明：
- 远端 HTTP 访问地址为 http://192.168.0.215:8001/，但上传使用 FTP 映射的路径。
- 默认使用 FTP Explicit SSL（FTPS），如需明文 FTP 可将 FTP_USE_TLS 设为 False。
- 借鉴 scripts/download.py 的 ftplib 用法。
"""

import os
import re
import sys
from ftplib import FTP, FTP_TLS, error_perm


# 本地路径配置
EXE_LOCAL_PATH = r"E:\PackageManager\bin\Debug\PackageManager.exe"
SUMMARY_LOCAL_PATH = r"E:\PackageManager\UpdateSummary.txt"
ASSEMBLY_INFO_PATH = r"E:\PackageManager\Properties\AssemblyInfo.cs"

# 远端 FTP 配置（根据需要修改）
FTP_HOST = os.environ.get("PKG_FTP_HOST", "192.168.0.215")
FTP_PORT = int(os.environ.get("PKG_FTP_PORT", "21"))
FTP_USER = os.environ.get("PKG_FTP_USER", "hwuser")
FTP_PASS = os.environ.get("PKG_FTP_PASS", "hongwa666.")
FTP_USE_TLS = (os.environ.get("PKG_FTP_USE_TLS", "true").lower() in ("1", "true", "yes"))
FTP_PASSIVE = (os.environ.get("PKG_FTP_PASSIVE", "true").lower() in ("1", "true", "yes"))

# 远端路径（与 HTTP 映射一致）
REMOTE_PACKAGES_BASE = os.environ.get("PKG_REMOTE_PACKAGES_BASE", "/PackageManager")
REMOTE_SUMMARY_BASE = os.environ.get("PKG_REMOTE_SUMMARY_BASE", "/UpdateSummary")


def read_text(path):
    with open(path, "r", encoding="utf-8", errors="ignore") as f:
        return f.read()


def get_version_tag(assembly_info_path):
    text = read_text(assembly_info_path)
    m = re.search(r"\[assembly:\s*AssemblyVersion\(\"(?P<v>\d+\.\d+\.\d+)(?:\.\d+)?\"\)\]", text)
    if not m:
        raise RuntimeError("无法解析 AssemblyVersion：%s" % assembly_info_path)
    v = m.group("v")
    return "v%s" % v


def ensure_dir(ftp, remote_dir):
    """逐层创建远端目录，如果存在则忽略 550 错误。"""
    # 规范化，确保以 / 开头
    parts = [p for p in remote_dir.strip("/").split("/") if p]
    cur = ""
    for seg in parts:
        cur = (cur + "/" + seg).lstrip("/")  # 去掉前导重复斜杠
        path = "/" + cur
        try:
            ftp.mkd(path)
        except error_perm as e:
            # 目录已存在通常是 550，忽略
            msg = str(e).lower()
            if "file exists" in msg or "directory exists" in msg or "already exists" in msg or "550" in msg:
                pass
            else:
                # 其他权限错误需要抛出
                raise


def upload_file(ftp, local_path, remote_dir, remote_name):
    if not os.path.isfile(local_path):
        raise RuntimeError("本地文件不存在：%s" % local_path)
    ensure_dir(ftp, remote_dir)
    remote_path = remote_dir.rstrip("/") + "/" + remote_name
    # 使用 STOR 完整路径上传
    with open(local_path, "rb") as f:
        ftp.storbinary("STOR " + remote_path, f)


def connect_ftp():
    if FTP_USE_TLS:
        ftps = FTP_TLS()
        ftps.connect(host=FTP_HOST, port=FTP_PORT, timeout=20)
        # FTP_TLS.login 会自动执行 AUTH，随后显式开启数据保护
        ftps.login(user=FTP_USER, passwd=FTP_PASS)
        try:
            ftps.prot_p()  # 开启保护模式
        except Exception:
            # 某些服务器仍然工作，即使不支持 PROT 命令
            pass
        ftps.set_pasv(FTP_PASSIVE)
        return ftps
    else:
        ftp = FTP()
        ftp.connect(host=FTP_HOST, port=FTP_PORT, timeout=20)
        ftp.login(user=FTP_USER, passwd=FTP_PASS)
        ftp.set_pasv(FTP_PASSIVE)
        return ftp


def main():
    version_tag = get_version_tag(ASSEMBLY_INFO_PATH)
    print("解析版本成功：%s" % version_tag)

    # 目标目录与文件
    exe_remote_dir = (REMOTE_PACKAGES_BASE.rstrip("/") + "/" + version_tag)
    summary_remote_dir = REMOTE_SUMMARY_BASE.rstrip("/")

    ftp = None
    try:
        ftp = connect_ftp()
        # 上传 EXE
        upload_file(ftp, EXE_LOCAL_PATH, exe_remote_dir, "PackageManager.exe")
        print("已上传 EXE 到：%s/PackageManager.exe" % exe_remote_dir)

        # 上传 UpdateSummary.txt
        upload_file(ftp, SUMMARY_LOCAL_PATH, summary_remote_dir, "UpdateSummary.txt")
        print("已上传 UpdateSummary.txt 到：%s/UpdateSummary.txt" % summary_remote_dir)

        print("发布完成")
    except Exception as e:
        print("发布失败：%s" % e)
        sys.exit(1)
    finally:
        try:
            if ftp:
                ftp.quit()
        except Exception:
            pass


if __name__ == "__main__":
    main()

