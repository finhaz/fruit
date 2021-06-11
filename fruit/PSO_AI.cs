using System;
using System.Linq;

namespace fruit
{
	public class PSO_analysis
	{
        //PSO常数
        double c1 = 1.49445;     // 学习因子
        double c2 = 1.49445;     // 学习因子
        int maxgen = 300;    //粒子群迭代次数
        int sizepop = 20;     //粒子数目
        double popmax = 100;      //粒子位置的最大值，即解的最大允许值 
        double popmin = -100;     //粒子位置的最小值，解的最小允许值
        double Vmax = 2;      // 粒子最大速度，粒子速度的大小和精度有关
        double Vmin = -2;     //粒子最小速度

        //PSO所需变量
        public float[,] u_dsp = new float[2, 10];
        public float[] u_g = new float[4];
        public bool pso_init = false;//初始化完成标志
        
        public double w ;
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

        double[] fitnessgbest = new double[20];          //个体粒子中达到最小值时的适应度值
        double[,] gbest = new double[20, 4];                 //个体粒子达到极值时对应的最优解位置
        double fitnesszbest = 0;          //群体粒子达到极值时的适应度值
        double[] zbest = new double[4];                 //群体粒子达到极值时对应解的位置
        double[] fitness = new double[20];  //矩阵，i个粒子的适应度值                      
        double[,] pop = new double[20, 4];//矩阵，用来存放粒子的位置，粒子的解
        double[,] V = new double[20, 4];  //矩阵，用来存在粒子的速度

        double[] u = new double[5];//代表输入参数
        Random ran = new Random();

