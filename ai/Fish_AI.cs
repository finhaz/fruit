using System;
using System.Linq;

namespace fruit
{
	public class Fish_Solution
	{

        //PSO所需变量
        public float[,] u_dsp = new float[2, 10];
        public float[] u_g = new float[4];
        public bool pso_init = false;//初始化完成标志

        public double w;
        public double Icbd1;
        public double Icbq1;
        public double Icbd2;
        public double Icbq2;
        public double Udp1;
        public double Uqp1;
        public double Udp2;
        public double Uqp2;
        public double Uodp1;
        public double Uoqp1;
        public double Uodp2;
        public double Uoqp2;

        public double Vd1 = 0;
        public double Vq1 = 0;
        public double Vd2 = 0;
        public double Vq2 = 0;
        /*
        double[] fitnessgbest = new double[20];          //个体粒子中达到最小值时的适应度值
        double[,] gbest = new double[20, 4];                 //个体粒子达到极值时对应的最优解位置
        double fitnesszbest = 0;          //群体粒子达到极值时的适应度值
        double[] zbest = new double[4];                 //群体粒子达到极值时对应解的位置
        double[] fitness = new double[20];  //矩阵，i个粒子的适应度值                      
        double[,] pop = new double[20, 4];//矩阵，用来存放粒子的位置，粒子的解
        double[,] V = new double[20, 4];  //矩阵，用来存在粒子的速度

        double[] u = new double[5];//代表输入参数
        */

        Random ran = new Random();


        //鱼群所需变量
        int fishnum = 100;//生成50条人工鱼
        int MAXGEN = 100;//最大迭代次数
        int try_number = 100;//最大试探次数
        int visual = 5;//感知距离2
        double delta = 0.618;//拥挤度因子
        double step = 1;//移动步长0.5
        //
        int[] lb_ub = new int[3] { -30, 30, 2 };

        double U_fu1 = 0;
        double Uo_fu1 = 0;
        double U_fu2 = 0;


        public Fish_Solution()
		{

		}

