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
        private float T;
        private float B;
        private float M1;
        private float M2;
        private float[] a;
        private float[] A;
        private float[] y;

        // параметры метода
        private int q;
        private float dt;

        private float[/*шаг*/][/*номер нейрона*/] x;                    // Характеристики нейрона
        private float[/*шаг*/][/*номер нейрона*/,/*номер слоя*/] w;		// синаптические веса нейрона

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


        private float CheckNumericValue(TextBox element)
        {
            float result;
            try
            {
                if (!float.TryParse(element.Text, out result))
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

        // Расчет фазовой траектории

        private void CreateW()
        {
            w = new float[q][,];

            for (int k = 0; k < q; k++)
                w[k] = new float[n, n];
        }
        private void CreateX()
        {
            x = new float[q + 1][];

            for (int k = 0; k < q; k++)
            {
                x[k + 1] = new float[n];
            }
        }

        private void SetRandomW()
        {
            Random rnd = new Random();
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    w[0][i, j] = rnd.Next((int)B);
                }
            }
            for (int k = 1; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        w[k][i, j] = w[0][i, j];
                    }
                }
            }
        }

        private void GetX()
        {
            x[0] = a;

            for (int k = 0; k < q; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    float S = 0;
                    for (int j = 0; j < n; j++)
                        S += w[k][i, j] * x[k][j];

                    x[k + 1][i] = x[k][i] + dt * (-y[i] * x[k][i] + S);
                }
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            q = 10;
            dt = T / q;

            GetDataFromDataGrid();

            CreateW();
            CreateX();

            GetX();

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
                        dataGridView2[i + 1, k].Value = x[k][i];
                }
            }
        }

        private void GetDataFromDataGrid()
        {
            if (!dataGridView1.ReadOnly)
            {
                // get a
                a = new float[n];
                for (int i = 0; i < n; i++)
                {
                    a[i] = float.Parse(dataGridView1[1, i].Value.ToString());
                }

                // get A
                A = new float[n];
                for (int i = 0; i < n; i++)
                {
                    A[i] = float.Parse(dataGridView1[2, i].Value.ToString());
                }

                // get A
                y = new float[n];
                for (int i = 0; i < n; i++)
                {
                    y[i] = float.Parse(dataGridView1[3, i].Value.ToString());
                }
            }
        }
    }
}
