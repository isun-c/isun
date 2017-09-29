using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace Games.ChinaChess
{
	public class CMoves
	{

		//统计得分 并向更高层递归
		private static void CountScore(Menace m, int x, int y, int mx, int my)
		{
			int score = 0;		//本轮得分

			if (m.cboard[mx, my] != null)
			{
				if (m.act_color == m.cboard[mx, my].color)	//目标为己方棋子
					return;

				score = m.cboard[mx, my].score;
			}

			if (m.currentDeep != Menace.searchDeep && score < 9000)	//不是最顶层搜索
			{
				CChessman[,] cboard = (CChessman[,])m.cboard.Clone();
				cboard[mx, my] = cboard[x, y];
				cboard[x, y] = null;

				score -= new Menace(m, cboard).Search();	//递归
			}

			if (score > m.maxScore)
			{
				m.maxScore = score;

				if (m.currentDeep == 0)
				{
					//Menace.nextSteps.x = x;
					//Menace.nextSteps.y = y;
					//Menace.nextSteps.tar_x = mx;
					//Menace.nextSteps.tar_y = my;
					Menace.nextSteps.Clear();
					Menace.nextSteps.Add(new CpuStep(x, y, mx, my));
				}
			}
			else if (score == m.maxScore && m.currentDeep == 0)
			{
				Menace.nextSteps.Add(new CpuStep(x, y, mx, my));
			}
		}


		/////////////////////////////////////////////////
		//遍历各种棋子可能的点位

		//卒
		public static void Army(Menace m, int x, int y)
		{

			if (m.act_color)
			{
				if(y != 0)
					CMoves.CountScore(m, x, y, x, y - 1);
				if (y < 5)
				{
					if (x != 0)
						CMoves.CountScore(m, x, y, x - 1, y);
					if (x != 8)
						CMoves.CountScore(m, x, y, x + 1, y);
				}
			}
			else
			{
				if (y != 9)
					CMoves.CountScore(m, x, y, x, y + 1);
				if (y > 4)
				{
					if (x != 0)
						CMoves.CountScore(m, x, y, x - 1, y);
					if (x != 8)
						CMoves.CountScore(m, x, y, x + 1, y);
				}
			}

		}

		//炮
		public static void Artillery(Menace m, int x, int y) 
		{

			int mx, my;

			for (int b = 0, xp, yp; true; b++)
			{
				switch (b)
				{ 
					case 0:
						xp = 1; yp = 0;
						break;
					case 1:
						xp = 0; yp = 1;
						break;
					case 2:
						xp = -1; yp = 0;
						break;
					case 3:
						xp = 0; yp = -1;
						break;
					default:
						return;
				}

				mx = x + xp;
				my = y + yp;

				for (bool cross = false ; mx > -1 && mx < 9 && my > -1 && my < 10; mx += xp,my += yp)
				{
					if (cross)	//是否有越过一个棋子
					{
						if (m.cboard[mx, my] != null)
						{
							CMoves.CountScore(m, x, y, mx, my);
							break;
						}
					}
					else
					{
						if (m.cboard[mx, my] == null)
							CMoves.CountScore(m, x, y, mx, my);
						else
							cross = true;
					}
				}
			}
		}

		//车
		public static void Car(Menace m, int x, int y) 
		{

			for (int i = x - 1; i > -1; i--)
			{
				CMoves.CountScore(m, x, y, i, y);
				if (m.cboard[i, y] != null)
					break;
			}

			for (int i = x + 1; i < 9; i++)
			{
				CMoves.CountScore(m, x, y, i, y);
				if (m.cboard[i, y] != null)
					break;
			}

			for (int i = y - 1; i > -1; i--)
			{
				CMoves.CountScore(m, x, y, x, i);
				if (m.cboard[x, i] != null)
					break;
			}

			for (int i = y + 1; i < 10; i++)
			{
				CMoves.CountScore(m, x, y, x, i);
				if (m.cboard[x, i] != null)
					break;
			}

		}

		//马
		public static void Horse(Menace m, int x, int y)
		{

			if (x + 2 < 9)
			{
				if (m.cboard[x + 1, y] == null)		//不蹩脚的情况
				{
					if (y + 1 < 10)
						CMoves.CountScore(m, x, y, x + 2, y + 1);
					if (y - 1 > -1)
						CMoves.CountScore(m, x, y, x + 2, y - 1);
				}
			}

			if (x - 2 > -1)
			{
				if (m.cboard[x - 1, y] == null)
				{
					if (y + 1 < 10)
						CMoves.CountScore(m, x, y, x - 2, y + 1);
					if (y - 1 > -1)
						CMoves.CountScore(m, x, y, x - 2, y - 1);
				}
			}

			if (y + 2 < 10)
			{
				if (m.cboard[x, y + 1] == null)
				{
					if (x + 1 < 9)
						CMoves.CountScore(m, x, y, x + 1, y + 2);
					if (x - 1 > -1)
						CMoves.CountScore(m, x, y, x - 1, y + 2);
				}
			}

			if (y - 2 > -1)
			{
				if (m.cboard[x, y - 1] == null)
				{
					if (x + 1 < 9)
						CMoves.CountScore(m, x, y, x + 1, y - 2);
					if (x - 1 > -1)
						CMoves.CountScore(m, x, y, x - 1, y - 2);
				}
			}
		}

		//象
		public static void Elephant(Menace m, int x, int y)
		{

			//		左上			右上			左下			右下
			bool ul = false, ur = false, dl = false, dr = false;

			if (x == 4)
				ul = ur = dl = dr = true;
			if (x == 0)
				ur = dr = true;
			if (x == 8)
				ul = dl = true;
			if (y == 0 || y == 5)
				dl = dr = true;
			if (y == 4 || y == 9)
				ul = ur = true;

			if (ul && m.cboard[x - 1, y - 1] == null)
				CMoves.CountScore(m, x, y, x - 2, y - 2);
			if (ur && m.cboard[x + 1, y - 1] == null)
				CMoves.CountScore(m, x, y, x + 2, y - 2);
			if (dl && m.cboard[x - 1, y + 1] == null)
				CMoves.CountScore(m, x, y, x - 2, y + 2);
			if (dr && m.cboard[x + 1, y + 1] == null)
				CMoves.CountScore(m, x, y, x + 2, y + 2);
		}

		//十
		public static void Official(Menace m, int x, int y) 
		{

			if (x == 4)
			{
				CMoves.CountScore(m, x, y, x - 1, y - 1);
				CMoves.CountScore(m, x, y, x + 1, y - 1);
				CMoves.CountScore(m, x, y, x - 1, y + 1);
				CMoves.CountScore(m, x, y, x + 1, y + 1);
			}
			else
			{
				CMoves.CountScore(m, x, y, 4, m.cboard[x, y].color ? 8 : 1);
			}
		}

		//将
		public static void King(Menace m, int x, int y) 
		{

			int p = m.cboard[x, y].color ? -1 : 1;
			int endy = m.cboard[x, y].color ? -1 : 10;
			//与对方将之间是否有阻挡
			for (int i = y + p; i != endy; i += p)
			{
				if (m.cboard[x, i] != null)
				{
					if (m.cboard[x, i].score == 9998)
						CMoves.CountScore(m, x, y, x, i);
					break;
				}
			}

			if (x < 5)
				CMoves.CountScore(m, x, y, x + 1, y);
			if (x > 3)
				CMoves.CountScore(m, x, y, x - 1, y);
			if ((m.cboard[x, y].color && y > 7) || (!m.cboard[x, y].color && y > 0))
				CMoves.CountScore(m, x, y, x, y - 1);
			if ((m.cboard[x, y].color && y < 9) || (!m.cboard[x, y].color && y < 2))
				CMoves.CountScore(m, x, y, x, y + 1);
		}
	}
}
