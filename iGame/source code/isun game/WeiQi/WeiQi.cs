using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Objs;
using iTopS2CSharp;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Games.WeiQi
{
	public class WeiQi : Obj
	{
		private PictureBox graphicBoard;		//主显示面板
		//private ExitGame exitgame;		//退出方法

		private Image wChess;	//白子图像
		private Image bChess;	//黑子图像

		private Image block;		//棋子的背景块图像
		private Graphics bg;		//背景块画笔

		private MyPoint[,] logicBoard = new MyPoint[19, 19];	//逻辑棋盘

		private bool act_color = true;	//活动的阵营方  true 黑  false 白

		private List<MyPoint> searchList = new List<MyPoint>(30);	//当前搜索的队列
		//private List<MyPoint> searchedSignList = new List<MyPoint>(30);	//管理 搜索标记 队列

		private MyPoint noEnable;		//标记不可走的点位（防止单子互提）

		private Stack<Stack<StepBack>> stepList = new Stack<Stack<StepBack>>(30);	//悔棋选项


		private ContextMenuStrip menu;		//游戏菜单
		private ContextMenuStrip layMenu;	//布局菜单
		private ToolStripMenuItem tsmiGoBack;	//悔棋选项
		private ToolStripMenuItem tsmiSetLayType_black;		//
		private ToolStripMenuItem tsmiSetLayType_white;		//布局选项菜单
		private ToolStripMenuItem tsmiSetLayType_empty;		//

		private SaveFileDialog saveDialog;
		private OpenFileDialog openDialog;

		#region 构造
		public WeiQi(PictureBox graphicBoard,ExitGame exitgame)
		{
			this.graphicBoard = graphicBoard;
			this.exitgame = exitgame;

			this.backImage = Image.FromFile("Resource/weiqi/back.jpg");
			this.board = (Image)this.backImage.Clone();

			this.bChess = Image.FromFile("Resource/weiqi/b.png");
			this.wChess = Image.FromFile("Resource/weiqi/w.png");

			this.saveDialog = new SaveFileDialog();
			this.saveDialog.Filter = "bin |*.bin";
			this.openDialog = new OpenFileDialog();
			this.openDialog.Filter = "bin |*.bin";

			//主画笔
			this.g = Graphics.FromImage(this.board);

			//提子后的背景操纵对象
			this.block = new Bitmap(this.bChess.Width, this.bChess.Height);
			this.bg = Graphics.FromImage(this.block);

			//初始化棋子
			for (int i = 0; i < 19; i++)
			{
				for (int j = 0; j < 19; j++)
				{
					this.logicBoard[i, j] = new MyPoint(i, j);
				}
			}

			//游戏菜单初始化
			this.menu = new ContextMenuStrip();
			this.menu.ShowImageMargin = false;

			ToolStripMenuItem item;
			item = new ToolStripMenuItem("悔棋");
			item.Enabled = false;
			item.Click += this.GoBack;
			this.menu.Items.Add(item);
			this.tsmiGoBack = item;

			item = new ToolStripMenuItem("布局");
			item.Click += this.Layout;
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("新开局");
			item.Click += this.NewStart;
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("进度");
			{
				ToolStripMenuItem item2;

				item2 = new ToolStripMenuItem("保存");
				item2.Click += this.SaveGame;
				item.DropDownItems.Add(item2);

				item2 = new ToolStripMenuItem("读取");
				item2.Click += this.LoadGame;
				item.DropDownItems.Add(item2);
			}
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("退出");
			item.Click += this.Exit;
			this.menu.Items.Add(item);

			this.graphicBoard.ContextMenuStrip = this.menu;

			//布局菜单
			this.layMenu = new ContextMenuStrip();
			item = new ToolStripMenuItem("黑子");
			item.Click += this.setLayType_black;
			this.tsmiSetLayType_black = item;
			this.layMenu.Items.Add(item);

			item = new ToolStripMenuItem("白子");
			item.Click += this.setLayType_white;
			this.tsmiSetLayType_white = item;
			this.layMenu.Items.Add(item);

			item = new ToolStripMenuItem("删除");
			item.Click += this.setLayType_empty;
			this.tsmiSetLayType_empty = item;
			this.layMenu.Items.Add(item);

			item = new ToolStripMenuItem("完成布局");
			item.Click += this.EndLayout;
			this.layMenu.Items.Add(item);

			this.g.DrawImage(this.bChess, this.backImage.Width - 18, 3, 15, 15);
		}
		#endregion

		#region 开始 结束
		public override void Start()
		{
			this.graphicBoard.MouseClick += this.m_click;
		}
		public override void Exit(object sender, EventArgs e)
		{
			if (MessageBox.Show("确定退出", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
				return;

			this.graphicBoard.MouseClick -= this.m_click;
			this.graphicBoard.MouseClick -= this.Layout_click;
			this.exitgame();
		}
		#endregion


		#region 棋盘单击
		private void m_click(object sender, MouseEventArgs e)
		{
			int x, y;

			x = (e.X - 12) / 32;
			y = (e.Y - 12) / 32;

			if (x > 18 || y > 18)
				return;

			if (!this.logicBoard[x, y].empty)//不是空点位
				return;

			if (this.logicBoard[x, y] == this.noEnable)	//不可走的点位
				return;
			this.noEnable = null;

			Stack<StepBack> step = new Stack<StepBack>();
			
			this.logicBoard[x, y].empty = false;
			this.logicBoard[x, y].color = this.act_color;

			//从下棋点的周围4颗棋子开始搜索
			bool outChess = false;	//标记此步是否有提子

			for (int i = 0, tx = 0, ty = 0; i < 4; i++)
			{
				switch (i)
				{ 
					case 0:
						tx = x + 1;
						ty = y;
						break;
					case 1:
						tx = x - 1;
						ty = y;
						break;
					case 2:
						tx = x;
						ty = y + 1;
						break;
					case 3:
						tx = x;
						ty = y - 1;
						break;
				}

				if (tx > 18 || ty > 18 || tx < 0 || ty < 0)
					continue;

				if(this.act_color ^ this.logicBoard[tx, ty].color && !this.logicBoard[tx, ty].empty)
					try
					{
						this.Search(this.logicBoard[tx, ty], !this.act_color);
						//单子互提是不允许的
						if (this.searchList.Count == 1 &&
							(x > 17 || !this.logicBoard[x + 1, y].empty && this.logicBoard[x + 1, y].color ^ this.act_color) &&
							(x < 1  || !this.logicBoard[x - 1, y].empty && this.logicBoard[x - 1, y].color ^ this.act_color) &&
							(y > 17 || !this.logicBoard[x, y + 1].empty && this.logicBoard[x, y + 1].color ^ this.act_color) &&
							(y < 1  || !this.logicBoard[x, y - 1].empty && this.logicBoard[x, y - 1].color ^ this.act_color)
							) {
							this.noEnable = this.logicBoard[tx, ty];
						}

						//记录此步被提棋子
						foreach (MyPoint p in this.searchList)
						{
							step.Push(new StepBack(p ,p.color));
						}

						this.OutChess();
						outChess = true;
					}
					catch (Exception)
					{
						//this.searchList.Clear();
						this.ClearSearchSign();
					}
			}

			this.ClearSearchSign();

			//如果没有提对方子，所下子不可以被包围
			if (!outChess)
			{
				try
				{
					this.Search(this.logicBoard[x, y], this.act_color);
					this.logicBoard[x, y].empty = true;
					return;
				}
				catch (Exception) { }
				finally 
				{
					this.ClearSearchSign();
					//this.searchList.Clear();
				}
			}

			step.Push(new StepBack(this.logicBoard[x, y]));	//记录下子点
			this.stepList.Push(step);
			this.tsmiGoBack.Enabled = true;

			this.g.DrawImage((this.act_color ? this.bChess : this.wChess), x * 32 + 12, y * 32 + 12, 24, 24);

			this.ChangeAct();

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		#endregion

		#region 搜索从某一个点开始的一片棋子的死活
		//如果活子 则抛出异常
		//如果死棋 则正常结束方法，并得到被围死棋子的集合
		private void Search(MyPoint sp, bool color)
		{
			MyPoint last;
			do
			{
				last = sp;

				//if (sp.empty)
				//{
				//    throw new Exception();
				//}

				if (!sp.searched)
				{
					sp.searched = true;
					this.searchList.Add(sp);
					//this.searchedSignList.Add(sp);

					MyPoint tar = null;

					bool cross = false;

					for (int i = 0; i < 4; i++)
					{
						try
						{
							switch (i)
							{
								case 0:
									tar = this.logicBoard[last.x + 1, last.y];
									break;
								case 1:
									tar = this.logicBoard[last.x - 1, last.y];
									break;
								case 2:
									tar = this.logicBoard[last.x, last.y + 1];
									break;
								case 3:
									tar = this.logicBoard[last.x, last.y - 1];
									break;
							}
						}
						catch (Exception) { continue; }

						if (tar.empty)
						{
							throw new Exception();
						}

						if (tar.color == color && !tar.searched)
						if (!cross)
						{
							cross = true;
							sp = tar;
						}
						else
						{
							this.Search(tar, color);
						}
					}
				}
			} while (last != sp);
		}
		#endregion

		#region 提子
		private void OutChess()
		{
			Rectangle temp = new Rectangle(0, 0, 24, 24);
			foreach(MyPoint p in this.searchList)
			{
				p.empty = true;

				int x = 32 * p.x + 12;
				int y = 32 * p.y + 12;
				this.bg.DrawImage(this.backImage, temp, new Rectangle(x, y, 24, 24), GraphicsUnit.Pixel);
				this.g.DrawImage(this.block, x, y, 24, 24);

				p.searched = false;
			}

			this.searchList.Clear();
		}
		#endregion

		#region 清除搜索标记
		private void ClearSearchSign()
		{
			foreach (MyPoint p in this.searchList)
			{
				p.searched = false;
			}

			this.searchList.Clear();
		}
		#endregion

		#region 更改行动方
		private void ChangeAct()
		{
			this.act_color = !this.act_color;
			this.g.DrawImage((this.act_color ? this.bChess : this.wChess), this.backImage.Width - 18, 3, 15, 15);
		}
		#endregion

		#region 悔棋
		private void GoBack(object sender, EventArgs e)
		{
			Stack<StepBack> step = this.stepList.Pop();

			Rectangle temp = new Rectangle(0, 0, 24, 24);

			int x;
			int y;

			while (step.Count != 0)
			{
				StepBack s = step.Pop();
				s.GoBack();

				x = 32 * s.p.x + 12;
				y = 32 * s.p.y + 12;
				if (s.empty)
				{
					this.bg.DrawImage(this.backImage, temp, new Rectangle(x, y, 24, 24), GraphicsUnit.Pixel);
					this.g.DrawImage(this.block, x, y, 24, 24);
				}
				else
				{
					this.g.DrawImage((s.color ? this.bChess : this.wChess), x, y, 24, 24);
				}
			}

			this.ChangeAct();

			this.graphicBoard.Image = this.graphicBoard.Image;

			if (this.stepList.Count == 0)
				this.tsmiGoBack.Enabled = false;
		}
		#endregion

		#region 布局
		private LayoutType layType;
		private bool hasLayout;		//标记是否执行过布局动作(布局后悔棋队列清空)
		private void Layout(object sender, EventArgs e)
		{
			this.graphicBoard.MouseClick -= this.m_click;
			this.graphicBoard.MouseClick += this.Layout_click;
			this.graphicBoard.ContextMenuStrip = this.layMenu;

			this.layType = LayoutType.black;
			this.tsmiSetLayType_black.Checked = true;

			this.hasLayout = false;
		}

		private void Layout_click(object sender, MouseEventArgs e)
		{
			int x, y;

			x = (e.X - 12) / 32;
			y = (e.Y - 12) / 32;

			if (x > 18 || y > 18)
				return;

			this.hasLayout = true;

			switch (this.layType)
			{ 
				case LayoutType.black:
					this.logicBoard[x, y].empty = false;
					this.logicBoard[x, y].color = true;
					this.g.DrawImage(this.bChess, x * 32 + 12, y * 32 + 12, 24, 24);
					break;
				case LayoutType.white:
					this.logicBoard[x, y].empty = false;
					this.logicBoard[x, y].color = false;
					this.g.DrawImage(this.wChess, x * 32 + 12, y * 32 + 12, 24, 24);
					break;
				case LayoutType.empty:
					this.logicBoard[x, y].empty = true;
					int xx = x * 32 + 12;
					int yy = y * 32 + 12;
					this.bg.DrawImage(this.backImage, new Rectangle(0, 0, 24, 24), new Rectangle(xx, yy, 24, 24), GraphicsUnit.Pixel);
					this.g.DrawImage(this.block, xx, yy, 24, 24);
					break;
			}

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		private void setLayType_black(object sender, EventArgs e)
		{
			this.layType = LayoutType.black;
			this.tsmiSetLayType_black.Checked = true;
			this.tsmiSetLayType_white.Checked = false;
			this.tsmiSetLayType_empty.Checked = false;
		}
		private void setLayType_white(object sender, EventArgs e)
		{
			this.layType = LayoutType.white;
			this.tsmiSetLayType_black.Checked = false;
			this.tsmiSetLayType_white.Checked = true;
			this.tsmiSetLayType_empty.Checked = false;
		}
		private void setLayType_empty(object sender, EventArgs e)
		{
			this.layType = LayoutType.empty;
			this.tsmiSetLayType_black.Checked = false;
			this.tsmiSetLayType_white.Checked = false;
			this.tsmiSetLayType_empty.Checked = true;
		}
		#endregion

		#region 结束布局
		private void EndLayout(object sender, EventArgs e)
		{
			this.graphicBoard.MouseClick += this.m_click;
			this.graphicBoard.MouseClick -= this.Layout_click;
			this.graphicBoard.ContextMenuStrip = this.menu;

			this.tsmiSetLayType_white.Checked = false;
			this.tsmiSetLayType_empty.Checked = false;

			if (!this.hasLayout)
				return;

			//处理布局后的死棋
			bool[,] searched = new bool[19, 19];
			for (int i = 0; i < 19; i++)	//非当前活动的颜色
			{
				for (int j = 0; j < 19; j++)
				{
					if (!this.logicBoard[i, j].empty && this.logicBoard[i, j].color ^ this.act_color && !searched[i,j])
					{
						try
						{
							this.Search(this.logicBoard[i, j], !this.act_color);
							foreach (MyPoint p2 in this.searchList)
							{
								searched[p2.x, p2.y] = true;
							}
							this.OutChess();
						}
						catch (Exception)
						{
							foreach (MyPoint p2 in this.searchList)
							{
								searched[p2.x, p2.y] = true;
							}
							this.ClearSearchSign();
							//searched[i, j] = true;
						}
					}
				}
			}

			for (int i = 0; i < 19; i++)	//是当前活动颜色
			{
				for (int j = 0; j < 19; j++)
				{
					if (!this.logicBoard[i, j].empty && this.logicBoard[i, j].color == this.act_color && !searched[i, j])
					{
						try
						{
							this.Search(this.logicBoard[i, j], this.act_color);
							foreach (MyPoint p2 in this.searchList)
							{
								searched[p2.x, p2.y] = true;
							}
							this.OutChess();
						}
						catch (Exception)
						{
							foreach (MyPoint p2 in this.searchList)
							{
								searched[p2.x, p2.y] = true;
							}
							this.ClearSearchSign();
							//searched[i, j] = true;
						}
					}
				}
			}

			//清空悔棋队列
			this.stepList.Clear();
			this.tsmiGoBack.Enabled = false;

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		#endregion

		#region 新开局
		private void NewStart(object sender, EventArgs e)
		{
			if (MessageBox.Show("重新开始？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
			{
				return;
			}

			this.stepList.Clear();
			this.act_color = false;
			this.ChangeAct();
			this.noEnable = null;
			this.tsmiGoBack.Enabled = false;

			foreach (MyPoint p in this.logicBoard)
			{
				p.empty = true;
			}

			this.g.DrawImage(this.backImage, 0, 0, this.backImage.Width, this.backImage.Height);
			this.g.DrawImage(this.bChess, this.backImage.Width - 18, 3, 15, 15);

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		#endregion

		#region 进度
		private void SaveGame(object sender, EventArgs e)
		{
			if (this.saveDialog.ShowDialog() == DialogResult.Cancel)
				return;

			FileStream fs = new FileStream(this.saveDialog.FileName + '_', FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter();

			try
			{
				bf.Serialize(fs, new SaveInfo(this.logicBoard, this.noEnable, this.act_color));
			}
			catch
			{
				MessageBox.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
				fs.Close();
				File.Delete(this.saveDialog.FileName + '_');
				return;
			}

			fs.Close();

			try
			{
				if (File.Exists(this.saveDialog.FileName))
					File.Delete(this.saveDialog.FileName);
				File.Move(this.saveDialog.FileName + '_', this.saveDialog.FileName);
			}
			catch
			{
				MessageBox.Show("保存失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		private void LoadGame(object sender, EventArgs e)
		{
			if (this.openDialog.ShowDialog() == DialogResult.Cancel)
				return;

			FileStream fs = new FileStream(this.openDialog.FileName, FileMode.Open);
			BinaryFormatter bf = new BinaryFormatter();

			SaveInfo sinfo;

			try
			{
				sinfo = (SaveInfo)bf.Deserialize(fs);
			}
			catch (Exception)
			{
				MessageBox.Show("读取失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}
			finally
			{
				fs.Close();
			}

			this.logicBoard = sinfo.logicBoard;
			this.act_color = sinfo.act_color;
			this.noEnable = sinfo.noEnable;

			this.stepList.Clear();
			this.tsmiGoBack.Enabled = false;

			//重绘元素
			this.g.DrawImage(this.backImage,0,0);

			this.g.DrawImage((this.act_color ? this.bChess : this.wChess), this.backImage.Width - 18, 3, 15, 15);

			foreach (MyPoint p in this.logicBoard)
			{
				if (!p.empty)
					this.g.DrawImage((p.color ? this.bChess : this.wChess), p.x * 32 + 12, p.y * 32 + 12, 24, 24);
			}

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		#endregion
	}

	
	class StepBack	//悔棋步骤记录
	{
		public MyPoint p;
		public bool empty;
		public bool color;

		public StepBack(MyPoint p)
		{
			this.p = p;
			this.empty = true;
		}

		public StepBack(MyPoint p, bool color)
		{
			this.p = p;
			this.empty = false;
			this.color = color;
		}

		public void GoBack()
		{
			this.p.empty = this.empty;
			this.p.color = this.color;
		}
	}
	
	[Serializable]
	class MyPoint	//逻辑棋盘点
	{
		public readonly int x;	//逻辑点位
		public readonly int y;
		public bool empty = true;	//标记点位是否不为空
		public bool color;	//标记点位棋子颜色	true 黑   false 白

		public bool searched;	//标记是否已被搜索过

		public MyPoint(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	[Serializable]
	class SaveInfo
	{
		public MyPoint[,] logicBoard;
		public MyPoint noEnable;
		public bool act_color;

		public SaveInfo(MyPoint[,] logicBoard, MyPoint noEnable, bool act_color)
		{
			this.logicBoard = logicBoard;
			this.noEnable = noEnable;
			this.act_color = act_color;
		}
	}

	enum LayoutType
	{ 
		empty,black,white
	}
}
