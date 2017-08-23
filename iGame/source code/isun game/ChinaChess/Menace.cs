using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Games.ChinaChess
{
	//威胁
	public class Menace
	{
		//public static int pointCount;		//搜索节点计数

		public static List<CpuStep> nextSteps = new List<CpuStep>();		//当前搜索到的得分最高的下一步棋

		public static int searchDeep = 3;		//搜索深度

		public int currentDeep;		//当前搜索深度
		public CChessman[,] cboard;		//逻辑棋盘
		public bool act_color;		//行动阵营方

		public int maxScore;	//本轮搜索到的走法中的最高分

		//public static CpuStep[] mScoreSteps = new CpuStep[6];
		//public static List<CpuStep> msSteps = new List<CpuStep>();		


		//调试
		public static List<int> scores = new List<int>();

		public Menace(ChinaChess cb,bool act_color)
		{
			//Menace.pointCount = 0;

			this.maxScore = -99999;

			this.act_color = act_color;

			this.cboard = new CChessman[9, 10];
			this.currentDeep = 0;

			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					if (cb.pts[i, j].che == null)
					{
						this.cboard[i, j] = null;
					}
					else
					{ 
						this.cboard[i, j] = new CChessman(cb.pts[i,j].che);
					}
				}
			}
		}

		public Menace(Menace m, CChessman[,] cboard/*, int score*/)
		{
			this.cboard = cboard;
			this.act_color = !m.act_color;
			this.currentDeep = m.currentDeep + 1;
			this.maxScore = -99999 + this.currentDeep * 3000;
		}

		public int Search()
		{
			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					if (this.cboard[i, j] != null && this.cboard[i, j].color == this.act_color)
					{
						this.cboard[i, j].move(this, i, j);
						//Menace.pointCount++;	//搜索节点计数
					}
				}
			}

			return this.maxScore;
		}
	}

	public class CChessman
	{
		/***
		 * 车：100  炮：80   马：6
		 * 象：40   士：40   卒： 过河 30，否则 20
		 * 将：9998
		 */
		public bool color;	//阵营方
		public CMove move;		//移动算法
		public int score;		//棋子分值

		public CChessman(Chessman c)
		{
			this.color = c.color;

			if (c is Army)			{this.move = CMoves.Army; this.score = ((c.color && c.pt.y < 5) || (!c.color && c.pt.y > 4) ? 30 : 20); return; }
			if (c is Artillery)		{this.move = CMoves.Artillery; this.score = 80; return; }
			if (c is Car)			{this.move = CMoves.Car; this.score = 100; return; }
			if (c is Horse)			{this.move = CMoves.Horse; this.score = 60; return; }
			if (c is Elephant)		{this.move = CMoves.Elephant; this.score = 40; return; }
			if (c is Official)		{this.move = CMoves.Official; this.score = 40; return; }
			if (c is King)			{this.move = CMoves.King; this.score = 9998; return; }
		}
	}

	public delegate void CMove(Menace m, int x, int y);

	//记录行动方式
	public class CpuStep
	{
		public int x;		//移动的棋子的点位
		public int y;
		public int tar_x;	//目标点位
		public int tar_y;

		//public int score;	//步骤得分

		public CpuStep() { /*this.score = -9999; */}

		public CpuStep(int x, int y, int tar_x, int tar_y /*,int score*/)
		{
			this.x = x;
			this.y = y;
			this.tar_x = tar_x;
			this.tar_y = tar_y;

			//this.score = score;
		}
	}

	public enum ChessType
	{ 
		empty,king,official,elephant,horse,car,artillery,army
	}
}
