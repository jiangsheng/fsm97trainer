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
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "选择游戏执行文件(Select Game Exe File)";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(230, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(465, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(702, 5);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "浏览(Browse)";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonPatch
            // 
            this.buttonPatch.Enabled = false;
            this.buttonPatch.Location = new System.Drawing.Point(218, 223);
            this.buttonPatch.Name = "buttonPatch";
            this.buttonPatch.Size = new System.Drawing.Size(135, 23);
            this.buttonPatch.TabIndex = 3;
            this.buttonPatch.Text = "打补丁(Patch)";
            this.buttonPatch.UseVisualStyleBackColor = true;
            this.buttonPatch.Click += new System.EventHandler(this.buttonPatch_Click);
            // 
            // buttonUnpatch
            // 
            this.buttonUnpatch.Enabled = false;
            this.buttonUnpatch.Location = new System.Drawing.Point(402, 223);
            this.buttonUnpatch.Name = "buttonUnpatch";
            this.buttonUnpatch.Size = new System.Drawing.Size(130, 23);
            this.buttonUnpatch.TabIndex = 4;
            this.buttonUnpatch.Text = "卸载补丁(Unpatch)";
            this.buttonUnpatch.UseVisualStyleBackColor = true;
            this.buttonUnpatch.Click += new System.EventHandler(this.buttonUnpatch_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 223);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "游戏版本";
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
            this.label3.Location = new System.Drawing.Point(595, 233);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(169, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "@2022 Sheng Jiang @jiangsheng";
            // 
            // checkBoxTrainingMatchNongativeEffect
            // 
            this.checkBoxTrainingMatchNongativeEffect.Checked = true;
            this.checkBoxTrainingMatchNongativeEffect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTrainingMatchNongativeEffect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxTrainingMatchNongativeEffect.Location = new System.Drawing.Point(3, 3);
            this.checkBoxTrainingMatchNongativeEffect.Name = "checkBoxTrainingMatchNongativeEffect";
            this.checkBoxTrainingMatchNongativeEffect.Size = new System.Drawing.Size(770, 28);
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
            this.checkBoxTrainingEffectX2.Location = new System.Drawing.Point(3, 37);
            this.checkBoxTrainingEffectX2.Name = "checkBoxTrainingEffectX2";
            this.checkBoxTrainingEffectX2.Size = new System.Drawing.Size(770, 28);
            this.checkBoxTrainingEffectX2.TabIndex = 9;
            this.checkBoxTrainingEffectX2.Text = "训练效果X2 (Training Effect X2)";
            this.checkBoxTrainingEffectX2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxTrainingEffectX2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.checkBoxPassingMatchImproveLeadership, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxThrowInImprovesThrowing, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTrainingMatchNongativeEffect, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTrainingEffectX2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxShootingTraingGreed, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 31);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(776, 172);
            this.tableLayoutPanel1.TabIndex = 10;
            // 
            // checkBoxPassingMatchImproveLeadership
            // 
            this.checkBoxPassingMatchImproveLeadership.Checked = true;
            this.checkBoxPassingMatchImproveLeadership.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxPassingMatchImproveLeadership.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxPassingMatchImproveLeadership.Location = new System.Drawing.Point(3, 139);
            this.checkBoxPassingMatchImproveLeadership.Name = "checkBoxPassingMatchImproveLeadership";
            this.checkBoxPassingMatchImproveLeadership.Size = new System.Drawing.Size(770, 30);
            this.checkBoxPassingMatchImproveLeadership.TabIndex = 12;
            this.checkBoxPassingMatchImproveLeadership.Text = "传球、比赛改善领导(Passing and Training Match improves Leadership）";
            this.checkBoxPassingMatchImproveLeadership.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxPassingMatchImproveLeadership.UseVisualStyleBackColor = true;
            // 
            // checkBoxThrowInImprovesThrowing
            // 
            this.checkBoxThrowInImprovesThrowing.AutoSize = true;
            this.checkBoxThrowInImprovesThrowing.Checked = true;
            this.checkBoxThrowInImprovesThrowing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThrowInImprovesThrowing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxThrowInImprovesThrowing.Location = new System.Drawing.Point(3, 71);
            this.checkBoxThrowInImprovesThrowing.Name = "checkBoxThrowInImprovesThrowing";
            this.checkBoxThrowInImprovesThrowing.Size = new System.Drawing.Size(770, 28);
            this.checkBoxThrowInImprovesThrowing.TabIndex = 10;
            this.checkBoxThrowInImprovesThrowing.Text = "掷球、比赛改善发边线球能力(Throwing and Training Match improves ThrowIn)";
            this.checkBoxThrowInImprovesThrowing.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxThrowInImprovesThrowing.UseVisualStyleBackColor = true;
            // 
            // checkBoxShootingTraingGreed
            // 
            this.checkBoxShootingTraingGreed.Checked = true;
            this.checkBoxShootingTraingGreed.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShootingTraingGreed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkBoxShootingTraingGreed.Location = new System.Drawing.Point(3, 105);
            this.checkBoxShootingTraingGreed.Name = "checkBoxShootingTraingGreed";
            this.checkBoxShootingTraingGreed.Size = new System.Drawing.Size(770, 28);
            this.checkBoxShootingTraingGreed.TabIndex = 13;
            this.checkBoxShootingTraingGreed.Text = "射门、比赛改善得分欲 (Shooting and Training Match improvesGreed)";
            this.checkBoxShootingTraingGreed.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBoxShootingTraingGreed.UseVisualStyleBackColor = true;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 258);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.labelGameVersion);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonUnpatch);
            this.Controls.Add(this.buttonPatch);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
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
    }
}

