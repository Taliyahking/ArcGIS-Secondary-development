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
    public partial class FinalForm : Form
    {
        private List<IFeature> featureList;
        private IActiveView activeView;
        private BlinkManager blinkManager;

        // 构造函数：用于运行时
        public FinalForm(List<IFeature> featureList, IActiveView activeView)
        {
            InitializeComponent();
            this.featureList = featureList;
            this.activeView = activeView;

            // 初始化 BlinkManager
            blinkManager = new BlinkManager(activeView);

            // DataGridView 的 CellClick 事件
            dataGridView1.CellClick += DataGridView1_CellClick;

            // 绑定 Load 事件
            this.Load += FinalForm_Load;

            // 在窗体关闭时释放资源
            this.FormClosing += FinalForm_FormClosing;
        }

        // 构造函数：无参数，用于设计器
        public FinalForm()
        {
            InitializeComponent();

        }

        // 释放 BlinkManager 资源
        private void FinalForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (blinkManager != null)
            {
                blinkManager.Dispose();
                blinkManager = null;
            }
        }

        // 窗体加载时，初始化 DataGridView 并显示要素属性
        private void FinalForm_Load(object sender, EventArgs e)
        {
            if (activeView == null || featureList == null)
                return;

            // 清空 DataGridView
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // 设置选择模式为整行选择
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;

            if (featureList.Count == 0)
            {
                return;
            }

            // 获取字段名
            IFields fields = featureList[0].Fields;

            // 添加字段名到 DataGridView 的列
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.get_Field(i);
                dataGridView1.Columns.Add(field.Name, field.AliasName);
            }

            // 添加要素属性到 DataGridView 的行
            foreach (var feature in featureList)
            {
                // 创建 DataGridView 的行
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView1);

                // 添加每个字段的值到行
                for (int i = 0; i < fields.FieldCount; i++)
                {
                    object value = feature.get_Value(i);
                    row.Cells[i].Value = value;
                }

                // 将行添加到 DataGridView
                dataGridView1.Rows.Add(row);
            }
        }

        // DataGridView 单元格点击事件
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 如果点击的是列头或无效单元格，直接返回
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            // 获取点击的单元格内容
            var cellValue = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            // 如果单元格内容为空，设置为 "无内容"
            string detail = cellValue != null ? cellValue.ToString() : "无内容";

            // 将内容显示到 TextBox
            textBoxDetail.Text = detail;

            // 高亮选中的要素
            HighlightSelectedFeature(e.RowIndex);
        }

        // 高亮选中的要素并开始闪烁
        private void HighlightSelectedFeature(int rowIndex)
        {
            if (activeView == null || featureList == null)
                return;

            // 添加边界检查
            if (rowIndex < 0 || rowIndex >= featureList.Count)
            {
                MessageBox.Show("选择的行无效。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            IMap map = activeView.FocusMap;

            // 清除之前的选择
            map.ClearSelection();

            // 清除之前的闪烁要素
            blinkManager.ClearBlinkElements();

            // 获取对应的要素
            IFeature selectedFeature = featureList[rowIndex];

            // 获取要素所属的图层
            IFeatureLayer featureLayer = GetFeatureLayer(selectedFeature);
            if (featureLayer != null)
            {
                // 选择要素
                map.SelectFeature(featureLayer, selectedFeature);

                // 刷新视图以显示选择
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, featureLayer, selectedFeature.Shape.Envelope);
            }

            // 高亮选中的要素并开始闪烁
            blinkManager.HighlightAndPrepareBlink(new List<IFeature> { selectedFeature });
            blinkManager.StartBlinking();
        }

        // 辅助方法：查找与指定要素类匹配的要素图层
        private IFeatureLayer GetFeatureLayer(IFeature feature)
        {
            IMap map = activeView.FocusMap;

            // 遍历地图中的所有图层，找到包含该要素的图层
            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer layer = map.get_Layer(i);
                if (layer is IFeatureLayer featureLayer)
                {
                    if (featureLayer.FeatureClass.Equals(feature.Class))
                    {
                        return featureLayer;
                    }
                }
            }
            return null;
        }
    }
}
