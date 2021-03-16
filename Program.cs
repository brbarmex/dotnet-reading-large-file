using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FastProcess
{
    internal struct Program
    {
        private static void Main()
        {
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            Span<byte> freelancer = Encoding.UTF8.GetBytes("FREE-LANCER");
            Span<byte> consult = Encoding.UTF8.GetBytes("CONSULTOR");
            Span<byte> pde = Encoding.UTF8.GetBytes("PDE");
            Span<byte> line, responsibility, ammountSale, totalSale;
            Span<decimal> totalSales = stackalloc decimal[3];
            Span<int> ammounts = stackalloc int[3];

            int bytesOffSet = 0;
            int bytesConsumed = 0;
            using var fs = File.OpenRead("personxsales.csv");
            byte[] bytesChunk = ArrayPool<byte>.Shared.Rent(1600 * 8);
            var sw = new Stopwatch();
            sw.Start();

            int byteRead;
            while ((byteRead = fs.Read(bytesChunk, bytesOffSet, bytesChunk.Length - bytesOffSet)) > 0)
            {
                bytesOffSet += byteRead;
                int position;

                while ((position = Array.IndexOf(bytesChunk, (byte)'\n', bytesConsumed, bytesOffSet - bytesConsumed)) > 0)
                {
                    line = new(bytesChunk, bytesConsumed, position - bytesConsumed);

                    var (rIndex, rLength) = GetPosition(line, 2);
                    responsibility = line.Slice(rIndex, rLength);

                    var (aIndex, aLength) = GetPosition(line, 4);
                    ammountSale = line.Slice(aIndex, aLength);

                    var (tIndex, tLength) = GetPosition(line, 5);
                    totalSale = line[tIndex..];

                    if (responsibility.SequenceEqual(freelancer))
                        ExecuteCalculation(ammountSale, ref ammounts[0], totalSale, ref totalSales[0]);
                    else if (responsibility.SequenceEqual(consult))
                        ExecuteCalculation(ammountSale, ref ammounts[1], totalSale, ref totalSales[1]);
                    else if (responsibility.SequenceEqual(pde))
                        ExecuteCalculation(ammountSale, ref ammounts[2], totalSale, ref totalSales[2]);

                    bytesConsumed += position - bytesConsumed + 1;
                }

                Buffer.BlockCopy(bytesChunk, bytesConsumed, bytesChunk, 0, bytesOffSet - bytesConsumed);
                bytesOffSet -= bytesConsumed;
                bytesConsumed = 0;
            }
            sw.Stop();

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"FREELANCER...total_vendido: {totalSales[0]}  quantidade: {ammounts[0]}");
            Console.WriteLine($"CONSULTOR....total_vendido: {totalSales[1]}  quantidade: {ammounts[1]}");
            Console.WriteLine($"PDE..........total_vendido: {totalSales[2]}  quantidade: {ammounts[2]}");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"Gen0: {GC.CollectionCount(0) - gen0} |Gen1: {GC.CollectionCount(1) - gen1} |Gen2: {GC.CollectionCount(2) - gen2} |Temp: {sw.ElapsedMilliseconds} ms |Allocated: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");
        }

        internal static (int StartIndex, int Length) GetPosition(Span<byte> line, int countLoop)
        {
            int start = 0;
            int rounds = 0;

            for (int index = 0; index < line.Length; index++)
            {
                if (line[index] == (byte)';')
                {
                    rounds++;

                    if (countLoop == rounds)
                    {
                        start = index + 1;
                        break;
                    }
                }
            }

            int end = rounds = 0;

            if (countLoop != 5)
            {
                for (int index = start; index < line.Length; index++)
                {
                    rounds++;
                    if (line[index] == (byte)';')
                    {
                        end = rounds - 1;
                        break;
                    }
                }
            }

            if (end == 0)
                end = line.Length - 2;

            return (start, end);
        }

        internal static void ExecuteCalculation(Span<byte> ammounts, ref int ammout, Span<byte> sales, ref decimal totalSale)
        {
            Span<char> _char = stackalloc char[ammounts.Length];

            for (int index = 0; index < ammounts.Length; index++)
            {
                _char[index] = (char)ammounts[index];
            }

            ammout += int.Parse(_char);

            _char = stackalloc char[sales.Length];

            for (int index = 0; index < sales.Length; index++)
            {
                _char[index] = (char)sales[index];
            }

            totalSale += decimal.Parse(_char);
        }

        internal static Span<byte> GetValueBetweenSemicolon(Span<byte> line, int positionInLine)
        => positionInLine <= 0 ? line.Slice(0, line.IndexOf((byte)';') > 0 ? line.IndexOf((byte)';') : line.Length) : GetValueBetweenSemicolon(line[(line.IndexOf((byte)';') + 1)..], positionInLine - 1);
    }
}