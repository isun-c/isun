using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Objs;
using Games.WeiQi;
using Games.ChinaChess;

namespace iTopS2CSharp
{

	public partial class Main : Form
	{
		private Obj currentGame;	//当前运行的游戏
		private Panel chooseGame;	//选择游戏面板

		public static Main main;

		#region 构造
		public Main()
		{
			Main.main = this;

			InitializeComponent();

			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Size = new Size(400,250);

			//添加选择游戏面板
			this.chooseGame = new Panel();
			this.chooseGame.BackgroundImage = Image.FromFile("Resource/back.jpg");
			this.chooseGame.Size = new Size(this.chooseGame.BackgroundImage.Width, this.chooseGame.BackgroundImage.Height);
			this.chooseGame.Dock = DockStyle.Fill;
			this.chooseGame.Visible = false;
			this.chooseGame.MouseDown += this.gra_mouseDown;
			this.chooseGame.MouseUp += this.gra_mouseUp;
			this.Controls.Add(this.chooseGame);

			Button btn;

			btn = new Button();
			btn.Text = "中国象棋";
			btn.Tag = 1;
			btn.FlatStyle = FlatStyle.Popup;
			btn.Location = new Point(350, 60);
			btn.BackColor = Color.Transparent;
			btn.Click += this.InitialGame;
			this.chooseGame.Controls.Add(btn);

			btn = new Button();
			btn.Text = "围棋";
			btn.Tag = 2;
			btn.FlatStyle = FlatStyle.Popup;
			btn.Location = new Point(350, 100);
			btn.BackColor = Color.Transparent;
			btn.Click += this.InitialGame;
			this.chooseGame.Controls.Add(btn);

			btn = new Button();
			btn.Text = "退出";
			btn.FlatStyle = FlatStyle.Popup;
			btn.Location = new Point(350, 250);
			btn.BackColor = Color.Transparent;
			btn.Click += this.MainClose;
			this.chooseGame.Controls.Add(btn);
		}
		private void Main_Load(object sender, EventArgs e)
		{
			this.graphicBoard.Image = Image.FromFile("Resource/Ison Games2.png");

			this.timerDriver.Tick += this.timer_Tick;
			this.timerDriver.Interval = 1000;
			this.timerDriver.Enabled = true;
		}
		private void timer_Tick(object sender, EventArgs e)
		{
			this.timerDriver.Enabled = false;
			this.ShowChooseMenu();
		}
		#endregion


		#region 选择面板拖拽
		private int mouse_x;
		private int mouse_y;
		private void gra_mouseDown(object sender, MouseEventArgs e)
		{
			this.chooseGame.MouseMove += this.gra_mouseMove;

			this.mouse_x = e.X;
			this.mouse_y = e.Y;
		}
		private void gra_mouseMove(object sender, MouseEventArgs e)
		{
			this.Location = new Point(this.Location.X + (e.X - this.mouse_x), this.Location.Y + (e.Y - this.mouse_y));
		}
		private void gra_mouseUp(object sender, MouseEventArgs e)
		{
			this.chooseGame.MouseMove -= this.gra_mouseMove;
		}
		#endregion

		#region 退出游戏并显示选择面板
		public void ShowChooseMenu()
		{
			this.currentGame = null;

			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Size = this.chooseGame.Size;

			this.Location = new Point((Screen.PrimaryScreen.Bounds.Width - this.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2);
			this.chooseGame.Visible = true;
			this.chooseGame.BringToFront();
		}
		#endregion

		#region 初始化、开始游戏
		private void InitialGame(object sender, EventArgs e)
		{
			switch ((int)((Button)sender).Tag)
			{ 
				case 1:
					this.currentGame = new ChinaChess(this.graphicBoard, this.ShowChooseMenu);
					break;
				case 2:
					this.currentGame = new WeiQi(this.graphicBoard, this.ShowChooseMenu);
					break;
			}

			this.StartGame();
		}
		private void StartGame()
		{
			this.chooseGame.Visible = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;

			this.Size = new Size(this.currentGame.Width + 6, this.currentGame.Height + 32);

			this.graphicBoard.Image = this.currentGame.Board;

			this.Location = new Point((Screen.PrimaryScreen.Bounds.Width - this.Width) / 2, (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2);

			this.currentGame.Start();
		}
		#endregion

		#region 退出
		private void MainClose(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (this.currentGame != null)
			{
				e.Cancel = true;

				this.currentGame.Exit(null,null);
			}
		}
		#endregion


	}
}