		public void cale_fish()
		{
            //接收的数据存入u_dsp中
            //两台逆变器dq坐标系下的负序电流输入以及频率                    
            double w = u_dsp[0, 2];

            Icbd1 = u_dsp[0, 0];
            Icbq1 = u_dsp[0, 1];
            Icbd2 = u_dsp[1, 0];
            Icbq2 = u_dsp[1, 1];
            Udp1 = u_dsp[0, 3];
            Uqp1 = u_dsp[0, 4];
            Udp2 = u_dsp[1, 3];
            Uqp2 = u_dsp[1, 4];
            Uodp1 = u_dsp[0, 5];
            Uoqp1 = u_dsp[0, 6];
            //Uodp2 = u_dsp[1, 5];
            //Uoqp2 = u_dsp[1, 6];
            double VUFout1 = u_dsp[0, 7];
            double VUFpcc = u_dsp[0, 8];
            double VUFout2 = u_dsp[1, 7];
            //double VUFpcc2 = u_dsp[1, 8];

            //double U_fu1 = 0;
            //double Uo_fu1 = 0;
            //double U_fu2 = 0; 

            U_fu1 = Math.Sqrt(Udp1 * Udp1 + Uqp1 * Uqp1);
            Uo_fu1 = Math.Sqrt(Uodp1 * Uodp1 + Uoqp1 * Uoqp1);
            U_fu2 = Math.Sqrt(Udp2 * Udp2 + Uqp2 * Uqp2);

            /*double Vd1 = 0;
            double Vq1 = 0;
            double Vd2 = 0;
            double Vq2 = 0;*/
            //if (!pso_init)
            double[,] X = new double[2, 100];
            double[] Y = new double[100];
            double[,] Xi1 = new double[2, 1];
            double[] Yi1 = new double[1];
            double[,] Xi2 = new double[2, 1];
            double[] Yi2 = new double[1];
            double[] BestY = new double[MAXGEN];
            double[,] BestX = new double[2, MAXGEN];
            double x0_sum;
            double y0_sum;
            double x0;
            double y0;
            double[,] bestx = new double[2, 1];
            double Ymin;
            int gen;
            if (VUFout1 > 0.02 || VUFout2 > 0.02 || VUFpcc > 0.02)
            {
                //lb_ub[0] =-30; lb_ub[1] = 30; lb_ub[2] =2;//产生[-100, 100]的数2个                       
                /*double [,] X=new double [2,100];
                double[] Y = new double[100];
                double[,] Xi1 = new double[2, 1];
                double[] Yi1 = new double[1];
                double[,] Xi2 = new double[2, 1];
                double []Yi2 = new double[1];*/

                AF_init(fishnum, lb_ub, ref X);

                double[,] LBUB = new double[2, 2] { { -30, 30 }, { -30, 30 } };
                /*List<List<double>> LBUB = new List<List<double>>();
                List<double> item1 = new List<double>(new double[] { -30,30 });
                for (int i_lbub = 0; i_lbub < lb_ub[2]; i_lbub++)
                {
                    LBUB.Add(item1);                            
                }*/

                //double[] BestY = new double[MAXGEN];
                //double[,] BestX = new double[2,MAXGEN];
                for (int j = 0; j < MAXGEN; j++)
                {
                    BestY[j] = 1;
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < MAXGEN; j++)
                    {
                        BestX[i, j] = 1;
                    }
                }
                double besty = 100;  //最优函数值

                //AF_foodconsistence(X, Icbd1, Icbq1, Icbd2, Icbq2, w,ref Y);
                Y = AF_foodconsistence(X, Icbd1, Icbq1, Icbd2, Icbq2, w, U_fu1, Uo_fu1, U_fu2);


                for (gen = 0; gen < MAXGEN; gen++)
                {
                    for (int i = 0; i < fishnum; i++)
                    {
                        AF_swarm(X, i, visual, delta, step, try_number, LBUB, Y, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Xi1, ref Yi1); //聚群行为
                        AF_follow(X, i, visual, delta, step, try_number, LBUB, Y, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Xi2, ref Yi2); //追尾行为
                        if (Yi1[0] < Yi2[0])
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                X[j, i] = Xi1[j, 0];
                            }
                            Y[i] = Yi1[0];
                        }
                        else
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                X[j, i] = Xi2[j, 0];
                            }
                            Y[i] = Yi2[0];
                        }
                    }

                    Ymin = Y.Min();
                    int index = 0;
                    for (int i = 0; i < fishnum; i++)
                    {
                        if (Y[i] == Ymin)
                        {
                            index = i;
                        }
                    }


                    if (Ymin <= besty)
                    {
                        besty = Ymin;
                        for (int j = 0; j < 2; j++)
                        {
                            bestx[j, 0] = X[j, index];
                            //BestY[gen] = Ymin;
                            BestX[j, gen] = X[j, index];
                        }
                        BestY[gen] = Ymin;
                    }
                    else if (gen == 0)
                    {
                        besty = Ymin;
                        for (int j = 0; j < 2; j++)
                        {
                            bestx[j, 0] = X[j, index];
                            //BestY[gen] = BestY[gen];
                            BestX[j, gen] = BestX[j, gen];
                        }
                        BestY[gen] = BestY[gen];
                    }
                    else
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            //BestY[gen] = BestY[gen - 1];
                            BestX[j, gen] = BestX[j, gen - 1];
                        }
                        BestY[gen] = BestY[gen - 1];
                    }
                    //gen = gen + 1;

                    x0_sum = 0;
                    y0_sum = 0;
                    for (int j = 0; j < MAXGEN; j++)
                    {
                        x0_sum = x0_sum + BestX[0, j];
                        y0_sum = y0_sum + BestX[1, j];
                    }
                    x0 = x0_sum / MAXGEN;
                    y0 = y0_sum / MAXGEN;

                    //显示计算得到的电压参考值
                    /*Vd1 = bestx[0,0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1;
                    Vq1 = bestx[1,0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1;
                    Vd2 = bestx[0,0] + w * (0.5e-3) * Icbq2 + 0.5 * Icbd2;
                    Vq2 = bestx[1,0] - w * (0.5e-3) * Icbd2 + 0.5 * Icbq2;*/
                    //////改进鱼群
                    Vd1 = x0 + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1;
                    Vq1 = y0 - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1;
                    Vd2 = x0 + w * (0.5e-3) * Icbq2 + 0.5 * Icbd2;
                    Vq2 = y0 - w * (0.5e-3) * Icbd2 + 0.5 * Icbq2;

                }
            }

            u_g[0] = Convert.ToSingle(Vd1);
            u_g[1] = Convert.ToSingle(Vq1);
            u_g[2] = Convert.ToSingle(Vd2);
            u_g[3] = Convert.ToSingle(Vq2);

            return;
        }

        private void AF_init(double Nfish, int[] lb_ub, ref double[,] X)
        {
            //int row = 1;
            int row = lb_ub.GetLength(0);//获取第一维度
            int lb, ub, nr;
            for (int k = 0; k < row; k++)
            {
                lb = lb_ub[0];
                ub = lb_ub[1];
                nr = lb_ub[2];
                for (int j = 0; j < nr; j++)
                {
                    for (int i = 0; i < Nfish; i++)
                    {
                        X[j, i] = lb + (ub - lb) * ran.NextDouble();
                    }
                }
            }
        }

        static double[] AF_foodconsistence(double[,] X, double Icbd1, double Icbq1, double Icbd2, double Icbq2, double w, double U_fu1, double Uo_fu1, double U_fu2)
        {
            //int fishnum = 100;//这里有点问题？？？不同实参传递过来的不一样
            int fishnum = X.GetLength(1);//获取第二维度
            double[] Vcbd1 = new double[fishnum];
            double[] Vcbq1 = new double[fishnum];
            double[] Vcbd2 = new double[fishnum];
            double[] Vcbq2 = new double[fishnum];
            double[] Vs = new double[fishnum];
            double[] Vcb1 = new double[fishnum];
            double[] Vcb2 = new double[fishnum];
            double[] m1 = new double[fishnum];
            double[] m2 = new double[fishnum];
            double[] m3 = new double[fishnum];
            double[] Y = new double[fishnum];
            for (int i = 0; i < fishnum; i++)
            {
                Vcbd1[i] = X[0, i] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1;
                Vcbq1[i] = X[1, i] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1;
                Vcbd2[i] = X[0, i] + w * (0.5e-3) * Icbq2 + 0.5 * Icbd2;
                Vcbq2[i] = X[1, i] - w * (0.5e-3) * Icbd2 + 0.5 * Icbq2;

                Vs[i] = Math.Sqrt(X[0, i] * X[0, i] + X[1, i] * X[1, i]);
                Vcb1[i] = Math.Sqrt(Vcbd1[i] * Vcbd1[i] + Vcbq1[i] * Vcbq1[i]);
                Vcb2[i] = Math.Sqrt(Vcbd2[i] * Vcbd2[i] + Vcbq2[i] * Vcbq2[i]);

                if (Vs[i] <= Uo_fu1 * 0.02)
                {
                    m1[i] = 0;
                }
                else
                {
                    m1[i] = Vs[i] - Uo_fu1 * 0.02;
                }

                if (Vcb1[i] <= U_fu1 * 0.02)
                {
                    m2[i] = 0;
                }
                else
                {
                    m2[i] = Vcb1[i] - U_fu1 * 0.02;
                }

                if (Vcb2[i] <= U_fu2 * 0.02)
                {
                    m3[i] = 0;
                }
                else
                {
                    m3[i] = Vcb2[i] - U_fu2 * 0.02;
                }

                Y[i] = 5 * m1[i] + 5 * m2[i] + 5 * m3[i];
            }
            return Y;
        }

        private void AF_swarm(double[,] X, int i, int visual, double delta, double step, int try_number, double[,] LBUB, double[] lastY, double Icbd1, double Icbq1, double Icbd2, double Icbq2, double w, ref double[,] Xi1, ref double[] Yi1)
        {
            //聚群行为
            //输入：
            //X          当前人工鱼的位置
            //i          当前人工鱼的序号
            //visual     感知范围
            //step       最大移动步长
            //try_number 最大尝试次数
            //LBUB       各个数的上下限
            //lastY      上次的各人工鱼位置的食物浓度
            //输出：
            //Xnext       Xi人工鱼的下一个位置     //改成Xi1
            //Ynext       Xi人工鱼的下一个位置的食物浓度    //改成Yi1
            double[,] Xi = new double[2, 1];
            double Yi = 0;
            double[] D = new double[100];
            int nf = 0;
            int i_index = 0;
            double[,] Xc = new double[2, 1];
            double[] Yc = new double[1];//得改   可能好了  
            double[,] Xnext = new double[2, 1];
            double[] Ynext = new double[1];
            for (int j = 0; j < 2; j++)
            {
                Xi[j, 0] = X[j, i];
            }
            AF_dist(Xi, X, ref D);
            for (int k = 0; k < 100; k++)
            {
                if (D[k] > 0 && D[k] < visual)
                {
                    nf = nf + 1;
                }
            }
            int[] Index = new int[nf];
            for (int k = 0; k < 100; k++)
            {
                if (D[k] > 0 && D[k] < visual)
                {
                    Index[i_index] = k;
                    i_index = i_index + 1;
                }
            }
            if (nf > 0)
            {
                for (int j = 0; j < X.GetLength(0); j++)
                {
                    double sum_X = 0;
                    for (int m = 0; m < i_index; m++)
                    {
                        sum_X = sum_X + X[j, Index[m]];
                    }
                    Xc[j, 0] = sum_X / i_index;
                }

                Yc = AF_foodconsistence(Xc, Icbd1, Icbq1, Icbd2, Icbq2, w, U_fu1, Uo_fu1, U_fu2);//也得改   可能好了
                Yi = lastY[i];
                if (Yc[0] / nf > delta * Yi)
                {
                    double sum_mod = 0;
                    double mod = 0;
                    double mod1 = 0;
                    double mod2 = 0;
                    double mod3 = 0;
                    double mod4 = 0;
                    double mod5 = 0;
                    double mod6 = 0;
                    double sum_mod1 = 0;
                    double sum_mod2 = 0;
                    double sum_mod3 = 0;
                    double sum_mod4 = 0;
                    double sum_mod5 = 0;
                    double sum_mod6 = 0;
                    for (int j = 0; j < 2; j++)
                    {
                        sum_mod = sum_mod + (Xc[j, 0] - Xi[j, 0]) * (Xc[j, 0] - Xi[j, 0]);
                    }
                    mod = Math.Sqrt(sum_mod);

                    /*for (int k = 0; k < 2; k++)
                   {
                       Xi1[k, 0] = Xi[k, 0] + ran.NextDouble() * step * (Xc[k, 0] - Xi[k, 0]) / mod;
                   } */

                    //改进鱼群
                    sum_mod1 = sum_mod1 + (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]) * (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]);
                    sum_mod2 = sum_mod2 + (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]) * (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]);
                    sum_mod3 = sum_mod3 + (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1)) * (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1));
                    sum_mod4 = sum_mod4 + (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1)) * (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1));
                    sum_mod5 = sum_mod5 + (Xc[0, 0] - Xi[0, 0]) * (Xc[0, 0] - Xi[0, 0]);
                    sum_mod6 = sum_mod6 + (Xc[1, 0] - Xi[1, 0]) * (Xc[1, 0] - Xi[1, 0]);
                    mod1 = Math.Sqrt(sum_mod1);
                    mod2 = Math.Sqrt(sum_mod2);
                    mod3 = Math.Sqrt(sum_mod3);
                    mod4 = Math.Sqrt(sum_mod4);
                    mod5 = Math.Sqrt(sum_mod5);
                    mod6 = Math.Sqrt(sum_mod6);
                    Xi1[0, 0] = Xi[0, 0] + ran.NextDouble() * step * ((Xc[0, 0] - Xi[0, 0]) / mod5 + (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]) / mod1 + (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1)) / mod3);
                    Xi1[1, 0] = Xi[1, 0] + ran.NextDouble() * step * ((Xc[1, 0] - Xi[1, 0]) / mod6 + (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]) / mod2 + (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1)) / mod4);

                    for (int j = 0; j < Xi1.Length; j++)
                    {
                        if (Xi1[j, 0] > LBUB[j, 1])
                        {
                            Xi1[j, 0] = LBUB[j, 1];
                        }
                        if (Xi1[j, 0] < LBUB[j, 0])
                        {
                            Xi1[j, 0] = LBUB[j, 0];
                        }
                    }
                    Yi1 = AF_foodconsistence(Xi1, Icbd1, Icbq1, Icbd2, Icbq2, w, U_fu1, Uo_fu1, U_fu2);
                }
                else
                {
                    AF_prey(Xi, i, visual, step, try_number, LBUB, lastY, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Xnext, ref Ynext);
                    Xi1 = Xnext;
                    Yi1 = Ynext;
                }
            }
            else
            {
                AF_prey(Xi, i, visual, step, try_number, LBUB, lastY, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Xnext, ref Ynext);
                Xi1 = Xnext;
                Yi1 = Ynext;
            }
        }

        private void AF_follow(double[,] X, int i, int visual, double delta, double step, int try_number, double[,] LBUB, double[] lastY, double Icbd1, double Icbq1, double Icbd2, double Icbq2, double w, ref double[,] Xi2, ref double[] Yi2)
        {
            //追尾行为
            //输入：
            //X          当前人工鱼的位置
            //i          当前人工鱼的序号
            //visual     感知范围
            //step       最大移动步长
            //try_number 最大尝试次数
            //LBUB       各个数的上下限
            //lastY      上次的各人工鱼位置的食物浓度
            //输出：
            //Xnext       Xi人工鱼的下一个位置    //改成Xi2
            //Ynext       Xi人工鱼的下一个位置的食物浓度   //改成Yi2
            double[,] Xi = new double[2, 1];
            double Yi = 0;
            double[] D = new double[100];
            int nf = 0;
            int i_index = 0;
            double[,] Xnext = new double[2, 1];
            double[] Ynext = new double[1];

            for (int j = 0; j < 2; j++)
            {
                Xi[j, 0] = X[j, i];
            }
            AF_dist(Xi, X, ref D);
            for (int k = 0; k < 100; k++)
            {
                if (D[k] > 0 && D[k] < visual)
                {
                    nf = nf + 1;
                }
            }
            int[] Index = new int[nf];
            for (int k = 0; k < 100; k++)
            {
                if (D[k] > 0 && D[k] < visual)
                {
                    Index[i_index] = k;
                    i_index = i_index + 1;
                }
            }
            if (nf > 0)
            {
                double[,] XX = new double[2, i_index];
                double[] YY = new double[i_index];
                for (int j = 0; j < 2; j++)
                {
                    for (int m = 0; m < i_index; m++)
                    {
                        XX[j, m] = X[j, Index[m]];
                    }
                }
                for (int m = 0; m < i_index; m++)
                {
                    YY[m] = lastY[Index[m]];
                }

                double Ymax = YY.Max();
                int Max_index = 0;
                for (int j = 0; j < i_index; j++)
                {
                    if (YY[j] == Ymax)
                    {
                        Max_index = j;
                    }
                }
                double[,] Xmax = new double[2, 1];
                for (int j = 0; j < 2; j++)
                {
                    Xmax[j, 0] = XX[j, Max_index];
                }
                Yi = lastY[i];

                if (Ymax / nf > delta * Yi)
                {
                    double sum_mod = 0;
                    double mod = 0;
                    double mod1 = 0;
                    double mod2 = 0;
                    double mod3 = 0;
                    double mod4 = 0;
                    double mod5 = 0;
                    double mod6 = 0;
                    double sum_mod1 = 0;
                    double sum_mod2 = 0;
                    double sum_mod3 = 0;
                    double sum_mod4 = 0;
                    double sum_mod5 = 0;
                    double sum_mod6 = 0;
                    for (int j = 0; j < 2; j++)
                    {
                        sum_mod = sum_mod + (Xmax[j, 0] - Xi[j, 0]) * (Xmax[j, 0] - Xi[j, 0]);
                    }
                    mod = Math.Sqrt(sum_mod);

                    /*for (int k = 0; k < 2; k++)
                   {
                       Xi2[k, 0] = Xi[k, 0] + ran.NextDouble() * step * (Xmax[k, 0] - Xi[k, 0]) / mod;
                   }*/

                    //改进鱼群
                    sum_mod1 = sum_mod1 + (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]) * (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]);
                    sum_mod2 = sum_mod2 + (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]) * (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]);
                    sum_mod3 = sum_mod3 + (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1)) * (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1));
                    sum_mod4 = sum_mod4 + (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1)) * (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1));
                    sum_mod5 = sum_mod5 + (Xmax[0, 0] - Xi[0, 0]) * (Xmax[0, 0] - Xi[0, 0]);
                    sum_mod6 = sum_mod6 + (Xmax[1, 0] - Xi[1, 0]) * (Xmax[1, 0] - Xi[1, 0]);
                    mod1 = Math.Sqrt(sum_mod1);
                    mod2 = Math.Sqrt(sum_mod2);
                    mod3 = Math.Sqrt(sum_mod3);
                    mod4 = Math.Sqrt(sum_mod4);
                    mod5 = Math.Sqrt(sum_mod5);
                    mod6 = Math.Sqrt(sum_mod6);
                    Xi2[0, 0] = Xi[0, 0] + ran.NextDouble() * step * ((Xmax[0, 0] - Xi[0, 0]) / mod5 + (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]) / mod1 + (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1)) / mod3);
                    Xi2[1, 0] = Xi[1, 0] + ran.NextDouble() * step * ((Xmax[1, 0] - Xi[1, 0]) / mod6 + (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]) / mod2 + (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1)) / mod4);

                    for (int j = 0; j < Xi2.Length; j++)
                    {
                        if (Xi2[j, 0] > LBUB[j, 1])
                        {
                            Xi2[j, 0] = LBUB[j, 1];
                        }
                        if (Xi2[j, 0] < LBUB[j, 0])
                        {
                            Xi2[j, 0] = LBUB[j, 0];
                        }
                    }
                    Yi2 = AF_foodconsistence(Xi2, Icbd1, Icbq1, Icbd2, Icbq2, w, U_fu1, Uo_fu1, U_fu2);
                }
                else
                {
                    AF_prey(Xi, i, visual, step, try_number, LBUB, lastY, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Xnext, ref Ynext);
                    Xi2 = Xnext;
                    Yi2 = Ynext;
                }
            }
            else
            {
                AF_prey(Xi, i, visual, step, try_number, LBUB, lastY, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Xnext, ref Ynext);
                Xi2 = Xnext;
                Yi2 = Ynext;
            }
        }

        static void AF_dist(double[,] Xi, double[,] X, ref double[] D)
        {
            //计算第i条鱼与所有鱼的位置，包括本身
            //输入：
            //Xi 第i条鱼的当前位置
            //X  所有鱼的当前位置
            //输出：
            //D 第i条鱼与所有鱼的距离
            int col = X.GetLength(1);
            double sum;
            for (int j = 0; j < col; j++)
            {
                D[j] = 0;
            }
            for (int j = 0; j < col; j++)
            {
                sum = 0;
                for (int k = 0; k < 2; k++)
                {
                    sum = sum + (Xi[k, 0] - X[k, j]) * (Xi[k, 0] - X[k, j]);
                }

                D[j] = Math.Sqrt(sum);
            }
        }

        private void AF_prey(double[,] Xi, int ii, int visual, double step, int try_number, double[,] LBUB, double[] lastY, double Icbd1, double Icbq1, double Icbd2, double Icbq2, double w, ref double[,] Xnext, ref double[] Ynext)
        {
            //觅食行为
            //输入：
            //Xi         当前人工鱼的位置
            //ii         当前人工鱼的序号
            //visual     感知范围
            //step       最大移动步长
            //try_number 最大尝试次数
            //LBUB       各个数的上下限
            //lastY      上次的各人工鱼位置的食物浓度
            //输出：
            //Xnext       Xi人工鱼的下一个位置
            //Ynext       Xi人工鱼的下一个位置的食物浓度
            double[] Yi = new double[1];
            double[,] Xj = new double[2, 1];
            double[] Yj = new double[1];
            Yi[0] = lastY[ii];
            for (int i = 0; i < try_number; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Xj[j, 0] = Xi[j, 0] + (2 * ran.NextDouble() - 1) * visual;
                }
                Yj = AF_foodconsistence(Xj, Icbd1, Icbq1, Icbd2, Icbq2, w, U_fu1, Uo_fu1, U_fu2);
                if (Yi[0] > Yj[0])
                {
                    double sum_mod = 0;
                    double mod = 0;
                    double mod1 = 0;
                    double mod2 = 0;
                    double mod3 = 0;
                    double mod4 = 0;
                    double mod5 = 0;
                    double mod6 = 0;
                    double sum_mod1 = 0;
                    double sum_mod2 = 0;
                    double sum_mod3 = 0;
                    double sum_mod4 = 0;
                    double sum_mod5 = 0;
                    double sum_mod6 = 0;
                    for (int j = 0; j < 2; j++)
                    {
                        sum_mod = sum_mod + (Xj[j, 0] - Xi[j, 0]) * (Xj[j, 0] - Xi[j, 0]);
                    }
                    mod = Math.Sqrt(sum_mod);

                    /*for (int k = 0; k < 2; k++)
                   {
                       Xnext[k, 0] = Xi[k, 0] + ran.NextDouble() * step * (Xj[k, 0] - Xi[k, 0]) / mod;
                   } */

                    //改进鱼群
                    sum_mod1 = sum_mod1 + (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]) * (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]);
                    sum_mod2 = sum_mod2 + (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]) * (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]);
                    sum_mod3 = sum_mod3 + (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1)) * (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1));
                    sum_mod4 = sum_mod4 + (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1)) * (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1));
                    sum_mod5 = sum_mod5 + (Xj[0, 0] - Xi[0, 0]) * (Xj[0, 0] - Xi[0, 0]);
                    sum_mod6 = sum_mod6 + (Xj[1, 0] - Xi[1, 0]) * (Xj[1, 0] - Xi[1, 0]);
                    mod1 = Math.Sqrt(sum_mod1);
                    mod2 = Math.Sqrt(sum_mod2);
                    mod3 = Math.Sqrt(sum_mod3);
                    mod4 = Math.Sqrt(sum_mod4);
                    mod5 = Math.Sqrt(sum_mod5);
                    mod6 = Math.Sqrt(sum_mod6);
                    Xnext[0, 0] = Xi[0, 0] + ran.NextDouble() * step * ((Xj[0, 0] - Xi[0, 0]) / mod5 + (-0.5 * (w * (0.5e-3) * Icbq1 + 0.5 * Icbd1) - Xi[0, 0]) / mod1 + (0 - (Xi[0, 0] + w * (0.5e-3) * Icbq1 + 0.5 * Icbd1)) / mod3);
                    Xnext[1, 0] = Xi[1, 0] + ran.NextDouble() * step * ((Xj[1, 0] - Xi[1, 0]) / mod6 + (-0.5 * (-w * (0.5e-3) * Icbd1 + 0.5 * Icbq1) - Xi[1, 0]) / mod2 + (0 - (Xi[1, 0] - w * (0.5e-3) * Icbd1 + 0.5 * Icbq1)) / mod4);

                    for (int j = 0; j < Xnext.Length; j++)
                    {
                        if (Xnext[j, 0] > LBUB[j, 1])
                        {
                            Xnext[j, 0] = LBUB[j, 1];
                        }
                        if (Xnext[j, 0] < LBUB[j, 0])
                        {
                            Xnext[j, 0] = LBUB[j, 0];
                        }
                    }
                    Xi = Xnext;
                    break;
                }
            }
            if (Xnext.Length == 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    Xj[j, 0] = Xi[j, 0] + (2 * ran.NextDouble() - 1) * visual;
                }
                Xnext = Xj;
                for (int j = 0; j < Xnext.Length; j++)
                {
                    if (Xnext[j, 0] > LBUB[j, 1])
                    {
                        Xnext[j, 0] = LBUB[j, 1];
                    }
                    if (Xnext[j, 0] < LBUB[j, 0])
                    {
                        Xnext[j, 0] = LBUB[j, 0];
                    }
                }
            }
            Ynext = AF_foodconsistence(Xnext, Icbd1, Icbq1, Icbd2, Icbq2, w, U_fu1, Uo_fu1, U_fu2);
        }


    }

}

