using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using System.Runtime.InteropServices;

namespace NeuInterrogate
{
    public class BlinkManager : IDisposable
    {
        // 定义成员变量
        private IActiveView activeView; // 当前地图视图
        private List<BlinkElement> blinkElements = new List<BlinkElement>(); // 存储所有需要闪烁的要素
        private Timer blinkTimer; // 定时器用于控制闪烁效果
        private bool isBlinkVisible = true; // 用于跟踪闪烁状态

        // 定义 BlinkElement 辅助类，存储元素及其符号
        private class BlinkElement
        {
            public IElement Element { get; set; } // 要素元素
            public ISymbol OriginalSymbol { get; set; } // 原始符号（高亮）
            public ISymbol TransparentSymbol { get; set; } // 透明符号
        }

        // 构造函数，初始化 BlinkManager
        public BlinkManager(IActiveView activeView)
        {
            this.activeView = activeView;

            // 初始化定时器
            blinkTimer = new Timer();
            blinkTimer.Interval = 500; // 闪烁间隔（毫秒）
            blinkTimer.Tick += BlinkTimer_Tick;
        }

        // 闪烁定时器的 Tick 事件，切换可见性
        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            ToggleBlinkVisibility(); // 切换要素可见性
        }

        // 切换闪烁元素的可见性
        private void ToggleBlinkVisibility()
        {
            isBlinkVisible = !isBlinkVisible;

            foreach (var blinkElement in blinkElements)
            {
                switch (blinkElement.Element)
                {
                    case IFillShapeElement fillShapeElement:
                        fillShapeElement.Symbol = isBlinkVisible
                            ? (IFillSymbol)blinkElement.OriginalSymbol
                            : (IFillSymbol)blinkElement.TransparentSymbol;
                        break;
                    case ILineElement lineElement:
                        lineElement.Symbol = isBlinkVisible
                            ? (ILineSymbol)blinkElement.OriginalSymbol
                            : (ILineSymbol)blinkElement.TransparentSymbol;
                        break;
                    case IMarkerElement markerElement:
                        markerElement.Symbol = isBlinkVisible
                            ? (IMarkerSymbol)blinkElement.OriginalSymbol
                            : (IMarkerSymbol)blinkElement.TransparentSymbol;
                        break;
                }
            }

            // 刷新地图视图以显示变化
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }

