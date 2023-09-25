namespace REngine.Sandbox
{
	partial class SampleForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.pSamplesList = new System.Windows.Forms.ListBox();
			this.pLoadSample = new System.Windows.Forms.Button();
			this.pSwapChainField = new System.Windows.Forms.Panel();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.pSwapChainField);
			this.splitContainer1.Size = new System.Drawing.Size(837, 461);
			this.splitContainer1.SplitterDistance = 229;
			this.splitContainer1.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.pSamplesList);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.pLoadSample);
			this.splitContainer2.Size = new System.Drawing.Size(229, 461);
			this.splitContainer2.SplitterDistance = 428;
			this.splitContainer2.TabIndex = 2;
			// 
			// pSamplesList
			// 
			this.pSamplesList.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pSamplesList.FormattingEnabled = true;
			this.pSamplesList.ItemHeight = 15;
			this.pSamplesList.Location = new System.Drawing.Point(0, 0);
			this.pSamplesList.Name = "pSamplesList";
			this.pSamplesList.Size = new System.Drawing.Size(229, 428);
			this.pSamplesList.TabIndex = 0;
			// 
			// pLoadSample
			// 
			this.pLoadSample.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pLoadSample.Location = new System.Drawing.Point(0, 0);
			this.pLoadSample.Name = "pLoadSample";
			this.pLoadSample.Size = new System.Drawing.Size(229, 29);
			this.pLoadSample.TabIndex = 1;
			this.pLoadSample.Text = "Load";
			this.pLoadSample.UseVisualStyleBackColor = true;
			this.pLoadSample.Click += new System.EventHandler(this.HandleLoadSample);
			// 
			// pSwapChainField
			// 
			this.pSwapChainField.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pSwapChainField.Location = new System.Drawing.Point(0, 0);
			this.pSwapChainField.Name = "pSwapChainField";
			this.pSwapChainField.Size = new System.Drawing.Size(604, 461);
			this.pSwapChainField.TabIndex = 0;
			// 
			// SampleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(837, 461);
			this.Controls.Add(this.splitContainer1);
			this.Name = "SampleForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "REngine - Samples";
			this.Load += new System.EventHandler(this.HandleLoad);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private SplitContainer splitContainer1;
		private SplitContainer splitContainer2;
		private ListBox pSamplesList;
		private Button pLoadSample;
		private Panel pSwapChainField;
	}
}