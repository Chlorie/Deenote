using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline
{
    private int n = 0;
    private double[] a; // size = n + 1
    private double[] b; // size = n
    private double[] c; // size = n + 1
    private double[] d; // size = n
    private double[] x; // size = n + 1
    public int Size { get { return n; } }
    public float Min { get { return (float)x[0]; } }
    public float Max { get { return (float)x[n]; } }
    public Spline(List<float> xl, List<float> yl, bool linear = false)
    {
        n = xl.Count - 1;
        // Linear for linear spline, not-linear for natural cubic spline interpolation
        if (linear) Linear(xl, yl); else Cubic(xl, yl);
    }
    private void Linear(List<float> xl, List<float> yl) // Linear interpolation
    {
        x = new double[n + 1]; a = new double[n + 1]; c = new double[n + 1];
        b = new double[n]; d = new double[n];
        for (int i = 0; i <= n; i++)
        {
            x[i] = xl[i];
            a[i] = yl[i];
            c[i] = 0;
            if (i < n)
            {
                b[i] = (yl[i + 1] - yl[i]) / (xl[i + 1] - xl[i]);
                d[i] = 0;
            }
        }
    }
    private void Cubic(List<float> xl, List<float> yl) // Natural cubic spline interpolation
    {
        x = new double[n + 1]; a = new double[n + 1]; c = new double[n + 1];
        b = new double[n]; d = new double[n];
        double[] alpha = new double[n];
        double[] h = new double[n];
        double[] mu = new double[n + 1];
        double[] l = new double[n + 1];
        double[] z = new double[n + 1];
        for (int i = 0; i <= n; i++) { x[i] = xl[i]; a[i] = yl[i]; if (i < n) h[i] = xl[i + 1] - xl[i]; }
        for (int i = 1; i < n; i++) { alpha[i] = 3 * (a[i + 1] - a[i]) / h[i] - 3 * (a[i] - a[i - 1]) / h[i - 1]; }
        l[0] = 1; mu[0] = z[0] = 0;
        for (int i = 1; i < n; i++)
        {
            l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
            mu[i] = h[i] / l[i];
            z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }
        l[n] = 1; z[n] = c[n] = 0;
        for (int i = n - 1; i >= 0; i--)
        {
            c[i] = z[i] - mu[i] * c[i + 1];
            b[i] = (a[i + 1] - a[i]) / h[i] - h[i] * (c[i + 1] + 2 * c[i]) / 3;
            d[i] = (c[i + 1] - c[i]) / (3 * h[i]);
        }
    }
    public float Value(float p)
    {
        if (p < x[0] || p > x[n]) return 0.0f;
        double result = 0.0;
        for (int i = 0; i < n; i++)
            if (p < x[i + 1]) // p is between x_i and x_(i+1)
            {
                double diff = p - x[i];
                result += a[i];
                result += b[i] * diff;
                result += c[i] * diff * diff;
                result += d[i] * diff * diff * diff;
                break;
            }
        return (float)result;
    }
}