        public void cale_pso()
        {
            //if (pso_data_flag == true)
            {
                //pso_data_flag = false;

                //timer3.Enabled = false;

                //if (n_dsp == Set_Num_DSP)
                {
                    //接收的数据存入u_dsp中
                    //两台逆变器dq坐标系下的负序电流输入以及频率

                    w = u_dsp[0, 2];
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
                    Uodp2 = u_dsp[1, 5];
                    Uoqp2 = u_dsp[1, 6];


                    double U_fu1 = 0;
                    double Uo_fu1 = 0;
                    double U_fu2 = 0;
                    double Uo_fu2 = 0;
                    U_fu1 = Math.Sqrt(Udp1 * Udp1 + Uqp1 * Uqp1);
                    Uo_fu1 = Math.Sqrt(Uodp1 * Uodp1 + Uoqp1 * Uoqp1);
                    U_fu2 = Math.Sqrt(Udp2 * Udp2 + Uqp2 * Uqp2);
                    Uo_fu2 = Math.Sqrt(Uodp2 * Uodp2 + Uoqp2 * Uoqp2);

                    if (!pso_init)
                    {
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            double[] find_pop = new double[4];
                            for (int j_p = 0; j_p < 4; j_p++)
                            {

                                //pop(i,:) = 100 * rands(1, 4);   % 第i个粒子的初始位置；矩阵的第i行是两个2乘0~1之间的随机数 pop(i,:)= 2 * rands(1, 2);
                                //V(i,:) = 2 * rands(1, 4);   % 第i个粒子的速度

                                pop[i_p, j_p] = 100 * (ran.NextDouble() * 2 - 1);//第i个粒子的初始位置；范围是0~1
                                                                                 //100rands（1,4）表示输出一个大小为1行4列的随机数，4个随机数分别代表4个自变量，即两台逆变器dq坐标下的负序电压参考值
                                V[i_p, j_p] = 2 * (ran.NextDouble() * 2 - 1); //第i个粒子的速度

                                //功能说明：限制粒子速度和粒子的位置（解）不超过最大允许值
                                //V[i, find(V(i, j) > Vmax)] = Vmax;
                                //V[i, find(V(i, j) < Vmin)] = Vmin;
                                //pop[i, find(pop(i, j) > popmax)] = popmax;
                                //pop[i, find(pop(i, j) < popmin)] = popmin;

                                if (pop[i_p, j_p] > popmax) pop[i_p, j_p] = popmax;
                                if (pop[i_p, j_p] < popmin) pop[i_p, j_p] = popmin;
                                if (V[i_p, j_p] > Vmax) V[i_p, j_p] = Vmax;
                                if (V[i_p, j_p] < Vmin) V[i_p, j_p] = Vmin;

                                find_pop[j_p] = pop[i_p, j_p];
                            }
                            //功能说明：基于第一次初始化的负序电压参考值，计算对应PCC点的负序电压值
                            double Vcbd1 = 0;
                            double Vcbq1 = 0;
                            double Vcbd2 = 0;
                            double Vcbq2 = 0;
                            //[Vcbd1, Vcbq1, Vcbd2, Vcbq2] = calculate(pop[i,4], Icbd1, Icbq1, Icbd2, Icbq2, w);
                            calculate(find_pop, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Vcbd1, ref Vcbq1, ref Vcbd2, ref Vcbq2);
                            // 功能说明：计算第i个粒子的适应度值，评价此次负序电压参考值下，不平衡电压补偿的效果（端口电压和PCC点电压是否满足限制条件）
                            //fitness(j)=fun(Vcbd1,Vcbq1,Vcbd2,Vcbq2,pop(j,:));
                            fitness[i_p] = fun(Vcbd1, Vcbq1, Vcbd2, Vcbq2, find_pop, U_fu1, Uo_fu1, U_fu2, Uo_fu2);
                        }

                        double bestfitness;
                        int bestindex = 0;
                        //计算结束后，比较个体极值和全局极值
                        //寻找初始极值
                        //[bestfitness, bestindex]=min(fitness); //20个个体中找到最小值，以及对应的位置
                        bestfitness = fitness.Min();
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            if (fitness[i_p] == bestfitness)
                            {
                                bestindex = i_p;
                            }
                        }
                        //zbest = pop(bestindex,:);         //群体极值位置 全局最优，一个点的位置，是个1行4列的矩阵
                        for (int j_p = 0; j_p < 4; j_p++)
                        {
                            zbest[j_p] = pop[bestindex, j_p];
                        }
                        //gbest = pop;                     //个体极值位置 个体最优，20个点的位置，假设初始化的点均为个体极值点，gbest是个矩阵，是个20行4列的矩阵
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            for (int j_p = 0; j_p < 4; j_p++)
                            {
                                gbest[i_p, j_p] = pop[i_p, j_p];
                                //gbest[i_p][j_p]= pop[i_p][j_p];
                            }
                        }



                        //fitnessgbest = fitness;          //个体极值适应度值 fitness是个数组，包含20个个体极值
                        //fitness.CopyTo(fitnessgbest, 0);
                        for (int i_p = 0; i_p < sizepop; i_p++)
                        {
                            fitnessgbest[i_p] = fitness[i_p];
                        }

                        //fitnesszbest = bestfitness;      //群体极值适应度值 全局的最小值，只有1个数
                        fitnesszbest = bestfitness;



                        pso_init = true;//初始化完成

                    }
                    else
                    {
                        //粒子每0.05s迭代100次然后输出结果
                        for (int n_k = 0; n_k < maxgen; n_k++)
                        {
                            //20个粒子位置和速度更新，负序电压参考指令的改变
                            for (int i_p = 0; i_p < sizepop; i_p++)
                            {
                                double[] find_pop = new double[4];
                                for (int j_p = 0; j_p < 4; j_p++)
                                {
                                    //%%%%%%%%%%%%%% 速度更新
                                    //V(j,:) = V(j,:) + c1 * rand * (gbest(j,:) - pop(j,:)) + c2 * rand * (zbest - pop(j,:));   %
                                    //V(j, find(V(j,:) > Vmax)) = Vmax;            %%% 限幅 %%%%
                                    //V(j, find(V(j,:) < Vmin)) = Vmin;            %%% 限幅 %%%%        

                                    V[i_p, j_p] = V[i_p, j_p] + c1 * ran.NextDouble() * (gbest[i_p, j_p] - pop[i_p, j_p]) + c2 * ran.NextDouble() * (zbest[j_p] - pop[i_p, j_p]);
                                    if (V[i_p, j_p] > Vmax) V[i_p, j_p] = Vmax;
                                    if (V[i_p, j_p] < Vmin) V[i_p, j_p] = Vmin;

                                    //%%%%%%%%%%%%%%% 粒子更新
                                    //pop(j,:) = pop(j,:) + V(j,:);
                                    //pop(j, find(pop(j,:) > popmax)) = popmax;              %%% 限幅 %%%%
                                    //pop(j, find(pop(j,:) < popmin)) = popmin;              %%% 限幅 %%%%

                                    pop[i_p, j_p] = pop[i_p, j_p] + V[i_p, j_p];
                                    if (pop[i_p, j_p] > popmax) pop[i_p, j_p] = popmax;
                                    if (pop[i_p, j_p] < popmin) pop[i_p, j_p] = popmin;

                                    find_pop[j_p] = pop[i_p, j_p];

                                }

                                //新粒子适应度值    过程与初始化过程一致
                                double Vcbd1 = 0;
                                double Vcbq1 = 0;
                                double Vcbd2 = 0;
                                double Vcbq2 = 0;
                                //[Vcbd1, Vcbq1, Vcbd2, Vcbq2] = calculate(pop[i,4], Icbd1, Icbq1, Icbd2, Icbq2, w);
                                calculate(find_pop, Icbd1, Icbq1, Icbd2, Icbq2, w, ref Vcbd1, ref Vcbq1, ref Vcbd2, ref Vcbq2);
                                // 功能说明：计算第i个粒子的适应度值，评价此次负序电压参考值下，不平衡电压补偿的效果（端口电压和PCC点电压是否满足限制条件）
                                //fitness(j)=fun(Vcbd1,Vcbq1,Vcbd2,Vcbq2,pop(j,:));
                                fitness[i_p] = fun(Vcbd1, Vcbq1, Vcbd2, Vcbq2, find_pop, U_fu1, Uo_fu1, U_fu2, Uo_fu2);
                            }


                            //比较大小，个体极值和群体极值更新,取较小值
                            for (int i_p = 0; i_p < sizepop; i_p++)
                            {
                                //个体极值更新
                                //if fitness(j) < fitnessgbest(j) % 第j个粒子与它的历史最优极值进行比较
                                //gbest(j,:) = pop(j,:);                   % 历史最优个体极值的位置变成当前粒子的位置
                                //fitnessgbest(j) = fitness(j);             % 历史最优极值变成当前粒子极值
                                //end

                                if (fitness[i_p] < fitnessgbest[i_p])// 第i个粒子与它的历史最优极值进行比较
                                {
                                    //gbest(j,:) = pop(j,:);                   //历史最优个体极值的位置变成当前粒子的位置
                                    for (int j_p = 0; j_p < 4; j_p++)
                                    {
                                        gbest[i_p, j_p] = pop[i_p, j_p];
                                    }
                                    fitnessgbest[i_p] = fitness[i_p];             //历史最优极值变成当前粒子极值

                                }

                                //群体极值更新
                                //if fitness(j) < fitnesszbest % 第j个粒子与群体的历史最优值进行比较
                                //zbest = pop(j,:);                  % 群体的历史最优值的位置变成当前粒子的位置
                                //fitnesszbest = fitness(j);            % 历史最优值变成当前粒子的极值
                                //end

                                if (fitness[i_p] < fitnesszbest) //第j个粒子与群体的历史最优值进行比较
                                {
                                    //zbest = pop(j,:);                  //群体的历史最优值的位置变成当前粒子的位置
                                    for (int j_p = 0; j_p < 4; j_p++)
                                    {
                                        zbest[j_p] = pop[i_p, j_p];
                                    }
                                    fitnesszbest = fitness[i_p];           //历史最优值变成当前粒子的极值

                                }
                            }

                            // 输出结果这句话，逻辑上放的位置稍微有点问题
                            // 输出最终结果，适应度值，负序电压参考指令（4个）   
                            //x(300) = fitnesszbest;
                            //x(301) = fitnesszbest;
                            //x(302) = zbest(1);
                            //x(303) = zbest(2);
                            //x(304) = zbest(3);
                            //x(305) = zbest(4);
                            //显示计算得到的PCC点预测的电压
                            //Vd1 = zbest(1) - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
                            //Vq1 = zbest(2) + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
                            //Vd2 = zbest(3) - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
                            //Vq2 = zbest(4) + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;
                            //x(306) = Vd1;
                            //x(307) = Vq1;
                            //x(308) = Vd2;
                            //x(309) = Vq2;

                            //Vd1 = zbest[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
                            //Vq1 = zbest[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
                            //Vd2 = zbest[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
                            //Vq2 = zbest[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;

                            //Vd1 = zbest[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
                            //Vq1 = zbest[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
                            //Vd2 = zbest[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
                            //Vq2 = zbest[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;

                            //Vd1 = zbest[0];
                            //Vq1 = zbest[1];
                            //Vd2 = zbest[2];
                            //Vq2 = zbest[3];

                            //   if (fitnesszbest <= 0.5)
                            //       break;

                        }


                    }




