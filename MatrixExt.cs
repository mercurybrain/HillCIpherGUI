using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HillCIpherGUI
{
    static class MatrixExt
    {
        // метод расширения для получения количества строк матрицы
        public static int RowsCount(this int[,] matrix)
        {
            return matrix.GetUpperBound(0) + 1;
        }

        // метод расширения для получения количества столбцов матрицы
        public static int ColumnsCount(this int[,] matrix)
        {
            return matrix.GetUpperBound(1) + 1;
        }

        // генерация матрицы
        public static int[,] GenerateMatrix(int n, int finish)
        {
            Random rnd = new Random();
            int[,] key = new int[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    key[i, j] = rnd.Next(0, finish-1);
                }
            }
            return key;
        }
        public static int[,] GenerateKeyMatrix(int n, int finish, out int detMatrix, out int x, int module = 256) {
            int[,] key = MatrixExt.GenerateMatrix(n, finish);
            detMatrix = MatrixExt.FindDeterminant(key, n);
            x = MatrixExt.CheckKeyMatrix(detMatrix, module);
            Console.WriteLine("\nПроверка генерации матрицы...");
            while (MatrixExt.CheckKeyMatrix(detMatrix, module) == 0)
                {
                    key = MatrixExt.GenerateMatrix(n, finish);
                    detMatrix = MatrixExt.FindDeterminant(key, n);
                    x = MatrixExt.CheckKeyMatrix(detMatrix, module);
                }
            Console.WriteLine("Матрица-ключ с обратным детерминантом сгенерирована!");
            return key;
        }
        /// <summary>
        /// Метод преобразования одномерного массива int [] в двумерный int [,].
        /// </summary>
        /// <param name="a">Исходный массив.</param>
        /// <param name="rows">Кол-во строк в матрице - результате.</param>
        /// <returns>Возвращает матрицу int [,] из исходного массива.</returns>
        public static int[,] ArrayReform(int[] a, int rows)
        {

            int columns = a.Length / rows;
            int[,] b = new int[columns, rows];
            Buffer.BlockCopy(a, 0, b, 0, sizeof(int) * a.Length);
            return b;
        }

        /// <summary>
        /// Вывод матрицы int [,] в консоль в наглядной форме.
        /// </summary>
        /// <param name="mtrx">Матрица для печати.</param>
        public static void PrintMatrix(int[,] mtrx)
        {
            for (int i = 0; i < mtrx.RowsCount(); i++)
            {
                for (int j = 0; j < mtrx.ColumnsCount(); j++)
                {
                    Console.Write(mtrx[i, j] + " ");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Метод умножения двух матриц int [,].
        /// </summary>
        /// <param name="matrixA">Первая матрица.</param>
        /// <param name="matrixB">Вторая матрица.</param>
        /// <returns>Возвращает результат умножения двух матриц.</returns>
        public static int[,] MatrixMultiplication(int[,] matrixA, int[,] matrixB)
        {
            if (matrixA.ColumnsCount() != matrixB.RowsCount())
            {
                throw new Exception("Умножение невозможно! Количество столбцов первой матрицы не равно количеству строк второй матрицы.");
            }
            var matrixC = new int[matrixA.RowsCount(), matrixB.ColumnsCount()];
            for (var i = 0; i < matrixA.RowsCount(); i++)
            {
                for (var j = 0; j < matrixB.ColumnsCount(); j++)
                {
                    matrixC[i, j] = 0;

                    for (var k = 0; k < matrixA.ColumnsCount(); k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
            return matrixC;
        }
        /// <summary>
        /// Метод умножения матрицы на число.
        /// </summary>
        /// <param name="inputMartix">Исходная матрица.</param>
        /// <param name="k">Число.</param>
        /// <returns>Возвращает результат умножения исходной матрицы на число.</returns>
        public static int[,] ScalarMatrixMultiplication(int[,] inputMartix, int k)
        {
            var resultMatrix = new int[inputMartix.RowsCount(), inputMartix.ColumnsCount()];

            for (var i = 0; i < inputMartix.RowsCount(); i++)
            {
                for (var j = 0; j < inputMartix.ColumnsCount(); j++)
                {
                    resultMatrix[i, j] = inputMartix[i, j] * k;
                }
            }

            return resultMatrix;
        }
        /// <summary>
        /// Метод транспонирования матрицы.
        /// </summary>
        /// <param name="mtrx">Исходная матрица.</param>
        /// <returns>Возвращает транспонированную матрицу int [,].</returns>
        public static int[,] TransMatrix(int[,] mtrx)
        {
            int tmp;
            for (int i = 0; i < mtrx.RowsCount(); i++)
            {
                for (int j = 0; j < i; j++)
                {
                    tmp = mtrx[i, j];
                    mtrx[i, j] = mtrx[j, i];
                    mtrx[j, i] = tmp;
                }
            }
            return mtrx;
        }

        /// <summary>
        /// Метод деления матрицы по модулю.
        /// </summary>
        /// <param name="mtrx">Исходная матрица.</param>
        /// <param name="module">Модуль дял деления.</param>
        public static int[,] MatrixRemainder(int[,] mtrx, int module)
        {
            int[,] remainded = new int[mtrx.RowsCount(), mtrx.ColumnsCount()];
            for (int i = 0; i < mtrx.RowsCount(); i++)
            {
                for (int j = 0; j < mtrx.ColumnsCount(); j++)
                {
                    remainded[i, j] = mtrx[i, j] % module;
                }
            }
            return remainded;
        }
        /* детерминант матрицы
        public static int DET(int n, int[,] Mat)
        {
            int d = 0;
            int k, i, j, subi, subj;
            int[,] SUBMat = new int[n, n];

            if (n == 2)
            {
                return ((Mat[0, 0] * Mat[1, 1]) - (Mat[1, 0] * Mat[0, 1]));
            }

            else
            {
                for (k = 0; k < n; k++)
                {
                    subi = 0;
                    for (i = 1; i < n; i++)
                    {
                        subj = 0;
                        for (j = 0; j < n; j++)
                        {
                            if (j == k)
                            {
                                continue;
                            }
                            SUBMat[subi, subj] = Mat[i, j];
                            subj++;
                        }
                        subi++;
                    }
                    d = d + (int)(Math.Pow(-1, k) * Mat[0, k] * DET(n - 1, SUBMat));
                }
            }
            return d;
        }*/
        /// <summary>
        /// Метод нахождения алгебраических дополнений матрицы и запись их в другую матрицу по ссылке.
        /// </summary>
        /// <param name="A">Исходная матрица.</param>
        /// <param name="size">Размер исходной матрицы.</param>
        /// <param name="B">Матрица для записи.</param>
        public static void FindAlgDop(int[,] A, int size, ref int[,] B)
        {
            int i, j;

            // находим определитель матрицы A
            int det = FindDeterminant(A, size);

            if (det > 0) // это для знака алгебраического дополнения
                det = -1;
            else
                det = 1;
            int[,] minor = new int[size - 1, size - 1];

            for (j = 0; j < size; j++)
            {
                for (i = 0; i < size; i++)
                {
                    // получаем алгебраическое дополнение
                    GetMinor(A, minor, j, i, size);
                    if ((i + j) % 2 == 0)
                        B[j, i] = -det * FindDeterminant(minor, size - 1);
                    else
                        B[j, i] = det * FindDeterminant(minor, size - 1);
                }
            }
        }

        private static int GetMinor(int[,] A, int[,] B, int x, int y, int size)
        {
            int xCount = 0, yCount = 0;
            int i, j;
            for (i = 0; i < size; i++)
            {
                if (i != x)
                {
                    yCount = 0;
                    for (j = 0; j < size; j++)
                    {
                        if (j != y)
                        {
                            B[xCount, yCount] = A[i, j];
                            yCount++;
                        }
                    }
                    xCount++;
                }
            }
            return 0;
        }
        /// <summary>
        /// Рекурсивный метод нахождения детерминанта матрицы int [,].
        /// </summary>
        /// <param name="A">Матрица.</param>
        /// <param name="size">Размер матрицы.</param>
        /// <returns>Возвращает детерминант матрицы.</returns>
        public static int FindDeterminant(int[,] A, int size)
        {
            // останавливаем рекурсию, если матрица
            // состоит из одного элемента
            if (size == 1)
            {
                return A[0, 0];
            }
            else
            {
                int det = 0;
                int i;
                int[,] Minor = new int[size - 1, size - 1];
                for (i = 0; i < size; i++)
                {
                    GetMinor(A, Minor, 0, i, size);
                    // Рекурсия
                    det += (int)Math.Pow(-1, i) * A[0, i] * FindDeterminant(Minor, size - 1);
                }
                return det;
            }
        }


        /// <summary>
        /// Метод нахождения НОД и коэффицентов при элементах.
        /// </summary>
        /// <param name="a">Первый элемент.</param>
        /// <param name="b">Второй элемент.</param>
        /// <returns>Возвращает наибольший общий делитель.</returns>
        public static int Gcd(int a, int b, out int x, out int y)
        {
            if (a == 0)
            {
                x = 0;
                y = 1;
                return b;
            }

            int gcd = Gcd(b % a, a, out x, out y);

            int newY = x;
            int newX = y - (b / a) * x;

            x = newX;
            y = newY;
            return gcd;
        }
        /// <summary>
        /// Метод нахождения обратного детерминанту элемента в кольце по модулю.
        /// </summary>
        /// <param name="det">Детерминант.</param>
        /// <param name="mod">Модуль.</param>
        public static int CheckKeyMatrix(int det, int mod)
        {
            int x, y;
            int g = MatrixExt.Gcd(Math.Abs(det), Math.Abs(mod), out x, out y);
            if (g != 1)
                return 0;
            else
            {
                x = (x % mod + mod) % mod;
                return x;
            }
        }
        /// <summary>
        /// Метод записи матрицы int[,] в DataTable для последующего отображения пользователю.
        /// </summary>
        /// <param name="det">Детерминант.</param>
        /// <param name="mod">Модуль.</param>
        public static void writeMatrix(ref DataTable matrix, int[,] arr) {
            matrix.Reset();
            for (int j = 0; j < arr.ColumnsCount(); j++)
            {
                matrix.Columns.Add("Столбец " + Convert.ToString(j+1), typeof(int));

            }
            for (int i = 0; i < arr.RowsCount(); i++)
            {
                matrix.Rows.Add(new Object[] { });
            }
            for (int i = 0; i < arr.RowsCount(); i++)
            {
                for (int j = 0; j < arr.ColumnsCount(); j++) {
                    matrix.Rows[i][j] = arr[i,j];
                }
            }
        }
        public static int[,] readMatrix(DataTable matrix) {
            if (matrix.Rows.Count != matrix.Columns.Count) throw new ArgumentException("Матрица-ключ должна быть квадратной!");
            int[,] arr = new int[matrix.Rows.Count, matrix.Columns.Count];
            for (int i = 0; i < matrix.Rows.Count; i++)
            {
                for (int j = 0; j < matrix.Columns.Count; j++)
                {
                    arr[i,j] = (int)matrix.Rows[i][j];
                }
            }
            return arr;
        }
    }
}
