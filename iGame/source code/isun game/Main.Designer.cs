namespace iTopS2CSharp
{
	partial class Main
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
			this.graphicBoard = new System.Windows.Forms.PictureBox();
			this.timerDriver = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.graphicBoard)).BeginInit();
			this.SuspendLayout();
			// 
			// graphicBoard
			// 
			this.graphicBoard.BackColor = System.Drawing.Color.Transparent;
			this.graphicBoard.Dock = System.Windows.Forms.DockStyle.Fill;
			this.graphicBoard.Location = new System.Drawing.Point(0, 0);
			this.graphicBoard.Name = "graphicBoard";
			this.graphicBoard.Size = new System.Drawing.Size(424, 282);
			this.graphicBoard.TabIndex = 0;
			this.graphicBoard.TabStop = false;
			// 
			// Main
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.DimGray;
			this.ClientSize = new System.Drawing.Size(424, 282);
			this.Controls.Add(this.graphicBoard);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Main";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "冰阳游戏";
			this.TransparencyKey = System.Drawing.Color.DimGray;
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
			this.Load += new System.EventHandler(this.Main_Load);
			((System.ComponentModel.ISupportInitialize)(this.graphicBoard)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.PictureBox graphicBoard;
		private System.Windows.Forms.Timer timerDriver;

	}
}

