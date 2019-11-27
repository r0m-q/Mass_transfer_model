﻿using System;
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
        public double[,] Conc = new double[1000, 1000];
        public double[,] qv = new double[1000, 1000];
        public double[] a = new double[1000];
        public double[] b = new double[1000];
        public double[] c = new double[1000];
        public double[] d = new double[1000];
        public double[] f = new double[1000];
        public double[] g = new double[1000];

        public void Output_Click(object sender, EventArgs e)
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
            double ConcIstochnik;
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
                ConcIstochnik = Convert.ToDouble(textBoxConcIstochnik.Text);//Внутренний источник
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }

            int y = 0;
            double k = 1, mju = 1, m = 1;//проницаемость, вязкость, пористость
            double dt = 0.04;//шаг по времени
            int j = 1;
            double Diff = 1;
            double ConcNach = 0;
            double t = 0;
            double l = 1.0;//принимаем длинну за 1
            double h = l / (Kol-1);//считаем шаг сетки
            int n;
            Kol = Kol - 1;

            PressureCalculation();

            СoncentrationCalculation();

            void PressureCalculation()
            {
                double x = 0;
                j = 1;
                ObjWorkSheet.Cells[j, 1] = "P";
                for (n = 0; n <= Kol; n++)
                {
                    x = n * h;
                    ObjWorkSheet.Cells[2, n + 3] = "X" + (n + 1) + "   =   " + x;
                    P[0, n] = Pnach;
                    ObjWorkSheet.Cells[3, n + 3] = P[0, n];//выводим начальную температуру во всех узлах
                }
                ObjWorkSheet.Cells[3, 1] = "Time" + (0);//оформляем вывод
                ObjWorkSheet.Cells[3, 2] = t;
                int i = 0;
                t = t + dt;

                //граничые условия

                if (radioButton6.Checked)
                {
                    //первого рода слева=0 справа=0
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1;
                    c[Kol] = 0;
                    d[Kol] = 0;
                }
                else if (radioButton7.Checked)
                {
                    //первого рода слева=0 справа=1
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1;
                    c[Kol] = 0;
                    d[Kol] = -1;
                }
                else if (radioButton8.Checked)
                {
                    //первого рода слева=0 второго рода справа=0
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = 0;
                }
                else if (radioButton9.Checked)
                {
                    //первого рода слева=0 второго рода справа=1 
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = -1;
                }
                else if (radioButton10.Checked)
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

                double[,] alpha = new double[1000, 1000];
                double[,] beta = new double[1000, 1000];

                while (t <= Tau)
                {
                    //оформляем вывод
                    ObjWorkSheet.Cells[j + 3, 1] = "Time" + j;
                    ObjWorkSheet.Cells[j + 3, 2] = t;
                    for (y = 1; y <= Kol - 1; y++) //считаем альфа и бета
                    {
                        alpha[j, y] = 0.5 * (P[j-1, y+1] + P[j-1, y]);
                        beta[j, y] = 0.5 * (P[j-1, y] + P[j-1, y-1]);
                    }

                    for (n = 1; n <= Kol - 1; n++)//считаем коэфициенты внутренних узлов
                    {
                        a[n] = -(k * alpha[j, n]) / (mju * h * h); 
                        b[n] = (k * alpha[j, n]) / (mju * h * h) + (k * beta[j, n]) / (mju * h * h) + m / dt;
                        c[n] = -(k * beta[j, n]) / (mju * h * h);
                        d[n] = -(m / dt) * P[j - 1, n];
                    }

                    for (n = 1; n <= Kol; n++)
                    {
                        f[n] = -a[n] / (b[n] + c[n] * f[n - 1]);
                        g[n] = -(d[n] + c[n] * g[n - 1]) / (b[n] + c[n] * f[n - 1]);
                    }

                    P[j, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);

                    for (n = Kol - 1; n >= 0; n = n - 1)//подсчёт температуры в текущий момент времени i во всех узлах
                    {
                        P[j, n] = P[j, n + 1] * f[n] + g[n];
                    }

                    for (n = 0; n <= Kol; n++)
                    {
                        ObjWorkSheet.Cells[j + 3, i + 3] = P[j, n];//вывод значений в таблицу exel в текуший момент времени і во всех узлах
                        i++;
                    }

                    i = 0;
                    t = t + dt;
                    j++;
                    //Вызываем эксель.
                    ObjExcel.Visible = true;
                    ObjExcel.UserControl = true;
                }
                y = j;
            }

            void СoncentrationCalculation()
            {
                t = 0;
                j = 1;
                double x = 0;
                y = y + 4;
                ObjWorkSheet.Cells[y, 1] = "C";
                for (n = 0; n <= Kol; n++)
                {
                    x = n * h;
                    ObjWorkSheet.Cells[y + 1, n + 3] = "X" + (n + 1) + "  =  " + x;
                    Conc[y, n] = ConcNach;
                    ObjWorkSheet.Cells[y + 2, n + 3] = Conc[y , n];//выводим начальную температуру во всех узлах
                }
                ObjWorkSheet.Cells[y + 2, 1] = "Time" + (0);//оформляем вывод
                ObjWorkSheet.Cells[y + 2, 2] = t;
                int i = 0;
                t = t + dt;

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
                else if (radioButton5.Checked)
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

                while (t <= Tau)
                {
                    ObjWorkSheet.Cells[y + 3, 1] = "Time" + (j);
                    ObjWorkSheet.Cells[y + 3, 2] = t;

                    //считаем внутренний источник
                    for (n = 0; n <= Kol; n++)
                    {
                        qv[j, n] = h * (ConcIstochnik - Conc[j, n]);
                    }
                    for (n = 1; n <= Kol - 1; n++)//считаем коэфициенты внутренних узлов
                    {
                        a[n] = -Diff / (h * h);
                        b[n] = 2 * Diff / (h * h) - (k * (P[j, n + 1] - P[j, n - 1])) / (2 * mju * h * h) + 1 / dt;
                        c[n] = (k * (P[j, n + 1] - P[j, n - 1])) / (2 * mju * h * h) - Diff / (h * h);
                        d[n] = -(Conc[j - 1, n]) / dt - qv[j, n];
                    }

                    for (n = 1; n <= Kol; n++)
                    {
                        f[n] = -a[n] / (b[n] + c[n] * f[n - 1]);
                        g[n] = -(d[n] + c[n] * g[n - 1]) / (b[n] + c[n] * f[n - 1]);
                    }
                    Conc[j, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);

                    for (n = Kol - 1; n >= 0; n--)//подсчёт температуры в текущий момент времени i во всех узлах
                    {
                        Conc[j, n] = Conc[j, n + 1] * f[n] + g[n];
                    }

                    for (n = 0; n <= Kol; n++)
                    {
                        x = i * h;
                        ObjWorkSheet.Cells[y + 3, i + 3] = Conc[j, n];//вывод значений в таблицу exel в текуший момент времени і во всех узлах
                        i++;
                    }

                    i = 0;
                    t = t + dt;
                    y++;
                    j++;
                    //Вызываем эксель.
                    ObjExcel.Visible = true;
                    ObjExcel.UserControl = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
    
}
