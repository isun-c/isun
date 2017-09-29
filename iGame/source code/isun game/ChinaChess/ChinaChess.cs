using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Objs;


namespace Games.ChinaChess
{
	public class ChinaChess : Obj
	{
		private PictureBox graphicBoard;
		//private ExitGame exitgame;

		public MyPoint[,] pts = new MyPoint[9, 10];

		private Image checkBox = Image.FromFile("Resource/ChinaChess/checkbox.png");
		private Image actSign_red;		//活动方标识图像
		private Image actSign_black;
		

		private List<Chessman> ches = new List<Chessman>(32);	//棋子集合

		private Image block = new Bitmap(50, 50);	//抠像储存
		private Graphics gBlock;	//抠像画笔
		private Point blockDestination = new Point(0, 0);

		private Chessman act_che;	//当前活动的棋子
		private bool act_color = true;		//当前活动的阵营方   true 红   false 黑

		private bool gameover;		//标记比赛结束

		private Stack<BackNode> stepList = new Stack<BackNode>();	//记录每一步的走法 (悔棋所用)

		private bool RedIsAuto;			//机器智能标志
		private bool BlackIsAuto;		//

		private Chessman redKing;		//双方的将
		//public static Chessman RedKing { get { return ChinaChess.redKing; } }
		private Chessman blackKing;		//
		//public static Chessman BlackKing { get { return ChinaChess.blackKing; } }

		//局面棋子数量
		//private byte r_num_army = 5;
		//private byte r_num_Artillery = 2;
		//private byte r_num_Car = 2;
		//private byte r_num_Horse = 2;
		//private byte r_num_Elephant = 2;
		//private byte r_num_Official = 2;
		//private byte r_num_King = 1;
		//private byte b_num_army = 5;
		//private byte b_num_Artillery = 2;
		//private byte b_num_Car = 2;
		//private byte b_num_Horse = 2;
		//private byte b_num_Elephant = 2;
		//private byte b_num_Official = 2;
		//private byte b_num_King = 1;

		System.Windows.Forms.Timer cpuMoveDriver;	//启动电脑移动

		private ToolStripMenuItem tsmiGoBack;
		private ToolStripMenuItem tsmiAutoRed;
		private ToolStripMenuItem tsmiAutoBlack;
		private ToolStripMenuItem tsmiChooseLeve;

		private ContextMenuStrip menu;
		private ContextMenuStrip layoutMenu;

		private SaveFileDialog saveDialog;
		private OpenFileDialog openDialog;

		#region 构造
		public ChinaChess(PictureBox graphicBoard, ExitGame exitgame)
		{
			this.graphicBoard = graphicBoard;
			this.exitgame = exitgame;

			for (int i = 0; i < 9; i++)
			{
				for (int j = 0; j < 10; j++)
				{
					this.pts[i, j] = new MyPoint(i, j);
				}
			}

			this.backImage = Image.FromFile("Resource/ChinaChess/board.jpg");
			this.board = (Image)this.backImage.Clone();		//将背景图拷贝到要显示的图像控件
			this.g = Graphics.FromImage(this.board);	//将之关联到画笔
			this.gBlock = Graphics.FromImage(this.block);	//背景块画笔

			this.actSign_red = Image.FromFile("Resource/ChinaChess/color_red.gif");
			this.actSign_black = Image.FromFile("Resource/ChinaChess/color_black.gif");

			this.tMove.Tick += this.MoveProcess;	//移动过程绘制方法
			this.tMove.Interval = 5;

			#region 构造 - 初始化菜单
			this.menu = new ContextMenuStrip();
			this.menu.ShowImageMargin = false;
			ToolStripMenuItem item;

			item = new ToolStripMenuItem("悔棋");
			item.Enabled = false;
			item.Click += this.tsmi_GoBack;
			this.menu.Items.Add(item);
			this.tsmiGoBack = item;

			item = new ToolStripMenuItem("设置机器控制");
			{
				ToolStripMenuItem item2;

				item2 = new ToolStripMenuItem("红方");
				item2.Click += this.CpuAutoSet_red;
				item.DropDownItems.Add(item2);
				this.tsmiAutoRed = item2;

				item2 = new ToolStripMenuItem("黑方");
				item2.Click += this.CpuAutoSet_black;
				item.DropDownItems.Add(item2);
				this.tsmiAutoBlack = item2;

				item2 = new ToolStripMenuItem("智能等级");
				{
					ToolStripMenuItem item3;

					for (int i = 1; i < 6; i++)
					{
						item3 = new ToolStripMenuItem(i.ToString());
						item3.Tag = i;
						item3.Click += this.SetCpuLevel;
						item3.Width = 30;
						item2.DropDownItems.Add(item3);
					}
				}
				item.DropDownItems.Add(item2);
				this.tsmiChooseLeve = item2;
			}
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("布局");
			item.Click += this.StartLayout;
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("新开局");
			item.Click += this.NewStart;
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("进度");
			{
				ToolStripMenuItem item2;

				item2 = new ToolStripMenuItem("保存");
				item2.Click += this.tsmiSave_click;
				item.DropDownItems.Add(item2);

				item2 = new ToolStripMenuItem("读取");
				item2.Click += this.tsmiRead_Click;
				item.DropDownItems.Add(item2);
			}
			this.menu.Items.Add(item);

			item = new ToolStripMenuItem("退出");
			item.Click += this.Exit;
			this.menu.Items.Add(item);

			this.layoutMenu = new ContextMenuStrip();
			this.layoutMenu.ShowImageMargin = false;

			item = new ToolStripMenuItem("布局完成");
			item.Click += this.EndLayout;
			this.layoutMenu.Items.Add(item);


			this.graphicBoard.ContextMenuStrip = this.menu;
			#endregion

			this.cpuMoveDriver = new System.Windows.Forms.Timer();
			this.cpuMoveDriver.Interval = 10;
			this.cpuMoveDriver.Tick += this.CpuMoveDriver;

			this.saveDialog = new SaveFileDialog();
			this.saveDialog.Filter = "bin |*.bin";
			this.openDialog = new OpenFileDialog();
			this.openDialog.Filter = "bin |*.bin";

			//初始化棋子
			this.FoundChessman();
			//绘制棋盘
			this.show();
		}
		#endregion

