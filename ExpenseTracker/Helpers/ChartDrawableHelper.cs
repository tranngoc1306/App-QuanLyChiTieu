using ExpenseTracker.ViewModels;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Helpers
{
    public class ChartDrawableHelper : IDrawable
    {
        private readonly ReportViewModel _viewModel;

        public ChartDrawableHelper(ReportViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_viewModel.Labels.Count == 0) return;

            DrawGridLines(canvas, dirtyRect);

            var maxValue = _viewModel.GetMaxValue();
            if (maxValue == 0) maxValue = 1000000;

            var spacing = (float)_viewModel.LabelWidth;
            var barWidth = _viewModel.IsMonthView ? 35f : 60f;

            switch (_viewModel.ChartType)
            {
                case "bar":
                    DrawBarChart(canvas, dirtyRect, maxValue, spacing, barWidth);
                    break;
                case "line":
                case "area":
                    DrawLineOrAreaChart(canvas, dirtyRect, maxValue, spacing);
                    break;
            }
        }

        private void DrawGridLines(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.LightGray;
            canvas.StrokeSize = 1;

            for (int i = 0; i <= 5; i++)
            {
                float y = dirtyRect.Height - (dirtyRect.Height / 5 * i);
                canvas.DrawLine(0, y, dirtyRect.Width, y);
            }
        }

        private void DrawBarChart(ICanvas canvas, RectF dirtyRect, float maxValue, float spacing, float barWidth)
        {
            for (int i = 0; i < _viewModel.Labels.Count; i++)
            {
                var centerX = i * spacing + spacing / 2;

                // Draw income bar (left side, green)
                if (_viewModel.ShowIncome && _viewModel.IncomeData[i] > 0)
                {
                    var height = (float)_viewModel.IncomeData[i] / maxValue * dirtyRect.Height * 0.9f;
                    var barX = centerX - barWidth / 2;
                    var barY = dirtyRect.Height - height;

                    canvas.FillColor = Color.FromArgb("#26DE81");
                    canvas.FillRectangle(barX, barY, barWidth / 2 - 2, height);

                    DrawValueLabel(canvas, _viewModel.IncomeData[i], barX + (barWidth / 4), barY - 5);
                }

                // Draw expense bar (right side, red)
                if (_viewModel.ShowExpense && _viewModel.ExpenseData[i] > 0)
                {
                    var height = (float)_viewModel.ExpenseData[i] / maxValue * dirtyRect.Height * 0.9f;
                    var barX = centerX + 2;
                    var barY = dirtyRect.Height - height;

                    canvas.FillColor = Color.FromArgb("#FC5C65");
                    canvas.FillRectangle(barX, barY, barWidth / 2 - 2, height);

                    DrawValueLabel(canvas, _viewModel.ExpenseData[i], barX + (barWidth / 4), barY - 5);
                }
            }
        }

        private void DrawLineOrAreaChart(ICanvas canvas, RectF dirtyRect, float maxValue, float spacing)
        {
            for (int i = 0; i < _viewModel.Labels.Count - 1; i++)
            {
                var centerX = i * spacing + spacing / 2;
                var nextCenterX = (i + 1) * spacing + spacing / 2;

                // Draw income line/area
                if (_viewModel.ShowIncome)
                {
                    var y1 = dirtyRect.Height - ((float)_viewModel.IncomeData[i] / maxValue * dirtyRect.Height * 0.9f);
                    var y2 = dirtyRect.Height - ((float)_viewModel.IncomeData[i + 1] / maxValue * dirtyRect.Height * 0.9f);

                    if (_viewModel.ChartType == "area" && (_viewModel.IncomeData[i] > 0 || _viewModel.IncomeData[i + 1] > 0))
                    {
                        DrawAreaSegment(canvas, centerX, y1, nextCenterX, y2, dirtyRect.Height, "#26DE81");
                    }

                    DrawLineSegment(canvas, centerX, y1, nextCenterX, y2, "#26DE81");
                    DrawPointWithLabel(canvas, centerX, y1, _viewModel.IncomeData[i], "#26DE81");

                    if (i == _viewModel.Labels.Count - 2)
                    {
                        DrawPointWithLabel(canvas, nextCenterX, y2, _viewModel.IncomeData[i + 1], "#26DE81");
                    }
                }

                // Draw expense line/area
                if (_viewModel.ShowExpense)
                {
                    var y1 = dirtyRect.Height - ((float)_viewModel.ExpenseData[i] / maxValue * dirtyRect.Height * 0.9f);
                    var y2 = dirtyRect.Height - ((float)_viewModel.ExpenseData[i + 1] / maxValue * dirtyRect.Height * 0.9f);

                    if (_viewModel.ChartType == "area" && (_viewModel.ExpenseData[i] > 0 || _viewModel.ExpenseData[i + 1] > 0))
                    {
                        DrawAreaSegment(canvas, centerX, y1, nextCenterX, y2, dirtyRect.Height, "#FC5C65");
                    }

                    DrawLineSegment(canvas, centerX, y1, nextCenterX, y2, "#FC5C65");
                    DrawPointWithLabel(canvas, centerX, y1, _viewModel.ExpenseData[i], "#FC5C65");

                    if (i == _viewModel.Labels.Count - 2)
                    {
                        DrawPointWithLabel(canvas, nextCenterX, y2, _viewModel.ExpenseData[i + 1], "#FC5C65");
                    }
                }
            }
        }

        private void DrawAreaSegment(ICanvas canvas, float x1, float y1, float x2, float y2, float height, string color)
        {
            var path = new PathF();
            path.MoveTo(x1, y1);
            path.LineTo(x2, y2);
            path.LineTo(x2, height);
            path.LineTo(x1, height);
            path.Close();
            canvas.FillColor = Color.FromArgb(color).WithAlpha(0.3f);
            canvas.FillPath(path);
        }

        private void DrawLineSegment(ICanvas canvas, float x1, float y1, float x2, float y2, string color)
        {
            canvas.StrokeColor = Color.FromArgb(color);
            canvas.StrokeSize = 3;
            canvas.DrawLine(x1, y1, x2, y2);
        }

        private void DrawPointWithLabel(ICanvas canvas, float x, float y, decimal value, string color)
        {
            canvas.FillColor = Color.FromArgb(color);
            canvas.FillCircle(x, y, 5);

            if (value > 0)
            {
                DrawValueLabel(canvas, value, x, y - 8);
            }
        }

        private void DrawValueLabel(ICanvas canvas, decimal value, float x, float y)
        {
            if (value <= 0) return;

            string label = FormatChartValue(value);

            canvas.FontColor = Colors.Black;
            canvas.FontSize = 10;

            var textSize = canvas.GetStringSize(label, Microsoft.Maui.Graphics.Font.Default, 10);
            canvas.DrawString(label, x - textSize.Width / 2, y, HorizontalAlignment.Left);
        }

        private string FormatChartValue(decimal value)
        {
            if (value >= 1000000)
            {
                return $"{value / 1000000:F1}M";
            }
            else if (value >= 1000)
            {
                return $"{value / 1000:F0}K";
            }
            else
            {
                return $"{value:F0}";
            }
        }
    }
}