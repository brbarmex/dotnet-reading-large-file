using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FastProcess
{
    internal static class Program
    {
        private static void Main()
        {
            var gen0 = GC.CollectionCount(0);
            var gen1 = GC.CollectionCount(1);
            var gen2 = GC.CollectionCount(2);

            Span<byte> freelancer = Encoding.UTF8.GetBytes("FREE-LANCER").AsSpan();
            Span<byte> consult = Encoding.UTF8.GetBytes("CONSULTOR").AsSpan();
            Span<byte> pde = Encoding.UTF8.GetBytes("PDE").AsSpan();
            Span<decimal> totalSales = stackalloc decimal[3];
            Span<int> ammounts = stackalloc int[3];

            int bytesOffSet = 0;
            int bytesConsumed = 0 ;
            using var fs = File.OpenRead("personxsales.csv");
            byte[] bytesChunk = new byte[1024];
            var sw = new Stopwatch();
            sw.Start();

            int byteRead;
            while ((byteRead = fs.Read(bytesChunk, bytesOffSet, bytesChunk.Length - bytesOffSet)) > 0)
            {
                bytesOffSet += byteRead;
                int position;

                while ((position = Array.IndexOf(bytesChunk, (byte)'\n', bytesConsumed, bytesOffSet - bytesConsumed)) > 0)
                {
                    Span<byte> line = new(bytesChunk, bytesConsumed, position-bytesConsumed);
                    Span<byte> responsibility = GetValueBetweenSemicolon(line, 2);
                    Span<byte> ammountSale = GetValueBetweenSemicolon(line, 4);

                    if(responsibility.SequenceEqual(freelancer))
                    {
                        AddSumTotalSales(line, ref totalSales[0]);
                        AddSumAmmountSales(ammountSale, ref ammounts[0]);
                    }
                    else if(responsibility.SequenceEqual(consult))
                    {
                        AddSumTotalSales(line, ref totalSales[1]);
                        AddSumAmmountSales(ammountSale, ref ammounts[1]);
                    }
                    else if(responsibility.SequenceEqual(pde))
                    {
                        AddSumTotalSales(line, ref totalSales[2]);
                       AddSumAmmountSales(ammountSale, ref ammounts[2]);
                    }

                    bytesConsumed += position - bytesConsumed + 1;
                }

                Array.Copy(bytesChunk, bytesConsumed, bytesChunk, 0, bytesOffSet - bytesConsumed);
                bytesOffSet -= bytesConsumed;
                bytesConsumed = 0;
            }
            sw.Stop();

            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"PDE..........total_vendido: {totalSales[0]}  quantidade: {ammounts[0]}");
            Console.WriteLine($"CONSULTOR....total_vendido: {totalSales[1]}  quantidade: {ammounts[1]}");
            Console.WriteLine($"FREELANCER...total_vendido: {totalSales[2]}  quantidade: {ammounts[2]}");
            Console.WriteLine("----------------------------------------------------------------------");
            Console.WriteLine($"Gen0: {GC.CollectionCount(0) - gen0} |Gen1: {GC.CollectionCount(1) - gen1} |Gen2: {GC.CollectionCount(2) - gen2} |Temp: {sw.ElapsedMilliseconds} ms |Allocated: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");
        }

        internal static void AddSumAmmountSales(Span<byte> ammountSale, ref int ammount)
        => ammount += int.Parse(Encoding.UTF8.GetString(ammountSale));

        internal static void AddSumTotalSales(Span<byte> line, ref decimal totalSale)
        => totalSale += decimal.Parse(Encoding.UTF8.GetString(line[(line.LastIndexOf((byte)';') + 1)..]));

        internal static Span<byte> GetValueBetweenSemicolon(Span<byte> line, int positionInLine)
        => positionInLine <= 0 ? line.Slice(0, line.IndexOf((byte)';') > 0 ? line.IndexOf((byte)';') : line.Length) : GetValueBetweenSemicolon(line[(line.IndexOf((byte)';') + 1)..], positionInLine - 1);
    }
}