using PackageManager.Models;

namespace PackageManager.Function.PackageManage
{
    /// <summary>
    /// 为包配置列表提供编辑/删除能力的宿主接口，供窗口或页面实现。
    /// </summary>
    public interface IPackageEditorHost
    {
        void EditItem(PackageItem item, bool isNew);
        void RemoveItem(PackageItem item);
    }
}

