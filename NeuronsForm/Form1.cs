using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NeuronsForm
{
    public partial class Form1 : Form
    {
        // Параметры задачи
        private int n;
        private double T;           // верхняя граница отрезка интегрирования
        private double B;           // ограничения для весовых коэффициентов
        private double M1;          // штрафной коэффициент на точность весовых коэффициентов
        private double M2;          // штрафной коэффициент на точность решения
        private double[] a;         // начальные значения
        private double[] A;         // конечные значения
        private double[] gamma;     // коэффициенты затухания

        // параметры метода
        private int q = 200;             // количество слоев, q = [10 ... 10 000]
        private double dt;              // мелкость разбиения, dt = T/q
        private double alpha = 1;       // шаг градиентного спуска, alpha = [10^-7 ... 10^3]
        private double epsilon = 1E-6;   // точность вычислений, epsilon = [10^-10 ... 10^-2]
        private double[][] p;           // множители Лагранжа

        private double[/*номер слоя*/][/*номер нейрона*/] x0;                                       // характеристики нейронов
        private double[/*номер слоя*/][/*номер нейрона*/] x1;

        private double[/*номер слоя*/][/*номер нейрона слоя k*/,/*номер нейрона слоя k+1*/] w0;		// синаптические веса нейронов
        private double[/*номер слоя*/][/*номер нейрона слоя k*/,/*номер нейрона слоя k+1*/] w1;

        private double I0;
        private double I1;

        public Form1()
        {
            StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        // Считывание данных формы + элементарная прверка на корректность введенных данных
        // при преобразовании типов
        private void SetDefaultTextBoxValue(ArgumentException e)
        {
            Object err_element = this.Controls.Find(e.Source, true).First();
            (err_element as TextBox).Text = "1";
        }
        private void MakeOthersReadOnly(ArgumentException e)
        {
            TextBox[] tbs = {
                textBoxN,
                textBoxB,
                textBoxT,
                textBoxM1,
                textBoxM2
            };

            foreach (var tb in tbs)
            {
                if (tb.Name != e.Source)
                {
                    tb.ReadOnly = true;
                }
            }

            if (e.Source != dataGridView1.Name)
            {
                dataGridView1.ReadOnly = true;
            }
        }
        private void MakeAllEditable()
        {
            TextBox[] tbs = {
                textBoxN,
                textBoxB,
                textBoxT,
                textBoxM1,
                textBoxM2
            };

            foreach (var tb in tbs)
            {
                tb.ReadOnly = false;
            }
            dataGridView1.ReadOnly = false;
        }


        private double CheckNumericValue(TextBox element)
        {
            double result;
            try
            {
                if (!double.TryParse(element.Text, out result))
                {
                    ArgumentException e = new ArgumentException();
                    e.Source = element.Name;
                    throw e;
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            return result;
        }

        private void textBoxN_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxN.Name;

            if (textBoxN.Text == "" || textBoxN.Text == "-" || textBoxN.Text == "+")
                return;

            int rows;

            try
            {
                // n - целое число
                if (!Int32.TryParse(CheckNumericValue(textBoxN).ToString(), out n))
                {
                    throw exc;
                }

                // n - положительное число
                if (n <= 0)
                {
                    throw exc;
                }
            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }


            MakeAllEditable();

            rows = dataGridView1.Rows.Count;
            if (rows < n)
            {
                dataGridView1.Rows.Add(n - rows);
                foreach (DataGridViewRow r in dataGridView1.Rows)
                {
                    dataGridView1["Column1", r.Index].Value = r.Index + 1;
                }
            }
            else
            {
                while (rows > n)
                {
                    dataGridView1.Rows.RemoveAt(rows - 1);
                    rows--;
                }
            }
        }
        private void textBoxT_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxT.Name;

            if (textBoxT.Text == "" || textBoxT.Text == "-" || textBoxT.Text == "+")
                return;

            try
            {
                // T - число
                T = CheckNumericValue(textBoxT);

                // T - положительное число
                if (T <= 0)
                {
                    throw exc;
                }
            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }

            MakeAllEditable();
        }
        private void textBoxB_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxB.Name;

            if (textBoxB.Text == "" || textBoxB.Text == "-" || textBoxB.Text == "+")
                return;

            try
            {
                // B - число
                B = CheckNumericValue(textBoxB);

                // B - положительное число
                if (B <= 0)
                {
                    throw exc;
                }
            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }

            MakeAllEditable();
        }
        private void textBoxM1_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxM1.Name;

            if (textBoxM1.Text == "" || textBoxM1.Text == "-" || textBoxM1.Text == "+")
                return;

            try
            {
                // M1 - число
                M1 = CheckNumericValue(textBoxM1);

            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }

            MakeAllEditable();
        }
        private void textBoxM2_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxM2.Name;

            if (textBoxM2.Text == "" || textBoxM2.Text == "-" || textBoxM2.Text == "+")
                return;

            try
            {
                // M2 - число
                M2 = CheckNumericValue(textBoxM2);

            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }

            MakeAllEditable();
        }

        private void textBoxQ_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxQ.Name;

            if (textBoxQ.Text == "" || textBoxQ.Text == "-" || textBoxQ.Text == "+")
                return;

            try
            {
                // q - целое число
                if (!Int32.TryParse(CheckNumericValue(textBoxQ).ToString(), out q))
                {
                    throw exc;
                }

                // q - положительное число
                if (q <= 0)
                {
                    throw exc;
                }
            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }


            MakeAllEditable();
        }
        private void textBoxAlpha_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxAlpha.Name;

            if (textBoxAlpha.Text == "" || textBoxAlpha.Text == "-" || textBoxAlpha.Text == "+")
                return;

            try
            {
                // alpha - число
                alpha = CheckNumericValue(textBoxAlpha);

                // alpha - положительное число
                if (alpha <= 0)
                {
                    throw exc;
                }
            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }

            MakeAllEditable();
        }
        private void textBoxEpsilon_TextChanged(object sender, EventArgs e)
        {
            ArgumentException exc = new ArgumentException();
            exc.Source = textBoxEpsilon.Name;

            if (textBoxEpsilon.Text == "" || textBoxEpsilon.Text == "-" || textBoxEpsilon.Text == "+")
                return;

            try
            {
                // epsilon - число
                epsilon = CheckNumericValue(textBoxEpsilon);

                // alpha - положительное число
                if (epsilon <= 0)
                {
                    throw exc;
                }
            }
            catch (ArgumentException exception)
            {
                MakeOthersReadOnly(exception);
                return;
            }

            MakeAllEditable();
        }


        // Создание
        private void CreateW()
        {
            w0 = new double[q][,];
            w1 = new double[q][,];

            for (int k = 0; k < q; k++)
            {
                w0[k] = new double[n, n];
                w1[k] = new double[n, n];
            }
        }
        private void CreateX()
        {
            x0 = new double[q + 1][];
            x1 = new double[q + 1][];

            for (int k = 0; k < q + 1; k++)
            {
                x0[k] = new double[n];
                x1[k] = new double[n];
            }
        }
        private void CreateP()
        {
            p = new double[q + 1][];
            p[q] = new double[n];
            for (int k = q; k >= 0; k--)
                p[k] = new double[n];
        }

        // Инициализация
        private void SetInitialX()
        {
            double S;
            x0[0] = a;

            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    S = 0;
                    for (int j = 0; j < n; j++)
                        S += w0[k][i, j] * x0[k][j];

                    x0[k + 1][i] = x0[k][i] + dt * (-gamma[i] * x0[k][i] + S);
                }
            }
        }
        private void SetInitialW()
        {
            Random rnd = new Random();
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j)
                        {
                            w0[k][i, j] = 0;
                            continue;
                        }
                        w0[k][i, j] = rnd.NextDouble() + rnd.Next((int)B) - rnd.Next((int)B);
                    }
                }
            }
        }
        private void SetInitialI()
        {
            I0 = 0;
            double S;

            S = 0;
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        S += Math.Pow(w0[k][i, j], 2);
                    }
                }
            }

            I0 += M1 * dt * S;

            S = 0;
            for (int i = 0; i < n; i++)
            {
                S += (x0[q][i] - A[i]) * (x0[q][i] - A[i]);
            }

            I0 += M2 * S;
        }

        // Расчет
        private void CalculateP()
        {
            double S;

            for (int i = 0; i < n; i++)
            {
                p[q][i] = -2 * M2 * (x0[q][i] - A[i]);
            }

            for (int k = q - 1; k > 0; k--)
            {
                for (int i = 0; i < n; i++)
                {
                    S = 0;
                    for (int j = 0; j < n; j++)
                    {
                        S += p[k + 1][j] * w0[k][j, i];
                    }
                    p[k][i] = p[k + 1][i] * (1 - dt * gamma[i]) + dt * S;
                }
            }
        }
        private void CalculateX()
        {
            double S;

            x1[0] = a;
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    S = 0;
                    for (int j = 0; j < n; j++)
                    {
                        S += w1[k][i, j] * x1[k][j];
                    }
                    x1[k+1][i] = x1[k][i] + dt * (-gamma[i] * x1[k][i] + S);
                }
            }
        }
        private void CalculateW()
        {
            double d;

            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i == j)
                        {
                            w1[k][i, j] = 0;
                            continue;
                        }
                        d = dt * (2 * w0[k][i, j] * M1 - p[k + 1][i] * x0[k][j]);
                        w1[k][i, j] = w0[k][i, j] - alpha * d;

                        if (w1[k][i, j] > B)
                        {
                            w1[k][i, j] = B;
                        }
                        if (w1[k][i, j] < -B)
                        {
                            w1[k][i, j] = -B;
                        }
                    }
                }
            }
        }
        private void CalculateI()
        {
            I1 = 0;
            double S;

            S = 0;
            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        S += Math.Pow(w1[k][i, j], 2);
                    }
                }
            }

            I1 += M1 * dt * S;

            S = 0;
            for (int i = 0; i < n; i++)
            {
                S += (x1[q][i] - A[i]) * (x1[q][i] - A[i]);
            }

            I1 += M2 * S;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            GetDataFromDataGrid();

            dt = T / q;

            CreateX();
            CreateW();
            CreateP();

            step1: SetInitialW();
            step2: SetInitialX();
            step3: SetInitialI();

            while (true)
            {
                step4: CalculateP();
                step5: CalculateW();
                step6: CalculateX();
                step7: CalculateI();

                if (I0 < I1)
                {
                    if (alpha == 0)
                    {
                        MessageBox.Show("alpha = 0");
                        break;
                    }
                    alpha = alpha / 2;
                    goto step5;
                }
                if (Math.Abs(I0 - I1) < epsilon)
                {
                    break;
                }
                if (Math.Abs(I0 - I1) >= epsilon)
                {
                    I0 = I1;
                    x0 = x1;
                    w0 = w1;
                    goto step4;
                }
            }

            MessageBox.Show("Finished");
            RefreshTab3();
        }

        private void RefreshTab3()
        {
            int rows = dataGridView2.Rows.Count;
            while (rows > 0)
            {
                dataGridView2.Rows.RemoveAt(0);
                rows--;
            }

            int cols = dataGridView2.Columns.Count;
            while (cols > 0)
            {
                dataGridView2.Columns.RemoveAt(0);
                cols--;
            }

            dataGridView2.Columns.Add("Column5", "Step");

            for (int i = 0; i < n; i++)
            {
                dataGridView2.Columns.Add(String.Format("Column{0}", 6 + i), String.Format("X{0}", i + 1));
            }

            rows = dataGridView2.Rows.Count;
            if (rows < q)
            {
                dataGridView2.Rows.Add(q);
                for (int k = 0; k < q; k++)
                {
                    dataGridView2["Column5", k].Value = k + 1;
                    for (int i = 0; i < n; i++)
                        dataGridView2[i + 1, k].Value = x1[k][i];
                }
            }
        }

        private void GetDataFromDataGrid()
        {
            if (!dataGridView1.ReadOnly)
            {
                // get a
                a = new double[n];
                for (int i = 0; i < n; i++)
                {
                    a[i] = double.Parse(dataGridView1[1, i].Value.ToString());
                }

                // get A
                A = new double[n];
                for (int i = 0; i < n; i++)
                {
                    A[i] = double.Parse(dataGridView1[2, i].Value.ToString());
                }

                // get y
                gamma = new double[n];
                for (int i = 0; i < n; i++)
                {
                    gamma[i] = double.Parse(dataGridView1[3, i].Value.ToString());
                }
            }
        } 
        private void GetDataFrom()

        {

        }


    }
}
