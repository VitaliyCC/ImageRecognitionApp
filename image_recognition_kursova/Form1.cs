using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZedGraph;

namespace ImageRecognitionApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Оберіть файл";
            openFileDialog1.Filter = "Зображення|*.jpg;*.png;*.bmp";
            openFileDialog1.ShowDialog();
            String pathimg = openFileDialog1.FileName;
            textBox1.Text = pathimg;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Оберіть файл";
            openFileDialog1.Filter = "Зображення|*.jpg;*.png;*.bmp";
            openFileDialog1.ShowDialog();
            String pathimg = openFileDialog1.FileName;
            textBox2.Text = pathimg;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Оберіть файл";
            openFileDialog1.Filter = "Зображення|*.jpg;*.png;*.bmp";
            openFileDialog1.ShowDialog();
            String pathimg = openFileDialog1.FileName;
            textBox3.Text = pathimg;
        }

        public int optimal_delta;

        private void button1_Click(object sender, EventArgs e)
        {

            //відкрити обрані зображення
            String[] path = new string[3];
            Bitmap[] bmp = new Bitmap[3];

            path[0] = textBox1.Text;
            path[1] = textBox2.Text;
            path[2] = textBox3.Text;

            for (int i = 0; i < 3; i++)
            {
                bmp[i] = new Bitmap(Image.FromFile(path[i]));
            }

            // створити навчальну матрицю
            int w = bmp[0].Width;
            int h = bmp[0].Height;
            byte[][,] r = new byte[3][,];
            Color[] c = new Color[3];

            for (int k = 0; k < 3; k++)
                r[k] = new byte[h, w];

            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++)
                    {
                        c[k] = bmp[k].GetPixel(i, j);
                        r[k][i, j] = (byte)(0.241 * c[k].R + 0.619 * c[k].G + 0.068 * c[k].B);
                    }
                }
            }

            //запис навчальної матриці у файл
            label2.Text = "Матриця яскравості побудована – створено файл matrBright1.txt";
            StreamWriter sw1 = new StreamWriter("matrBright1.txt");
            for (int k = 0; k < 3; k++)
            {
                sw1.WriteLine("Класс №" + k);
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++)
                    {
                        sw1.Write(r[k][i, j]);
                        sw1.Write(" ");
                    }
                    sw1.WriteLine("\n");
                }
            }

            //Знаходимо середнє арифметичне по кожному стовпцю навчальної матриці (нижні та верхні допуски)
            sw1.Close();
            double[,] sa = new double[3, w];
            for (int k = 0; k < 3; k++)
            {
                for (int j = 0; j < h; j++)
                {
                    sa[k, j] = 0;
                    for (int i = 0; i < w; i++)
                    {
                        sa[k, j] = sa[k, j] + r[k][i, j];
                    }
                    sa[k, j] = sa[k, j] / h;
                }
            }
            double[,] ni = new double[3, w];
            double[,] vi = new double[3, w];
            StreamWriter sw20 = new StreamWriter("matNi.txt");
            StreamWriter sw21 = new StreamWriter("matVi.txt");
            label_ni_vi.Text = "Середнє арифметичне по кожному стовпцю навчальної матриці – створено файли matNi.txt та matVi.txt";

            //формування бінарної матриці
            double delta = 20;
            byte[][,] bin = new byte[3][,];
            for (int k = 0; k < 3; k++)
                bin[k] = new byte[h, w];
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    vi[k, i] = sa[k, i] + delta;
                    ni[k, i] = sa[k, i] - delta;
                    for (int j = 0; j < w; j++)
                    {

                        if (r[k][i, j] > sa[k, i] - delta && r[k][i, j] < sa[k, i] + delta)
                        {
                            bin[k][i, j] = 1;
                        }
                        else
                        {
                            bin[k][i, j] = 0;
                        }
                    }
                }
            }
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < w; i++)
                {
                    sw20.Write(ni[k, i]);
                    sw21.Write(vi[k, i]);
                    sw20.WriteLine("\n");
                    sw21.WriteLine("\n");
                }
                sw20.WriteLine("--------------------------------------");
                sw21.WriteLine("\n");
            }
            sw20.Close();
            sw21.Close();

            //розрахунок середнього арифметичного по стовпцях бинарної матриці
            label3.Text = "Бінарна матриця побудована файл – створено файл matBinary1.txt";
            double[,] sab = new double[3, w];
            for (int k = 0; k < 3; k++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++)
                    {
                        sab[k, j] = sab[k, j] + bin[k][i, j];
                    }
                    sab[k, j] = (sab[k, j]) / h;
                }
            }

            //формування еталонного вектору
            int[,] evect = new int[3, w];
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < w; i++)
                {

                    if (sab[k, i] >= 0.5)
                    {
                        evect[k, i] = 1;
                    }
                    else
                    {
                        evect[k, i] = 0;
                    }
                }
            }

            //запис у файл бінорної матриці та еталонного вектору
            label4.Text = "Еталонний вектор побудований – створено файл vector1.txt";
            StreamWriter sw3 = new StreamWriter("matBinary1.txt");
            StreamWriter sw5 = new StreamWriter("vector1.txt");
            for (int k = 0; k < 3; k++)
            {
                sw3.WriteLine("Класс №" + k);
                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        sw3.Write(bin[k][i, j]);
                        sw3.Write(" ");
                    }
                    sw3.WriteLine("\n");
                }
            }
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < w; i++)
                {
                    sw5.Write(evect[k, i]);
                    sw5.Write(" ");
                }
                sw5.WriteLine("\n");
            }
            sw3.Close();
            sw5.Close();
            int d, d1;
            int[] para = new int[3];
            int[][,] sk = new int[3][,];
            for (int k = 0; k < 3; k++)
                sk[k] = new int[2, h];
            d = 0;
            d1 = 2147483647;

            //формування масиву ПАРА
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != k)
                    {
                        d = 0;
                        for (int j = 0; j < h; j++)
                        {
                            if (evect[k, j] != evect[i, j])
                            {
                                d++;
                            }
                        }
                        if (d < d1)
                        {
                            d1 = d;
                            para[k] = i;
                        }
                    }
                }
            }

            //формування кодових відстаней
            int sum1;
            int sum2;
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    sum1 = 0;
                    sum2 = 0;
                    for (int j = 0; j < w; j++)
                    {
                        if (evect[k, j] != bin[k][i, j]) sum1++;
                        if (evect[k, j] != bin[para[k]][i, j]) sum2++;
                    }
                    sk[k][0, i] = sum1;
                    sk[k][1, i] = sum2;
                }
            }

            //запис до файлу масиву кодових відстаней
            label5.Text = "Масив кодових відстаней сформований – створено файл masSk1.txt";
            StreamWriter sw7 = new StreamWriter("masSk1.txt");
            for (int k = 0; k < 3; k++)
            {
                sw7.WriteLine("Класс №" + k);
                for (int i = 0; i < 2; ++i)
                {
                    for (int j = 0; j < h; ++j)
                    {
                        sw7.Write(sk[k][i, j]);
                        sw7.Write(" ");
                    }
                    sw7.WriteLine("\n");
                }
            }
            sw7.Close();
            int kk1, kk2, kk3, kk4;
            double dd1 = 0, dd2 = 0, a = 0, b = 0, ee = 0;
            StreamWriter sw15 = new StreamWriter("writedEe.txt");
            StreamWriter sw16 = new StreamWriter("writedD1.txt");
            StreamWriter sw17 = new StreamWriter("writedD2.txt");
            GraphPane pane1 = zedGraphControl1.GraphPane;
            pane1.XAxis.Title.Text = "r";
            pane1.YAxis.Title.Text = "E";
            PointPairList list1 = new PointPairList();
            pane1.Title.Text = "Клас 1";
            GraphPane pane2 = zedGraphControl2.GraphPane;
            pane2.XAxis.Title.Text = "r";
            pane2.YAxis.Title.Text = "E";
            PointPairList list2 = new PointPairList();
            pane2.Title.Text = "Клас 2";
            PointPairList list4 = new PointPairList();
            PointPairList list3 = new PointPairList();
            GraphPane pane3 = zedGraphControl3.GraphPane;
            pane3.XAxis.Title.Text = "r";
            pane3.YAxis.Title.Text = "E";
            PointPairList list5 = new PointPairList();
            pane3.Title.Text = "Клас 3";
            PointPairList list6 = new PointPairList();

            GraphPane pane4 = zedGraphControl4.GraphPane;
            pane4.XAxis.Title.Text = "delta";
            pane4.YAxis.Title.Text = "E";
            PointPairList list7 = new PointPairList();
            pane4.Title.Text = "Opt (E sr)";

            pane1.XAxis.Scale.Max = 50;
            pane2.XAxis.Scale.Max = 50;
            pane3.XAxis.Scale.Max = 50;
            pane4.XAxis.Scale.Max = 50;

            //розрахунок точнісних характеристик
            double max = 0;
            int[] rad = new int[3];

            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    kk1 = 0;
                    kk2 = 0;
                    kk3 = 0;
                    kk4 = 0;
                    for (int j = 0; j < h; j++)
                    {
                        if (sk[k][0, j] <= i) kk1++; else kk2 = h - kk1;
                        if (sk[k][1, j] <= i) kk3++; else kk4 = h - kk3;
                        dd1 = (double)kk1 / h;
                        dd2 = (double)kk4 / h;
                        a = (double)kk2 / h;
                        b = (double)kk3 / h;

                    }
                    ee = Math.Log(((2 - (a + b)) / (a + b)), 2) * (1 - (a + b));
                    max = 0;

                    //будуємо графік
                    if (k == 0)
                    {
                        double x1;
                        x1 = i;
                        double y1 = ee;
                        list1.Add(x1, y1);
                        double x3 = 0;
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                            x3 = i;
                            double y3 = ee;
                            list3.Add(x3, y3);
                        }

                    }
                    max = 0;
                    if (k == 1)
                    {
                        double x2;
                        x2 = i;
                        double y2 = ee;
                        list2.Add(x2, y2);
                        double x4 = 0;
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                            x4 = i;
                            double y4 = ee;
                            list4.Add(x4, y4);
                        }

                    }
                    max = 0;
                    if (k == 2)
                    {
                        double x3;
                        x3 = i;
                        double y3 = ee;
                        list5.Add(x3, y3);
                        double x5 = 0;
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                            x5 = i;
                            double y5 = ee;
                            list6.Add(x5, y5);
                        }

                    }
                    sw15.Write(ee);
                    sw16.Write(dd1);
                    sw17.Write(dd2);
                    sw15.WriteLine("\n");
                    sw16.WriteLine("\n");
                    sw17.WriteLine("\n");
                }

                sw15.Write("----------------------------------------------------------------\n");
                sw16.Write("----------------------------------------------------------------\n");
                sw17.Write("----------------------------------------------------------------\n");
            }

            //розрахунок оптимальных допусків
            double[,] maxE = new double[3, h];
            int te = 0;
            for (delta = 0; delta < 50; delta++)
            {
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < h; i++)
                    {
                        vi[k, i] = sa[k, i] + delta;
                        ni[k, i] = sa[k, i] - delta;
                        for (int j = 0; j < w; j++)
                        {
                            if (r[k][i, j] > sa[k, i] - delta && r[k][i, j] < sa[k, i] + delta)
                            {
                                bin[k][i, j] = 1;
                            }
                            else
                            {
                                bin[k][i, j] = 0;
                            }
                        }
                    }
                }

                //розрахунок середнього арифметичного по стовпцях бинарної матриці
                for (int k = 0; k < 3; k++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        for (int i = 0; i < w; i++)
                        {
                            sab[k, j] = sab[k, j] + bin[k][i, j];
                        }
                        sab[k, j] = (sab[k, j]) / h;
                    }
                }

                //формування еталонного вектору
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < w; i++)
                    {
                        if (sab[k, i] >= 0.5)
                        {
                            evect[k, i] = 1;
                        }
                        else
                        {
                            evect[k, i] = 0;
                        }
                    }
                }
                d = 0;
                d1 = 2147483647;

                //формування масиву ПАРА
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i != k)
                        {
                            d = 0;
                            for (int j = 0; j < h; j++)
                            {
                                if (evect[k, j] != evect[i, j])
                                {
                                    d++;
                                }
                            }
                            if (d < d1)
                            {
                                d1 = d;
                                para[k] = i;
                            }
                        }
                    }
                }

                //формування кодових відстаней
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < h; i++)
                    {
                        sum1 = 0;
                        sum2 = 0;
                        for (int j = 0; j < w; j++)
                        {
                            if (evect[k, j] != bin[k][i, j]) sum1++;
                            if (evect[k, j] != bin[para[k]][i, j]) sum2++;
                        }
                        sk[k][0, i] = sum1;
                        sk[k][1, i] = sum2;
                    }
                }
                double maxx = 0;
                for (int k = 0; k < 3; k++)
                {
                    for (int i = 0; i < h; i++)
                    {
                        kk1 = 0;
                        kk2 = 0;
                        kk3 = 0;
                        kk4 = 0;
                        maxx = 0;
                        for (int j = 0; j < h; j++)
                        {
                            if (sk[k][0, j] <= i) kk1++; else kk2 = h - kk1;
                            if (sk[k][1, j] <= i) kk3++; else kk4 = h - kk3;
                            dd1 = (double)kk1 / h;
                            dd2 = (double)kk4 / h;
                            a = (double)kk2 / h;
                            b = (double)kk3 / h;
                        }
                        ee = Math.Log(((2 - (a + b)) / (a + b)), 2) * (1 - (a + b));
                        
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            maxE[k, te] = maxE[k, te] + ee;
                            if (ee > maxx)
                            {
                                rad[k] = i;
                                maxx = ee;
                            }
                            else
                            {
                                if (ee > max) { max = ee; rad[k] = i; }
                            }
                        }
                    }
                    maxE[k, te] = maxE[k, te] / h;
                }
                te++;
            }
            StreamWriter sw22 = new StreamWriter("writedMaxE.txt");
            for (int k = 0; k < 3; k++)
            {
                sw22.WriteLine("Клас " + (k + 1) + '\n');
                for (int i = 0; i < te; i++)
                {
                    sw22.Write(maxE[k, i]);
                    sw22.WriteLine("\n");
                }

            }
            sw22.Close();

            //знаходимо  E середнє
            double[] Esr = new double[h];
            double summ = 0;
            double max_Esr = 0;
            int opt_delta = 0;
            for (int i = 0; i < h; i++)
            {
                summ = 0;
                for (int klass = 0; klass < 3; klass++)
                {
                    summ += maxE[klass, i];
                }
                Esr[i] = summ / 3;
                if (Esr[i] > max_Esr && !Double.IsInfinity(Esr[i]))
                {
                    max_Esr = Esr[i];
                    opt_delta = i + 1;
                }
            }
            optimal_delta = opt_delta;
            MaxEsr.Text = "Максимальне значення Е(ср) = " + max_Esr + " Оптимальна delta = " + opt_delta;
           
            //записуємо E-sr до файлу
            StreamWriter swEsr = new StreamWriter("writedEsr.txt");
            swEsr.WriteLine("Е середнє" + '\n');
            for (int i = 0; i < h; i++)
            {
                swEsr.Write(Esr[i]);
                swEsr.WriteLine("\n");
            }
            swEsr.Close();

            //будуємо його графік
            double x7;
            double y7;
            for (int i = 0; i < delta; i++)
            {
                x7 = i;
                y7 = Esr[i];
                list7.Add(x7, y7);
            }

            LineItem myCurve7 = pane4.AddCurve(" ", list7, Color.Black, SymbolType.None);
            myCurve7.Line.IsSmooth = true; zedGraphControl4.AxisChange();
            zedGraphControl4.Invalidate();


            LineItem myCurve = pane1.AddCurve(" ", list1, Color.Black, SymbolType.None);
            myCurve.Line.IsSmooth = true;
            LineItem myCurve3 = pane1.AddCurve(" ", list3, Color.Coral, SymbolType.None);
            myCurve3.Line.IsSmooth = true;
            myCurve3.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            LineItem myCurve1 = pane2.AddCurve(" ", list2, Color.Black, SymbolType.None);
            myCurve1.Line.IsSmooth = true;
            LineItem myCurve4 = pane2.AddCurve(" ", list4, Color.Coral, SymbolType.None);
            myCurve4.Line.IsSmooth = true;
            myCurve4.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();
            LineItem myCurve5 = pane3.AddCurve(" ", list5, Color.Black, SymbolType.None);
            myCurve4.Line.IsSmooth = true;
            myCurve4.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            LineItem myCurve6 = pane3.AddCurve(" ", list6, Color.Coral, SymbolType.None);
            myCurve4.Line.IsSmooth = true;
            myCurve4.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            zedGraphControl3.AxisChange();
            zedGraphControl3.Invalidate();
            sw15.Close();
            sw16.Close();
            sw17.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {

            String[] path = new string[4];
            Bitmap[] bmp = new Bitmap[4];

            path[0] = textBox1.Text;
            path[1] = textBox2.Text;
            path[2] = textBox3.Text;
            path[3] = textBox4.Text;


            for (int i = 0; i < 4; i++)
            {
                bmp[i] = new Bitmap(Image.FromFile(path[i]));
            }
            int w = bmp[0].Width;
            int h = bmp[0].Height;
            byte[][,] r = new byte[4][,];
            Color[] c = new Color[4];
            for (int k = 0; k < 4; k++)
                r[k] = new byte[h, w];
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < w; j++)
                    {
                        c[k] = bmp[k].GetPixel(i, j);
                        r[k][i, j] = (byte)(0.241 * c[k].R + 0.619 * c[k].G + 0.068 * c[k].B);
                    }
                }
            }

            double[,] sa = new double[4, w];
            for (int k = 0; k < 4; k++)
            {
                for (int j = 0; j < h; j++)
                {
                    sa[k, j] = 0;
                    for (int i = 0; i < w; i++)
                    {
                        sa[k, j] = sa[k, j] + r[k][i, j];
                    }
                    sa[k, j] = sa[k, j] / h;
                }
            }
            double[,] ni = new double[4, w];
            double[,] vi = new double[4, w];
            StreamWriter sw20 = new StreamWriter("writedMatNiOpt.txt");
            StreamWriter sw21 = new StreamWriter("writedMatViOpt.txt");

            //формування бінарної матриці
            double delta = optimal_delta;
            byte[][,] bin = new byte[4][,];
            for (int k = 0; k < 4; k++)
                bin[k] = new byte[h, w];
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    vi[k, i] = sa[k, i] + delta;
                    ni[k, i] = sa[k, i] - delta;
                    for (int j = 0; j < w; j++)
                    {

                        if (r[k][i, j] > sa[k, i] - delta && r[k][i, j] < sa[k, i] + delta)
                        {
                            bin[k][i, j] = 1;
                        }
                        else
                        {
                            bin[k][i, j] = 0;
                        }
                    }
                }
            }
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < w; i++)
                {
                    sw20.Write(ni[k, i]);
                    sw21.Write(vi[k, i]);
                    sw20.WriteLine("\n");
                    sw21.WriteLine("\n");
                }
                sw20.WriteLine("--------------------------------------");
                sw21.WriteLine("\n");
            }
            sw20.Close();
            sw21.Close();

            //розрахунок середнього арифметичного по стовпцях бинарної матриці
            double[,] sab = new double[4, w];
            for (int k = 0; k < 4; k++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int i = 0; i < w; i++)
                    {
                        sab[k, j] = sab[k, j] + bin[k][i, j];
                    }
                    sab[k, j] = (sab[k, j]) / h;
                }
            }
            //формування еталонного вектору
            int[,] evect = new int[4, w];
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < w; i++)
                {

                    if (sab[k, i] >= 0.5)
                    {
                        evect[k, i] = 1;
                    }
                    else
                    {
                        evect[k, i] = 0;
                    }
                }
            }
            //запис у файл бінорної матриці та еталонного вектору
            StreamWriter sw3 = new StreamWriter("writedMatBinary1Opt.txt");
            StreamWriter sw5 = new StreamWriter("writedVector1Opt.txt");
            for (int k = 0; k < 3; k++)
            {
                sw3.WriteLine("Класс №" + k);
                for (int i = 0; i < h; ++i)
                {
                    for (int j = 0; j < w; ++j)
                    {
                        sw3.Write(bin[k][i, j]);
                        sw3.Write(" ");
                    }
                    sw3.WriteLine("\n");
                }
            }
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < w; i++)
                {
                    sw5.Write(evect[k, i]);
                    sw5.Write(" ");
                }
                sw5.WriteLine("\n");
            }
            sw3.Close();
            sw5.Close();
            int d, d1;
            int[] para = new int[3];
            int[][,] sk = new int[3][,];
            for (int k = 0; k < 3; k++)
                sk[k] = new int[2, h];
            d = 0;
            d1 = 2147483647;
            //формування масиву ПАРА
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (i != k)
                    {
                        d = 0;
                        for (int j = 0; j < h; j++)
                        {
                            if (evect[k, j] != evect[i, j])
                            {
                                d++;
                            }
                        }
                        if (d < d1)
                        {
                            d1 = d;
                            para[k] = i;
                        }
                    }
                }
            }
            //формування кодових відстаней
            int sum1;
            int sum2;
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    sum1 = 0;
                    sum2 = 0;
                    for (int j = 0; j < w; j++)
                    {
                        if (evect[k, j] != bin[k][i, j]) sum1++;
                        if (evect[k, j] != bin[para[k]][i, j]) sum2++;
                    }
                    sk[k][0, i] = sum1;
                    sk[k][1, i] = sum2;
                }
            }
            //запис до файлу масиву кодових відстаней
            StreamWriter sw7 = new StreamWriter("writedSk1Opt.txt");
            for (int k = 0; k < 3; k++)
            {
                sw7.WriteLine("Класс №" + k);
                for (int i = 0; i < 2; ++i)
                {
                    for (int j = 0; j < h; ++j)
                    {
                        sw7.Write(sk[k][i, j]);
                        sw7.Write(" ");
                    }
                    sw7.WriteLine("\n");
                }
            }
            sw7.Close();
            int kk1, kk2, kk3, kk4;
            double dd1 = 0, dd2 = 0, a = 0, b = 0, ee = 0;
            StreamWriter sw15 = new StreamWriter("writedEeOpt.txt");
            StreamWriter sw16 = new StreamWriter("writedD1Opt.txt");
            StreamWriter sw17 = new StreamWriter("writedD2Opt.txt");
            GraphPane pane5 = zedGraphControl5.GraphPane;
            pane5.XAxis.Title.Text = "r";
            pane5.YAxis.Title.Text = "E";
            PointPairList list1 = new PointPairList();
            pane5.Title.Text = "Клас 1 з оптимальною delta";
            GraphPane pane6 = zedGraphControl6.GraphPane;
            pane6.XAxis.Title.Text = "r";
            pane6.YAxis.Title.Text = "E";
            PointPairList list2 = new PointPairList();
            pane6.Title.Text = "Клас 2 з оптимальною delta";
            PointPairList list4 = new PointPairList();
            PointPairList list3 = new PointPairList();
            GraphPane pane7 = zedGraphControl7.GraphPane;
            pane7.XAxis.Title.Text = "r";
            pane7.YAxis.Title.Text = "E";
            PointPairList list5 = new PointPairList();
            pane7.Title.Text = "Клас 3 з оптимальною delta";
            PointPairList list6 = new PointPairList();

            pane5.XAxis.Scale.Max = 50;
            pane6.XAxis.Scale.Max = 50;
            pane7.XAxis.Scale.Max = 50;

            //розрахунок точнісних характеристик
            double max = 0;
            int[] rad = new int[3];
            for (int k = 0; k < 3; k++)
            {
                for (int i = 0; i < h; i++)
                {
                    kk1 = 0;
                    kk2 = 0;
                    kk3 = 0;
                    kk4 = 0;
                    for (int j = 0; j < h; j++)
                    {
                        if (sk[k][0, j] <= i) kk1++; else kk2 = h - kk1;
                        if (sk[k][1, j] <= i) kk3++; else kk4 = h - kk3;
                        dd1 = (double)kk1 / h;
                        dd2 = (double)kk4 / h;
                        a = (double)kk2 / h;
                        b = (double)kk3 / h;

                    }
                    ee = Math.Log(((2 - (a + b)) / (a + b)), 2) * (1 - (a + b));
                    max = 0;

                    //будуємо графік
                    if (k == 0)
                    {
                        double x1;
                        x1 = i;
                        double y1 = ee;
                        list1.Add(x1, y1);
                        double x3 = 0;
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                            x3 = i;
                            double y3 = ee;
                            list3.Add(x3, y3);
                        }
                        else if (rad[k] == 0)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                        }
                    }
                    max = 0;
                    if (k == 1)
                    {
                        double x2;
                        x2 = i;
                        double y2 = ee;
                        list2.Add(x2, y2);
                        double x4 = 0;
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                            x4 = i;
                            double y4 = ee;
                            list4.Add(x4, y4);
                        }
                        else if (rad[k] == 0)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                        }
                    }
                    max = 0;
                    if (k == 2)
                    {
                        double x3;
                        x3 = i;
                        double y3 = ee;
                        list5.Add(x3, y3);
                        double x5 = 0;
                        if (dd1 > 0.5 && dd2 > 0.5)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                            x5 = i;
                            double y5 = ee;
                            list6.Add(x5, y5);
                        }
                        else if (rad[k] == 0)
                        {
                            if (ee > max) { max = ee; rad[k] = i; }
                        }
                    }
                    sw15.Write(ee);
                    sw16.Write(dd1);
                    sw17.Write(dd2);
                    sw15.WriteLine("\n");
                    sw16.WriteLine("\n");
                    sw17.WriteLine("\n");
                }
                rad_for_opt_delt.Text = "Оптимальні радіуси: " + rad[0] + " " + rad[1] + " " + rad[2];
                sw15.Write("----------------------------------------------------------------\n");
                sw16.Write("----------------------------------------------------------------\n");
                sw17.Write("----------------------------------------------------------------\n");
            }


            //алгоритм екзамену
            double[] XP = new double[h];
            int[,] XPbin = new int[h, w];
            int[] DD = new int[3];
            double[] F = new double[3];
            int[] Ev_XP = new int[h];

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    XP[i] = r[3][i, j];
                }

            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                {
                    XPbin[i, j] = bin[3][i, j];
                }

            for (int i = 0; i < h; i++)
            {
                Ev_XP[i] = evect[3, i];
            }


            int counter = 0;
            for (int i = 0; i < h; i++)
            {
                if (Ev_XP[i] != evect[0, i])
                    counter++;
            }

            int counter2 = 0;
            for (int i = 0; i < h; i++)
            {
                if (Ev_XP[i] != evect[1, i])
                    counter2++;
            }

            int counter3 = 0;
            for (int i = 0; i < h; i++)
            {
                if (Ev_XP[i] != evect[2, i])
                    counter3++;
            }



            F[0] = 1.0 - Convert.ToDouble(counter) / Convert.ToDouble(rad[0]);
            F[1] = 1.0 - Convert.ToDouble(counter2) / Convert.ToDouble(rad[1]);
            F[2] = 1.0 - Convert.ToDouble(counter3) / Convert.ToDouble(rad[2]);


            label6.Text = "F: " + F[0] + " " + F[1] + " " + F[2];

            double max_f = F[0];
            int ind_f = 0;
            for (int k = 0; k < 3; k++)
            {
                if (F[k] > max_f)
                {
                    max_f = F[k];
                    ind_f = k;
                }
            }

            //записуємо F до файлу
            StreamWriter swF = new StreamWriter("writedF.txt");
            swF.WriteLine("F для класів" + '\n');
            for (int i = 0; i < 3; i++)
            {
                swF.Write(F[i]);
                swF.WriteLine("\n");
            }
            swF.Close();

            if (max_f >= 0)
                label7.Text = "Вибране зображення належить класу " + (ind_f + 1);
            else
                label7.Text = "Не вийшло розпізнати реалізацію";

            LineItem myCurve = pane5.AddCurve(" ", list1, Color.Black, SymbolType.None);
            myCurve.Line.IsSmooth = true;
            LineItem myCurve3 = pane5.AddCurve(" ", list3, Color.Coral, SymbolType.None);
            myCurve3.Line.IsSmooth = true;
            myCurve3.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            zedGraphControl5.AxisChange();
            zedGraphControl5.Invalidate();
            LineItem myCurve1 = pane6.AddCurve(" ", list2, Color.Black, SymbolType.None);
            myCurve1.Line.IsSmooth = true;
            LineItem myCurve4 = pane6.AddCurve(" ", list4, Color.Coral, SymbolType.None);
            myCurve4.Line.IsSmooth = true;
            myCurve4.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            zedGraphControl6.AxisChange();
            zedGraphControl6.Invalidate();
            LineItem myCurve5 = pane7.AddCurve(" ", list5, Color.Black, SymbolType.None);
            myCurve4.Line.IsSmooth = true;
            myCurve4.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            LineItem myCurve6 = pane7.AddCurve(" ", list6, Color.White, SymbolType.None);
            myCurve4.Line.IsSmooth = true;
            myCurve4.Line.Fill = new ZedGraph.Fill(Color.Blue, Color.Yellow, 90.0f);
            zedGraphControl7.AxisChange();
            zedGraphControl7.Invalidate();
            sw15.Close();
            sw16.Close();
            sw17.Close();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Оберіть файл";
            openFileDialog1.Filter = "Зображення|*.jpg;*.png;*.bmp";
            openFileDialog1.ShowDialog();
            String pathimg = openFileDialog1.FileName;
            textBox4.Text = pathimg;
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void name_pic3_Click(object sender, EventArgs e)
        {

        }
    }
}
