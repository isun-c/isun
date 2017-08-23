using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Games.ChinaChess
{
	[Serializable]
	public abstract class Chessman
	{
		public bool color;	// true 红   false 黑
		public MyPoint pt;
		public bool eaten;	//标记是否存在于棋盘

		[NonSerialized]
		public Image img;
		[NonSerialized]
		public Image block;


		public Chessman(bool color, MyPoint p)
		{
			this.color = color;
			this.pt = p;
		}

		public void show(Graphics g)
		{
			g.DrawImage(this.img,this.pt.bx,this.pt.by);
		}

		public abstract void SetImg();	//反序列化后，设置图片

		public abstract bool MoveMode(int x, int y, MyPoint[,] p);

		//public virtual int CountStepToKing(MyPoint[,] board, CpuStep step, Chessman king)		//计算走到对方将的位置需要的步数
		//{
		//    return 998;
		//}

	}

	[Serializable]
	class Army : Chessman
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h兵.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b卒.png");

		public Army(bool color, MyPoint p) : base(color,p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			int direction = this.color ? -1 : 1;

			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if (mx == 0 && my * direction == 1)
			{
				return true;
			}
			else if (my == 0 && Math.Abs(mx) == 1)
			{ 
				if((this.color && this.pt.y < 5) || (!this.color && this.pt.y > 4))
				{
					return true;
				}
			}

			return false;
		}

		//public override int CountStepToKing(MyPoint[,] board, CpuStep step, Chessman king)
		//{
		//    int sc, msc;

		//    sc = Math.Abs(step.x - king.pt.x) + Math.Abs(step.y - king.pt.y);
		//    msc = Math.Abs(step.tar_x - king.pt.x) + Math.Abs(step.tar_y - king.pt.y);

		//    return msc > sc ? 998 : msc;
		//}
	}

	[Serializable]
	class Artillery : Chessman 
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h炮.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b炮.png");

		public Artillery(bool color, MyPoint p)
			: base(color, p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if (mx != 0 && my != 0)
			{
				return false;
			}

			int xp = mx != 0 ? mx / Math.Abs(mx) : 0;
			int yp = my != 0 ? my / Math.Abs(my) : 0;

			if (p[x, y].che == null)
			{
				for (int i = this.pt.x + xp, j = this.pt.y + yp; i != x || j != y; i += xp, j += yp)
				{
					if (p[i, j].che != null)
					{
						return false;
					}
				}
			}
			else
			{
				bool sign = false;	//越过棋子标记
				for (int i = this.pt.x + xp, j = this.pt.y + yp; i != x || j != y; i += xp, j += yp)
				{
					if (p[i, j].che != null)
					{
						if (sign)
						{
							return false;
						}
						sign = true;
					}
				}
				if (!sign)
				{
					return false;
				}
			}

			return true;
		}

		//public override int CountStepToKing(MyPoint[,] board, CpuStep step, Chessman king)
		//{
		//    int count1 = this.CountStepToKing(board, step.x, step.y, king);
		//    int count2 = this.CountStepToKing(board, step.tar_x, step.tar_x, king);

		//    return count1 > count2 ? count2 : 998;
		//}

		//private int CountStepToKing(MyPoint[,] board, int x, int y, Chessman king)
		//{
		//    int count, min = 998;

		//    count = -1;

		//    if (king.pt.x != x)
		//    {
		//        bool cross = false;	//标记是否越过一个棋子
		//        bool doCross = true;	//标记是否还考虑越子
		//        int p = x > king.pt.x ? -1 : 1;		//前进方向
		//        for (int i = x + p; i != king.pt.x; i += p)
		//        {
		//            if (board[i, king.pt.y].che != null)
		//            {
		//                count++;
		//                if (doCross)
		//                    if (cross)
		//                    {
		//                        if (board[i, king.pt.y].che.color ^ this.color)
		//                        {
		//                            cross = false;
		//                            count--;
		//                        }
		//                        else
		//                        {
		//                            doCross = false;
		//                        }
		//                    }
		//                    else
		//                    {
		//                        cross = true;
		//                    }
		//            }
		//        }

		//        count = Math.Abs(count);

		//        if (y != king.pt.y)
		//        {
		//            count++;
		//            cross = false;
		//            p = y > king.pt.y ? -1 : 1;
		//            for (int i = y + p; true; i += p)
		//            {
		//                if (board[x, i].che != null)
		//                {
		//                    if (cross)
		//                    {
		//                        if (i != king.pt.y || board[x, i].che.color == this.color)
		//                        {
		//                            count++;
		//                        }

		//                        cross = false;
		//                        break;
		//                    }
		//                    else
		//                    {
		//                        cross = true;
		//                    }
		//                }

		//                if (i == king.pt.y)
		//                    break;
		//            }

		//            if (cross)
		//                count++;
		//        }

		//        min = count;
		//    }
		//    else
		//    {
		//        count = 2;
		//    }


		//    count = -1;

		//    if (y != king.pt.y)
		//    {
		//        bool cross = false;	//标记是否越过一个棋子
		//        bool doCross = true;	//标记是否还考虑越子
		//        int p = y > king.pt.y ? -1 : 1;
		//        for (int i = y + p; i != king.pt.y; i += p)
		//        {
		//            if (board[king.pt.x, i].che != null)
		//            {
		//                count++;
		//                if (doCross)
		//                    if (cross)
		//                    {
		//                        if (board[king.pt.x, i].che.color == this.color)
		//                        {
		//                            count--;
		//                            cross = false;
		//                        }
		//                        else
		//                        {
		//                            doCross = false;
		//                        }
		//                    }
		//                    else
		//                    {
		//                        cross = true;
		//                    }
		//            }
		//        }

		//        count = Math.Abs(count);

		//        if (x != king.pt.x)
		//        {
		//            count++;
		//            cross = false;
		//            p = x > king.pt.x ? -1 : 1;
		//            for (int i = x + p; true; i += p)
		//            {
		//                if (board[i, y].che != null)
		//                {
		//                    if (cross)
		//                    {
		//                        if (i != king.pt.x || board[i, y].che.color == this.color)
		//                        {
		//                            count++;
		//                        }

		//                        cross = false;
		//                        break;
		//                    }
		//                    else
		//                    {
		//                        cross = true;
		//                    }
		//                }

		//                if (i == king.pt.x)
		//                    break;
		//            }

		//            if (cross)
		//                count++;
		//        }
		//    }
		//    else
		//    {
		//        count = 2;
		//    }

		//    min = min > count ? count : min;

		//    if (min > 1)
		//        min *= 2 - 1;

		//    return min + 1;
		//}
	}

	[Serializable]
	class Car : Chessman
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h車.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b車.png");

		public Car(bool color,MyPoint p) : base(color,p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if (mx != 0 && my != 0)
			{
				return false;
			}

			int xp = mx != 0 ? mx / Math.Abs(mx) : 0;
			int yp = my != 0 ? my / Math.Abs(my) : 0;

			for (int i = this.pt.x + xp, j = this.pt.y + yp; i != x || j != y; i += xp, j += yp)
			{
				if (p[i, j].che != null)
				{
					return false;
				}
			}
			
			return true;
		}

		//public override int CountStepToKing(MyPoint[,] board, CpuStep step, Chessman king)
		//{
			
		//}

		//public int CountStepToKing(MyPoint[,] board, int x, int y, Chessman king)
		//{
		//    int count, min = 998;

			
		//}

	}

	[Serializable]
	class Horse : Chessman
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h馬.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b馬.png");

		public Horse(bool color, MyPoint p)
			: base(color, p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if (mx == 0 || my == 0 || Math.Abs(mx) + Math.Abs(my) != 3)
			{
				return false;
			}

			if (p[this.pt.x + mx / 2, this.pt.y + my / 2].che != null)
			{
				return false;
			}

			return true;
		}
	}

	[Serializable]
	class Elephant : Chessman
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h相.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b象.png");

		public Elephant(bool color, MyPoint p)
			: base(color, p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			if ((this.color && y < 5) || (!this.color && y > 4))
			{
				return false;
			}

			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if (Math.Abs(mx) != 2 || Math.Abs(my) != 2)
			{
				return false;
			}

			if (p[this.pt.x + mx / 2, this.pt.y + my / 2].che != null)
			{
				return false;
			}

			return true;
		}
	}

	[Serializable]
	class Official : Chessman
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h仕.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b士.png");

		public Official(bool color, MyPoint p)
			: base(color, p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			if (Math.Abs(x - 4) > 1 || ((this.color && Math.Abs(y - 8) > 1) || (!this.color && Math.Abs(y - 1) > 1)))
			{
				return false;
			}

			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if(Math.Abs(mx) != 1 || Math.Abs(my) != 1)
			{
				return false;
			}

			return true;
		}
	}

	[Serializable]
	class King : Chessman
	{
		public static readonly Image r_img = Image.FromFile("Resource/ChinaChess/h帥.png");
		public static readonly Image b_img = Image.FromFile("Resource/ChinaChess/b將.png");

		public King(bool color, MyPoint p)
			: base(color, p)
		{
			this.img = this.color ? r_img : b_img;
		}

		public override void SetImg()
		{
			this.img = this.color ? r_img : b_img;
		}

		public override bool MoveMode(int x, int y, MyPoint[,] p)
		{
			if (p[x, y] != null && p[x, y].che is King && x == this.pt.x)
			{
				for (int i = (this.color ? p[x, y].y : this.pt.y) + 1; i < (this.color ? this.pt.y : p[x, y].y); i++)
				{
					if (p[x, i].che != null)
					{
						return false;
					}
				}
				return true;
			}

			if (Math.Abs(x - 4) > 1 || ((this.color && Math.Abs(y - 8) > 1) || (!this.color && Math.Abs(y - 1) > 1)))
			{
				return false;
			}

			int mx = x - this.pt.x;
			int my = y - this.pt.y;

			if (Math.Abs(mx) + Math.Abs(my) != 1)
			{
				return false;
			}

			return true;
		}
	}
}