        // 清除所有闪烁要素
        public void ClearBlinkElements()
        {
            if (blinkElements.Count > 0)
            {
                try
                {
                    // 停止定时器
                    if (blinkTimer.Enabled)
                    {
                        blinkTimer.Stop();
                    }

                    IGraphicsContainer graphicsContainer = (IGraphicsContainer)activeView;
                    foreach (var blinkElement in blinkElements)
                    {
                        try
                        {
                            // 删除图形容器中的元素
                            graphicsContainer.DeleteElement(blinkElement.Element);
                        }
                        catch (COMException comEx)
                        {
                            Console.WriteLine($"删除元素时发生错误: {comEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"删除元素时发生未预料的错误: {ex.Message}");
                        }
                    }
                    blinkElements.Clear();

                    // 刷新地图视图
                    activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"清除闪烁要素时发生错误: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 获取 RGB 颜色，用于设置符号颜色
        private IRgbColor GetColor(int red, int green, int blue, byte transparency = 0)
        {
            IRgbColor rgbColor = new RgbColorClass
            {
                Red = red,
                Green = green,
                Blue = blue,
                Transparency = transparency
            };
            return rgbColor;
        }

        // 创建符号和元素的方法
        public void CreateBlinkElement(IFeature feature, IGraphicsContainer graphicsContainer)
        {
            if (activeView == null)
                return;

            IGeometry geometry = feature.Shape; // 获取要素的几何体
            esriGeometryType geometryType = geometry.GeometryType; // 几何类型

            IElement element = null;
            ISymbol originalSymbol = null;
            ISymbol transparentSymbol = null;

            if (geometryType == esriGeometryType.esriGeometryPolygon) // 如果是多边形
            {
                ISimpleFillSymbol fillSymbol = new SimpleFillSymbolClass
                {
                    Color = GetColor(255, 255, 0, 0), // 黄色，高亮
                    Style = esriSimpleFillStyle.esriSFSSolid
                };
                ISimpleFillSymbol transparentFillSymbol = (ISimpleFillSymbol)((IClone)fillSymbol).Clone();
                transparentFillSymbol.Color = GetColor(255, 255, 0, 255); // 透明

                element = new PolygonElementClass
                {
                    Geometry = geometry,
                    Symbol = fillSymbol
                };

                originalSymbol = (ISymbol)fillSymbol;
                transparentSymbol = (ISymbol)transparentFillSymbol;
            }
            else if (geometryType == esriGeometryType.esriGeometryPolyline) // 如果是线
            {
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbolClass
                {
                    Color = GetColor(255, 255, 0, 0), // 黄色，高亮
                    Width = 3.0,
                    Style = esriSimpleLineStyle.esriSLSSolid
                };
                ISimpleLineSymbol transparentLineSymbol = (ISimpleLineSymbol)((IClone)lineSymbol).Clone();
                transparentLineSymbol.Color = GetColor(255, 255, 0, 255); // 透明

                element = new LineElementClass
                {
                    Geometry = geometry,
                    Symbol = lineSymbol
                };

                originalSymbol = (ISymbol)lineSymbol;
                transparentSymbol = (ISymbol)transparentLineSymbol;
            }
            else if (geometryType == esriGeometryType.esriGeometryPoint) // 如果是点
            {
                ISimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbolClass
                {
                    Color = GetColor(255, 255, 0, 0), // 黄色，高亮
                    Style = esriSimpleMarkerStyle.esriSMSCircle,
                    Size = 10
                };
                ISimpleMarkerSymbol transparentMarkerSymbol = (ISimpleMarkerSymbol)((IClone)markerSymbol).Clone();
                transparentMarkerSymbol.Color = GetColor(255, 255, 0, 255); // 透明

                element = new MarkerElementClass
                {
                    Geometry = geometry,
                    Symbol = markerSymbol
                };

                originalSymbol = (ISymbol)markerSymbol;
                transparentSymbol = (ISymbol)transparentMarkerSymbol;
            }

            if (element != null)
            {
                graphicsContainer.AddElement(element, 0); // 将元素添加到图形容器
                blinkElements.Add(new BlinkElement
                {
                    Element = element,
                    OriginalSymbol = originalSymbol,
                    TransparentSymbol = transparentSymbol
                });
            }
        }

        // 启动闪烁定时器
        public void StartBlinking()
        {
            if (blinkTimer != null)
            {
                isBlinkVisible = true;
                blinkTimer.Start(); // 启动定时器
            }
        }

        // 停止并清理闪烁
        public void StopBlinking()
        {
            if (blinkTimer != null && blinkTimer.Enabled)
            {
                blinkTimer.Stop(); // 停止定时器
            }
            ClearBlinkElements();
        }

        // 高亮并准备闪烁要素
        public void HighlightAndPrepareBlink(List<IFeature> featureList)
        {
            if (activeView == null || featureList == null)
                return;

            IGraphicsContainer graphicsContainer = (IGraphicsContainer)activeView;
            graphicsContainer.DeleteAllElements(); // 清空所有元素
            ClearBlinkElements();

            foreach (var feat in featureList)
            {
                CreateBlinkElement(feat, graphicsContainer); // 为每个要素创建闪烁元素
            }

            activeView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null); // 刷新视图
        }

        // 实现 IDisposable 接口
        public void Dispose()
        {
            StopBlinking(); // 停止闪烁
            if (blinkTimer != null)
            {
                blinkTimer.Tick -= BlinkTimer_Tick; // 移除事件处理器
                blinkTimer.Dispose();
                blinkTimer = null;
            }
        }
    }
}
