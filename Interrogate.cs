using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Framework;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NeuInterrogate
{
    // 定义一个唯一的 GUID，用于 COM 注册
    [Guid("1A81FD0D-79EB-476C-A048-A45314272298")]
    // 指定类接口类型为 None，避免自动生成接口
    [ClassInterface(ClassInterfaceType.None)]
    // 指定程序标识符，用于 COM 创建对象
    [ProgId("NeuInterrogate.Interrogate")]
    public sealed class Interrogate : BaseCommand
    {
        #region COM 注册函数

        // COM 注册函数，用于将类注册到系统中
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            ArcGISCategoryRegistration(registerType);
        }

        // COM 反注册函数，用于将类从系统中移除
        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            ArcGISCategoryUnregistration(registerType);
        }

        #region ArcGIS 组件类别注册器生成的代码

        // ArcGIS 组件类别注册方法
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            // 构建注册表键路径
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            // 注册为 ArcMap 命令
            MxCommands.Register(regKey);
        }

        // ArcGIS 组件类别反注册方法
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            // 构建注册表键路径
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            // 反注册 ArcMap 命令
            MxCommands.Unregister(regKey);
        }

        #endregion // End of ArcGIS 组件类别注册器生成的代码

        #endregion // End of COM 注册函数

        // 保存 ArcGIS 应用程序的引用
        private IApplication m_application;

        // 类的构造函数，初始化命令的基本属性
        public Interrogate()
        {
            base.m_category = "查询工具"; // 设置命令所属类别
            base.m_caption = "Query"; // 设置命令标题
            base.m_message = "NEU查询系统"; // 设置命令提示信息
            base.m_toolTip = "NEU Query System"; // 设置命令工具提示
            base.m_name = "AttributeQuery"; // 设置命令名称

            try
            {
                // 尝试加载与类名同名的位图资源
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                // 如果加载位图失败，输出错误信息
                System.Diagnostics.Trace.WriteLine(ex.Message, "无效的位图");
            }
        }

        #region 重写的类方法

        // 当命令被创建时调用
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            // 将 hook 对象转换为 IApplication 接口
            m_application = hook as IApplication;

            // 如果 hook 是 IMxApplication 类型，则启用命令
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false; // 否则禁用命令
        }

        // 当命令被点击时调用
        public override void OnClick()
        {
            // 创建并显示主窗体
            MainForm f_login = new MainForm();
            f_login.Show();
        }

        #endregion // End of 重写的类方法
    }
}
