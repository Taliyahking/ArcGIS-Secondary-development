using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;

namespace NeuInterrogate
{
    public partial class QueryForm : Form
    {
        private IActiveView activeView; // 用于存储当前地图视图的引用
        private BlinkManager blinkManager; // 用于管理高亮和闪烁效果的工具

        // 构造函数
        public QueryForm(IActiveView activeView)
        {
            InitializeComponent();
            this.activeView = activeView; // 传入地图视图引用

            // 初始化 BlinkManager
            blinkManager = new BlinkManager(activeView);

            // 为按钮和下拉框绑定事件处理器
            this.btn_search.Click += btn_search_Click;
            this.comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;

            // 在窗体关闭时释放资源
            this.FormClosing += QueryForm_FormClosing;
        }

        // 构造函数：无参数，用于设计器
        public QueryForm()
        {
            InitializeComponent();
            // 设计器中无需初始化 BlinkManager
        }

        // 释放 BlinkManager 资源
        private void QueryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (blinkManager != null)
            {
                blinkManager.Dispose(); // 调用 BlinkManager 的 Dispose 方法释放资源
                blinkManager = null;
            }
        }

        // 窗体加载时，初始化图层选择框，加载所有图层
        private void select_Load(object sender, EventArgs e)
        {
            IMap map = activeView.FocusMap; // 获取当前地图

            if (map != null)
            {
                // 遍历地图中的所有图层并添加到 comboBox1 中
                for (int i = 0; i < map.LayerCount; i++)
                {
                    ILayer layer = map.get_Layer(i);

                    if (layer is IFeatureLayer featureLayer) // 检查图层是否为要素图层
                    {
                        IFeatureClass featureClass = featureLayer.FeatureClass;
                        comboBox1.Items.Add(featureClass.AliasName); // 将图层别名添加到下拉框
                    }
                }

                // 设置默认选中项
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;  // 默认选择第一个图层
                }

                // 默认选中第一个图层时加载字段
                LoadFieldsForSelectedLayer();
            }
        }

        // 图层选择变化时，更新字段列表
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFieldsForSelectedLayer(); // 更新字段列表
        }

        // 动态加载选中图层的字段到字段选择框
        private void LoadFieldsForSelectedLayer()
        {
            // 清空字段 ComboBox
            comboBoxFields.Items.Clear();

            string selectedFeatureClassName = comboBox1.Text.Trim(); // 获取选中的图层名称
            IMap map = activeView.FocusMap;
            IFeatureLayer selectedFeatureLayer = FindFeatureLayer(map, selectedFeatureClassName); // 查找对应的要素图层

            if (selectedFeatureLayer != null)
            {
                IFeatureClass featureClass = selectedFeatureLayer.FeatureClass;
                IFields fields = featureClass.Fields;

                bool nameFieldFound = false; // 标记是否找到 Name 字段

                // 遍历字段并填充到 comboBoxFields 中
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    IField field = fields.get_Field(i);
                    comboBoxFields.Items.Add(field.Name); // 将字段名添加到字段选择框

                    // 检查是否是 Name 字段
                    if (field.Name.Equals("Name", StringComparison.OrdinalIgnoreCase))
                    {
                        nameFieldFound = true;
                    }
                }

                // 默认选中 Name 字段（如果存在）
                if (nameFieldFound)
                {
                    comboBoxFields.SelectedItem = "Name";
                }
                else
                {
                    // 如果没有 Name 字段，选中第一个字段
                    if (comboBoxFields.Items.Count > 0)
                    {
                        comboBoxFields.SelectedIndex = 0;
                    }
                }
            }
        }

        // 辅助方法：查找与指定要素类名称匹配的要素图层
        private IFeatureLayer FindFeatureLayer(IMap map, string featureClassName)
        {
            IEnumLayer layers = map.Layers;
            ILayer layer = layers.Next();

            while (layer != null)
            {
                if (layer is IFeatureLayer featureLayer)
                {
                    string layerFeatureClassName = featureLayer.Name;

                    if (string.Equals(featureClassName, layerFeatureClassName, StringComparison.OrdinalIgnoreCase))
                    {
                        return featureLayer; // 找到匹配的图层并返回
                    }
                }

                layer = layers.Next(); // 获取下一个图层
            }

            return null; // 未找到匹配的图层
        }

        // 查询按钮点击事件，执行查询并显示匹配要素
        private void btn_search_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = tb_search.Text.Trim(); // 获取搜索文本
                string selectedFieldName = comboBoxFields.Text.Trim(); // 获取选中的字段
                string selectedFeatureClassName = comboBox1.Text.Trim(); // 获取选中的图层名

                if (string.IsNullOrEmpty(searchText))
                {
                    MessageBox.Show("请输入要搜索的地物名称。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                IMap map = activeView.FocusMap;
                IFeatureLayer selectedFeatureLayer = FindFeatureLayer(map, selectedFeatureClassName); // 查找选中的图层

                if (selectedFeatureLayer != null)
                {
                    // 使用查询过滤器构建 SQL 查询语句
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = $"{selectedFieldName} LIKE '*{searchText.Replace("'", "''").Replace("*", "")}*'";

                    // 执行查询并获取匹配的要素
                    IFeatureCursor cursor = null;
                    List<IFeature> featureList = new List<IFeature>();
                    try
                    {
                        cursor = selectedFeatureLayer.Search(queryFilter, false);
                        IFeature feature = cursor.NextFeature();

                        while (feature != null)
                        {
                            featureList.Add(feature); // 将匹配的要素添加到列表
                            feature = cursor.NextFeature();
                        }
                    }
                    finally
                    {
                        // 释放游标
                        if (cursor != null)
                        {
                            Marshal.ReleaseComObject(cursor);
                            cursor = null;
                        }
                    }

                    // 清除之前的闪烁要素
                    blinkManager.ClearBlinkElements();

                    if (featureList.Count > 0)
                    {
                        MessageBox.Show($"在要素图层 {selectedFeatureClassName} 中找到 {featureList.Count} 个匹配的要素：{searchText}！", "查询结果", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 调整地图视图范围
                        AdjustMapExtent(featureList);

                        // 高亮并准备闪烁要素
                        blinkManager.HighlightAndPrepareBlink(featureList);
                        blinkManager.StartBlinking();

                        // 显示详细信息的表单，传递 activeView
                        FinalForm showForm = new FinalForm(featureList, this.activeView);
                        showForm.Show();
                    }
                    else
                    {
                        MessageBox.Show($"在要素图层 {selectedFeatureClassName} 中未找到匹配的要素：{searchText}！", "查询结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show($"无法找到地图中的要素图层 {selectedFeatureClassName}！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"查询过程中发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 调整地图视图范围，使得所有查询结果都在视图范围内
        private void AdjustMapExtent(List<IFeature> featureList)
        {
            if (featureList == null || featureList.Count == 0)
                return;

            if (featureList.Count == 1)
            {
                IFeature feature = featureList[0];
                IGeometry geometry = feature.ShapeCopy;
                IEnvelope featureEnvelope = geometry.Envelope;

                if (geometry.GeometryType == esriGeometryType.esriGeometryPoint)
                {
                    double bufferDistance = 100; // 为点几何体设置缓冲范围
                    ITopologicalOperator topoOperator = (ITopologicalOperator)geometry;
                    IGeometry bufferGeometry = topoOperator.Buffer(bufferDistance);
                    IEnvelope bufferEnvelope = bufferGeometry.Envelope;

                    activeView.Extent = bufferEnvelope;
                }
                else
                {
                    featureEnvelope.Expand(1.5, 1.5, true); // 扩大范围
                    activeView.Extent = featureEnvelope;
                }
            }
            else
            {
                IEnvelope unionEnvelope = new EnvelopeClass();
                foreach (var feat in featureList)
                {
                    if (feat.ShapeCopy != null)
                    {
                        if (unionEnvelope.IsEmpty)
                            unionEnvelope = feat.ShapeCopy.Envelope;
                        else
                            unionEnvelope.Union(feat.ShapeCopy.Envelope); // 合并范围
                    }
                }

                if (!unionEnvelope.IsEmpty)
                {
                    unionEnvelope.Expand(1.2, 1.2, true); // 稍微扩大范围
                    activeView.Extent = unionEnvelope;
                }
            }

            activeView.Refresh(); // 刷新视图
        }
    }
}
