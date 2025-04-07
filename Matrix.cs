namespace TerminalRacer;

internal class Matrix
{
    private const double FourthVectorComponent = 1.0;
    private const int ColumnCount = 4;
    private const int RowCount = 3;
    
    private readonly double[,] _data  = new double[ColumnCount, RowCount];

    public Matrix()
    {
        SetRow(0, [1,0,0,0]);
        SetRow(1, [0,1,0,0]);
        SetRow(2, [0,0,1,0]);
    }
    
    internal void Set(int col, int row, double value)
    {
        _data[col, row] = value;
    }

    internal void SetRow(int row, double[] values)
    {
        for (int col = 0; col < ColumnCount; col++)
        {
            _data[col, row] = values[col];
        }
    }
    
    internal double Get(int col, int row)
    {
        if (row >= RowCount || col >= ColumnCount)
        {
            if (col == row)
                return 1;
            return 0;
        }
        return _data[col, row];
    }

    public static Matrix operator *(Matrix m2, Matrix m1)
    {
        Matrix m = new Matrix();
        for (int col = 0; col < ColumnCount; col++)
        {
            for (int row = 0; row < RowCount; row++)
            {
                double n = 0;
                // because we know that ColumnCount is bigger than RowCount, we use the value of ColumnCount in order to ensure that every data value is accounted for
                for (int i = 0; i < ColumnCount; i++) 
                {
                    n += m1.Get(i, row) * m2.Get(col, i);
                }
                m.Set(col, row, n);
            }
        }

        return m; 
    }
    
    // operator *(Matrix, Vector3D) and function SumRow don't work as intended if ColumnCount or RowCount are changed

    public static Vector3D operator *(Vector3D p, Matrix m)
    {
        return m * p;
    }
    
    public static Vector3D operator *(Matrix m, Vector3D p)
    {
        double x = SumRow(0, m, p);
        double y = SumRow(1, m, p);
        double z = SumRow(2, m, p);
        return new Vector3D(x, y, z);
    }

    private static double SumRow(int row, Matrix m, Vector3D p)
    {
        return p.X * m.Get(0, row) + p.Y * m.Get(1, row) + p.Z * m.Get(2, row) + FourthVectorComponent * m.Get(3, row);
    }
}
