namespace IronLadleThermoDetectManageCtrlSystem
{
    partial class Form1
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.StartMeasTempeProcess = new System.Windows.Forms.Button();
            this.dtGrdViewThick = new System.Windows.Forms.DataGridView();
            this.label8 = new System.Windows.Forms.Label();
            this.EndLine = new System.Windows.Forms.Button();
            this.StartLine1 = new System.Windows.Forms.Button();
            this.EndLine1 = new System.Windows.Forms.Button();
            this.btnStartMonitCrnblk = new System.Windows.Forms.Button();
            this.btnStopMonitCrnblk = new System.Windows.Forms.Button();
            this.txtBxThickHistoryInstru = new System.Windows.Forms.TextBox();
            this.grpBxMeasTempThickButtonShow = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.txtBxTempHistory = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dtGrdViewHianCheInstru = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.timertxtBxShow = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dtGrdViewThick)).BeginInit();
            this.grpBxMeasTempThickButtonShow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtGrdViewHianCheInstru)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartMeasTempeProcess
            // 
            this.StartMeasTempeProcess.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StartMeasTempeProcess.Location = new System.Drawing.Point(5, 394);
            this.StartMeasTempeProcess.Margin = new System.Windows.Forms.Padding(2);
            this.StartMeasTempeProcess.Name = "StartMeasTempeProcess";
            this.StartMeasTempeProcess.Size = new System.Drawing.Size(78, 132);
            this.StartMeasTempeProcess.TabIndex = 28;
            this.StartMeasTempeProcess.Text = "存储过程数据";
            this.StartMeasTempeProcess.UseVisualStyleBackColor = true;
            this.StartMeasTempeProcess.Click += new System.EventHandler(this.StartMeasTempe_Click);
            // 
            // dtGrdViewThick
            // 
            this.dtGrdViewThick.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dtGrdViewThick.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dtGrdViewThick.ColumnHeadersHeight = 20;
            this.dtGrdViewThick.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dtGrdViewThick.Location = new System.Drawing.Point(3, 54);
            this.dtGrdViewThick.Margin = new System.Windows.Forms.Padding(2);
            this.dtGrdViewThick.Name = "dtGrdViewThick";
            this.dtGrdViewThick.RowHeadersWidth = 50;
            this.dtGrdViewThick.RowTemplate.Height = 23;
            this.dtGrdViewThick.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dtGrdViewThick.Size = new System.Drawing.Size(1225, 60);
            this.dtGrdViewThick.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(-1, 190);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 12);
            this.label8.TabIndex = 34;
            // 
            // EndLine
            // 
            this.EndLine.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.EndLine.Location = new System.Drawing.Point(101, 394);
            this.EndLine.Margin = new System.Windows.Forms.Padding(2);
            this.EndLine.Name = "EndLine";
            this.EndLine.Size = new System.Drawing.Size(79, 132);
            this.EndLine.TabIndex = 35;
            this.EndLine.Text = "停止储存数据";
            this.EndLine.UseVisualStyleBackColor = true;
            this.EndLine.Click += new System.EventHandler(this.StopMeasTempe_Click);
            // 
            // StartLine1
            // 
            this.StartLine1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.StartLine1.Location = new System.Drawing.Point(5, 230);
            this.StartLine1.Margin = new System.Windows.Forms.Padding(2);
            this.StartLine1.Name = "StartLine1";
            this.StartLine1.Size = new System.Drawing.Size(80, 105);
            this.StartLine1.TabIndex = 36;
            this.StartLine1.Text = "启动测厚过程";
            this.StartLine1.UseVisualStyleBackColor = true;
            this.StartLine1.Click += new System.EventHandler(this.StartLine1_Click);
            // 
            // EndLine1
            // 
            this.EndLine1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.EndLine1.Location = new System.Drawing.Point(101, 230);
            this.EndLine1.Margin = new System.Windows.Forms.Padding(2);
            this.EndLine1.Name = "EndLine1";
            this.EndLine1.Size = new System.Drawing.Size(79, 105);
            this.EndLine1.TabIndex = 37;
            this.EndLine1.Text = "结束测厚过程";
            this.EndLine1.UseVisualStyleBackColor = true;
            this.EndLine1.Click += new System.EventHandler(this.EndLine1_Click);
            // 
            // btnStartMonitCrnblk
            // 
            this.btnStartMonitCrnblk.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStartMonitCrnblk.Location = new System.Drawing.Point(7, 54);
            this.btnStartMonitCrnblk.Name = "btnStartMonitCrnblk";
            this.btnStartMonitCrnblk.Size = new System.Drawing.Size(78, 122);
            this.btnStartMonitCrnblk.TabIndex = 38;
            this.btnStartMonitCrnblk.Text = "启动测温过程";
            this.btnStartMonitCrnblk.UseVisualStyleBackColor = true;
            this.btnStartMonitCrnblk.Click += new System.EventHandler(this.BtnStartMonitCrnblk_Click);
            // 
            // btnStopMonitCrnblk
            // 
            this.btnStopMonitCrnblk.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStopMonitCrnblk.Location = new System.Drawing.Point(101, 54);
            this.btnStopMonitCrnblk.Name = "btnStopMonitCrnblk";
            this.btnStopMonitCrnblk.Size = new System.Drawing.Size(79, 122);
            this.btnStopMonitCrnblk.TabIndex = 38;
            this.btnStopMonitCrnblk.Text = "结束测温过程";
            this.btnStopMonitCrnblk.UseVisualStyleBackColor = true;
            this.btnStopMonitCrnblk.Click += new System.EventHandler(this.btnStopMonitCrnblk_Click);
            // 
            // txtBxThickHistoryInstru
            // 
            this.txtBxThickHistoryInstru.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBxThickHistoryInstru.Location = new System.Drawing.Point(61, 394);
            this.txtBxThickHistoryInstru.Multiline = true;
            this.txtBxThickHistoryInstru.Name = "txtBxThickHistoryInstru";
            this.txtBxThickHistoryInstru.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBxThickHistoryInstru.Size = new System.Drawing.Size(1229, 132);
            this.txtBxThickHistoryInstru.TabIndex = 40;
            // 
            // grpBxMeasTempThickButtonShow
            // 
            this.grpBxMeasTempThickButtonShow.Controls.Add(this.EndLine1);
            this.grpBxMeasTempThickButtonShow.Controls.Add(this.StartLine1);
            this.grpBxMeasTempThickButtonShow.Controls.Add(this.StartMeasTempeProcess);
            this.grpBxMeasTempThickButtonShow.Controls.Add(this.EndLine);
            this.grpBxMeasTempThickButtonShow.Controls.Add(this.btnStartMonitCrnblk);
            this.grpBxMeasTempThickButtonShow.Controls.Add(this.btnStopMonitCrnblk);
            this.grpBxMeasTempThickButtonShow.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.grpBxMeasTempThickButtonShow.Location = new System.Drawing.Point(12, 27);
            this.grpBxMeasTempThickButtonShow.Name = "grpBxMeasTempThickButtonShow";
            this.grpBxMeasTempThickButtonShow.Size = new System.Drawing.Size(207, 544);
            this.grpBxMeasTempThickButtonShow.TabIndex = 42;
            this.grpBxMeasTempThickButtonShow.TabStop = false;
            this.grpBxMeasTempThickButtonShow.Text = "测量操作区";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.Location = new System.Drawing.Point(2, 2);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(1225, 91);
            this.dataGridView1.TabIndex = 23;
            // 
            // txtBxTempHistory
            // 
            this.txtBxTempHistory.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtBxTempHistory.Location = new System.Drawing.Point(63, 133);
            this.txtBxTempHistory.Multiline = true;
            this.txtBxTempHistory.Name = "txtBxTempHistory";
            this.txtBxTempHistory.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtBxTempHistory.Size = new System.Drawing.Size(1228, 136);
            this.txtBxTempHistory.TabIndex = 43;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Controls.Add(this.panel1);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.txtBxThickHistoryInstru);
            this.groupBox1.Controls.Add(this.txtBxTempHistory);
            this.groupBox1.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(235, 27);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1301, 544);
            this.groupBox1.TabIndex = 44;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "过程显示区";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dtGrdViewThick);
            this.panel2.Controls.Add(this.dtGrdViewHianCheInstru);
            this.panel2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panel2.Location = new System.Drawing.Point(61, 272);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1233, 116);
            this.panel2.TabIndex = 49;
            // 
            // dtGrdViewHianCheInstru
            // 
            this.dtGrdViewHianCheInstru.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dtGrdViewHianCheInstru.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dtGrdViewHianCheInstru.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dtGrdViewHianCheInstru.Location = new System.Drawing.Point(3, 4);
            this.dtGrdViewHianCheInstru.Name = "dtGrdViewHianCheInstru";
            this.dtGrdViewHianCheInstru.ReadOnly = true;
            this.dtGrdViewHianCheInstru.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
            this.dtGrdViewHianCheInstru.RowTemplate.Height = 23;
            this.dtGrdViewHianCheInstru.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dtGrdViewHianCheInstru.Size = new System.Drawing.Size(1224, 50);
            this.dtGrdViewHianCheInstru.TabIndex = 12;
            this.dtGrdViewHianCheInstru.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dataGridView1);
            this.panel1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panel1.Location = new System.Drawing.Point(63, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1231, 95);
            this.panel1.TabIndex = 39;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(23, 411);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(37, 99);
            this.button4.TabIndex = 47;
            this.button4.Text = "测厚过程";
            this.button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(23, 152);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(37, 99);
            this.button3.TabIndex = 46;
            this.button3.Text = "测温过程";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(23, 286);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(37, 99);
            this.button2.TabIndex = 45;
            this.button2.Text = "测厚指令";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(37, 94);
            this.button1.TabIndex = 44;
            this.button1.Text = "测温指令";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(344, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 45;
            this.label1.Text = "label1";
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(403, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 46;
            this.label2.Text = "label2";
            this.label2.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(451, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(41, 12);
            this.label3.TabIndex = 47;
            this.label3.Text = "label3";
            this.label3.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(499, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(41, 12);
            this.label4.TabIndex = 48;
            this.label4.Text = "label4";
            this.label4.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(565, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 12);
            this.label5.TabIndex = 49;
            this.label5.Text = "label5";
            this.label5.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(613, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 12);
            this.label6.TabIndex = 50;
            this.label6.Text = "label6";
            this.label6.Visible = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(661, 8);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(41, 12);
            this.label7.TabIndex = 51;
            this.label7.Text = "label7";
            this.label7.Visible = false;
            // 
            // timertxtBxShow
            // 
            this.timertxtBxShow.Interval = 1000;
            this.timertxtBxShow.Tick += new System.EventHandler(this.TimertxtBxShow_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1548, 583);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.grpBxMeasTempThickButtonShow);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "铁包自动化热检数据采集系统";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.dtGrdViewThick)).EndInit();
            this.grpBxMeasTempThickButtonShow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtGrdViewHianCheInstru)).EndInit();
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button StartMeasTempeProcess;
        private System.Windows.Forms.DataGridView dtGrdViewThick;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button EndLine;
        private System.Windows.Forms.Button StartLine1;
        private System.Windows.Forms.Button EndLine1;
        private System.Windows.Forms.Button btnStartMonitCrnblk;
        private System.Windows.Forms.Button btnStopMonitCrnblk;
        private System.Windows.Forms.TextBox txtBxThickHistoryInstru;
        private System.Windows.Forms.GroupBox grpBxMeasTempThickButtonShow;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox txtBxTempHistory;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Timer timertxtBxShow;
        private System.Windows.Forms.DataGridView dtGrdViewHianCheInstru;
        private System.Windows.Forms.Panel panel2;
    }
}

