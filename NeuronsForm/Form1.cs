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

        private void InitializationW()
        {

        }

        private void GetX()
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