                    //放在esle内的话，相当于第一次的值就没有下发下去，尽管初始化了，但是没有下发，可能会舍弃最优值
                    Vd1 = zbest[0];
                    Vq1 = zbest[1];
                    Vd2 = zbest[2];
                    Vq2 = zbest[3];

                    //Vd1 = 0.5;
                    //Vd2 = 0.7;
                    //Vq1 = 0.8;
                    //Vq2 = 0.9;

                    u_g[0] = Convert.ToSingle(Vd1);
                    u_g[1] = Convert.ToSingle(Vq1);
                    u_g[2] = Convert.ToSingle(Vd2);
                    u_g[3] = Convert.ToSingle(Vq2);

                }

            }
            return;
        }

        static void calculate(double[] Vsdq, double Icbd1, double Icbq1, double Icbd2, double Icbq2, double w, ref double Vcbd1, ref double Vcbq1, ref double Vcbd2, ref double Vcbq2)
        {
            //计算在不同端口电压下，PCC点电压的预测值
            Vcbd1 = Vsdq[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
            Vcbq1 = Vsdq[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
            Vcbd2 = Vsdq[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
            Vcbq2 = Vsdq[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;
        }

        static double fun(double Vcbd1, double Vcbq1, double Vcbd2, double Vcbq2, double[] Vsdq, double U_fu1, double Uo_fu1, double U_fu2, double Uo_fu2)
        {
            double Vs1, Vs2, Vcb1, Vcb2, m1, m2, m3, m4, m5, m6, y;
            //目标函数，计算函数的适应度值
            Vs1 = Math.Sqrt(Vsdq[0] * Vsdq[0] + Vsdq[1] * Vsdq[1]);
            Vs2 = Math.Sqrt(Vsdq[2] * Vsdq[2] + Vsdq[3] * Vsdq[3]);
            Vcb1 = Math.Sqrt(Vcbd1 * Vcbd1 + Vcbq1 * Vcbq1);
            Vcb2 = Math.Sqrt(Vcbd2 * Vcbd2 + Vcbq2 * Vcbq2);
            double VUFout = Vs1 / U_fu1;
            double VUFpcc = Vcb1 / Uo_fu1;
            //if (Vs1 <= 3.11)//1%
            if (Vs1 <= U_fu1 * 0.02)//2%U_fu1*0.02
            //if (Vs1 <= 9.33)//3%
            {
                m1 = 0;
            }

            else
            {
                //m1 = Vs1 - 3.11;//1%
                m1 = Vs1 - U_fu1 * 0.02;//2% U_fu1 * 0.02
                //m1 = Vs1 - 9.33;//3%
            }

            //if (Vs2 <= 3.11)//1%
            if (Vs2 <= U_fu2 * 0.02)//2%U_fu2 * 0.02
            //if (Vs2 <= 9.33)//3%
            {
                m2 = 0;
            }
            else
            {
                //m2 = Vs2 - 3.11;//1%
                m2 = Vs2 - U_fu2 * 0.02;//2% U_fu2 * 0.02
                //m2 = Vs2 - 9.33;//3%
            }

            // m2 = 0;//第二台的权重为0

            m3 = Math.Abs(Vsdq[0] - Vsdq[2]);
            m4 = Math.Abs(Vsdq[1] - Vsdq[3]);

            //if (Vcb1 - 2.95 <= 0)//1%
            if (Vcb1 - Uo_fu1 * 0.02 <= 0)//2%Uo_fu1*0.01
            //if (Vcb1 - 8.85 <= 0)//3%
            {
                m5 = 0;
            }
            else
            {
                //m5 = Vcb1 - 2.95;//1%
                m5 = Vcb1 - Uo_fu1 * 0.02;//2%Uo_fu1 * 0.01
                //m5 = Vcb1 - 8.85;//3%
            }

            //if (Vcb2 - 2.95 <= 0)//1%
            if (Vcb2 - Uo_fu2 * 0.02 <= 0)//2%Uo_fu2 * 0.01
            //if (Vcb2 - 8.85 <= 0)//3%
            {
                m6 = 0;
            }
            else
            {
                //m6 = Vcb2 - 2.95;//1%
                m6 = Vcb2 - Uo_fu2 * 0.02;//2%Uo_fu2 * 0.01
                //m6 = Vcb2 - 8.85;//3%
            }

            //m6 = 0;//第二台的权重为0
            y = 5 * m1 + 5 * m2 + 50 * m3 + 50 * m4 + 5 * m5 + 5 * m6;    //考虑环流
            //y = 5 * m1 + 5 * m2 + 5 * m5 + 5 * m6;                  // 不考虑环流
            return y;
        }

    }
}
