using FSM97Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FSM97Patcher
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }
        int detectAddressPatch = 0;
        int trainingEffectAddress = 0;
        byte[] patched = new byte[] { 0xb8, 0x5, 0x0, 0x0, 0x0 };
        byte[] unpatched = new byte[] { 0x8b, 0xc8, 0xff, 0x52, 0x40 };
        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.FileName = "menus.exe";
                ofd.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
                ofd.Title = "选择游戏可执行文件(Select Game Exe File)";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    
                    textBox1.Text = ofd.FileName; 
                    OnNewFileEntered();
                }
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            OnNewFileEntered();
        }


        private int CompareBits(byte[] buffer1, int buffer1Offset, byte[] buffer2, int buffer2Offset, int length)
        {
            for (int i = 0; i < length; i++)
            { 
                int result= buffer1[buffer1Offset + i]- buffer2[buffer2Offset+i] ;
                if (result != 0) return result;
            }
            return 0;
        }

        private void buttonPatch_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(textBox1.Text))
                {
                    MessageBox.Show(String.Format("无法打开文件 (Cannot open file):{0}",textBox1.Text));
                }
                using (Stream outStream = File.Open(textBox1.Text, FileMode.Open))
                {
                    outStream.Seek(detectAddressPatch, SeekOrigin.Begin);
                    outStream.Write(patched,0,patched.Length);
                    var patchSchedulingEffect = TrainingScheduleEffect.GetTrainingScheduleEffect(GetTrainingEffectModifier());
                    if (trainingEffectAddress != 0)
                    {
                        outStream.Seek(trainingEffectAddress, SeekOrigin.Begin);
                        outStream.Write(patchSchedulingEffect, 0, patchSchedulingEffect.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            OnNewFileEntered();
        }

        private TrainingEffectModifier GetTrainingEffectModifier()
        {
            TrainingEffectModifier result = new TrainingEffectModifier();
            result.TrainingEffectX2 = checkBoxTrainingEffectX2.Checked;
            result.ThrowingTrainThrowIn = checkBoxThrowInImprovesThrowing.Checked;
            result.ShootingTrainGreed=checkBoxShootingTraingGreed.Checked;
            result.PassingTrainLeadership = checkBoxPassingMatchImproveLeadership.Checked;
            result.RemoveNegativeTraining = checkBoxTrainingMatchNongativeEffect.Checked; 
            return result;
        }

        private void OnNewFileEntered()
        {
            string gameVersion;
            string patchVersion;
            try
            {
                FileInfo fi = new FileInfo(textBox1.Text);
                int detectAddress1 = 0;
                int detectAddress2 = 0;

                byte[] detect1, detect2;
                switch (fi.Length)
                {
                    case 1378816:
                        detectAddress1 = 0x9e993;
                        detectAddress2 = 0x9e99c;
                        detectAddressPatch = 0x9e997;
                        trainingEffectAddress = 0xe2aa0;
                        detect1 = new byte[] { 0xf9, 0xff, 0x8b, 0x10 };
                        detect2 = new byte[] { 0xa3, 0x10, 0x46, 0x61 };                        
                        gameVersion = "中文版 (Chinese Ver)";
                        break;
                    case 1135104:
                        gameVersion = "英文版 (English Ver)";
                        //004A89D7 - 8B C8  -mov ecx,eax
                        //004A89D9 - FF 52 40 - call dword ptr[edx + 40]
                        //004A89DC - A3 E8465800 - mov[MENUS.EXE + 1846E8],eax 
                        detectAddress1 = 0xa7dd3;
                        detectAddress2 = 0xa7ddc;
                        detectAddressPatch = 0xa7dd7;
                        detect1 = new byte[] { 0xfc, 0xff, 0x8b, 0x10 };
                        detect2 = new byte[] { 0xa3, 0xe8, 0x46, 0x58};
                        break;
                    default:
                        gameVersion = null;
                        labelGameVersion.Text = "不支持的游戏版本 Unsupported Game version 文件大小错误";
                        return;
                }
                var gamebinary = File.ReadAllBytes(textBox1.Text);
                int detectResult = CompareBits(gamebinary, detectAddress1, detect1, 0, detect1.Length);
                if (detectResult != 0)
                {
                    labelGameVersion.Text = "不支持的游戏版本 Unsupported Game version 特征码1错误";
                    return;
                }
                detectResult = CompareBits(gamebinary, detectAddress2, detect2, 0, detect2.Length);
                if (detectResult != 0)
                {
                    labelGameVersion.Text = "不支持的游戏版本 Unsupported Game version 特征码2错误";
                    return;
                }
                detectResult = CompareBits(gamebinary, detectAddressPatch, patched, 0, patched.Length);
                if (detectResult != 0)
                {
                    detectResult = CompareBits(gamebinary, detectAddressPatch, unpatched, 0, unpatched.Length);
                    if (detectResult != 0)
                    {
                        patchVersion = "不支持的补丁版本 Unsupported Patch version";
                    }
                    else
                    {
                        patchVersion = "原版 (Original)";
                        buttonPatch.Enabled = true;
                        buttonUnpatch.Enabled = false;
                    }
                }
                else
                {
                    patchVersion = "补丁版 (Patched)";
                    buttonPatch.Enabled = false;
                    buttonUnpatch.Enabled = true;
                }
                labelGameVersion.Text = string.Format("{0} ({1})", gameVersion, patchVersion);
            }
            catch (Exception ex)
            {
                labelGameVersion.Text = ex.Message;
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonUnpatch_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(textBox1.Text))
                {
                    MessageBox.Show(String.Format("无法打开文件 (Cannot open file):{0}", textBox1.Text));
                }
                using (Stream outStream = File.Open(textBox1.Text, FileMode.Open))
                {
                    outStream.Seek(detectAddressPatch, SeekOrigin.Begin);
                    outStream.Write(unpatched, 0, unpatched.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            OnNewFileEntered();

        }
    }
}
