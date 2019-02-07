using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using System.Numerics;
using Microsoft.Graphics.Canvas.Text;
using System.Threading.Tasks;
using System.Globalization;
using Windows.UI.Input;

namespace iDGraphic
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        //Armazena as linhas do gráfico
        public List<line> Lines = new List<line>(); 

        public class line
        {
            public float x1 { get; set; }
            public float y1 { get; set; }
            public float x2 { get; set; }
            public float y2 { get; set; }
            public float value { get; set; } //Armazena o valor do final da linha
            public bool isMouseOver { get; set; } //Se true, exibe o retangulo azul e o texto do valor
            public Rect re { get; set; } //Armazena o retangulo do final da linha
        }

        CanvasStrokeStyle strokeStyle = new CanvasStrokeStyle
        {
            DashStyle = CanvasDashStyle.Dot,
            DashCap = CanvasCapStyle.Round,
            StartCap = CanvasCapStyle.Round,
            EndCap = CanvasCapStyle.Round,
            LineJoin = CanvasLineJoin.Bevel,
        };

        CanvasTextFormat MonthsFormat = new CanvasTextFormat
        {
            FontSize = 15
        };

        CanvasTextFormat ValuesFormat = new CanvasTextFormat
        {
            FontSize = 12
        };

        Random rnd = new Random(Environment.TickCount);

        //Define a cultura da formatação da string monetaria
        CultureInfo cultureInfo = new CultureInfo("pt-BR");

        //Mouse
        PointerPoint ptrPt;

        float x1 = 0;
        float y1 = 0;
        float x2 = 0;
        float y2 = 0;
        float a1 = 0;
        float b1 = 0;
        float a2 = 0;
        float b2 = 0;

        const int near = 15; //Define a 'proximidade' para exibir o retangulo azul

        float drawAreaHorizontal, drawAreaVertical; //Limites da area de desenho
        const float lateralArea = 100; //Espaço da area lateral do gráfico
        int itemsH, itemsV; //Total de itens na horizontal e vertical do gráfico
        int itemsTotal; //Total de meses corridos no gráfico
        float vMax; //Valor máximo do gráfico
        float monthValue; //Valor manual de um único mês
        float totalVL; //Total de linhas verticais
        float totalHL; //Total de linhas horizontais
        bool draw = false;

        //Define o valor das variáveis essenciais
        public void startupGraphic()
        {
            itemsH = Convert.ToInt16(tBox_H.Text);
            itemsV = Convert.ToInt16(tBox_V.Text);
            itemsTotal = Convert.ToInt16(tBox_I.Text);
            vMax = Convert.ToUInt32(tBox_Vmax.Text);
            monthValue = Convert.ToInt32(tBox_Vm.Text);

            drawAreaHorizontal = (float)canvas.ActualWidth - lateralArea; //Desconta a area lateral
            drawAreaVertical = (float)canvas.ActualHeight - 30; //Desconta 30 da parte inferior

            totalVL = drawAreaHorizontal / itemsH;
            totalHL = (drawAreaVertical - 10) / itemsV; //Desconta 10 da parte superior
        }

        private void canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            startupGraphic();            

            //Horizontal da canvas
            float vx = totalVL + lateralArea;
            while (vx < drawAreaHorizontal + lateralArea)
            {
                args.DrawingSession.DrawLine(vx, 0, vx, drawAreaVertical + 30, Colors.DarkGray, (float)1.5, strokeStyle);
                vx = vx + totalVL;
            }

            //Linha de isolamento superior
            args.DrawingSession.DrawLine(0 + lateralArea, 10, drawAreaHorizontal + lateralArea, 10, Colors.DarkGray, (float)1.5, strokeStyle);
            //Linha de isolamento lateral
            args.DrawingSession.DrawLine(0 + lateralArea, 0, 0 + lateralArea, drawAreaVertical + 30, Colors.DarkGray, (float)1.5, strokeStyle);
            //Linha de isolamento inferior
            args.DrawingSession.DrawLine(0 + lateralArea, drawAreaVertical, drawAreaHorizontal + lateralArea, drawAreaVertical, Colors.DarkGray, (float)1.5, strokeStyle);

            //Texto inferior
            float tx = 0;
            int ix = 0;
            string month = "JAN";
            while (tx <= drawAreaHorizontal)
            {
                #region IF MESES
                if (ix == 0)
                {
                    month = "JAN";
                }
                if (ix == 1)
                {
                    month = "FEV";
                }
                if (ix == 2)
                {
                    month = "MAR";
                }
                if (ix == 3)
                {
                    month = "ABR";
                }
                if (ix == 4)
                {
                    month = "MAI";
                }
                if (ix == 5)
                {
                    month = "JUN";
                }
                if (ix == 6)
                {
                    month = "JUL";
                }
                if (ix == 7)
                {
                    month = "AGO";
                }
                if (ix == 8)
                {
                    month = "SET";
                }
                if (ix == 9)
                {
                    month = "OUT";
                }
                if (ix == 10)
                {
                    month = "NOV";
                }
                if (ix == 11)
                {
                    month = "DEZ";
                }
                #endregion

                args.DrawingSession.DrawText(month, new Vector2(tx + ((totalVL / 2) - 15) + lateralArea, (float)canvas.ActualHeight - 25), Colors.DarkGray, MonthsFormat);
                tx = tx + totalVL;
                ix++;
            }

            //Texto lateral
            float ty = 0;
            float iy = vMax;
            while (ty <= drawAreaVertical - 10)
            {
                args.DrawingSession.DrawText(string.Format(cultureInfo, "{0:C}", iy), new Vector2(10, ty), Colors.DarkGray, ValuesFormat);
                iy = iy - (vMax / itemsV);

                ty = ty + totalHL;
            }

            //Vertical da canvas
            float hy = totalHL + 10;
            while (hy < (drawAreaVertical - 10))
            {
                args.DrawingSession.DrawLine(0 + lateralArea, hy, drawAreaHorizontal + lateralArea, hy, Colors.DarkGray, (float)1.5, strokeStyle);
                hy = hy + totalHL;
            }

            //Desenha o gráfico mensal manual
            foreach (line l in Lines)
            {
                args.DrawingSession.DrawLine(l.x1, l.y1, l.x2, l.y2, Colors.Red, 1);

                if (l.isMouseOver == true)
                {
                    args.DrawingSession.DrawRectangle(new Rect(new Point(l.x2 + 5, l.y2 + 5), new Point(l.x2 - 5, l.y2 - 5)), Colors.DeepSkyBlue);

                    args.DrawingSession.DrawText(string.Format(cultureInfo, "{0:C}", l.value), new Vector2(l.x2 + 10, l.y2 - 8), Colors.DarkGray, ValuesFormat);
                }
            }

            //Desenha itens aleatórios no gráfico
            if (draw == true)
            {                
                int iT = 0;                
                while (iT < itemsTotal)
                {
                    if (iT == 0)
                    {
                        x1 = totalVL + lateralArea;
                        y1 = rnd.Next(10, (int)drawAreaVertical + 1);
                        args.DrawingSession.DrawLine(lateralArea, rnd.Next(10, (int)drawAreaVertical), x1, y1, Colors.Red, 1);

                        a1 = totalVL + lateralArea;
                        b1 = rnd.Next(10, (int)drawAreaVertical + 1);
                        args.DrawingSession.DrawLine(lateralArea, rnd.Next(10, (int)drawAreaVertical), a1, b1, Colors.YellowGreen, 1);
                        iT++;
                    }
                    if (iT > 0)
                    {
                        x2 = x1 + totalVL;
                        y2 = rnd.Next(10, (int)drawAreaVertical + 1);
                        args.DrawingSession.DrawLine(x1, y1, x2, y2, Colors.Red, 1);
                        x1 = x2;
                        y1 = y2;

                        a2 = a1 + totalVL;
                        b2 = rnd.Next(10, (int)drawAreaVertical + 1);
                        args.DrawingSession.DrawLine(a1, b1, a2, b2, Colors.YellowGreen, 1);
                        a1 = a2;
                        b1 = b2;
                        iT++;
                    }
                }
                draw = false;
            }            
        }

        private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ptrPt = e.GetCurrentPoint(canvas);

            foreach (line l in Lines)
            {
                if (l.re.Contains(ptrPt.Position)) //Se o retangulo - invisivel - ta com o mouse em cima dele
                {
                    l.isMouseOver = true;
                    canvas.Invalidate();
                }
                else
                {
                    l.isMouseOver = false;
                    canvas.Invalidate();
                }
            }
        }               

        //Adicionar o item especificado no gráfico
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            startupGraphic();

            if (x1 == 0 && y1 == 0)
            {
                x1 = lateralArea;
                y1 = Math.Abs(((monthValue * (drawAreaVertical - 10)) / vMax) - drawAreaVertical);

                Lines.Add(new line
                {
                    x1 = x1,
                    y1 = y1,
                    x2 = x1,
                    y2 = y1,
                    value = monthValue,
                    isMouseOver = false,
                    re = new Rect(new Point(x1 + near, y1 + near), new Point(x1 - near, y1 - near))
                });                
            }
            else
            { 
                x2 = x1 + totalVL;
                y2 = Math.Abs(((monthValue * (drawAreaVertical - 10)) / vMax) - drawAreaVertical);

                Lines.Add(new line
                {
                    x1 = x1,
                    y1 = y1,
                    x2 = x2,
                    y2 = y2,
                    value = monthValue,
                    isMouseOver = false,
                    re = new Rect(new Point(x2 + near, y2 + near), new Point(x2 - near, y2 - near))
                });

                x1 = x2;
                y1 = y2;
            }

            canvas.Invalidate();
        }

        //Gerar itens aleatórios no gráfico
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            startupGraphic();

            draw = true;
            Lines.Clear();
            canvas.Invalidate();
        }
    }
}
