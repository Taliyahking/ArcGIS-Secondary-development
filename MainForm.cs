using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI; 
using ESRI.ArcGIS.Framework;



namespace NeuInterrogate
{
    public partial class MainForm : Form
    {
        // 预设的默认用户名和密码
        private const string DefaultUsername = "NEU";
        private const string DefaultPassword = "123456";

        public MainForm()
        {
            InitializeComponent();
            // 通过代码关联事件
            lblForgotPassword.Click += new EventHandler(lblForgotPassword_Click);
        }

        /// <summary>
        /// 进入按钮点击事件
        /// </summary>
        private void btn_login_Click(object sender, EventArgs e)
        {
            try
            {
                // 获取用户输入的用户名和密码
                string enteredUsername = txtUsername.Text.Trim();
                string enteredPassword = txtPassword.Text;

                // 验证用户名和密码
                if (ValidateCredentials(enteredUsername, enteredPassword))
                {
                    // 获取 ArcMap 的当前活动视图
                    IApplication app = GetArcMapApplication();
                    if (app == null)
                    {
                        MessageBox.Show("无法获取 ArcMap 的实例！请确保 ArcMap 已启动。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    IMxDocument mxDocument = app.Document as IMxDocument;
                    IActiveView activeView = mxDocument.ActiveView;

                    // 打开 QueryForm
                    QueryForm queryForm = new QueryForm(activeView); // 传递 IActiveView
                    queryForm.Show();

                    // 关闭登录窗口
                    this.Close();
                }
                else
                {
                    // 验证失败，提示用户
                    MessageBox.Show("用户名或密码不正确，请重试。", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                // 捕获错误并显示
                MessageBox.Show($"登录过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // 获取 ArcMap 的当前活动实例
        private IApplication GetArcMapApplication()
        {
            Type t = Type.GetTypeFromProgID("esriFramework.AppRef");
            object obj = Activator.CreateInstance(t);
            return obj as IApplication;
        }



        /// <summary>
        /// 验证用户名和密码的方法
        /// </summary>
        /// <param name="username">输入的用户名</param>
        /// <param name="password">输入的密码</param>
        /// <returns>如果匹配则返回true，否则返回false</returns>
        private bool ValidateCredentials(string username, string password)
        {
            return username.Equals(DefaultUsername, StringComparison.OrdinalIgnoreCase) && password == DefaultPassword;
        }

        /// <summary>
        /// 退出按钮点击事件
        /// </summary>
        private void btn_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 表单加载事件
        /// </summary>
        private void login_Load(object sender, EventArgs e)
        {
            this.TopMost = true;

            // 设置密码框隐藏字符
            txtPassword.UseSystemPasswordChar = true;

            // 设置默认用户名和密码
            txtUsername.Text = DefaultUsername;
            txtPassword.Text = DefaultPassword;

            // 设置Forgot Password标签的外观
            lblForgotPassword.Cursor = Cursors.Hand;
            lblForgotPassword.Font = new System.Drawing.Font(lblForgotPassword.Font, System.Drawing.FontStyle.Underline);

            // 确保密码信息标签初始时隐藏
            lblPasswordInfo.Visible = false;

            // 美化密码信息标签
            lblPasswordInfo.ForeColor = System.Drawing.Color.Black;
            lblPasswordInfo.BackColor = System.Drawing.Color.LightYellow;
            lblPasswordInfo.BorderStyle = BorderStyle.FixedSingle;
            lblPasswordInfo.Padding = new Padding(5);
        }

        /// <summary>
        /// Forgot Password 标签点击事件
        /// </summary>
        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            // 检查 lblPasswordInfo 是否已经可见
            if (lblPasswordInfo.Visible)
            {
                // 如果已经可见，隐藏它
                lblPasswordInfo.Visible = false;
            }
            else
            {
                // 显示预设的用户名和密码
                lblPasswordInfo.Text = $"用户名: {DefaultUsername}\n密码: {DefaultPassword}";
                lblPasswordInfo.Visible = true;
            }
        }
    }
}
