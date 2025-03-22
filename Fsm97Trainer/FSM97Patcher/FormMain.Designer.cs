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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.labelSelectGameExePath = new System.Windows.Forms.Label();
            this.textBoxExePath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonPatch = new System.Windows.Forms.Button();
            this.buttonUnpatch = new System.Windows.Forms.Button();
            this.labelGameVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.checkBoxTrainingMatchNongativeEffect = new System.Windows.Forms.CheckBox();
            this.checkBoxTrainingEffectX2 = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.checkBoxThrowInImprovesThrowing = new System.Windows.Forms.CheckBox();
            this.checkBoxShootingTraingGreed = new System.Windows.Forms.CheckBox();
            this.checkBoxPassingMatchImproveLeadership = new System.Windows.Forms.CheckBox();
            this.checkBoxImproveSpeed = new System.Windows.Forms.CheckBox();
            this.checkBoxKickingImprovesSpeed = new System.Windows.Forms.CheckBox();
            this.checkBoxHandlingImprovesAgility = new System.Windows.Forms.CheckBox();
            this.checkBoxHeadingImprovesDetermination = new System.Windows.Forms.CheckBox();
            this.buttonShowTrainingEffects = new System.Windows.Forms.Button();
            this.comboBoxLanguages = new System.Windows.Forms.ComboBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelSelectGameExePath
            // 
            resources.ApplyResources(this.labelSelectGameExePath, "labelSelectGameExePath");
            this.labelSelectGameExePath.Name = "labelSelectGameExePath";
            // 
            // textBoxExePath
            // 
            resources.ApplyResources(this.textBoxExePath, "textBoxExePath");
            this.textBoxExePath.Name = "textBoxExePath";
            this.textBoxExePath.TextChanged += new System.EventHandler(this.textBoxExePath_TextChanged);
            // 
            // buttonBrowse
            // 
            resources.ApplyResources(this.buttonBrowse, "buttonBrowse");
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonPatch
            // 
            resources.ApplyResources(this.buttonPatch, "buttonPatch");
            this.buttonPatch.Name = "buttonPatch";
            this.buttonPatch.UseVisualStyleBackColor = true;
            this.buttonPatch.Click += new System.EventHandler(this.buttonPatch_Click);
            // 
            // buttonUnpatch
            // 
            resources.ApplyResources(this.buttonUnpatch, "buttonUnpatch");
            this.buttonUnpatch.Name = "buttonUnpatch";
            this.buttonUnpatch.UseVisualStyleBackColor = true;
            this.buttonUnpatch.Click += new System.EventHandler(this.buttonUnpatch_Click);
            // 
            // labelGameVersion
            // 
            resources.ApplyResources(this.labelGameVersion, "labelGameVersion");
            this.labelGameVersion.Name = "labelGameVersion";
            // 
            // labelCopyright
            // 
            resources.ApplyResources(this.labelCopyright, "labelCopyright");
            this.labelCopyright.Name = "labelCopyright";
            // 
            // checkBoxTrainingMatchNongativeEffect
            // 
            this.checkBoxTrainingMatchNongativeEffect.Checked = true;
            this.checkBoxTrainingMatchNongativeEffect.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxTrainingMatchNongativeEffect, "checkBoxTrainingMatchNongativeEffect");
            this.checkBoxTrainingMatchNongativeEffect.Name = "checkBoxTrainingMatchNongativeEffect";
            this.checkBoxTrainingMatchNongativeEffect.UseVisualStyleBackColor = true;
            // 
            // checkBoxTrainingEffectX2
            // 
            resources.ApplyResources(this.checkBoxTrainingEffectX2, "checkBoxTrainingEffectX2");
            this.checkBoxTrainingEffectX2.Checked = true;
            this.checkBoxTrainingEffectX2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTrainingEffectX2.Name = "checkBoxTrainingEffectX2";
            this.checkBoxTrainingEffectX2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.Controls.Add(this.buttonBrowse, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxExePath, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labelSelectGameExePath, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxThrowInImprovesThrowing, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxShootingTraingGreed, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxPassingMatchImproveLeadership, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTrainingMatchNongativeEffect, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTrainingEffectX2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxImproveSpeed, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.labelCopyright, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.buttonPatch, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.buttonUnpatch, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.labelGameVersion, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxKickingImprovesSpeed, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxHandlingImprovesAgility, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxHeadingImprovesDetermination, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonShowTrainingEffects, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxLanguages, 2, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // checkBoxThrowInImprovesThrowing
            // 
            resources.ApplyResources(this.checkBoxThrowInImprovesThrowing, "checkBoxThrowInImprovesThrowing");
            this.checkBoxThrowInImprovesThrowing.Checked = true;
            this.checkBoxThrowInImprovesThrowing.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxThrowInImprovesThrowing.Name = "checkBoxThrowInImprovesThrowing";
            this.checkBoxThrowInImprovesThrowing.UseVisualStyleBackColor = true;
            // 
            // checkBoxShootingTraingGreed
            // 
            this.checkBoxShootingTraingGreed.Checked = true;
            this.checkBoxShootingTraingGreed.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxShootingTraingGreed, "checkBoxShootingTraingGreed");
            this.checkBoxShootingTraingGreed.Name = "checkBoxShootingTraingGreed";
            this.checkBoxShootingTraingGreed.UseVisualStyleBackColor = true;
            // 
            // checkBoxPassingMatchImproveLeadership
            // 
            this.checkBoxPassingMatchImproveLeadership.Checked = true;
            this.checkBoxPassingMatchImproveLeadership.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxPassingMatchImproveLeadership, "checkBoxPassingMatchImproveLeadership");
            this.checkBoxPassingMatchImproveLeadership.Name = "checkBoxPassingMatchImproveLeadership";
            this.checkBoxPassingMatchImproveLeadership.UseVisualStyleBackColor = true;
            // 
            // checkBoxImproveSpeed
            // 
            this.checkBoxImproveSpeed.Checked = true;
            this.checkBoxImproveSpeed.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxImproveSpeed, "checkBoxImproveSpeed");
            this.checkBoxImproveSpeed.Name = "checkBoxImproveSpeed";
            this.checkBoxImproveSpeed.UseVisualStyleBackColor = true;
            // 
            // checkBoxKickingImprovesSpeed
            // 
            this.checkBoxKickingImprovesSpeed.Checked = true;
            this.checkBoxKickingImprovesSpeed.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxKickingImprovesSpeed, "checkBoxKickingImprovesSpeed");
            this.checkBoxKickingImprovesSpeed.Name = "checkBoxKickingImprovesSpeed";
            this.checkBoxKickingImprovesSpeed.UseVisualStyleBackColor = true;
            // 
            // checkBoxHandlingImprovesAgility
            // 
            this.checkBoxHandlingImprovesAgility.Checked = true;
            this.checkBoxHandlingImprovesAgility.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxHandlingImprovesAgility, "checkBoxHandlingImprovesAgility");
            this.checkBoxHandlingImprovesAgility.Name = "checkBoxHandlingImprovesAgility";
            this.checkBoxHandlingImprovesAgility.UseVisualStyleBackColor = true;
            // 
            // checkBoxHeadingImprovesDetermination
            // 
            this.checkBoxHeadingImprovesDetermination.Checked = true;
            this.checkBoxHeadingImprovesDetermination.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.checkBoxHeadingImprovesDetermination, "checkBoxHeadingImprovesDetermination");
            this.checkBoxHeadingImprovesDetermination.Name = "checkBoxHeadingImprovesDetermination";
            this.checkBoxHeadingImprovesDetermination.UseVisualStyleBackColor = true;
            // 
            // buttonShowTrainingEffects
            // 
            resources.ApplyResources(this.buttonShowTrainingEffects, "buttonShowTrainingEffects");
            this.buttonShowTrainingEffects.Name = "buttonShowTrainingEffects";
            this.buttonShowTrainingEffects.UseVisualStyleBackColor = true;
            this.buttonShowTrainingEffects.Click += new System.EventHandler(this.buttonShowTrainingEffects_Click);
            // 
            // comboBoxLanguages
            // 
            resources.ApplyResources(this.comboBoxLanguages, "comboBoxLanguages");
            this.comboBoxLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
            this.comboBoxLanguages.FormattingEnabled = true;
            this.comboBoxLanguages.Items.AddRange(new object[] {
            resources.GetString("comboBoxLanguages.Items"),
            resources.GetString("comboBoxLanguages.Items1")});
            this.comboBoxLanguages.Name = "comboBoxLanguages";
            this.comboBoxLanguages.SelectedIndexChanged += new System.EventHandler(this.comboBoxLanguages_SelectedIndexChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "menus.exe";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.FileName = "trainingEffects.csv";
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FormMain";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSelectGameExePath;
        private System.Windows.Forms.TextBox textBoxExePath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonPatch;
        private System.Windows.Forms.Button buttonUnpatch;
        private System.Windows.Forms.Label labelGameVersion;
        private System.Windows.Forms.Label labelCopyright;
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
        private System.Windows.Forms.Button buttonShowTrainingEffects;
        private System.Windows.Forms.ComboBox comboBoxLanguages;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}

