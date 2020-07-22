using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FFT
{

    public struct COMPLEX {
        public double real, imag;
        public COMPLEX(double x, double y) {
            real = x;
            imag = y;
        }
        public float Magnitude() {
            return ((float)Math.Sqrt(real * real + imag * imag));
        }
    }

    public int[,] GreyImage;         
    public float[,] FourierMagnitude;
    public int[,] FFTNormalized;

    public float[,] FFTLog;


    int nx, ny;                      
    int Width, Height;
    COMPLEX[,] Fourier;
    public COMPLEX[,] FFTShifted;    
    public COMPLEX[,] Output;        
    public COMPLEX[,] FFTNormal;

    public FFT(int[,] grating) {
        GreyImage = grating;
        Width = nx = grating.GetLength(0);
        Height = ny = grating.GetLength(1);
    }

    public void ForwardFFT() {
        //Initializing Fourier Transform Array

        int i, j;
        float max;

        Fourier = new COMPLEX[Width, Height];
        Output = new COMPLEX[Width, Height];
        //Copy Image Data to the Complex Array
        for (i = 0; i <= Width - 1; i++)
            for (j = 0; j <= Height - 1; j++) {
                Fourier[i, j].real = (double)GreyImage[i, j];
                Fourier[i, j].imag = 0;
            }
        //Calling Forward Fourier Transform
        Output = FFT2D(Fourier, nx, ny, 1);

        //Frequency Shifting
        FFTShifted = new COMPLEX[nx, ny];

        for (i = 0; i <= (nx / 2) - 1; i++)
            for (j = 0; j <= (ny / 2) - 1; j++) {
                FFTShifted[i + (nx / 2), j + (ny / 2)] = Output[i, j];
                FFTShifted[i, j] = Output[i + (nx / 2), j + (ny / 2)];
                FFTShifted[i + (nx / 2), j] = Output[i, j + (ny / 2)];
                FFTShifted[i, j + (nx / 2)] = Output[i + (nx / 2), j];
            }

        //raising to log to normalise between 0-1
        FFTLog = new float[nx, ny];

        FourierMagnitude = new float[nx, ny];

        FFTNormalized = new int[nx, ny];

        for (i = 0; i <= Width - 1; i++)
            for (j = 0; j <= Height - 1; j++) {
                FourierMagnitude[i, j] = FFTShifted[i, j].Magnitude();
                FFTLog[i, j] = (float)Math.Log(1 + FourierMagnitude[i, j]);
            }

        max = FFTLog[0, 0];
        for (i = 0; i <= Width - 1; i++)
            for (j = 0; j <= Height - 1; j++) {
                if (FFTLog[i, j] > max)
                    max = FFTLog[i, j];
            }
        for (i = 0; i <= Width - 1; i++)
            for (j = 0; j <= Height - 1; j++) {
                FFTLog[i, j] = FFTLog[i, j] / max;
            }

        //normalising between desired value
        for (i = 0; i <= Width - 1; i++)
            for (j = 0; j <= Height - 1; j++) {
                FFTNormalized[i, j] = (int)(2000 * FFTLog[i, j]);
            }

        return;
    }

    public COMPLEX[,] FFT2D(COMPLEX[,] c, int nx, int ny, int dir) {
        int i, j;
        int m;//Power of 2 for current number of points
        double[] real;
        double[] imag;
        COMPLEX[,] output;//=new COMPLEX [nx,ny];
        output = c; // Copying Array
                    // Transform the Rows 
        real = new double[nx];
        imag = new double[nx];

        for (j = 0; j < ny; j++) {
            for (i = 0; i < nx; i++) {
                real[i] = c[i, j].real;
                imag[i] = c[i, j].imag;
            }
            // Calling 1D FFT Function for Rows
            m = (int)Math.Log((double)nx, 2);//Finding power of 2 for current number of points e.g. for nx=512 m=9
            FFT1D(dir, m, ref real, ref imag);

            for (i = 0; i < nx; i++) {
                //  c[i,j].real = real[i];
                //  c[i,j].imag = imag[i];
                output[i, j].real = real[i];
                output[i, j].imag = imag[i];
            }
        }
        // Transform the columns  
        real = new double[ny];
        imag = new double[ny];

        for (i = 0; i < nx; i++) {
            for (j = 0; j < ny; j++) {
                //real[j] = c[i,j].real;
                //imag[j] = c[i,j].imag;
                real[j] = output[i, j].real;
                imag[j] = output[i, j].imag;
            }
            // Calling 1D FFT Function for Columns
            m = (int)Math.Log((double)ny, 2);//Finding power of 2 for current number of points e.g. for nx=512 m=9
            FFT1D(dir, m, ref real, ref imag);
            for (j = 0; j < ny; j++) {
                //c[i,j].real = real[j];
                //c[i,j].imag = imag[j];
                output[i, j].real = real[j];
                output[i, j].imag = imag[j];
            }
        }

        // return(true);
        return (output);
    }

    private void FFT1D(int dir, int m, ref double[] x, ref double[] y) {
        long nn, i, i1, j, k, i2, l, l1, l2;
        double c1, c2, tx, ty, t1, t2, u1, u2, z;
        /* Calculate the number of points */
        nn = 1;
        for (i = 0; i < m; i++)
            nn *= 2;
        /* Do the bit reversal */
        i2 = nn >> 1;
        j = 0;
        for (i = 0; i < nn - 1; i++) {
            if (i < j) {
                tx = x[i];
                ty = y[i];
                x[i] = x[j];
                y[i] = y[j];
                x[j] = tx;
                y[j] = ty;
            }
            k = i2;
            while (k <= j) {
                j -= k;
                k >>= 1;
            }
            j += k;
        }
        /* Compute the FFT */
        c1 = -1.0;
        c2 = 0.0;
        l2 = 1;
        for (l = 0; l < m; l++) {
            l1 = l2;
            l2 <<= 1;
            u1 = 1.0;
            u2 = 0.0;
            for (j = 0; j < l1; j++) {
                for (i = j; i < nn; i += l2) {
                    i1 = i + l1;
                    t1 = u1 * x[i1] - u2 * y[i1];
                    t2 = u1 * y[i1] + u2 * x[i1];
                    x[i1] = x[i] - t1;
                    y[i1] = y[i] - t2;
                    x[i] += t1;
                    y[i] += t2;
                }
                z = u1 * c1 - u2 * c2;
                u2 = u1 * c2 + u2 * c1;
                u1 = z;
            }
            c2 = Math.Sqrt((1.0 - c1) / 2.0);
            if (dir == 1)
                c2 = -c2;
            c1 = Math.Sqrt((1.0 + c1) / 2.0);
        }
        /* Scaling for forward transform */
        if (dir == 1) {
            for (i = 0; i < nn; i++) {
                x[i] /= (double)nn;
                y[i] /= (double)nn;

            }
        }



        //  return(true) ;
        return;
    }


}
