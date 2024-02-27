using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace TrackPSO
{
    class GenerateTrack
    {
        static int noPoints = 18;
        static int angle = 20; // the angle come from divid 360/noPoints 
        int j = 300;
        int Rmin = 150;
        int Rmax = 350;
        int Jsteps = 10;
        int rang = 25;

        float[] radial = new float[noPoints + 1];
        float[] theta = new float[noPoints + 1];
        PointF[] trackPoints = new PointF[noPoints + 1];


        public GenerateTrack()
        {

        }


        public void GenerateNewTrack(int TNo, string path)
        {
            System.Random r = new System.Random();

            for (int i = 0; i < noPoints; ++i)
            {
                // Assign rand distance away from the origin between 100 and 250 pixels
                radial[i] = (float)r.Next(Rmin, Rmax);

                theta[i] = (float)System.Math.PI / 180 * i * angle;

                // Convert the polar coordinates to cartesian and

                trackPoints[i] = new PointF(radial[i] * (float)System.Math.Cos(theta[i]) + j, (int)radial[i] * (float)System.Math.Sin(theta[i]) + 400);

                PointF p2 = PointF.Add(trackPoints[i], new Size(10, 0));

                if (i < noPoints / 2)
                    j = j + Jsteps;
                else j = j - Jsteps;
            }



            trackPoints[noPoints] = trackPoints[0];
            // Show thr track using splines

            GetNewTrack(TNo, path);
        }
        void GetNewTrack(int TrackNo, string path)
        {
            StreamWriter BizFile = new StreamWriter(Path.Combine(path, "TrackBz" + TrackNo + ".txt"));
            System.Random r = new System.Random();
            System.Random r1 = new System.Random();


            for (int i = 0; i < trackPoints.Length - 1; i++)
            {

                float X1 = trackPoints[i].X;
                float X2 = trackPoints[i + 1].X;
                int max = (int)X1 + rang;
                int min = (int)X2 - rang;

                if (X2 > X1)
                {
                    max = (int)X2 + rang;
                    min = (int)X1 - rang;
                }

                float cX1 = r.Next(min, max) + (float)r1.NextDouble();
                float cX2 = r.Next(min, max) + (float)r1.NextDouble();
                float Y1 = trackPoints[i].Y;
                float Y2 = trackPoints[i + 1].Y;
                max = (int)Y1 + rang;
                min = (int)Y2 - rang;
                if (Y2 > Y1)
                {
                    max = (int)Y2 + rang;
                    min = (int)Y1 - rang;
                }
                float cY1 = r.Next(min, max) + (float)r1.NextDouble();
                float cY2 = r.Next(min, max) + (float)r1.NextDouble();
                PointF C1 = new PointF(cX1, cY1);
                PointF C2 = new PointF(cX2, cY2);
                float w = getWieght(trackPoints[i], C1, C2, trackPoints[i + 1]);


                BizFile.WriteLine(trackPoints[i].X + "," + trackPoints[i].Y + "," + C1.X + "," + C1.Y + "," + C2.X + "," + C2.Y
                                  + "," + trackPoints[i + 1].X + "," + trackPoints[i + 1].Y + "," + w);


            }



            BizFile.Close();

        }


        float getWieght(PointF P1, PointF P2, PointF P3, PointF P4)
        {
            float a1 = P1.X - P2.X;
            float b1 = P1.Y - P2.Y;
            float side1 = (float)Math.Sqrt(a1 * a1 + b1 * b1);

            float a2 = P4.X - P2.X;
            float b2 = P4.Y - P2.Y;
            float side2 = (float)Math.Sqrt(a2 * a2 + b2 * b2);
            float angl1 = Math.Abs(side1 - side2);

            float a3 = P1.X - P3.X;
            float b3 = P1.Y - P3.Y;
            float side3 = (float)Math.Sqrt(a3 * a3 + b3 * b3);

            float a4 = P4.X - P3.X;
            float b4 = P4.Y - P3.Y;
            float side4 = (float)Math.Sqrt(a4 * a4 + b4 * b4);
            float angl2 = Math.Abs(side3 - side4);
            return (angl1 + angl2);
        }
        public void drawTrack(Particle CTrack, Pen pen, PaintEventArgs e1)
        {

            Graphics g = e1.Graphics;
            for (int i = 0; i < noPoints - 1; i++)
                g.DrawBezier(pen, CTrack.Parti[i], CTrack.PartiC1[i], CTrack.PartiC2[i], CTrack.Parti[i + 1]);

        }

        



        }

    }
