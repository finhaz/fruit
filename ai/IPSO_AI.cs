using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fruit
{
    class IPSO_analysis
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
        public double In_VCM;
        public double In_VCM_linjie;
        public double fitnesszbest_last;

        double[] fitnessgbest = new double[20];          //个体粒子中达到最小值时的适应度值
        double[,] gbest = new double[20, 4];                 //个体粒子达到极值时对应的最优解位置
        double fitnesszbest = 0;          //群体粒子达到极值时的适应度值
        double[] zbest = new double[4];                 //群体粒子达到极值时对应解的位置
        double[] fitness = new double[20];  //矩阵，i个粒子的适应度值                      
        double[,] pop = new double[20, 4];//矩阵，用来存放粒子的位置，粒子的解
        double[,] V = new double[20, 4];  //矩阵，用来存在粒子的速度

        double[] u = new double[5];//代表输入参数

        Random ran = new Random();

        public double Vd_VCM = 0;
        public double Vq_VCM = 0;
        public double Id_CCM = 0;
        public double Iq_CCM = 0;

        double Vcbd1_last = 0;
        double Vcbq1_last = 0;

        public void cale_pso()
        {
            //接收的数据存入u_dsp中
            //两台逆变器dq坐标系下的负序电流输入以及频率
            //double Icbd1 = u_dsp[0, 0];
            //double Icbq1 = u_dsp[0, 1];
            //double Icbd2 = u_dsp[1, 0];
            //double Icbq2 = u_dsp[1, 1];
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
            double VUFout1 = u_dsp[0, 7];
            double VUFpcc1 = u_dsp[0, 8];
            //double VUFout2 = u_dsp[1, 7];
            //double VUFpcc2 = u_dsp[1, 8];
            double Idn_z = u_dsp[1, 0];
            double Iqn_z = u_dsp[1, 1];


            //Icbd1 = 0.80 * Icbd1+ 0.20 * u_dsp[0, 0];
            //Icbq1 = 0.80 * Icbq1+ 0.20 * u_dsp[0, 1];
            //Icbd2 = 0.80 * Icbd2+ 0.20 * u_dsp[1, 0];
            //Icbq2 = 0.80 * Icbq2+ 0.20 * u_dsp[1, 1];
            //Udp1 = 0.80 * Udp1 + 0.20 * u_dsp[0, 3];
            //Uqp1 = 0.80 * Uqp1 + 0.20 * u_dsp[0, 4];
            //Udp2 = 0.80 * Udp2 + 0.20 * u_dsp[1, 3];
            //Uqp2 = 0.80 * Uqp2 + 0.20 * u_dsp[1, 4];
            //Uodp1 = 0.80 * Uodp1 + 0.20 * u_dsp[0, 5];
            //Uoqp1 = 0.80 * Uoqp1 + 0.20 * u_dsp[0, 6];
            //Uodp2 = 0.80 * Uodp2 + 0.20 * u_dsp[1, 5];
            //Uoqp2 = 0.80 * Uoqp2 + 0.20 * u_dsp[1, 6];

            double In_VCM = 0;
            double In_VCM_linjie = 0;
            double U_fu1 = 0;
            double Uo_fu1 = 0;
            //double U_fu2 = 0;
            //double Uo_fu2 = 0;

            In_VCM = Math.Sqrt(Icbd1 * Icbd1 + Icbq1 * Icbq1);
            In_VCM_linjie = (0.02 * Math.Sqrt(Udp1 * Udp1 + Uqp1 * Uqp1) + 0.02 * Math.Sqrt(Uodp1 * Uodp1 + Uoqp1 * Uoqp1)) / (Math.Sqrt(0.5 * 0.5 + (w * 0.5e-3) * (w * 0.5e-3)));
            U_fu1 = Math.Sqrt(Udp1 * Udp1 + Uqp1 * Uqp1);
            Uo_fu1 = Math.Sqrt(Uodp1 * Uodp1 + Uoqp1 * Uoqp1);
            //U_fu2 = Math.Sqrt(Udp2 * Udp2 + Uqp2 * Uqp2);
            //Uo_fu2 = Math.Sqrt(Uodp2 * Uodp2 + Uoqp2 * Uoqp2);



            /*double Vd1 = 0;
            double Vq1 = 0;
            double Vd2 = 0;
            double Vq2 = 0;*/
            //if (!pso_init)
            //if (VUFout1>0.02 || VUFout2 > 0.02 || VUFpcc1 > 0.02 || VUFpcc2 > 0.02)
            if (VUFout1 > 0.02 || VUFpcc1 > 0.02)
            {
                zbest[0] = Vd_VCM;
                zbest[1] = Vq_VCM;
                zbest[2] = Id_CCM;
                zbest[3] = Iq_CCM;
                calculate(zbest, Icbd1, Icbq1, w, ref Vcbd1_last, ref Vcbq1_last);  //%测试上个周期的参考指令在本周期是否依然适用 
                fitnesszbest_last = fun(Vcbd1_last, Vcbq1_last, zbest, In_VCM, In_VCM_linjie, Idn_z, Iqn_z, U_fu1, Uo_fu1);
                if (fitnesszbest_last > 0)
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
                        //double Vcbd2 = 0;
                        //double Vcbq2 = 0;                       
                        calculate(find_pop, Icbd1, Icbq1, w, ref Vcbd1, ref Vcbq1);
                        // 功能说明：计算第i个粒子的适应度值，评价此次负序电压参考值下，不平衡电压补偿的效果（端口电压和PCC点电压是否满足限制条件）
                        fitness[i_p] = fun(Vcbd1, Vcbq1, find_pop, In_VCM, In_VCM_linjie, Idn_z, Iqn_z, U_fu1, Uo_fu1);
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

                    /*}
                    //else
                    if (pso_init)
                    {*/
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
                            //fitness[i_p] = fun(Vcbd1, Vcbq1, find_pop, In_VCM, In_VCM_linjie, Idn_z, Iqn_z, U_fu1, Uo_fu1);
                            calculate(find_pop, Icbd1, Icbq1, w, ref Vcbd1, ref Vcbq1);
                            // 功能说明：计算第i个粒子的适应度值，评价此次负序电压参考值下，不平衡电压补偿的效果（端口电压和PCC点电压是否满足限制条件）
                            fitness[i_p] = fun(Vcbd1, Vcbq1, find_pop, In_VCM, In_VCM_linjie, Idn_z, Iqn_z, U_fu1, Uo_fu1);
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
                    if (fitnesszbest < fitnesszbest_last)                                //判断上一周期最优解与本周期寻优得到的最优解谁更好
                    {
                        Vd_VCM = zbest[0];
                        Vq_VCM = zbest[1];
                        if (In_VCM > In_VCM_linjie)
                        {
                            Id_CCM = zbest[2];
                            Iq_CCM = zbest[3];
                        }
                    }
                    else
                    {
                        fitnesszbest = fitnesszbest_last;
                    }

                }


            }

            u_g[0] = Convert.ToSingle(Vd_VCM);
            u_g[1] = Convert.ToSingle(Vq_VCM);
            u_g[2] = Convert.ToSingle(Id_CCM);
            u_g[3] = Convert.ToSingle(Iq_CCM);

            return;
        }

        static void calculate(double[] Vsdq, double Icbd1, double Icbq1, double w, ref double Vcbd1, ref double Vcbq1)
        {
            //计算在不同端口电压下，PCC点电压的预测值
            Vcbd1 = Vsdq[0] - w * (0.5e-3) * Icbq1 - 0.5 * Icbd1;
            Vcbq1 = Vsdq[1] + w * (0.5e-3) * Icbd1 - 0.5 * Icbq1;
            //Vcbd2 = Vsdq[2] - w * (0.5e-3) * Icbq2 - 0.5 * Icbd2;
            //Vcbq2 = Vsdq[3] + w * (0.5e-3) * Icbd2 - 0.5 * Icbq2;
        }

        static double fun(double Vcbd1, double Vcbq1, double[] Vsdq, double In_VCM, double In_VCM_linjie, double Idn_z, double Iqn_z, double U_fu1, double Uo_fu1)
        {
            double Vs1, Vcb1, m1, m5, m7, m8, m9, m10, m11, m12, m13, m14, m15, y;
            double y1 = 0;
            double y2 = 0;
            double y3 = 0;
            double y4 = 0;
            //目标函数，计算函数的适应度值
            Vs1 = Math.Sqrt(Vsdq[0] * Vsdq[0] + Vsdq[1] * Vsdq[1]);
            //Vs2 = Math.Sqrt(Vsdq[2] * Vsdq[2] + Vsdq[3] * Vsdq[3]);
            Vcb1 = Math.Sqrt(Vcbd1 * Vcbd1 + Vcbq1 * Vcbq1);
            //Vcb2 = Math.Sqrt(Vcbd2 * Vcbd2 + Vcbq2 * Vcbq2);
            double VUFout = Vs1 / U_fu1;
            double VUFpcc = Vcb1 / Uo_fu1;

            if (Vs1 <= U_fu1 * 0.02)
            {
                m1 = 0;
            }
            else
            {
                m1 = Vs1 - U_fu1 * 0.02;
            }

            if (Vcb1 <= Uo_fu1 * 0.02)
            {
                m5 = 0;
            }
            else
            {
                m5 = Vcb1 - Uo_fu1 * 0.02;
            }

            if ((Math.Sqrt((Idn_z - Vsdq[2]) * (Idn_z - Vsdq[2]) + (Iqn_z - Vsdq[3]) * (Iqn_z - Vsdq[3]))) >= In_VCM_linjie)
            {
                m7 = (Math.Sqrt((Idn_z - Vsdq[2]) * (Idn_z - Vsdq[2]) + (Iqn_z - Vsdq[3]) * (Iqn_z - Vsdq[3]))) - In_VCM_linjie;
            }
            else
            {
                m7 = 0;
            }

            if (Idn_z >= 0 && Iqn_z >= 0)
            {
                if (Vsdq[2] >= 0 && Vsdq[2] <= Idn_z)
                {
                    m8 = 0;
                }
                else if (Vsdq[2] > Idn_z)
                {
                    m8 = Vsdq[2] - Idn_z;
                }
                else
                {
                    m8 = Idn_z - Vsdq[2];
                }

                if (Vsdq[3] >= 0 && Vsdq[3] <= Iqn_z)
                {
                    m9 = 0;
                }
                else if (Vsdq[3] > Iqn_z)
                {
                    m9 = Vsdq[3] - Iqn_z;
                }
                else
                {
                    m9 = Iqn_z - Vsdq[3];
                }
                //y = 5 * m1 + 5 * m5 + 15 * m7 + 5 * m8 + 5 * m9;
                y1 = 5 * m8 + 5 * m9;
                y2 = 0;
                y3 = 0;
                y4 = 0;
            }

            if (Idn_z >= 0 && Iqn_z < 0)
            {
                if (Vsdq[2] >= 0 && Vsdq[2] <= Idn_z)
                {
                    m10 = 0;
                }
                else if (Vsdq[2] > Idn_z)
                {
                    m10 = Vsdq[2] - Idn_z;
                }
                else
                {
                    m10 = Idn_z - Vsdq[2];
                }

                if (Vsdq[3] <= 0 && Vsdq[3] >= Iqn_z)
                {
                    m11 = 0;
                }
                else if (Vsdq[3] < Iqn_z)
                {
                    m11 = Iqn_z - Vsdq[3];
                }
                else
                {
                    m11 = Vsdq[3] - Iqn_z;
                }
                //y = 5 * m1 + 5 * m5 + 15 * m7 + 5 * m10 + 5 * m11;
                y1 = 0;
                y2 = 5 * m10 + 5 * m11;
                y3 = 0;
                y4 = 0;
            }

            if (Idn_z < 0 && Iqn_z < 0)
            {
                if (Vsdq[2] <= 0 && Vsdq[2] >= Idn_z)
                {
                    m12 = 0;
                }
                else if (Vsdq[2] < Idn_z)
                {
                    m12 = Idn_z - Vsdq[2];
                }
                else
                {
                    m12 = Vsdq[2] - Idn_z;
                }

                if (Vsdq[3] <= 0 && Vsdq[3] >= Iqn_z)
                {
                    m13 = 0;
                }
                else if (Vsdq[3] < Iqn_z)
                {
                    m13 = Iqn_z - Vsdq[3];
                }
                else
                {
                    m13 = Vsdq[3] - Iqn_z;
                }
                //y = 5 * m1 + 5 * m5 + 15 * m7 + 5 * m12 + 5 * m13;                
                y1 = 0;
                y2 = 0;
                y3 = 5 * m12 + 5 * m13;
                y4 = 0;
            }

            if (Idn_z < 0 && Iqn_z >= 0)
            {
                if (Vsdq[2] <= 0 && Vsdq[2] >= Idn_z)
                {
                    m14 = 0;
                }
                else if (Vsdq[2] < Idn_z)
                {
                    m14 = Idn_z - Vsdq[2];
                }
                else
                {
                    m14 = Vsdq[2] - Idn_z;
                }

                if (Vsdq[3] >= 0 && Vsdq[3] <= Iqn_z)
                {
                    m15 = 0;
                }
                else if (Vsdq[3] > Iqn_z)
                {
                    m15 = Vsdq[3] - Iqn_z;
                }
                else
                {
                    m15 = Iqn_z - Vsdq[3];
                }
                //y = 5 * m1 + 5 * m5 + 15 * m7 + 5 * m14 + 5 * m15;               
                y1 = 0;
                y2 = 0;
                y3 = 0;
                y4 = 5 * m14 + 5 * m15;
            }
            y = 5 * m1 + 5 * m5 + 15 * m7 + y1 + y2 + y3 + y4;
            return y;
        }

    }
}
