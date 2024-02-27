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
    class Particle
    {
        static int noPoints = 18;
        public int NoG = noPoints;
        private int TrackNo;
        int rang = 25;
        public float maxValue = 0;
        public PointF[] Parti = new PointF[noPoints];
        public PointF[] PartiC1 = new PointF[noPoints];
        public PointF[] PartiC2 = new PointF[noPoints];
        public float[] PartiW = new float[noPoints];
        static int angle = 20; // the angle come from divid 360/noPoints 
        int j = 300;
        int Rmin = 150;
        int Rmax = 350;
        int Jsteps = 10;


        float[] radial = new float[noPoints + 1];
        float[] theta = new float[noPoints + 1];
        public Particle()
        {

        }
        public int Track
        {
            get { return TrackNo; }
            set { TrackNo = value; }
        }



        public void getNewWeight(int SegNo)
        {
            System.Random r = new System.Random();
            System.Random r1 = new System.Random();
            int SegNo1 = SegNo + 1;
            if (SegNo1 > noPoints - 1)
                SegNo1 = 0;

            float X1 = this.Parti[SegNo].X;
            float X2 = this.Parti[SegNo1].X;
            int max = (int)X1 + rang;
            int min = (int)X2 - rang;

            if (X2 > X1)
            {
                max = (int)X2 + rang;
                min = (int)X1 - rang;
            }

            float cX1 = r.Next(min, max) + (float)r1.NextDouble();
            float cX2 = r.Next(min, max) + (float)r1.NextDouble();
            float Y1 = this.Parti[SegNo].Y;
            float Y2 = this.Parti[SegNo1].Y;
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
            float w = getWieght(Parti[SegNo], C1, C2, Parti[SegNo1]);
            this.PartiC1[SegNo] = C1;
            this.PartiC2[SegNo] = C2;
            this.PartiW[SegNo] = w;

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

        public float getFitness()
        {
            float t = 0;
            for (int i = 0; i < PartiW.Length; i++)
            {
                t = t + PartiW[i];

            }
            return t;
        }
        public float getMaxSegmentWeight()
        {
            this.maxValue = PartiW.Max();
            return PartiW.Max();
        }

        public int indexMax(float maxValue)
        {
            return Array.IndexOf(PartiW, maxValue);
        }

        public float getMinSegmentWeight()
        {
            this.maxValue = PartiW.Min();
            return PartiW.Min();
        }

        public void CopyParticale(int index, Particle Xparticle)
        {

            this.Track = index;
            for (int i = 0; i < NoG; i++)
            {
                this.Parti[i] = Xparticle.Parti[i];
                this.PartiC1[i] = Xparticle.PartiC1[i];
                this.PartiC2[i] = Xparticle.PartiC2[i];
                this.PartiW[i] = Xparticle.PartiW[i];
            }
        }
        public void saveTrack(string path)
        {
            StreamWriter BizFile = new StreamWriter(Path.Combine(path, "TrackBz" + this.Track + ".txt"));
            for (int i = 0; i < this.NoG - 1; i++)
                BizFile.WriteLine(Parti[i].X + "," + Parti[i].Y + "," + PartiC1[i].X + "," + PartiC1[i].Y + ","
                    + PartiC2[i].X + "," + PartiC2[i].Y + "," + Parti[i + 1].X + "," + Parti[i + 1].Y + "," + PartiW[i]);
            BizFile.Close();
        }

        public void GenerateNewPositions(int TNo)
        {
            System.Random r = new System.Random();

            for (int i = 0; i < noPoints; ++i)
            {
                // Assign rand distance away from the origin between 100 and 250 pixels
                radial[i] = (float)r.Next(Rmin, Rmax);

                theta[i] = (float)System.Math.PI / 180 * i * angle;

                // Convert the polar coordinates to cartesian and

                Parti[i] = new PointF(radial[i] * (float)System.Math.Cos(theta[i]) + j, (int)radial[i] * (float)System.Math.Sin(theta[i]) + 400);



                if (i < noPoints / 2)
                    j = j + Jsteps;
                else j = j - Jsteps;
            }



            Parti[noPoints-1] = Parti[0];
            // Show thr track using splines

            GetNew(TNo);
        }

        void GetNew(int TrackNo)
        {

            System.Random r = new System.Random();
            System.Random r1 = new System.Random();


            for (int i = 0; i < noPoints - 1; i++)
            {

                float X1 = Parti[i].X;
                float X2 = Parti[i + 1].X;
                int max = (int)X1 + rang;
                int min = (int)X2 - rang;

                if (X2 > X1)
                {
                    max = (int)X2 + rang;
                    min = (int)X1 - rang;
                }

                float cX1 = r.Next(min, max) + (float)r1.NextDouble();
                float cX2 = r.Next(min, max) + (float)r1.NextDouble();
                float Y1 = Parti[i].Y;
                float Y2 = Parti[i + 1].Y;
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
                PartiW[i]= getWieght(Parti[i], C1, C2, Parti[i + 1]);
                PartiC1[i] = C1;
                PartiC2[i] = C2;
            }

        }
    }
}