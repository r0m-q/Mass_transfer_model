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
        public double[,] Temp = new double[1000, 1000];
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
            double Tau = Convert.ToDouble(TextBoxTau.Text);//время
            int Kol = Convert.ToInt32(TextBoxKol.Text);//кол-во узлов
            double Templ = Convert.ToDouble(TextBoxTempl.Text);//начальная температура
            double qv = Convert.ToDouble(textBoxQv.Text);//Внутренний источник

            double dt = 0.04;//шаг по времени
            double lam = 1;
            double C = 1;
            double ro = 1;
            double tau = 0;
            double l = 1.0;//принимаем длинну за 1
            double h = l / (Kol-1);//считаем шаг сетки


            Kol = Kol - 1;
            for (int k = 0; k <= Kol; k++)
            {
                Temp[0, k] = Templ;
                ObjWorkSheet.Cells[3, k + 3] = Temp[0, k];//выводим начальную температуру во всех узлах
            }
            ObjWorkSheet.Cells[3, 1] = "Time" + (0);//оформляем вывод
            ObjWorkSheet.Cells[3, 2] = tau;
            int i = 1;
            double x = 0;
            int j = 0;
            tau = tau + dt;
            while (tau <= Tau)
            {
                for (int k = 1; k <= Kol - 1; k++)//считаем коэфициенты для внутренних узлов
                {
                    a[k] = (dt * lam) / (C * ro * h * h);
                    c[k] = (dt * lam) / (C * ro * h * h);
                    b[k] = -1 - 2 * (dt * lam) / (C * ro * h * h);
                    d[k] = Temp[i - 1, k] + dt / (C * ro) * qv;
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

                for (int k = 1; k <= Kol; k++)
                {
                    f[k] = -a[k] / (b[k] + c[k] * f[k - 1]);
                    g[k] = -(d[k] + c[k] * g[k - 1]) / (b[k] + c[k] * f[k - 1]);
                }
                Temp[i, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);
                for (int k = Kol - 1; k >= 0; k = k - 1)//подсчёт температуры в текущий момент времени i во всех узлах
                {
                    Temp[i, k] = Temp[i, k + 1] * f[k] + g[k];
                }
                for (int k = 0; k <= Kol; k++)
                {
                    x = j * h;
                    ObjWorkSheet.Cells[1, j + 3] = "X" + (j + 1);
                    ObjWorkSheet.Cells[2, j + 3] = x;
                    ObjWorkSheet.Cells[i + 3, j + 3] = Temp[i, k];//вывод значений в таблицу exel в текуший момент времени і во всех узлах
                    j++;

                    //if (x > 1) { break; }
                }
                j = 0;
                ObjWorkSheet.Cells[i + 3, 1] = "Time" + i;
                ObjWorkSheet.Cells[i + 3, 2] = tau;
                tau = tau + dt;
                i++;
                //Вызываем эксель.
                ObjExcel.Visible = true;
                ObjExcel.UserControl = true;
            }
        }
    }
    
}
