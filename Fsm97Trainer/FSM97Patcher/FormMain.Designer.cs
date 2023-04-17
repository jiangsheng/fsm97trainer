namespace FSM97Patcher
{
    partial class FormMain
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonPatch = new System.Windows.Forms.Button();
            this.buttonUnpatch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.labelGameVersion = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxTrainingMatchNongativeEffect = new System.Windows.Forms.CheckBox();
            this.checkBoxTrainingEffectX2 = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxPassingMatchImproveLeadership = new System.Windows.Forms.CheckBox();
            this.checkBoxThrowInImprovesThrowing = new System.Windows.Forms.CheckBox();
            this.checkBoxShootingTraingGreed = new System.Windows.Forms.CheckBox();
            this.checkBoxImproveSpeed = new System.Windows.Forms.CheckBox();
            this.checkBoxKickingImprovesSpeed = new System.Windows.Forms.CheckBox();
            this.checkBoxHandlingImprovesAgility = new System.Windows.Forms.CheckBox();
            this.checkBoxHeadingImprovesDetermination = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(361, 57);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择游戏执行文件(Select Game Exe File)";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(370, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(361, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonBrowse.Location = new System.Drawing.Point(737, 3);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(363, 51);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "浏览(Browse)";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonPatch
            // 
            this.buttonPatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPatch.Enabled = false;
            this.buttonPatch.Location = new System.Drawing.Point(3, 288);
            this.buttonPatch.Name = "buttonPatch";
            this.buttonPatch.Size = new System.Drawing.Size(361, 53);
            this.buttonPatch.TabIndex = 3;
            this.buttonPatch.Text = "打补丁(Patch)";
            this.buttonPatch.UseVisualStyleBackColor = true;
            this.buttonPatch.Click += new System.EventHandler(this.buttonPatch_Click);
            // 
            // buttonUnpatch
            // 
            this.buttonUnpatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonUnpatch.Enabled = false;
            this.buttonUnpatch.Location = new System.Drawing.Point(370, 288);
            this.buttonUnpatch.Name = "buttonUnpatch";
            this.buttonUnpatch.Size = new System.Drawing.Size(361, 53);
            this.buttonUnpatch.TabIndex = 4;
            this.buttonUnpatch.Text = "卸载补丁(Unpatch)";
            this.buttonUnpatch.UseVisualStyleBackColor = true;
            this.buttonUnpatch.Click += new System.EventHandler(this.buttonUnpatch_Click);
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 228);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(361, 57);
            this.label2.TabIndex = 5;
            this.label2.Text = "游戏版本";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelGameVersion
            // 
            this.labelGameVersion.AutoSize = true;
            this.labelGameVersion.Location = new System.Drawing.Point(230, 37);
            this.labelGameVersion.Name = "labelGameVersion";
            this.labelGameVersion.Size = new System.Drawing.Size(0, 13);
            this.labelGameVersion.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(737, 285);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(363, 59);
            this.label3.TabIndex = 7;
            this.label3.Text = "@2022-2023 Sheng Jiang @jiangsheng";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // checkBoxTrainingMatchNongativeEffect
            // 
            this.checkBoxTrainingMatchNongativeEffect.Checked = true;
            this.checkBoxTrainingMatchNongativeEffect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTrainingMatchNongativeEffect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTrainingMatchNongativeEffect.Location = new System.Drawing.Point(3, 117);
            this.checkBoxTrainingMatchNongativeEffect.Name = "checkBoxTrainingMatchNongativeEffect";
            this.checkBoxTrainingMatchNongativeEffect.Size = new System.Drawing.Size(361, 51);
            this.checkBoxTrainingMatchNongativeEffect.TabIndex = 8;
            this.checkBoxTrainingMatchNongativeEffect.Text = "取消训练负面影响(Remove negative effect in training)";
            this.checkBoxTrainingMatchNongativeEffect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxTrainingMatchNongativeEffect.UseVisualStyleBackColor = true;
            // 
            // checkBoxTrainingEffectX2
            // 
            this.checkBoxTrainingEffectX2.AutoSize = true;
            this.checkBoxTrainingEffectX2.Checked = true;
            this.checkBoxTrainingEffectX2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTrainingEffectX2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTrainingEffectX2.Location = new System.Drawing.Point(370, 117);
            this.checkBoxTrainingEffectX2.Name = "checkBoxTrainingEffectX2";
            this.checkBoxTrainingEffectX2.Size = new System.Drawing.Size(361, 51);
            this.checkBoxTrainingEffectX2.TabIndex = 9;
            this.checkBoxTrainingEffectX2.Text = "训练效果X2 (Training Effect X2)";
            this.checkBoxTrainingEffectX2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxTrainingEffectX2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.buttonBrowse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxThrowInImprovesThrowing, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxShootingTraingGreed, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxPassingMatchImproveLeadership, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTrainingMatchNongativeEffect, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTrainingEffectX2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxImproveSpeed, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.buttonPatch, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.buttonUnpatch, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxKickingImprovesSpeed, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxHandlingImprovesAgility, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxHeadingImprovesDetermination, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1103, 344);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // checkBoxPassingMatchImproveLeadership
            // 
            this.checkBoxPassingMatchImproveLeadership.Checked = true;
            this.checkBoxPassingMatchImproveLeadership.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPassingMatchImproveLeadership.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxPassingMatchImproveLeadership.Location = new System.Drawing.Point(737, 60);
            this.checkBoxPassingMatchImproveLeadership.Name = "checkBoxPassingMatchImproveLeadership";
            this.checkBoxPassingMatchImproveLeadership.Size = new System.Drawing.Size(363, 51);
            this.checkBoxPassingMatchImproveLeadership.TabIndex = 12;
            this.checkBoxPassingMatchImproveLeadership.Text = "传球、无人赛和比赛改善领导(Passing, 5 a side and training matches improves Leadership）";
            this.checkBoxPassingMatchImproveLeadership.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxPassingMatchImproveLeadership.UseVisualStyleBackColor = true;
            // 
            // checkBoxThrowInImprovesThrowing
            // 
            this.checkBoxThrowInImprovesThrowing.AutoSize = true;
            this.checkBoxThrowInImprovesThrowing.Checked = true;
            this.checkBoxThrowInImprovesThrowing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThrowInImprovesThrowing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxThrowInImprovesThrowing.Location = new System.Drawing.Point(3, 60);
            this.checkBoxThrowInImprovesThrowing.Name = "checkBoxThrowInImprovesThrowing";
            this.checkBoxThrowInImprovesThrowing.Size = new System.Drawing.Size(361, 51);
            this.checkBoxThrowInImprovesThrowing.TabIndex = 10;
            this.checkBoxThrowInImprovesThrowing.Text = "掷球、五人赛和比赛改善发边线球能力(Throwing, 5 a side and training match improves ThrowIn)";
            this.checkBoxThrowInImprovesThrowing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxThrowInImprovesThrowing.UseVisualStyleBackColor = true;
            // 
            // checkBoxShootingTraingGreed
            // 
            this.checkBoxShootingTraingGreed.Checked = true;
            this.checkBoxShootingTraingGreed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShootingTraingGreed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxShootingTraingGreed.Location = new System.Drawing.Point(370, 60);
            this.checkBoxShootingTraingGreed.Name = "checkBoxShootingTraingGreed";
            this.checkBoxShootingTraingGreed.Size = new System.Drawing.Size(361, 51);
            this.checkBoxShootingTraingGreed.TabIndex = 13;
            this.checkBoxShootingTraingGreed.Text = "射门、五人赛和比赛改善得分欲 (Shooting, 5 a side and training matches improves Greed)";
            this.checkBoxShootingTraingGreed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxShootingTraingGreed.UseVisualStyleBackColor = true;
            // 
            // checkBoxImproveSpeed
            // 
            this.checkBoxImproveSpeed.Checked = true;
            this.checkBoxImproveSpeed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxImproveSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxImproveSpeed.Location = new System.Drawing.Point(737, 117);
            this.checkBoxImproveSpeed.Name = "checkBoxImproveSpeed";
            this.checkBoxImproveSpeed.Size = new System.Drawing.Size(363, 51);
            this.checkBoxImproveSpeed.TabIndex = 14;
            this.checkBoxImproveSpeed.Text = "练习、慢跑、盯人、区域、五人赛、比赛改善速度和启动\r\nExercise, Jogging, Marking, Zonal Defence, 5 a side an" +
    "d Training Match improves Speed and Acceleration\r\n";
            this.checkBoxImproveSpeed.UseVisualStyleBackColor = true;
            // 
            // checkBoxKickingImprovesSpeed
            // 
            this.checkBoxKickingImprovesSpeed.Checked = true;
            this.checkBoxKickingImprovesSpeed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxKickingImprovesSpeed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxKickingImprovesSpeed.Location = new System.Drawing.Point(3, 174);
            this.checkBoxKickingImprovesSpeed.Name = "checkBoxKickingImprovesSpeed";
            this.checkBoxKickingImprovesSpeed.Size = new System.Drawing.Size(361, 51);
            this.checkBoxKickingImprovesSpeed.TabIndex = 15;
            this.checkBoxKickingImprovesSpeed.Text = "踢球增加速度(Kicking improves speed)";
            this.checkBoxKickingImprovesSpeed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxKickingImprovesSpeed.UseVisualStyleBackColor = true;
            // 
            // checkBoxHandlingImprovesAgility
            // 
            this.checkBoxHandlingImprovesAgility.Checked = true;
            this.checkBoxHandlingImprovesAgility.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHandlingImprovesAgility.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxHandlingImprovesAgility.Location = new System.Drawing.Point(370, 174);
            this.checkBoxHandlingImprovesAgility.Name = "checkBoxHandlingImprovesAgility";
            this.checkBoxHandlingImprovesAgility.Size = new System.Drawing.Size(361, 51);
            this.checkBoxHandlingImprovesAgility.TabIndex = 16;
            this.checkBoxHandlingImprovesAgility.Text = "救球改善敏捷(Goalkeeper handling improves Agility)";
            this.checkBoxHandlingImprovesAgility.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxHandlingImprovesAgility.UseVisualStyleBackColor = true;
            // 
            // checkBoxHeadingImprovesDetermination
            // 
            this.checkBoxHeadingImprovesDetermination.Checked = true;
            this.checkBoxHeadingImprovesDetermination.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHeadingImprovesDetermination.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxHeadingImprovesDetermination.Location = new System.Drawing.Point(737, 174);
            this.checkBoxHeadingImprovesDetermination.Name = "checkBoxHeadingImprovesDetermination";
            this.checkBoxHeadingImprovesDetermination.Size = new System.Drawing.Size(363, 51);
            this.checkBoxHeadingImprovesDetermination.TabIndex = 17;
            this.checkBoxHeadingImprovesDetermination.Text = "头球改善判断(Heading improves Determination)";
            this.checkBoxHeadingImprovesDetermination.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1103, 344);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.labelGameVersion);
            this.Name = "FormMain";
            this.Text = "FSM97 补丁 (FSM97 patch)";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonPatch;
        private System.Windows.Forms.Button buttonUnpatch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelGameVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxTrainingMatchNongativeEffect;
        private System.Windows.Forms.CheckBox checkBoxTrainingEffectX2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox checkBoxThrowInImprovesThrowing;
        private System.Windows.Forms.CheckBox checkBoxPassingMatchImproveLeadership;
        private System.Windows.Forms.CheckBox checkBoxShootingTraingGreed;
        private System.Windows.Forms.CheckBox checkBoxImproveSpeed;
        private System.Windows.Forms.CheckBox checkBoxKickingImprovesSpeed;
        private System.Windows.Forms.CheckBox checkBoxHandlingImprovesAgility;
        private System.Windows.Forms.CheckBox checkBoxHeadingImprovesDetermination;
    }
}

