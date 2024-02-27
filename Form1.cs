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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static int noPoints = 18;// number of segemnts to each track
        static int PubSize = 600;
        static int Maxloop = 101;// the number of iterartion to search for solution 
        static int MaxIexp = 1; // the highest number of experments 
        double pi = Math.PI;
        int rang = 25;
        int row = 1;
        int col = 3;
        static System.Random r = new System.Random();
        public struct velolcity
        {
           public float Inertia;
           public  int C1; // explotation 
           public  int C2; // exploration
        }

        public static string getPath1 = @"H:\16-11-PHD\PhDprojects\dataPSO\";
        
        public static string getPath3 = @"H:\16-11-PHD\PhDprojects\Testfile\";
        public static int CurrentCurve = Directory.GetFiles(getPath1).Length;
        ExcelFile excelTest = new ExcelFile(@"H:\16-11-PHD\PhDprojects\Testfile\PSOresult.xlsx", 1);
        // StreamWriter testFile = new StreamWriter(Path.Combine(getPath3, "testfilePSO.txt"));
        List<Particle> ParticlesPub = new List<Particle>(PubSize);
        List<Particle> BestPosition= new List<Particle>(PubSize);
        Particle PartiTrack = new Particle();
        Particle Gbest= new Particle();
        GenerateTrack trackCurve = new GenerateTrack();
        velolcity[] VelocityCompunt = new velolcity[PubSize];
        int[] reveiwPositionGlobal = new int[noPoints - 1];
        int[] reveiwPositionLocal = new int[noPoints - 1];




        private void Form1_Load(object sender, EventArgs e)
        {
            //excelTest.excelclear();
            for (int i = 0; i < PubSize; i++)
                trackCurve.GenerateNewTrack(i, getPath1);
            for (int i = 0; i < MaxIexp; i++)
            {
                DoPSO();
               // row = 1;
              //  col = col + 5;
            }

            excelTest.ExcelSave();
            excelTest.excelClose();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            GenerateTrack track1 = new GenerateTrack();
            Pen pen1 = new Pen(Brushes.Red, 4);
            track1.drawTrack(Gbest, pen1, e);





        }


        void DoPSO()
        {
            int iteration = 0;

            Particle PartiTrack = new Particle();
            
            float gbest = 0;
            System.Diagnostics.Debug.WriteLine("start+++++++++++++++++5+5");
            for (int i = 0; i < PubSize; i++)
            {
                GetTracks(i, PartiTrack);
                ParticlesPub.Insert(i, new Particle());
                ParticlesPub[i].CopyParticale(i, PartiTrack);
                

                BestPosition.Insert(i, new Particle());
                BestPosition[i].CopyParticale(i, PartiTrack);
              //  System.Diagnostics.Debug.WriteLine(" Initioal pupbulation is "+ VelocityCompunt[i].C1+" , "+VelocityCompunt[i].C2) ;

            }
            int fitTrack = getFitted(BestPosition);
            Gbest.CopyParticale(fitTrack, BestPosition[fitTrack]);
            float gb = Gbest.getFitness();
            System.Diagnostics.Debug.WriteLine(iteration + "( first is  " + fitTrack + ")" + "," + gbest);
            row++;
            excelTest.writeXcelsheet(row, col, iteration.ToString());

            excelTest.writeXcelsheet(row, col + 1, gb.ToString());
            do
            {
                UpdateInertiaWeight(gb, BestPosition, ref VelocityCompunt);// 
                ParticlesPub.Clear();
                for (int i = 0; i < PubSize; i++)
                {
                    
                    ParticlesPub.Insert(i, new Particle());
                    ParticlesPub[i].CopyParticale(i, BestPosition[i]);
                   // for (int j=0; j< noPoints; j++)
                   // System.Diagnostics.Debug.WriteLine("the iteration is "+ iteration    +", the point of  " + i + " is  " + ParticlesPub[i].Parti[j]);

                }
                BestPosition.Clear();
                // update positions 
                UpdatePositions(Gbest, BestPosition, ParticlesPub, fitTrack, VelocityCompunt);


                fitTrack = getFitted(BestPosition);
                Gbest.CopyParticale(fitTrack, BestPosition[fitTrack]);
                 gb = Gbest.getFitness();
                
                
                
                System.Diagnostics.Debug.WriteLine(iteration + "(" + fitTrack + ")" + "," + gb);


                
             
                row++;
                excelTest.writeXcelsheet(row, col, iteration.ToString());
               
                excelTest.writeXcelsheet(row, col + 1, gb.ToString());

                iteration++;
            } while (iteration < Maxloop);
           

        }
        void GetTracks(int index, Particle X)
        {

            string[] lines = new string[noPoints];

            StreamReader readTrack = new(getPath1 + "TrackBz" + index + ".txt");

            int i = 0;
            while (!readTrack.EndOfStream)
            {
                lines[i] = readTrack.ReadLine();
                System.Console.WriteLine(lines[i]);
                i++;
            }
            readTrack.Close();
            X.Track = index;
            for (int j = 0; j < i; j++)
            {

                X.Parti[j] = ConPoint(lines[j]); // funcation call to return the the point of the segment
                X.PartiC1[j] = ConC1(lines[j]);  // return the second point in the line which is the first control in the segment 
                X.PartiC2[j] = ConC2(lines[j]); // return the third point in the line of read file which represent the second control in the segment 
                X.PartiW[j] = ConW(lines[j]);// return the list value in line which is the weight 

            }
            X.Parti[noPoints - 1] = X.Parti[0];

            readTrack.Close();
        }

        PointF ConPoint(string rowfile)
        {
            string[] sArray = rowfile.Split(',');
            PointF result = new PointF(float.Parse(sArray[0]), float.Parse(sArray[1]));
            return result;
        }
        PointF ConC1(string rowfile)
        {
            string[] sArray = rowfile.Split(',');
            PointF result = new PointF(float.Parse(sArray[2]), float.Parse(sArray[3]));
            return result;
        }
        PointF ConC2(string rowfile)
        {
            string[] sArray = rowfile.Split(',');
            PointF result = new PointF(float.Parse(sArray[4]), float.Parse(sArray[5]));
            return result;
        }
        float ConW(string rowfile)
        {
            string[] sArray = rowfile.Split(',');
            float result = float.Parse(sArray[8]);
            return result;
        }



        public float getWieght(PointF P1, PointF P2, PointF P3, PointF P4)
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
            //System.Diagnostics.Debug.WriteLine(" after ** " + side1 +" , "+ side2 + " , " + side3 + " , " + side4);
            return (angl1 + angl2);
        }
        int getFitted(List<Particle> X) // return the best track in the new position

        {

            float fittedValue = float.MaxValue;

            int fitted = 0;
            for (int i = 0; i < X.Count(); i++)
            {
                if (X[i].getFitness() < fittedValue)
                {
                    fittedValue = X[i].getFitness();
                    fitted = i;
                }

            }
            return fitted;
        }

        void UpdateInertiaWeight(float gb1, List<Particle> BestPosition, ref velolcity[] VelocityCompunt)
        {
            float best;
            float[] dest = new float[PubSize];
            for (int j = 0; j < PubSize; j++)
            {
                
                best= BestPosition[j].getFitness();
               
                dest[j] = best - gb1;
                

            }
            float s=dest.Max();
            for (int j = 0; j < PubSize; j++)
            {
                VelocityCompunt[j].Inertia = dest[j] / s;
                if (VelocityCompunt[j].Inertia < 0.3)
                {
                    VelocityCompunt[j].C1 = 2;// local 
                    VelocityCompunt[j].C2 = 3;// global
                }
                else
                {
                    VelocityCompunt[j].C1 = 2;
                    VelocityCompunt[j].C2 = 4;
                }
               
            }

        }
        void UpdatePositions(Particle Gbest1,  List<Particle> BestPosition, List<Particle> ParticlesPub, int Fitness, velolcity[] V)
        {
            
            reviewpositions(ref reveiwPositionGlobal, Gbest1);
            for (int j = 0; j < PubSize; j++)
            {
                int Xc1 = V[j].C1;
                int Xc2 = V[j].C2;
                if (V[j].Inertia < 0.6)
                {
                    // System.Diagnostics.Debug.WriteLine(" C1 is " + Xc1 + " C2 is " + Xc2);
                    BestPosition.Insert(j, new Particle());
                    BestPosition[j].CopyParticale(j, ParticlesPub[j]);
                    //  System.Diagnostics.Debug.WriteLine(" before "+ BestPosition[j].getFitness()+" and Gbest is "+Gbest.getFitness());
                    if (j != Fitness)
                    {
                        reviewpositions(ref reveiwPositionLocal, BestPosition[j]);
                        int k1 = noPoints - 2;
                        int localIndex;
                        int globalIndex;
                        for (int K = 0; K < Xc1; K++)
                        {
                            localIndex = reveiwPositionLocal[k1];
                            globalIndex = reveiwPositionGlobal[K];
                            // GetNewWhight(BestPosition[j], Gbest, localIndex, globalIndex);
                            PointF NewC1 = new PointF();
                            PointF NewC2 = new PointF();

                            PointF P0 = Gbest1.Parti[globalIndex];
                            PointF P1 = Gbest1.Parti[globalIndex + 1];
                            PointF C1 = Gbest1.PartiC1[globalIndex];
                            PointF C2 = Gbest1.PartiC2[globalIndex];
                            PointF OldP0 = BestPosition[j].Parti[localIndex];
                            // calculte new C1;
                            float a1 = P0.X - C1.X;
                            float b1 = P0.Y - C1.Y;

                            NewC1.X = OldP0.X + a1;
                            NewC1.Y = OldP0.Y + b1;

                            BestPosition[j].PartiC1[localIndex] = NewC1;

                            // calculte new C2;
                            a1 = P0.X - C2.X;
                            b1 = P0.Y - C2.Y;


                            NewC2.X = OldP0.X + a1;
                            NewC2.Y = OldP0.Y + b1;


                            BestPosition[j].PartiC2[localIndex] = NewC2;


                            float f2 = getWieght(OldP0, NewC1, NewC2, BestPosition[j].Parti[localIndex + 1]);

                            BestPosition[j].PartiW[localIndex] = f2;
                            k1--;


                        }
                        int Xc3 = noPoints - (Xc1 + Xc2);
                        for (int z = 0; z < Xc3-2; z++)
                        {
                          
                            localIndex = reveiwPositionLocal[k1];
                            float FW = float.MaxValue;
                            PointF firstPosition = BestPosition[j].Parti[localIndex];
                            PointF secondPosition = BestPosition[j].Parti[localIndex + 1];
                            int Flag = 0;
                            while ((FW > BestPosition[j].PartiW[localIndex]) && Flag < 3 )
                            {
                                int max = (int)firstPosition.X + rang;
                                int min = (int)secondPosition.X - rang;

                                if (secondPosition.X > firstPosition.X)
                                {
                                    min = (int)firstPosition.X - rang;
                                    max = (int)secondPosition.X + rang;
                                }

                                float cX1 = r.Next(min, max) + (float)r.NextDouble();
                                float cX2 = r.Next(min, max) + (float)r.NextDouble();

                                max = (int)firstPosition.Y + rang;
                                min = (int)secondPosition.Y - rang;

                                if (secondPosition.Y > firstPosition.Y)
                                {
                                    min = (int)firstPosition.Y - rang;
                                    max = (int)secondPosition.Y + rang;
                                }

                                float cY1 = r.Next(min, max) + (float)r.NextDouble();
                                float cY2 = r.Next(min, max) + (float)r.NextDouble();

                                PointF Dc1 = new PointF(cX1, cY1);
                                PointF Dc2 = new PointF(cX2, cY2);
                                FW = getWieght(firstPosition, Dc1, Dc2, secondPosition);
                                if (FW < BestPosition[j].PartiW[localIndex])
                                {
                                    BestPosition[j].PartiC1[localIndex] = Dc1;
                                    BestPosition[j].PartiC2[localIndex] = Dc2;
                                    BestPosition[j].PartiW[localIndex] = FW;
                                }
                                Flag++;
                            }
                            // System.Diagnostics.Debug.WriteLine(" the whighet  is   " + BestPosition[j].PartiW[k2]);
                            //  System.Diagnostics.Debug.WriteLine("first is " + firstPosition + " second is " + secondPosition + " control are  " + Dc1 + " ," + Dc2 + " the wieght is " + FW);

                            k1--;

                        }
                    }
                    // System.Diagnostics.Debug.WriteLine(" after ** " + BestPosition[j].getFitness());
                }
                else
                {
                    float oldPositionFitness = ParticlesPub[j].getFitness();
                    int Flag = 0;
                    float newPosition = float.MaxValue;
                    while ((oldPositionFitness < newPosition) && (Flag < 3))
                    { 
                        BestPosition.Insert(j, new Particle());
                        BestPosition[j].GenerateNewPositions(j);
                        newPosition = BestPosition[j].getFitness();
                        Flag++;
                    }
                }
                // end update position 
                //System.Diagnostics.Debug.WriteLine(" position of 0 " + BestPosition[0].getFitness());
            }
            
        }

        void reviewpositions(ref int[] review, Particle xParti)
        {
            review = new int[noPoints-1];
            float[] Wreview = new float[noPoints-1];
            for (int i = 0; i < noPoints-1; i++)
            {
                review[i] = i;
                Wreview[i] = xParti.PartiW[i];
            }
            var itemMoved = false;
            do
            {
                itemMoved = false;
                for (int i = 0; i < noPoints-2; i++)
                {
                    if (Wreview[i]>Wreview[i+1])
                    {
                        float lowerValu = Wreview[i + 1];
                        Wreview[i + 1] = Wreview[i];
                        Wreview[i] = lowerValu;
                        int index = review[i + 1];
                        review[i + 1] = review[i];
                        review[i] = index;
                        itemMoved = true;
                    }
                    
                }

            } while (itemMoved);
           
        }

        void GetNewWhight( Particle BestPosition1, Particle Gbest, int L, int G)
        {

           
            PointF NewC1 = new PointF();
            PointF NewC2 = new PointF();
         
            

            PointF P0 = Gbest.Parti[G];
            PointF P1 = Gbest.Parti[G + 1];
            PointF C1 = Gbest.PartiC1[G];
            PointF C2 = Gbest.PartiC2[G];
            PointF OldP0 = BestPosition1.Parti[L];
            // calculte new C1;
            float a1 = P0.X - C1.X;
            float b1 = P0.Y - C1.Y;
           /* float side1 = MathF.Sqrt((a1 * a1) + (b1 * b1));
            float a2 = P0.X - P1.X;
            float b2 = P0.Y - P1.Y;
            float side2 = MathF.Sqrt((a2 * a2) + (b2 * b2));
            float a3 = P1.X - C1.X;
            float b3 = P1.Y - C1.Y;
            float side3 = MathF.Sqrt((a3 * a3) + (b3 * b3));
            float prod1 = ((side1 * side1) + (side2 * side2) - (side3 * side3)) / (2 * side1 * side2);
            float alpha = MathF.Acos(prod1);
            System.Diagnostics.Debug.WriteLine(side1 + " Sides of C1 " + side2 + " ," + side3);*/
            NewC1.X = OldP0.X + a1;
            NewC1.Y = OldP0.Y + b1;
             
            BestPosition1.PartiC1[L] = NewC1;

            // calculte new C2;
            a1 = P0.X - C2.X;
            b1 = P0.Y - C2.Y;
           

            NewC2.X = OldP0.X + a1;
            NewC2.Y = OldP0.Y + b1;
           

            BestPosition1.PartiC2[L] = NewC2;

            float f1 = getWieght(P0, C1, C2, P1);
            float f2 = getWieght(OldP0, NewC1, NewC2, BestPosition1.Parti[L + 1]);
           // System.Diagnostics.Debug.WriteLine(OldP0+" , "+NewC1+ " , " + NewC2 + " , " + BestPosition1.Parti[L+1]);
           // System.Diagnostics.Debug.WriteLine(P0 + " , " + C1 + " , " + C2 + " , " + P1 + " are the orignal " );
           // System.Diagnostics.Debug.WriteLine(f1 + " the Wieght " + f2 );
            BestPosition1.PartiW[L] = f2;
        }



    }

   




}

