using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace HillCIpherGUI
{
    public partial class Form1 : Form
    {
        int[,] key;
        int module = 256;
        DataTable matrix = new DataTable("Матрица");
        const string Alph = "AÀÁBCDÛEûúùöõôÜFGØHIðòóJÙKLÚMN×OPQRSTUVWXYZÄабèвгдÒеёжÃзийÕклмçÖнÝоÔпрс÷туфхцÂчшщøþъыьэюÓï !#$Þ%&\'()*+,-./01â23456789:;<=>?@яАÊБВГДßЕЁàЖЗИÉáЙКЛÌМНО[\\]^_`{|}~ПæРСÆТУФÎХЦÑЧШЩÇЪÈЫÐЬЭЮЯabcdefghijklmnÍopqrstuvwxyz€ƒ„…†‡‰ŠŒŽ•–—˜™šœžŸ¡¢£¤¥¦§©ª«¬®¯°±²³µ¶·¹º»¼½¾¿";
        public Form1()
        {
            InitializeComponent();
        }
        public static int letterToNumber(char letter)
        {
            return Alph.IndexOf(letter);
        }
        public static char numberToLetter(int number)
        {
            return Alph[number];
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        private string encypher(string rawmessage, int n) {
            Console.WriteLine("\n\n\n=============ШИФРОВАНИЕ=============\n\n\n");
            Console.WriteLine("Проверка наличия матрицы в таблице...");
            int detMatrix, x;
            if (matrix.Rows.Count > 0)
            {
                key = MatrixExt.readMatrix(matrix);
                Console.WriteLine("Матрица найдена!");
                detMatrix = MatrixExt.FindDeterminant(key, n);
                x = MatrixExt.CheckKeyMatrix(detMatrix, module);
                if (x == 0)
                {
                    throw new ArgumentException("Матрица-ключ не подходит для работы с алгоритмом!\n" +
                    "Необходимо, чтобы столбцы матрицы-ключа были линейно-независимы.");
                }
            }
            else {
                Console.WriteLine("Матрица не найдена, генерирую...");
                key = MatrixExt.GenerateKeyMatrix(n, module, out detMatrix, out x);
                matrix = new DataTable("Матрица"); // Я не знаю почему, но Reset() не сбрасывает матрицу ПОЛНОСТЬЮ, и DGV не обновляется НИКАК
                MatrixExt.writeMatrix(ref matrix, key);
                dataGridView1.DataSource = matrix;
                dataGridView1.Update();
                dataGridView1.Refresh();
            }
            Console.WriteLine("\nМатрица-ключ: ");
            MatrixExt.PrintMatrix(key);
            Console.WriteLine("Детерминант: " + detMatrix);
            Console.WriteLine("Обратный детерминанту элемент: " + x);
            int key_rows = key.RowsCount();
            int key_columns = key.ColumnsCount();
            if (key_rows != key_columns)
            {
                throw new ArgumentException("Ключ должен быть квадратной матрицей!");
            }

            Console.WriteLine("Строк: {0}, столбцов: {1}.", key_rows, key_columns);
            List<int> message = new List<int>();
            for (int i = 0; i < rawmessage.Length; i++)
            {
                char current_letter = Char.ToLower(rawmessage[i]);
                if (current_letter != ' ')
                {
                    int letterIndex = letterToNumber(current_letter);
                    message.Add(letterIndex);
                }
            }
            int dops = 0;
            if (message.ToArray().Length % key_rows != 0)
            {
                Console.WriteLine("Уравниваю кол-во столбцов...");
                for (int i = 0; i < message.ToArray().Length; i++)
                {
                    message.Add(message[i]);
                    dops++;
                    if (message.ToArray().Length % key_rows == 0)
                    {
                        Console.WriteLine("Выполнено!\n");
                        break;
                    }
                }
            }
            Console.WriteLine("Массив из цифр: ");
            foreach (var item in message.ToArray())
            {
                Console.Write(item + " ");
            }

            int[,] messageResized = MatrixExt.ArrayReform(message.ToArray(), key_rows);

            Console.WriteLine("\nМатрица сообщения: ");
            MatrixExt.PrintMatrix(messageResized);
            for (int i = 0; i < messageResized.RowsCount(); i++)
            {
                for (int j = 0; j < messageResized.ColumnsCount(); j++)
                {
                    Console.Write(numberToLetter(messageResized[i, j]) + " ");
                }
            }
            Console.WriteLine("\nМатрица-ключ: ");
            MatrixExt.PrintMatrix(key);

            Console.WriteLine("\nЗашифрованная матрица: ");
            int[,] encrypted = MatrixExt.MatrixMultiplication(messageResized, key);
            encrypted = MatrixExt.MatrixRemainder(encrypted, module);
            MatrixExt.PrintMatrix(encrypted);

            string encMessage = "";
            for (int i = 0; i < encrypted.RowsCount(); i++)
            {
                for (int j = 0; j < encrypted.ColumnsCount(); j++)
                {
                    Console.Write(numberToLetter(encrypted[i, j]) + " ");
                    encMessage += numberToLetter(encrypted[i, j]);
                }
            }
            Console.WriteLine("\nЗашифрованная строка: {0}, длина зашифрованной строки: {1}, лишних символов(для перемножения матриц): {2}.", encMessage, encMessage.Length, dops);
            label8.Text = "Доп. символов: " + dops;
            return encMessage;
        }
        private string decypher(string rawmessage, int dops) {
            if (key == null) {
                key = MatrixExt.readMatrix(matrix);
            }
            int n = key.RowsCount();
            int[,] messageResized;
            Console.WriteLine("\n\n\n=============РАСШИФРОВКА=============\n\n\n");
            Console.WriteLine("Превращаю сообщение в матрицу...");
            List<int> message = new List<int>();
            for (int i = 0; i < rawmessage.Length; i++)
            {
                int letterIndex = letterToNumber(rawmessage[i]);
                message.Add(letterIndex);
            }
            messageResized = MatrixExt.ArrayReform(message.ToArray(), key.RowsCount());

            Console.WriteLine("\nМатрица сообщения: ");
            MatrixExt.PrintMatrix(messageResized);
            for (int i = 0; i < messageResized.RowsCount(); i++)
            {
                for (int j = 0; j < messageResized.ColumnsCount(); j++)
                {
                    Console.Write(numberToLetter(messageResized[i, j]) + " ");
                }
            }
            int detMatrix = MatrixExt.FindDeterminant(key, n);
            int x = MatrixExt.CheckKeyMatrix(detMatrix, module);
            Console.WriteLine("\nПроверка матрицы-ключа...");
            if (x == 0) { // Ну а вдруг
                throw new ArgumentException("Матрица-ключ не подходит для работы с алгоритмом!\n" +
                    "Необходимо, чтобы столбцы матрицы-ключа были линейно-независимы.");
            }
            int[,] algDop = new int[n, n];
            MatrixExt.FindAlgDop(key, n, ref algDop);
            Console.WriteLine("\nМатрица-ключ: ");
            MatrixExt.PrintMatrix(key);
            Console.WriteLine("Матрица алгебраических дополнений: ");
            MatrixExt.PrintMatrix(algDop);
            int[,] preDecrypted = MatrixExt.MatrixRemainder(algDop, module);
            Console.WriteLine("Алг. допы деленные на модуль: ");
            MatrixExt.PrintMatrix(preDecrypted);
            Console.WriteLine("Предыдущая матрица, умноженная на обратный детерминанту элемент({0}): ", x);
            preDecrypted = MatrixExt.ScalarMatrixMultiplication(preDecrypted, x);
            MatrixExt.PrintMatrix(preDecrypted);
            Console.WriteLine("Предыдущая матрица, деленная по модулю({0}): ", module);
            preDecrypted = MatrixExt.MatrixRemainder(preDecrypted, module);
            MatrixExt.PrintMatrix(preDecrypted);
            Console.WriteLine("Транспонированная: ");
            preDecrypted = MatrixExt.TransMatrix(preDecrypted);
            MatrixExt.PrintMatrix(preDecrypted);

            for (int i = 0; i < preDecrypted.RowsCount(); i++)
            {
                for (int j = 0; j < preDecrypted.ColumnsCount(); j++)
                {
                    if (preDecrypted[i, j] < 0)
                    {
                        preDecrypted[i, j] = module + preDecrypted[i, j];
                    }
                }
            }

            Console.WriteLine("Заменены отрицательные элементы по формуле: элемент + модуль({0})", module);
            MatrixExt.PrintMatrix(preDecrypted);
            Console.WriteLine("Последняя полученная матрица является обратной по модулю к матрице ключа." +
                "\nЕсли перемножить матрицу ключа и эту матрицу, а потом результат разделить по модулю на {0}," +
                " мы получим единичную матрицу.", module);
            Console.WriteLine("Проверка: ");
            int[,] check = MatrixExt.MatrixMultiplication(key, preDecrypted);
            check = MatrixExt.MatrixRemainder(check, module);
            MatrixExt.PrintMatrix(check);

            Console.WriteLine("Матрица расшифровки: ");
            int[,] decrypted = MatrixExt.MatrixMultiplication(messageResized, preDecrypted);
            MatrixExt.PrintMatrix(decrypted);
            decrypted = MatrixExt.MatrixRemainder(decrypted, module);

            string decMessage = "";
            for (int i = 0; i < decrypted.RowsCount(); i++)
            {
                for (int j = 0; j < decrypted.ColumnsCount(); j++)
                {
                    Console.Write(numberToLetter(decrypted[i, j]) + " ");
                    decMessage += numberToLetter(decrypted[i, j]);
                }
            }
            Console.WriteLine("\nРасшифрованная строка: {0}, длина расшифрованной строки: {1}.", decMessage, decMessage.Length);
            Console.WriteLine("Лишних символов: {0}", dops);
            decMessage = decMessage.Remove((decMessage.Length - dops), dops);
            Console.WriteLine("Исходная строка: {0}", decMessage);
            return decMessage;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string rawmessage = textBox1.Text;
            int n = Convert.ToInt32(textBox2.Text);
            textBox3.Text = encypher(rawmessage, n);
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string encoded = textBox7.Text;
            int dops = Convert.ToInt32(textBox8.Text);
            if (matrix.Rows.Count < 1 && key == null)
                MessageBox.Show("Нет ключевой матрицы!\n" +
                    "Сгенерируйте её или зашифруйте сообщение!");
            else textBox6.Text = decypher(encoded, dops);
        }
        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            textBox8.Text = "0";
            textBox8.Enabled = (checkBox1.CheckState == CheckState.Checked);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeConsole();

        private void button4_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => {
                AllocConsole();
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (matrix.Rows.Count != 0)
            {
                saveFileDialog1.Filter = "XML файл (*.xml)|*.xml";
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    matrix.WriteXml(saveFileDialog1.FileName, true);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML файл (*.xml)|*.xml";
            saveFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                matrix.ReadXmlSchema(@openFileDialog1.FileName);
                matrix.ReadXml(@openFileDialog1.FileName);
                dataGridView1.DataSource = matrix;
                dataGridView1.Update();
                MessageBox.Show("Успешно загружена матрица: " + openFileDialog1.FileName);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string input = "3";
            int detMatrix, x;
            if (InputBox.Input("Введите размерность генерируемой матрицы", "Размерность:", ref input) == DialogResult.OK)
            {
                int num = Convert.ToInt32(input);
                key = MatrixExt.GenerateKeyMatrix(num, module, out detMatrix, out x);
                matrix = new DataTable("Матрица"); // Я не знаю почему, но Reset() не сбрасывает матрицу ПОЛНОСТЬЮ, и DGV не обновляется НИКАК
                MatrixExt.writeMatrix(ref matrix, key);
                dataGridView1.DataSource = matrix;
                dataGridView1.Update();
                dataGridView1.Refresh();
            }
        }

        private void открытьКонсольToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AllocConsole();
            Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: Если закроете консоль - программа прекратит работу.");
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F) {
                AllocConsole();
                Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: Если закроете консоль - программа прекратит работу.");
            }
        }
    }
}
