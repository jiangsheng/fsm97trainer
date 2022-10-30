using Binarysharp.MemoryManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fsm97Trainer
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            
        }
        BindingList<Division> divisions;
        MenusProcess menusProcess = new MenusProcess();
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("MENUS");
                if (processes.Count() > 1)
                {
                    toolStripStatusLabel1.Text = "检测到多个游戏进程 (More than one menus process found)";
                    return;
                }
                else if (processes.Count() == 0)
                {
                    toolStripStatusLabel1.Text = "未检测到游戏进程 (Menuse process not found)";
                    return;
                }
                else
                {
                    toolStripStatusLabel1.Text = String.Empty;
                }

                var gameProcess = processes.First();
                var gameExeFilePath = gameProcess.MainModule.FileName;
                FileInfo fi = new FileInfo(gameExeFilePath);
                switch (fi.Length)
                {
                    case 1378816:
                        menusProcess.SubCountAddress = 0x614610;
                        menusProcess.DivisionFactorAddress = 0x4f3a60;
                        break;
                    case 1135104:
                        menusProcess.SubCountAddress = 0x5846e8;
                        menusProcess.DivisionFactorAddress = 0x4f5178;
                        break;
                    default:
                        toolStripStatusLabel1.Text = "不支持的游戏版本 Unsupported Game version";
                        return;
                }
                if (toolStripButton5Sub.Checked)
                {

                    var sharp = new MemorySharp(gameProcess);
                    sharp.Write<byte>(new IntPtr(menusProcess.SubCountAddress), 5, false);

                }

                if (divisions == null)
                {
                    divisions = LoadDivisions(gameProcess, menusProcess);
                    bindingSource1.DataSource = divisions;
                    bindingSource1.Position = 0;
                    propertyGrid1.SelectedObject = divisions[0].Factors;
                }
                else
                {
                    SaveDivisions(gameProcess, menusProcess);
                }
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = ex.Message;
                return;
            }
        }

        private void SaveDivisions(Process gameProcess, MenusProcess menusProcess)
        {
            var sharp = new MemorySharp(gameProcess);
            int i = 0;
            foreach (var division in divisions)
            {
                Program.CopyProperties<IDivisionFactor>(division.Factors,divisionFactors[i++]);
            }
            sharp.Write<DivisionFactorStruct>(new IntPtr(menusProcess.DivisionFactorAddress), divisionFactors, false);
        }
        DivisionFactorStruct[] divisionFactors;
        private BindingList<Division> LoadDivisions(Process gameProcess, MenusProcess menusProcess)
        {
            var sharp = new MemorySharp(gameProcess);
            divisionFactors=sharp.Read<DivisionFactorStruct>(
                new IntPtr(menusProcess.DivisionFactorAddress), 12, false);

            var divisions = new BindingList<Division>();
            Division division = new Division();
            division.EnglishName = "English Premier";
            division.ChineseName = "英超";
            division.Factors =new DivisionFactor(divisionFactors[0]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "English First";
            division.ChineseName = "英甲";
            division.Factors = new DivisionFactor(divisionFactors[1]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "English Second";
            division.ChineseName = "英乙";
            division.Factors = new DivisionFactor(divisionFactors[2]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "English Third";
            division.ChineseName = "英丙";
            division.Factors = new DivisionFactor(divisionFactors[3]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "German First";
            division.ChineseName = "德甲";
            division.Factors = new DivisionFactor(divisionFactors[4]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "German Second";
            division.ChineseName = "德乙";
            division.Factors = new DivisionFactor(divisionFactors[5]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "French First";
            division.ChineseName = "法甲";
            division.Factors = new DivisionFactor(divisionFactors[6]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "French Second";
            division.ChineseName = "法乙";
            division.Factors = new DivisionFactor(divisionFactors[7]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "Italian First";
            division.ChineseName = "意甲";
            division.Factors = new DivisionFactor(divisionFactors[8]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "Italian Second";
            division.ChineseName = "意乙";
            division.Factors = new DivisionFactor(divisionFactors[9]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "Scottish Premier";
            division.ChineseName = "苏超";
            division.Factors = new DivisionFactor(divisionFactors[10]);
            divisions.Add(division);
            division = new Division();
            division.EnglishName = "Scottish First";
            division.ChineseName = "苏甲";
            division.Factors = new DivisionFactor(divisionFactors[11]);
            divisions.Add(division);
            return divisions;
        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {
            var division = bindingSource1.Current as Division;
            propertyGrid1.SelectedObject = division.Factors;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
           
        }

        private void toolStripButton5Sub_CheckedChanged(object sender, EventArgs e)
        {
            if (toolStripButton5Sub.Checked)
            {
                toolStripButton5Sub.Font = new Font(toolStripButton5Sub.Font, FontStyle.Bold);
                toolStripButton5Sub.Text = "总是5替补(always 5 subs)";
            }
            else
            {
                toolStripButton5Sub.Font = new Font(toolStripButton5Sub.Font, FontStyle.Regular);
                toolStripButton5Sub.Text = "3/5替补(3/5 subs)";
            }
        }
    }
}
