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
            int j = 1;//часова координата
            double x = 0;//просторова координата
            double dt = Tau / 20;
            double t = 0;
            double l = 1.0;//????????????????
            Kol = Kol - 1;//з урахуванням що нумерація масиву починається з 0 
            double dx = l / Kol; 
            int n;
            
            PressureCalculation();//рахуємо тиск

            СoncentrationCalculation();//рахуємо концентрацію

            void PressureCalculation()
            {
                j = 1;
                ObjWorkSheet.Cells[j, 1] = "P";
                ObjWorkSheet.Cells[3, 1] = "Time" + (0);
                ObjWorkSheet.Cells[3, 2] = t;
                t = t + dt;
                for (n = 0; n <= Kol; n++)
                {
                    x = n * dx;
                    ObjWorkSheet.Cells[2, n + 3] = "X" + (n + 1);
                    ObjWorkSheet.Cells[3, n + 3] = x;
                    ObjWorkSheet.Cells[3, n + 3] = P[0, n] = Pnach;//виводимо початковий тиск в усіх узлах
                }

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

                while (t <= Tau + dt)
                {
                    ObjWorkSheet.Cells[j + 3, 1] = "Time" + j;
                    ObjWorkSheet.Cells[j + 3, 2] = t;
                    //MessageBox.Show(Convert.ToString(j));
                    for (y = 1; y <= Kol - 1; y++)
                    {
                        alpha[j, y] = 0.5 * (P[j - 1, y + 1] + P[j - 1, y]);
                        beta[j, y] = 0.5 * (P[j - 1, y] + P[j - 1, y - 1]);
                    }

                    for (n = 1; n <= Kol - 1; n++)//коефіцієнти внутрішніх вузлів
                    {
                        a[n] = -alpha[j, n] / (dx * dx); 
                        b[n] = alpha[j, n] / (dx * dx) + beta[j, n] / (dx * dx) + m / dt;
                        c[n] = -beta[j, n] / (dx * dx);
                        d[n] = -(m / dt) * P[j - 1, n];
                    }

                    for (n = 1; n <= Kol; n++)
                    {
                        f[n] = -a[n] / (b[n] + c[n] * f[n - 1]);
                        g[n] = -(d[n] + c[n] * g[n - 1]) / (b[n] + c[n] * f[n - 1]);
                    }

                    P[j, Kol] = -(d[Kol] + c[Kol] * g[Kol - 1]) / (b[Kol] + c[Kol] * f[Kol - 1]);

                    for (n = Kol - 1; n >= 0; n-- )//підрахунок тиску в данний момент часу в усіх вузлах
                    {
                        P[j, n] = P[j, n + 1] * f[n] + g[n];
                    }

                    for (n = 0; n <= Kol; n++)
                    {
                        ObjWorkSheet.Cells[j + 3, n + 3] = P[j, n];//запис значень до таблиці Exel
                    }
                    
                    t = t + dt;
                    j++;
                    ObjExcel.Visible = true;
                    ObjExcel.UserControl = true;
                }
                y = j;

                ObjWorkSheet.Cells[24, 6] = "y";
                ObjWorkSheet.Cells[24, 7] = y;//21
            }

            void СoncentrationCalculation()
            {
                t = 0;
                j = 1;
                y = y + 4;//25
                ObjWorkSheet.Cells[y, 1] = "C";
                ObjWorkSheet.Cells[y + 2, 1] = "Time" + (0);
                ObjWorkSheet.Cells[y + 2, 2] = t;
                t = t + dt;
                for (n = 0; n <= Kol; n++)
                {
                    x = n * dx;
                    ObjWorkSheet.Cells[y, n + 3] = "X" + (n + 1);
                    ObjWorkSheet.Cells[y + 1, n + 3] = x;
                    ObjWorkSheet.Cells[y + 2, n + 3] = Conc[0, n] = ConcNach;//виводимо початкову концентрацію в усіх узлах
                }

                //граничні умови
                {
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
                        b[Kol] = 1;
                        c[Kol] = -1;
                        d[Kol] = 0;
                    }
                    else if (radioButton4.Checked)
                    {
                        //першого роду зліва=0 другого роду зправа=1 
                        a[0] = 0;
                        b[0] = 1;
                        d[0] = 0;
                        b[Kol] = 1 / dx;
                        c[Kol] = -1 / dx;
                        d[Kol] = -1;
                    }
                    else if (radioButton5.Checked)
                    {
                        //першого роду зліва=1 другого роду зправа=1 
                        a[0] = 0;
                        b[0] = 1;
                        d[0] = -1;
                        b[Kol] = 1 / dx;
                        c[Kol] = -1 / dx;
                        d[Kol] = -1;
                    }
                }
                
                f[0] = -a[0] / b[0];
                g[0] = -d[0] / b[0];

                while (t <= Tau + dt)
                {
                    for (n = 0; n <= Kol; n++)
                    {
                        qv[j, n] = dx * (ConcIstochnik - Conc[j - 1, n]);
                       
                    }
                        int asd = j;
                    for (n = 1; n <= Kol - 1; n++)//рахуемо коуфицієнти
                    {
                        a[n] = -1 / (dx * dx);
                        b[n] = 2 / (dx * dx) + (P[j, n + 1] - P[j, n - 1]) / (2 * dx * dx) + 1 / dt;
                        c[n] = (-(P[j, n + 1] - P[j, n - 1])) / (2 * dx * dx) + 1 / (dx * dx);
                        d[n] = -Conc[j - 1, n] / dt - 0;// qv[j, n];
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

                    ObjWorkSheet.Cells[y + 3, n + 14] = dt;
                    ObjWorkSheet.Cells[y + 3, n + 15] = t;
                    ObjWorkSheet.Cells[y + 3, n + 16] = j;
                    ObjWorkSheet.Cells[y + 3, n + 17] = n;
                    ObjWorkSheet.Cells[y + 3, n + 18] = Kol;
                    ObjWorkSheet.Cells[y + 3, 1] = "Time" + (j);
                    ObjWorkSheet.Cells[y + 3, 2] = t;
                    for (n = 0; n <= Kol; n++)
                    {
                        x = n * dx;
                        ObjWorkSheet.Cells[y + 3, n + 3] = Conc[j, n];//вивід значень в таблицю Exel
                    }

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
