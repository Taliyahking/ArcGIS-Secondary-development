using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Framework;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace NeuInterrogate
{
    // ����һ��Ψһ�� GUID������ COM ע��
    [Guid("1A81FD0D-79EB-476C-A048-A45314272298")]
    // ָ����ӿ�����Ϊ None�������Զ����ɽӿ�
    [ClassInterface(ClassInterfaceType.None)]
    // ָ�������ʶ�������� COM ��������
    [ProgId("NeuInterrogate.Interrogate")]
    public sealed class Interrogate : BaseCommand
    {
        #region COM ע�ắ��

        // COM ע�ắ�������ڽ���ע�ᵽϵͳ��
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            ArcGISCategoryRegistration(registerType);
        }

        // COM ��ע�ắ�������ڽ����ϵͳ���Ƴ�
        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            ArcGISCategoryUnregistration(registerType);
        }

        #region ArcGIS ������ע�������ɵĴ���

        // ArcGIS ������ע�᷽��
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            // ����ע����·��
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            // ע��Ϊ ArcMap ����
            MxCommands.Register(regKey);
        }

        // ArcGIS ������ע�᷽��
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            // ����ע����·��
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            // ��ע�� ArcMap ����
            MxCommands.Unregister(regKey);
        }

        #endregion // End of ArcGIS ������ע�������ɵĴ���

        #endregion // End of COM ע�ắ��

        // ���� ArcGIS Ӧ�ó��������
        private IApplication m_application;

        // ��Ĺ��캯������ʼ������Ļ�������
        public Interrogate()
        {
            base.m_category = "��ѯ����"; // ���������������
            base.m_caption = "Query"; // �����������
            base.m_message = "NEU��ѯϵͳ"; // ����������ʾ��Ϣ
            base.m_toolTip = "NEU Query System"; // �����������ʾ
            base.m_name = "AttributeQuery"; // ������������

            try
            {
                // ���Լ���������ͬ����λͼ��Դ
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
            }
            catch (Exception ex)
            {
                // �������λͼʧ�ܣ����������Ϣ
                System.Diagnostics.Trace.WriteLine(ex.Message, "��Ч��λͼ");
            }
        }

        #region ��д���෽��

        // ���������ʱ����
        public override void OnCreate(object hook)
        {
            if (hook == null)
                return;

            // �� hook ����ת��Ϊ IApplication �ӿ�
            m_application = hook as IApplication;

            // ��� hook �� IMxApplication ���ͣ�����������
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false; // �����������
        }

        // ��������ʱ����
        public override void OnClick()
        {
            // ��������ʾ������
            MainForm f_login = new MainForm();
            f_login.Show();
        }

        #endregion // End of ��д���෽��
    }
}