		#region 开始  结束
		public override void Start()
		{
			this.graphicBoard.MouseClick += this.this_mouseClick;
			this.g.DrawImage(this.actSign_red, 12, 12, 20, 20);
		}

		public override void Exit(object sender, EventArgs e)
		{
			if (MessageBox.Show("确定退出", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
				return;

			this.graphicBoard.MouseClick -= this.this_mouseClick;
			this.graphicBoard.MouseDown -= this.layout_MouseDown;
			this.exitgame();
		}
		#endregion


		#region 初始化棋子
		private void FoundChessman()
		{
			Chessman c;

			//添加红方棋子
			this.ches.Add(c = new King(true, this.pts[4, 9])); this.redKing = c;
			this.ches.Add(new Official(true, this.pts[3, 9]));
			this.ches.Add(new Official(true, this.pts[5, 9]));
			this.ches.Add(new Elephant(true, this.pts[2, 9]));
			this.ches.Add(new Elephant(true, this.pts[6, 9]));
			this.ches.Add(new Horse(true, this.pts[1, 9]));
			this.ches.Add(new Horse(true, this.pts[7, 9]));
			this.ches.Add(new Car(true, this.pts[0, 9]));
			this.ches.Add(new Car(true, this.pts[8, 9]));
			this.ches.Add(new Artillery(true, this.pts[1, 7]));
			this.ches.Add(new Artillery(true, this.pts[7, 7]));
			this.ches.Add(new Army(true, this.pts[0, 6]));
			this.ches.Add(new Army(true, this.pts[2, 6]));
			this.ches.Add(new Army(true, this.pts[4, 6]));
			this.ches.Add(new Army(true, this.pts[6, 6]));
			this.ches.Add(new Army(true, this.pts[8, 6]));
			//添加黑方棋子
			this.ches.Add(c = new King(false, this.pts[4, 0])); this.blackKing = c;
			this.ches.Add(new Official(false, this.pts[3, 0]));
			this.ches.Add(new Official(false, this.pts[5, 0]));
			this.ches.Add(new Elephant(false, this.pts[2, 0]));
			this.ches.Add(new Elephant(false, this.pts[6, 0]));
			this.ches.Add(new Horse(false, this.pts[1, 0]));
			this.ches.Add(new Horse(false, this.pts[7, 0]));
			this.ches.Add(new Car(false, this.pts[0, 0]));
			this.ches.Add(new Car(false, this.pts[8, 0]));
			this.ches.Add(new Artillery(false, this.pts[1, 2]));
			this.ches.Add(new Artillery(false, this.pts[7, 2]));
			this.ches.Add(new Army(false, this.pts[0, 3]));
			this.ches.Add(new Army(false, this.pts[2, 3]));
			this.ches.Add(new Army(false, this.pts[4, 3]));
			this.ches.Add(new Army(false, this.pts[6, 3]));
			this.ches.Add(new Army(false, this.pts[8, 3]));

			this.InitialChessman();
		}
		//重置棋子点位
		private void ResetChessmanPoint()
		{ 
			this.ches[0].pt = this.pts[4,9];
			this.ches[1].pt = this.pts[3,9];
			this.ches[2].pt = this.pts[5,9];
			this.ches[3].pt = this.pts[2,9];
			this.ches[4].pt = this.pts[6,9];
			this.ches[5].pt = this.pts[1,9];
			this.ches[6].pt = this.pts[7,9];
			this.ches[7].pt = this.pts[0,9];
			this.ches[8].pt = this.pts[8,9];
			this.ches[9].pt = this.pts[1,7];
			this.ches[10].pt = this.pts[7,7];
			this.ches[11].pt = this.pts[0,6];
			this.ches[12].pt = this.pts[2,6];
			this.ches[13].pt = this.pts[4,6];
			this.ches[14].pt = this.pts[6,6];
			this.ches[15].pt = this.pts[8,6];
			this.ches[16].pt = this.pts[4,0];
			this.ches[17].pt = this.pts[3,0];
			this.ches[18].pt = this.pts[5,0];
			this.ches[19].pt = this.pts[2,0];
			this.ches[20].pt = this.pts[6,0];
			this.ches[21].pt = this.pts[1,0];
			this.ches[22].pt = this.pts[7,0];
			this.ches[23].pt = this.pts[0,0];
			this.ches[24].pt = this.pts[8,0];
			this.ches[25].pt = this.pts[1,2];
			this.ches[26].pt = this.pts[7,2];
			this.ches[27].pt = this.pts[0,3];
			this.ches[28].pt = this.pts[2,3];
			this.ches[29].pt = this.pts[4,3];
			this.ches[30].pt = this.pts[6,3];
			this.ches[31].pt = this.pts[8,3];

			foreach (Chessman c in this.ches)
			{
				c.eaten = false;
			}

			this.InitialChessman();
		}
		private void InitialChessman()
		{
			foreach(Chessman c in this.ches)
			{
				//c.eaten = false;
				//将棋子与点对应
				c.pt.che = c;
				//将棋子背景块保存到棋子对象中
				//this.gBlock.CopyFromScreen(new Point(this.Parent.Location.X + 4 + c.pt.bx, this.Parent.Location.Y + 30 + c.pt.by), this.blockDestination, this.block.Size);
				this.gBlock.DrawImage(this.backImage, this.temprec, new Rectangle(c.pt.bx, c.pt.by, 50, 50), GraphicsUnit.Pixel);
				c.block = (Image)this.block.Clone();
			}
		}
		#endregion

		#region 重绘当前场景
		private void show()
		{
			this.g.DrawImage(this.backImage,0,0);
			foreach (Chessman c in this.ches)
			{
				if(!c.eaten)
					c.show(this.g);
			}

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		#endregion

		#region 棋盘单击事件(选择点位)
		private void this_mouseClick(object sender, MouseEventArgs e)
		{
			int x = (e.X - 33) / 57;
			int y = (e.Y - 31) / 57;

			if(x < 0 || x > 8 || y < 0 || y > 9)
			{
				return;
			}

			//判断做的是何种操作
			if (this.pts[x, y].che != null)
			{
				if (this.act_color == this.pts[x, y].che.color)	//目标是己方棋子
				{
					if (this.act_che != null)
					{
						this.g.DrawImage(this.act_che.block, this.act_che.pt.bx, this.act_che.pt.by, 50, 50);
						this.g.DrawImage(this.act_che.img, this.act_che.pt.bx, this.act_che.pt.by);
					}
					this.g.DrawImage(this.checkBox, this.pts[x, y].bx, this.pts[x, y].by);
					this.act_che = this.pts[x, y].che;
					this.graphicBoard.Image = this.graphicBoard.Image;
					return;
				}
				else if (this.act_che != null)
				{
					if (!this.act_che.MoveMode(x, y, this.pts))
					{
						return;
					}
				}
				else
				{
					return;
				}
			}
			else
			{
				if (this.act_che == null)
				{
					return;
				}

				if (!this.act_che.MoveMode(x, y, this.pts))
				{
					return;
				}
			}

			this.InitialDrawing(x, y);
		}
		#endregion

		#region 绘制动作初始化
		private void InitialDrawing(int x, int y)
		{
			//用棋子对应的背景覆盖棋子
			this.g.DrawImage(this.act_che.block, this.act_che.pt.bx, this.act_che.pt.by, 50, 50);

			//计算棋子移动过程的贞数
			this.stepCount = (int)((Math.Sqrt(Math.Pow(x - this.act_che.pt.x, 2) + Math.Pow(y - this.act_che.pt.y, 2)) + 8) * 2);

			//赋值移动需要的参数以方便timer使用
			this.step_x = (double)(this.pts[x, y].bx - this.act_che.pt.bx) / this.stepCount;
			this.step_y = (double)(this.pts[x, y].by - this.act_che.pt.by) / this.stepCount;
			this.location_x = this.act_che.pt.bx;
			this.location_y = this.act_che.pt.by;

			//移动过程中移除棋盘单击事件
			this.graphicBoard.MouseClick -= this.this_mouseClick;
			//移除悔棋事件
			this.tsmiGoBack.Click -= this.tsmi_GoBack;

			//绘制第一贞，为保证一个timer事件结束前的最后一个绘图动作是绘制棋子
			this.gBlock.DrawImage(this.graphicBoard.Image, temprec, new Rectangle((int)(location_x += step_x), (int)(location_y += step_y), 50, 50), GraphicsUnit.Pixel);
			this.g.DrawImage(this.act_che.img, (int)location_x, (int)location_y, 50, 50);

			this.tMove.Enabled = true;

			//传递目标点位，以做绘制后的逻辑处理
			this.tar_x = x;
			this.tar_y = y;
		}
		#endregion

		#region 绘制移动路径
		private System.Windows.Forms.Timer tMove = new System.Windows.Forms.Timer();
		private int stepCount;
		private double step_x;
		private double step_y;
		private double location_x;
		private double location_y;
		private Rectangle temprec = new Rectangle(0, 0, 50, 50);
		private void MoveProcess(object sender, EventArgs e)
		{
			this.g.DrawImage(this.block, (int)location_x, (int)location_y, 50, 50);
			if (--this.stepCount == 2)
			{
				this.tMove.Enabled = false;
				this.AfterMove();
				return;
			}

			this.gBlock.DrawImage(this.graphicBoard.Image, temprec, new Rectangle((int)(location_x += step_x), (int)(location_y += step_y), 50, 50), GraphicsUnit.Pixel);
			this.g.DrawImage(this.act_che.img, (int)location_x, (int)location_y, 50, 50);

			this.graphicBoard.Image = this.graphicBoard.Image;

		}
		#endregion

		#region 路径完成后的处理，逻辑同步
		private int tar_x;
		private int tar_y;
		private void AfterMove()
		{
			BackNode node = new BackNode();
			//记录这一步移动的棋子
			node.move_che = this.act_che;
			node.mp = this.act_che.pt;
			//记录目标点位
			node.ep = this.pts[tar_x, tar_y];

			if (this.pts[tar_x, tar_y].che != null)
			{
				//记录被吃掉的棋子
				node.eaten_che = this.pts[tar_x, tar_y].che;

				this.pts[tar_x, tar_y].che.eaten = true;
				this.act_che.block = this.pts[tar_x, tar_y].che.block;
				if (pts[tar_x, tar_y].che is King)
				{
					this.gameover = true;
				}
			}
			else
			{
				this.gBlock.DrawImage(this.backImage, new Rectangle(0, 0, 50, 50), new Rectangle(this.pts[tar_x, tar_y].bx, this.pts[tar_x, tar_y].by, 50, 50), GraphicsUnit.Pixel);
				this.act_che.block = (Image)this.block.Clone();
			}

			this.stepList.Push(node);
			this.tsmiGoBack.Enabled = true;

			this.g.DrawImage(this.act_che.img, this.pts[tar_x, tar_y].bx, this.pts[tar_x, tar_y].by);
			this.graphicBoard.Image = this.graphicBoard.Image;
			this.pts[tar_x, tar_y].che = this.act_che;
			this.act_che.pt.che = null;
			this.act_che.pt = this.pts[tar_x, tar_y];

			this.act_che = null;

			if(!this.gameover) this.graphicBoard.MouseClick += this.this_mouseClick;
			this.act_color = !this.act_color;
			this.g.DrawImage((this.act_color ? this.actSign_red : this.actSign_black), 12, 12, 20, 20);

			this.tsmiGoBack.Click += this.tsmi_GoBack;

			//如果打开机器智能 下一步棋由电脑决定
			if(!this.gameover)
				if ((this.act_color && this.RedIsAuto) || (!this.act_color && this.BlackIsAuto))
					this.cpuMoveDriver.Enabled = true;
		}
		#endregion

		#region 执行机器走棋
		//为了不因为电脑搜索而卡在移动过程的最后一帧  使用timer过度
		private void CpuMoveDriver(object sender, EventArgs e)
		{
			this.cpuMoveDriver.Enabled = false;
			this.CpuMove();
		}
		private void CpuMove()
		{
			new Menace(this, this.act_color).Search();

			CpuStep cs;
			if (Menace.nextSteps.Count == 1)
			{
				cs = Menace.nextSteps[0];
			}
			else
			{
				////Dictionary<int, CpuStep> nextStep = new Dictionary<int, CpuStep>();
				//List<int> stepCount = new List<int>();	//记录每一步到对方将的步数

				//foreach (CpuStep c in Menace.nextSteps)
				//{
				//    //nextStep.Add(this.pts[c.x, c.y].che.NearToKing(c, this.act_color ? this.blackKing : this.redKing), c);
				//    stepCount.Add(this.pts[c.x, c.y].che.CountStepToKing(this.pts, c, this.act_color ? this.blackKing : this.redKing));
				//}

				//int min = 998;
				////int index = 0;
				//List<CpuStep> betterSteps = new List<CpuStep>();

				//for (int i = 0; i < stepCount.Count; i++)
				//{
				//    if (min > stepCount[i])
				//    {
				//        min = stepCount[i];
				//        //index = i;
				//        betterSteps.Clear();
				//    }

				//    betterSteps.Add(Menace.nextSteps[i]);
				//}

				////cs = Menace.nextSteps[index];
				//cs = Menace.nextSteps[new Random().Next(betterSteps.Count)];	//随机选择一步得分相等且到王距离相等的棋走

				cs = Menace.nextSteps[new Random().Next(Menace.nextSteps.Count)];
			}

			this.act_che = this.pts[cs.x, cs.y].che;
			this.InitialDrawing(cs.tar_x, cs.tar_y);
		}
		#endregion

		#region 悔棋
		private bool backTwice;
		private void tsmi_GoBack(object sender, EventArgs e)
		{
			if (this.RedIsAuto && this.BlackIsAuto)
			{
				return;
			}

			if (this.stepList.Count == 0)
			{
				this.tsmiGoBack.Enabled = false;
				return;
			}

			BackNode node = this.stepList.Pop();
			node.mp.che = node.move_che;
			node.move_che.pt = node.mp;
			this.gBlock.DrawImage(this.backImage, this.temprec, new Rectangle(node.mp.bx, node.mp.by, 50, 50), GraphicsUnit.Pixel);
			node.move_che.block = (Image)this.block.Clone();

			node.ep.che = node.eaten_che;
			if(node.eaten_che != null) node.eaten_che.eaten = false;

			this.act_color = !this.act_color;
			this.act_che = null;
			this.show();

			this.g.DrawImage((this.act_color ? this.actSign_red : this.actSign_black), 12, 12, 20, 20);

			if (this.stepList.Count == 0)
			{
				this.tsmiGoBack.Enabled = false;
			}

			if (this.RedIsAuto || this.BlackIsAuto)
			{
				if (!this.backTwice && (this.act_color && this.RedIsAuto || !this.act_color && this.BlackIsAuto))
				{
					this.backTwice = true;
					this.tsmi_GoBack(sender, e);
				}
				else
				{
					this.backTwice = false;
				}
			}

			if (this.gameover)
			{
				this.graphicBoard.MouseClick += this.this_mouseClick;
				this.gameover = false;
			}

		}
		#endregion

		#region 新开局
		private void NewStart(object sender, EventArgs e)
		{
			if (this.stepList.Count == 0 && !this.hasLayout)
			{
				return;
			}

			if (MessageBox.Show("确定放弃当前对局？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
			{
				return;
			}

			foreach (MyPoint p in this.pts)
			{
				p.che = null;
			}

			this.stepList.Clear();

			this.ResetChessmanPoint();

			this.act_che = null;
			this.act_color = true;
			//this.gameover = false;

			this.tsmiGoBack.Enabled = false;

			this.hasLayout = false;

			this.show();

			this.g.DrawImage(this.actSign_red, 12, 12, 20, 20);

			if (this.RedIsAuto)
			{
				this.CpuMove();
			}

			if (this.gameover)
			{
				this.graphicBoard.MouseClick += this.this_mouseClick;
				this.gameover = false;
			}
		}
		#endregion

		#region 设置机器智能
		private void CpuAutoSet_red(object sender, EventArgs e)
		{
			if (this.tsmiAutoRed.Checked)
				this.RedIsAuto = false;
			else
			{
				this.RedIsAuto = true;
				if (this.act_color && !this.gameover)
					this.CpuMove();
			}

			this.tsmiAutoRed.Checked = !this.tsmiAutoRed.Checked;
		}
		private void CpuAutoSet_black(object sender, EventArgs e)
		{
			if (this.tsmiAutoBlack.Checked)
				this.BlackIsAuto = false;
			else
			{
				this.BlackIsAuto = true;
				if (!this.act_color && !this.gameover)
					this.CpuMove();
			}

			this.tsmiAutoBlack.Checked = !this.tsmiAutoBlack.Checked;
		}
		private void SetCpuLevel(object sender, EventArgs e)
		{
			ToolStripMenuItem item = ((ToolStripMenuItem)sender);
			Menace.searchDeep = (int)item.Tag;

			foreach (ToolStripMenuItem i in this.tsmiChooseLeve.DropDownItems)
			{
				i.Checked = false;
			}

			item.Checked = true;
		}
		#endregion

		#region 布局管理
		private bool layColor;		//当前布局的棋子的阵营方
		private const int layoutp1 = 75 + 0 * 57;
		private const int layoutp2 = 75 + 1 * 57;
		private const int layoutp3 = 75 + 2 * 57;
		private const int layoutp4 = 75 + 3 * 57;
		private const int layoutp5 = 75 + 4 * 57;
		private const int layoutp6 = 75 + 5 * 57;
		private const int layoutp7 = 75 + 6 * 57;
		private const int layoutp8 = 75 + 7 * 57;
		private bool hasLayout;	//标记布局中是否执行过动作(布局后清空悔棋队列)
		private void StartLayout(object sender, EventArgs e)
		{
			if (this.gameover)
				return;

			this.layColor = true;
			this.hasLayout = false;

			this.graphicBoard.ContextMenuStrip = this.layoutMenu;
			this.graphicBoard.MouseClick -= this.this_mouseClick;
			this.graphicBoard.MouseDown += this.layout_MouseDown;

			this.g.DrawImage(this.backImage, new Rectangle(12, 12, 20, 20), new Rectangle(12, 12, 20, 20), GraphicsUnit.Pixel);

			if (this.act_che != null)
			{
				this.g.DrawImage(this.act_che.block, this.act_che.pt.bx, this.act_che.pt.by, 50, 50);
				this.g.DrawImage(this.act_che.img, this.act_che.pt.bx, this.act_che.pt.by, 50, 50);
				this.act_che = null;
			}

			layout_DrawChess();

			this.graphicBoard.Image = this.graphicBoard.Image;
		}

		private void layout_DrawChess()
		{
			if (this.layColor)
			{
				this.g.DrawImage(Army.r_img, layoutp1, 300, 25, 25);
				this.g.DrawImage(Artillery.r_img, layoutp2, 300, 25, 25);
				this.g.DrawImage(Car.r_img, layoutp3, 300, 25, 25);
				this.g.DrawImage(Horse.r_img, layoutp4, 300, 25, 25);
				this.g.DrawImage(Elephant.r_img, layoutp5, 300, 25, 25);
				this.g.DrawImage(Official.r_img, layoutp6, 300, 25, 25);
				this.g.DrawImage(King.r_img, layoutp7, 300, 25, 25);
				this.g.DrawImage(this.actSign_black, layoutp8, 302, 20, 20);
			}
			else
			{
				this.g.DrawImage(Army.b_img, layoutp1, 300, 25, 25);
				this.g.DrawImage(Artillery.b_img, layoutp2, 300, 25, 25);
				this.g.DrawImage(Car.b_img, layoutp3, 300, 25, 25);
				this.g.DrawImage(Horse.b_img, layoutp4, 300, 25, 25);
				this.g.DrawImage(Elephant.b_img, layoutp5, 300, 25, 25);
				this.g.DrawImage(Official.b_img, layoutp6, 300, 25, 25);
				this.g.DrawImage(King.b_img, layoutp7, 300, 25, 25);
				this.g.DrawImage(this.actSign_red, layoutp8, 302, 20, 20);
			}

			this.graphicBoard.Image = this.graphicBoard.Image;
		}

		private void EndLayout(object sender, EventArgs e)
		{
			this.graphicBoard.ContextMenuStrip = this.menu;
			this.graphicBoard.MouseClick += this.this_mouseClick;
			this.graphicBoard.MouseDown -= this.layout_MouseDown;

			this.g.DrawImage((this.act_color ? this.actSign_red : this.actSign_black), 12, 12, 20, 20);
			this.g.DrawImage(this.backImage, new Rectangle(75, 300, 500, 25), new Rectangle(75, 300, 500, 25),GraphicsUnit.Pixel);

			for (int i = 4; i < 6; i++)	//在河界附近的棋子的图像在布局后重绘
			{
				for (int j = 0; j < 9; j++)
				{
					if (this.pts[j, i].che != null)
						this.pts[j, i].che.show(this.g);
				}
			}

			this.graphicBoard.Image = this.graphicBoard.Image;

			if (this.hasLayout)
			{
				this.stepList.Clear();
				this.tsmiGoBack.Enabled = false;
			}
		}
		#endregion

		#region 布局事件
		private void layout_MouseDown(object sender, MouseEventArgs e)
		{
			this.drogChessman = null;

			int x = e.X;
			int y = e.Y;

			bool add = true;	//是否新增棋子(画背景的方式不同)
			if (y > 299 && y < 321)
			{
				if (x > layoutp1 && x < layoutp1 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is Army && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp2 && x < layoutp2 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is Artillery && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp3 && x < layoutp3 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is Car && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp4 && x < layoutp4 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is Horse && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp5 && x < layoutp5 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is Elephant && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp6 && x < layoutp6 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is Official && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp7 && x < layoutp7 + 25)
				{
					foreach (Chessman c in this.ches)
					{
						if (c is King && c.color == this.layColor && c.eaten)
						{
							this.drogChessman = c;
							this.drogImg = c.img;
							break;
						}
					}
				}
				else if (x > layoutp8 && x < layoutp8 + 20)
				{
					this.layColor = !this.layColor;

					this.layout_DrawChess();
				}
				else
				{
					return;
				}

				if (this.drogChessman == null)
					return;
			}
			else
			{
				x = (e.X - 33) / 57;
				y = (e.Y - 31) / 57;

				if (x < 0 || x > 8 || y < 0 || y > 9)
					return;

				if (this.pts[x, y].che == null)
					return;

				add = false;

				this.drogChessman = this.pts[x, y].che;
				this.drogImg = this.pts[x, y].che.img;
			}

			this.graphicBoard.MouseMove += this.layout_Drog;
			this.graphicBoard.MouseUp += this.layout_MouseUp;
			this.lastx = e.X;
			this.lasty = e.Y;

			if (!add)	//如果拖动已在棋盘上的棋子，先用背景覆盖掉它自身图像
			{
				this.g.DrawImage(this.drogChessman.block, this.drogChessman.pt.bx, this.drogChessman.pt.by, 50, 50);
			}
			this.gBlock.DrawImage(this.graphicBoard.Image, this.temprec, new Rectangle(e.X - 25, e.Y - 25, 50, 50), GraphicsUnit.Pixel);
		}

		//拖动棋子
		private Image drogImg;		//拖动的目标的图像
		private Chessman drogChessman;	//拖动的棋子
		private int lastx, lasty;	//上一帧坐标
		private void layout_Drog(object sender, MouseEventArgs e)
		{
			if (this.drogImg == null)
				return;

			//用背景覆盖上一帧
			this.g.DrawImage(this.block, this.lastx - 25, this.lasty - 25, 50, 50);
			//抓取下一帧背景
			this.gBlock.DrawImage(this.graphicBoard.Image, this.temprec, new Rectangle(e.X - 25, e.Y - 25, 50, 50), GraphicsUnit.Pixel);
			//绘制下一帧棋子
			this.g.DrawImage(this.drogChessman.img, e.X - 25, e.Y - 25, 50, 50);

			this.lastx = e.X;
			this.lasty = e.Y;

			this.graphicBoard.Image = this.graphicBoard.Image;
		}
		//放下棋子
		private void layout_MouseUp(object sender, MouseEventArgs e)
		{
			this.graphicBoard.MouseMove -= this.layout_Drog;
			this.graphicBoard.MouseUp -= this.layout_MouseUp;

			//覆盖移动过程最后一帧图像
			this.g.DrawImage(this.block, this.lastx - 25, this.lasty - 25, 50, 50);

			int x = (e.X - 33) / 57;
			int y = (e.Y - 31) / 57;

			Chessman che = this.drogChessman;

			try
			{
				if (this.pts[x, y].che != null)
					return;

				MyPoint pt = this.pts[x, y];		//目标点位

				if (che is Elephant)
				{
					if (
						(che.color &&
							(
								pt != this.pts[0, 7] &&
								pt != this.pts[2, 9] &&
								pt != this.pts[2, 5] &&
								pt != this.pts[4, 7] &&
								pt != this.pts[6, 9] &&
								pt != this.pts[6, 5] &&
								pt != this.pts[8, 7]
							)
						) ||
						(!che.color &&
								pt != this.pts[0, 2] &&
								pt != this.pts[2, 4] &&
								pt != this.pts[2, 0] &&
								pt != this.pts[4, 2] &&
								pt != this.pts[6, 4] &&
								pt != this.pts[6, 0] &&
								pt != this.pts[8, 2]
						)
					) return;
				}

				if (che is Official)
				{
					if (
						(che.color &&
							(
								pt != this.pts[3, 9] &&
								pt != this.pts[3, 7] &&
								pt != this.pts[4, 8] &&
								pt != this.pts[5, 9] &&
								pt != this.pts[5, 7]
							)
						) ||
						(!che.color &&
								pt != this.pts[3, 0] &&
								pt != this.pts[3, 2] &&
								pt != this.pts[4, 1] &&
								pt != this.pts[5, 0] &&
								pt != this.pts[5, 2]
						)
					) return;
				}

				if (che is King)
				{
					if (pt.x < 3 || pt.x > 5)
						return;

					if (
						(che.color && (pt.y < 7 || pt.y > 9)) ||
						(!che.color && (pt.y < 0 || pt.y > 2))
					) return;
				}

				if (che != null && !che.eaten)
				{
					che.pt.che = null;
				}
				che.pt = this.pts[x, y];
				this.pts[x, y].che = che;
				che.eaten = false;
				this.hasLayout = true;
			}
			catch (Exception)
			{
				if (!che.eaten)
				{
					che.pt.che = null;
				}
				che.eaten = true;
			}
			finally
			{
				if (!che.eaten)
				{
					//this.g.DrawImage(this.block, this.lastx - 25, this.lasty - 25, 50, 50);
					//绘制棋子到目标点位
					this.g.DrawImage(che.img, che.pt.bx, che.pt.by, 50, 50);
					//抓取棋子对应的背景保存至棋子对象
					this.gBlock.DrawImage(this.backImage, this.temprec, new Rectangle(che.pt.bx, che.pt.by, 50, 50), GraphicsUnit.Pixel);
					che.block = (Image)this.block.Clone();

					this.layout_DrawChess();

				}
				this.graphicBoard.Image = this.graphicBoard.Image;
			}
		}
		#endregion

		#region 储存进度
		private void tsmiSave_click(object sender, EventArgs e)
		{
			if (this.saveDialog.ShowDialog() == DialogResult.Cancel)
				return;

			FileStream fs = new FileStream(this.saveDialog.FileName + '_', FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter();

			SaveInfo sinfo = new SaveInfo(this.act_color, this.gameover, this.RedIsAuto, this.BlackIsAuto, this.ches);

			//List<SaveChess> saveInfo = new List<SaveChess>();

			//foreach (Chessman c in this.ches)
			//{
				//if (!c.eaten)
				//{
				//    ChessType type;
				//    if (c is Army) type = ChessType.army;
				//    else if (c is Artillery) type = ChessType.artillery;
				//    else if (c is Car) type = ChessType.car;
				//    else if (c is Horse) type = ChessType.horse;
				//    else if (c is Elephant) type = ChessType.elephant;
				//    else if (c is Official) type = ChessType.official;
				//    else if (c is King) type = ChessType.king;
				//    else continue;

				//    saveInfo.Add(new SaveChess(c.color, c.pt.x, c.pt.y, type));
				//}
			//}

			try
			{
				bf.Serialize(fs, sinfo);
			}
			catch (Exception)
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
		#endregion

		#region 读取进度
		private void tsmiRead_Click(object sender, EventArgs e)
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

			this.ches = sinfo.ches;
			this.BlackIsAuto = sinfo.blackIsAuto;
			this.tsmiAutoBlack.Checked = sinfo.blackIsAuto;
			this.RedIsAuto = sinfo.redIsAuto;
			this.tsmiAutoRed.Checked = sinfo.redIsAuto;
			this.act_color = sinfo.act_color;
			this.g.DrawImage((this.act_color ? this.actSign_red : this.actSign_black), 12, 12, 20, 20);
			if (this.gameover && !sinfo.gameover)
			{
				this.graphicBoard.MouseClick += this.this_mouseClick;
			}
			else if (!this.gameover && sinfo.gameover)
			{
				this.graphicBoard.MouseClick -= this.this_mouseClick;
			}
			this.gameover = sinfo.gameover;

			foreach (MyPoint p in this.pts)		//清除棋盘上所有棋子
			{
				p.che = null;
			}

			foreach (Chessman c in this.ches)	//将图片重新加载到棋子对象
			{
				c.SetImg();
				if(!c.eaten)
					c.pt = this.pts[c.pt.x, c.pt.y];
			}

			this.stepList.Clear();
			this.tsmiGoBack.Enabled = false;

			this.InitialChessman();
			this.show();
		}
		#endregion
	}



	//坐标点
	[Serializable]
	public class MyPoint
	{
		public readonly int x;	//逻辑点位X
		public readonly int y;	//逻辑点位Y
		public readonly int bx;	//像素点位X
		public readonly int by;	//像素点位Y
		public Chessman che;

		public MyPoint(int x,int y)
		{
			this.x = x;
			this.y = y;
			this.bx = 33 + x * 57;
			this.by = 31 + y * 57;
		}
	}

	//记录棋局步骤类
	class BackNode
	{
		public Chessman move_che;	//移动的棋子
		public MyPoint mp;	//移动前的坐点位

		public Chessman eaten_che;	//这一步被吃掉的子
		public MyPoint ep;	//被吃掉的子的点位
	}

	//保存棋子信息
	[Serializable]
	class SaveInfo
	{
		public bool act_color;
		public bool gameover;
		public bool redIsAuto;
		public bool blackIsAuto;

		public List<Chessman> ches;

		public SaveInfo(bool act_color, bool gameover, bool redIsAuto, bool blackIsAuto, List<Chessman> ches)
		{
			this.act_color = act_color;
			this.gameover = gameover;
			this.redIsAuto = redIsAuto;
			this.blackIsAuto = blackIsAuto;
			this.ches = ches;
		}
	}
}

