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
            double ConcNach;
            double Pnach = 1;
            double m;
            double Pleft = 1;
            double Pright;
            double ConcIstochnik;

            try
            {
                Tau = Convert.ToDouble(TextBoxTau.Text);//час
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                Kol = Convert.ToInt32(TextBoxKol.Text);//кількість вузлів сітки
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                ConcNach = Convert.ToDouble(TextBoxConcNach.Text);//почпткова концентрація
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                m = Convert.ToDouble(textBoxM.Text);//пористість середовища
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                ConcIstochnik = Convert.ToDouble(textBoxqv.Text); //конецентрація речовини в пористому середовищі
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            try
            {
                Pright = Convert.ToDouble(textBoxP.Text)+1; //тиск на видавальній скважині
            }
            catch (Exception ex)
            {
                MessageBox.Show("неверный формат ввода");
                MessageBox.Show($"Исключение:{ex.Message}");
                return;
            }
            int y = 0;
            int j = 1;
            double dt = 0.04;
            double t = 0;
            double l = 1.0;
            double h = l / (Kol - 1); 
            int n;

            Kol = Kol - 1;

            PressureCalculation();//рахуємо тиск

            СoncentrationCalculation();//рахуємо концентрацію

            void PressureCalculation()
            {
                double x = 0;
                j = 1;
                ObjWorkSheet.Cells[j, 1] = "P";
                for (n = 0; n <= Kol; n++)
                {
                    x = n * h;
                    ObjWorkSheet.Cells[2, n + 3] = "X" + (n + 1);
                    ObjWorkSheet.Cells[3, n + 3] = x;
                    P[0, n] = Pnach;
                    ObjWorkSheet.Cells[3, n + 3] = P[0, n];//виводимо початковий тиск в усіх узлах
                }
                ObjWorkSheet.Cells[3, 1] = "Time" + (0);
                ObjWorkSheet.Cells[3, 2] = t;
                int i = 0;
                t = t + dt;

                //граничні умови
                
                a[0] = 0;
                b[0] = 1;
                d[0] = -Pleft;
                b[Kol] = 1;
                c[Kol] = 0;
                d[Kol] = -Pright;
                
                f[0] = -a[0] / b[0];
                g[0] = -d[0] / b[0];

                double[,] alpha = new double[1000, 1000];
                double[,] beta = new double[1000, 1000];

                while (t <= Tau)
                {
                    ObjWorkSheet.Cells[j + 3, 1] = "Time" + j;
                    ObjWorkSheet.Cells[j + 3, 2] = t;
                    for (y = 1; y <= Kol - 1; y++)
                    {
                        alpha[j, y] = 0.5 * (P[j-1, y+1] + P[j-1, y]);
                        beta[j, y] = 0.5 * (P[j-1, y] + P[j-1, y-1]);
                    }

                    for (n = 1; n <= Kol - 1; n++)//коефіцієнти внутрішніх вузлів
                    {
                        a[n] = -alpha[j, n] / (h * h); 
                        b[n] = alpha[j, n] / (h * h) + beta[j, n] / (h * h) + m / dt;
                        c[n] = -beta[j, n] / (h * h);
                        d[n] = -(m / dt) * P[j - 1, n];
                    }

                    for (n = 1; n <= Kol; n++)
                    {
                        f[n] = -a[n] / (b[n] + c[n] * f[n - 1]);
                        g[n] = -(d[n] + c[n] * g[n - 1]) / (b[n] + c[n] * f[n - 1]);
                    }

                    P[j, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);

                    for (n = Kol - 1; n >= 0; n = n - 1)//підрахунок тиску в данний момент часу в усіх вузлах
                    {
                        P[j, n] = P[j, n + 1] * f[n] + g[n];
                    }

                    for (n = 0; n <= Kol; n++)
                    {
                        ObjWorkSheet.Cells[j + 3, i + 3] = P[j, n];//вивід значень в таблицю Exel
                        i++;
                    }

                    i = 0;
                    t = t + dt;
                    j++;
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
                //MessageBox.Show(Convert.ToString(y));
                ObjWorkSheet.Cells[y, 1] = "C";
                for (n = 0; n <= Kol; n++)
                {
                    x = n * h;
                    ObjWorkSheet.Cells[y, n + 3] = "X" + (n + 1);
                    ObjWorkSheet.Cells[y + 1, n + 3] = x;
                    Conc[y-29, n] = ConcNach;
                    ObjWorkSheet.Cells[y + 2, n + 3] = Conc[y-29 , n];//виводимо початкову концентрацію в усіх узлах
                }
                ObjWorkSheet.Cells[y + 2, 1] = "Time" + (0);
                ObjWorkSheet.Cells[y + 2, 2] = t;
                int i = 0;
                t = t + dt;

                //граничні умови

                if (radioButton1.Checked)
                {
                    //першого роду зліва=0, зправа=0
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1;
                    c[Kol] = 0;
                    d[Kol] = 0;
                }
                else if (radioButton2.Checked)
                {
                    //першого роду зліва=0, зправа=1
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1;
                    c[Kol] = 0;
                    d[Kol] = -1;
                }
                else if (radioButton3.Checked)
                {
                    //першого роду зліва=0 другого роду зправа=0
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = 0;
                }
                else if (radioButton4.Checked)
                {
                    //першого роду зліва=0 другого роду зправа=1 
                    a[0] = 0;
                    b[0] = 1;
                    d[0] = 0;
                    b[Kol] = 1 / h;
                    c[Kol] = -1 / h;
                    d[Kol] = -1;
                }
                else if (radioButton5.Checked)
                {
                    //першого роду зліва=1 другого роду зправа=1 
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
                    for (n = 0; n <= Kol; n++)
                    {
                        qv[j, n] = h * (ConcIstochnik - Conc[j - 1, n]);
                    }
                    for (n = 1; n <= Kol - 1; n++)//рахуемо коуфицієнти
                    {
                        a[n] = -1 / (h * h);
                        b[n] = 2 / (h * h) - (P[j, n + 1] - P[j, n - 1]) / (2 * h * h) + 1 / dt;
                        c[n] = (P[j, n + 1] - P[j, n - 1]) / (2 * h * h) - 1 / (h * h);
                        d[n] = -Conc[j - 1, n] / dt - qv[j, n];
                    }

                    for (n = 1; n <= Kol; n++)
                    {
                        f[n] = -a[n] / (b[n] + c[n] * f[n - 1]);
                        g[n] = -(d[n] + c[n] * g[n - 1]) / (b[n] + c[n] * f[n - 1]);
                    }
                    Conc[j, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);

                    for (n = Kol - 1; n >= 0; n--)//підрахунок концентрації
                    {
                        Conc[j, n] = Conc[j, n + 1] * f[n] + g[n];
                    }

                    for (n = 0; n <= Kol; n++)
                    {
                        x = i * h;
                        ObjWorkSheet.Cells[y + 3, i + 3] = Conc[j, n];//вивід значень в таблицю Exel
                        i++;
                    }

                    i = 0;
                    t = t + dt;
                    y++;
                    j++;

                    ObjExcel.Visible = true;
                    ObjExcel.UserControl = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBoxdt_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
    
}
