using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Objs
{
	public abstract class Obj
	{
		protected Image board;
		public Image Board
		{
			get { return this.board; }
		}
		protected Image backImage;
		protected Graphics g;

		protected ExitGame exitgame;

		public int Width
		{
			get { return this.backImage.Width; }
		}
		public int Height
		{
			get { return this.backImage.Height; }
		}


		public abstract void Start();
		public abstract void Exit(object sender, EventArgs e);
	}

	public delegate void ExitGame ();
}
