using System;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

namespace Mass_transfer_model
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public double[,] P = new double[1000, 1000];
        public double[] a = new double[1000];
        public double[] b = new double[1000];
        public double[] c = new double[1000];
        public double[] d = new double[1000];
        public double[] f = new double[1000];
        public double[] g = new double[1000];

        private void Output_Click(object sender, EventArgs e)
        {
            Microsoft.Office.Interop.Excel.Application ObjExcel = new Microsoft.Office.Interop.Excel.Application();
            Workbook ObjWorkBook;
            Worksheet ObjWorkSheet;
            //Книга.
            ObjWorkBook = ObjExcel.Workbooks.Add(System.Reflection.Missing.Value);
            //Таблица.
            ObjWorkSheet = (Worksheet)ObjWorkBook.Sheets[1];
            //ObjWorkSheet = ObjWorkBook.ActiveSheet as Microsoft.Office.Interop.Excel.Worksheet;

            int Kol;
            double Tau;
            double qv;
            double Pnach;

            try
            {
                Tau = Convert.ToDouble(TextBoxTau.Text);//время
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                Kol = Convert.ToInt32(TextBoxKol.Text);//кол-во узлов
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                Pnach = Convert.ToDouble(TextBoxPnach.Text);//начальная температура
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                qv = Convert.ToDouble(textBoxQv.Text);//Внутренний источник
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            double k = 1, alpha = 1, beta = 1, mju = 1, m = 1;
            double dt = 0.04;//шаг по времени
            double lam = 1;
            double C = 1;
            double ro = 1;
            double tau = 0;
            double l = 1.0;//принимаем длинну за 1
            double h = l / (Kol-1);//считаем шаг сетки
            int n;
            Kol = Kol - 1;
            for ( n = 0; n <= Kol; n++)
            {
                P[0, n] = Pnach;
                ObjWorkSheet.Cells[3, n + 3] = P[0, n];//выводим начальную температуру во всех узлах
            }
            ObjWorkSheet.Cells[3, 1] = "Time" + (0);//оформляем вывод
            ObjWorkSheet.Cells[3, 2] = tau;
            int j = 1;
            double x = 0;
            int i = 0;
            tau = tau + dt;
            while (tau <= Tau)
            {
                //for (n = 1; n <= Kol - 1; n++)//считаем коэфициенты внутренних узлов
                //{
                //    a[n] = (dt * lam) / (C * ro * h * h);
                //    c[n] = (dt * lam) / (C * ro * h * h);
                //    b[n] = -1 - 2 * (dt * lam) / (C * ro * h * h);
                //    d[n] = P[j - 1, n] + dt / (C * ro) * qv;
                //}

                for (n = 1; n <= Kol - 1; n++)//считаем коэфициенты внутренних узлов
                {
                    a[n] = (k * alpha) / (mju * h * h);
                    b[n] = (k * alpha) / (mju * h * h) - (k * beta) / (mju * h * h) - m / dt;
                    c[n] = (k * beta) / (mju * h * h);
                    d[n] = (m / dt) * P[j - 1, n];
                }

                //граничые условия

                if (radioButton1.Checked)
                {
                    //первого рода слева=0 справа=0
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1;
                    c[Kol] = 0;
                    d[Kol] = 0;
                }
                else if (radioButton2.Checked)
                {
                    //первого рода слева=0 справа=1
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1;
                    c[Kol] = 0;
                    d[Kol] = -1;
                }
                //if (radioButton.Checked)
                //{
                //    //первого рода слева=1 справа=1
                //    a[0] = 0;
                //    b[0] = 1;
                //    d[0] = -1;
                //    b[Kol] = 1;
                //    c[Kol] = 0;
                //    d[Kol] = -1;
                //}
                else if (radioButton3.Checked)
                {
                    //первого рода слева=0 второго рода справа=0
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = 0;
                }
                else if (radioButton4.Checked)
                {
                    //первого рода слева=0 второго рода справа=1 
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = -1;
                }
                else
                {
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = -1;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = -1;
                }

                f[0] = -a[0] / b[0];
                g[0] = -d[0] / b[0];

                for ( n = 1; n <= Kol; n++)
                {
                    f[n] = -a[n] / (b[n] + c[n] * f[n - 1]);
                    g[n] = -(d[n] + c[n] * g[n - 1]) / (b[n] + c[n] * f[n - 1]);
                }
                P[j, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);
                for ( n = Kol - 1; n >= 0; n = n - 1)//подсчёт температуры в текущий момент времени i во всех узлах
                {
                    P[j, n] = P[j, n + 1] * f[n] + g[n];
                }
                for ( n = 0; n <= Kol; n++)
                {
                    x = i * h;
                    ObjWorkSheet.Cells[1, i + 3] = "X" + (i + 1);
                    ObjWorkSheet.Cells[2, i + 3] = x;
                    ObjWorkSheet.Cells[j + 3, i + 3] = P[j, n];//вывод значений в таблицу exel в текуший момент времени і во всех узлах
                    i++;

                    //if (x > 1) { break; }
                }
                i = 0;
                ObjWorkSheet.Cells[j + 3, 1] = "Time" + j;
                ObjWorkSheet.Cells[j + 3, 2] = tau;
                tau = tau + dt;
                j++;
                //Вызываем эксель.
                ObjExcel.Visible = true;
                ObjExcel.UserControl = true;
            }
        }
    }
    
}
