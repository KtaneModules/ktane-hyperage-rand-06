using System;
using System.Collections.Generic;
using System.Text;

public static class Matrix
{
    private static int[] discard(this int[] matrix, int z)
    {
        int size = (int)Math.Sqrt(matrix.Length);
        int[] ans = new int[(size - 1) * (size - 1)];
        int ii = 0;
        for (int i = 0; i < matrix.Length; i++)
        {
            int x=i%size, y=i/size;
            if (x==z || y==0) continue;
            ans[ii] = matrix[i];
            ii++;
        }
        return ans;
    }
    
    public static long det(this int[] matrix)
    {
        int size = (int)Math.Sqrt(matrix.Length);
        if (size == 1) return matrix[0];
        long ans = 0;
        for (int i = 0; i < size; i++)
        {
            ans += matrix[i] * (int)Math.Pow(-1, i) * det(matrix.discard(i));
        }

        return ans;
    }

    private static string base6(long whole, long fraction)
    {
        var digits = new List<char>();
        while (whole > 0)
        {
            long remainder = whole % 6;
            digits.Add((char)('0' + remainder));
            whole /= 6;
        }
        
        digits.Reverse();
        string wholeString = digits.Join("");
        
        var result = new StringBuilder(".");
        
        decimal current = fraction / 1000000000000M;
        for (int i = 0; i < 3; i++)
        {
            current *= 6;
            int digit = (int)current;
            result.Append(digit);
            current -= digit;
        }
        
        return wholeString + result;
        
    }
    
    public static string getBase6Determinant(this long det)
    {
        det = Math.Abs(det);
        long wholePart =    det / 1000000000000L; 
        long nonWholePart = det % 1000000000000L;
        return base6(wholePart, nonWholePart);
    }
}