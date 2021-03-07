using System;
using System.IO;
using System.Text;

namespace FastProcess
{
    internal static class Program
    {
        private const byte lineBreakChar = (byte)'\n';
        private const byte semiColonChar = (byte)';';
        private const string pathFile = "person_x_sale.csv";

        private static void Main()
        {
            Span<byte> freelancer = Encoding.UTF8.GetBytes("FREE-LANCER").AsSpan();
            Span<byte> consult = Encoding.UTF8.GetBytes("CONSULTOR").AsSpan();
            Span<byte> pde = Encoding.UTF8.GetBytes("PDE").AsSpan();

            decimal totalSaleByFreelancer = 0, totalSaleByConsult = 0, totalSaleByPDE = 0;
            int ammountSaleByFreelancer = 0, ammountSaleByConsult = 0, ammountSaleByPDE = 0;

            int bytesOffSet = 0;
            int bytesConsumed = 0;
            using var fs = File.OpenRead(pathFile);
            byte[] bytesChunk = new byte[fs.Length];

            int byteRead;
            while ((byteRead = fs.Read(bytesChunk, bytesOffSet, bytesChunk.Length - bytesOffSet)) > 0)
            {
                bytesOffSet += byteRead;
                int position;

                while ((position = Array.IndexOf(bytesChunk, lineBreakChar, bytesConsumed, bytesOffSet - bytesConsumed)) > 0)
                {
                    Span<byte> line = new(bytesChunk, bytesConsumed, position-bytesConsumed);
                    Span<byte> responsibility = GetValueBetweenSemicolon(line, 2);
                    Span<byte> ammountSale = GetValueBetweenSemicolon(line, 4);
                    Span<byte> totalSale = GetValueBetweenSemicolon(line, 5);

                    if(responsibility.SequenceEqual(freelancer))
                        AddTotalToTheSum(ref totalSale, ref ammountSale, ref totalSaleByFreelancer, ref ammountSaleByFreelancer);
                    else if(responsibility.SequenceEqual(consult))
                        AddTotalToTheSum(ref totalSale, ref ammountSale, ref totalSaleByConsult, ref ammountSaleByConsult);
                    else if(responsibility.SequenceEqual(pde))
                        AddTotalToTheSum(ref totalSale, ref ammountSale, ref totalSaleByPDE, ref ammountSaleByPDE);

                    bytesConsumed += position - bytesConsumed + 1;
                }

                Array.Copy(bytesChunk, bytesConsumed, bytesChunk, 0, bytesOffSet - bytesConsumed);
                bytesOffSet -= bytesConsumed;
                bytesConsumed = 0;
            }

            Console.WriteLine($"PDE..........total_vendido: {totalSaleByPDE}  quantidade: {ammountSaleByPDE}");
            Console.WriteLine($"CONSULTOR....total_vendido: {totalSaleByConsult}  quantidade: {ammountSaleByConsult}");
            Console.WriteLine($"FREELANCER...total_vendido: {totalSaleByFreelancer}  quantidade: {ammountSaleByFreelancer}");
        }

        internal static void AddTotalToTheSum(ref Span<byte> totalSale, ref Span<byte> ammountSale, ref decimal total, ref int ammount)
        {
            total += decimal.Parse(Encoding.UTF8.GetString(totalSale));
            ammount += int.Parse(Encoding.UTF8.GetString(ammountSale));
        }

        internal static Span<byte> GetValueBetweenSemicolon(Span<byte> line, int positionInLine)
        => positionInLine <= 0
            ? line.Slice(0, line.IndexOf(semiColonChar) > 0 ? line.IndexOf(semiColonChar) : line.Length)
            : GetValueBetweenSemicolon(line[(line.IndexOf(semiColonChar)+1)..], positionInLine - 1);
    }
}