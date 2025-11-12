using System.Linq;

public static class SquaresChecker
{
    private static bool valid(this string[] rotations)
    {
        if(rotations.ToList().ConvertAll(a => a[0]).Distinct().Count()!=4) return false;
        return rotations[0][1]==rotations[1][0] && rotations[1][1]==rotations[2][0] && rotations[2][1]==rotations[3][0] && rotations[3][1]==rotations[0][0];
    }

    private static string[] getSquare(this string[] rotations, int xCenter, int yCenter, int xRelative, int yRelative)
    {
        int index1 = (xCenter + xRelative) + (yCenter + yRelative)*6;
        int index2 = (xCenter - yRelative) + (yCenter + xRelative)*6;
        int index3 = (xCenter - xRelative) + (yCenter - yRelative)*6;
        int index4 = (xCenter + yRelative) + (yCenter - xRelative)*6;
        return new[]{rotations[index1],rotations[index2],rotations[index3],rotations[index4]};
    }
    
    public static int squares(string[] rotations)
    {
        if (rotations.Contains(null)) return -1;
        int ans = 0;
        for (int i = 0; i < rotations.Length; i++)
        {
            int x = i%6,  y = i/6;
            
            if(x==0 || y==0 || x==5 || y==5) continue;
            if (x == 1 || y == 1 || x == 4 || y == 4)
            {
                if (valid(rotations.getSquare(x, y, 1, 0))) ans++;
                if (valid(rotations.getSquare(x, y, 1, 1))) ans++;
                continue;
            }
            if (rotations.getSquare(x, y, 1, 0).valid()) ans++;
            if (rotations.getSquare(x, y, 1, 1).valid()) ans++;
            if (rotations.getSquare(x, y, 1, 2).valid()) ans++;
            if (rotations.getSquare(x, y, 2, 0).valid()) ans++;
            if (rotations.getSquare(x, y, 2, 1).valid()) ans++;
            if (rotations.getSquare(x, y, 2, 2).valid()) ans++;
        }

        return ans;
    }

    public static int[] getSquare(this string[] rotations)
    {
        int x = -1, y = -1;
        int dx = -1, dy = -1;
        for (int i = 0; i < rotations.Length; i++)
        {
            x = i % 6;  y = i/6;
            
            if(x==0 || y==0 || x==5 || y==5) continue;
            if (x == 1 || y == 1 || x == 4 || y == 4)
            {
                if (valid(rotations.getSquare(x, y, 1, 0))) { dx = 1; dy = 0; break; }
                if (valid(rotations.getSquare(x, y, 1, 1))) { dx = 1; dy = 1; break; }
                continue;
            }
            if (rotations.getSquare(x, y, 1, 0).valid()) { dx = 1; dy = 0; break; }
            if (rotations.getSquare(x, y, 1, 1).valid()) { dx = 1; dy = 1; break; }
            if (rotations.getSquare(x, y, 1, 2).valid()) { dx = 1; dy = 2; break; }
            if (rotations.getSquare(x, y, 2, 0).valid()) { dx = 2; dy = 0; break; }
            if (rotations.getSquare(x, y, 2, 1).valid()) { dx = 2; dy = 1; break; }
            if (rotations.getSquare(x, y, 2, 2).valid()) { dx = 2; dy = 2; break; }
        }
        if (dx==-1 || dy==-1) return new[] { -1, -1, -1, -1, -1};
        var ans =  new[]
        {
            (x) + (y) * 6,
            (x + dx) + (y + dy) * 6,
            (x - dy) + (y + dx) * 6,
            (x - dx) + (y - dy) * 6,
            (x + dy) + (y - dx) * 6,
        };
        var minIndex = ans.IndexOf(a => a == ans.Min(b => b));
        return ans.Take(1).Union(ans.Skip(minIndex)).Union(ans.Skip(1).Take(minIndex-1)).ToArray();
    }
}