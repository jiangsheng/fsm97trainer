using FSM97Lib;
using Fsm97Trainer;
using Fsm97Trainer.Models;
using Konsole;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace SchedulingTest
{
    internal static class Program
    {
        static void TestPlayerManagers()
        {
            using (FormMainModel formMainModel = new FormMainModel())
            {
                var teams = formMainModel.GetMenusProcess().ReadTeams();
                var players= formMainModel.GetMenusProcess().ReadPlayers(false);
                foreach (var team in teams)
                {
                    var firstName=team.Data.ManagerFirstName;
                    var lastName=team.Data.ManagerLastName;
                    var playerFound= players.Where(p => p.Data.FirstName == firstName && p.Data.LastName == lastName).FirstOrDefault();
                    if (playerFound!=null)
                    {
                        if(playerFound.TeamNode.Data.Id==team.Data.Id)
                            continue;
                        Debug.WriteLine("Player " + firstName + " " + lastName
                            + " plays for team " + playerFound.TeamNode.Data.Name + " but manages team " + team.Data.Name);
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-CN");
            TestEval();
            //TestTeams();
            //TestEvalPlayer();
            //TestPlayerManagers();
        }
        static void TestTeams()
        {
            using (FormMainModel formMainModel = new FormMainModel())
            {
                var teams = formMainModel.GetMenusProcess().ReadTeams();
                foreach (var team in teams)
                {
                    Debug.WriteLine(team);
                }
            }
        }

        static int lastDisplayedEvalProgress;
        private static void TestEval()
        {
            using (FormMainModel formMainModel = new FormMainModel())
            {
                lastDisplayedEvalProgress = 0;
                formMainModel.AutoTrain = true;
                formMainModel.MaxEvalAge = 19;
                formMainModel.MaxPower = true;
                formMainModel.NoAlternativeTraining = true;
                formMainModel.MaxEnergy = true;
                formMainModel.CurrentLanguage = "zh-CN";
                formMainModel.AlwaysTrainConsistency = false;
                formMainModel.DebugTraining = false;// true;
                //formMainModel.OnFastTimer();
                formMainModel.OnEvalProgressChanged += FormMainModel_OnEvalProgressChanged; 
                try
                {

                    //formMainModel.EvaluateYoungPlayers(PlayerPosition.GK, "吉文", 60);
                    //formMainModel.EvaluateYoungPlayers(PlayerPosition.LWB, string.Empty, 60);
                    formMainModel.EvaluateYoungPlayers(PlayerPosition.Count,string.Empty,19);
                    /*formMainModel.MaxEvalAge = 19;
                    formMainModel.TotalPlayerPositionsToEval=0;
                    formMainModel.EvaluateYoungPlayers(PlayerPosition.FOR, null, 60);*/
                    Debug.WriteLine(formMainModel.EvalYoungPlayersResult);
                    var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".htm";
                    File.WriteAllText(fileName, formMainModel.EvalYoungPlayersResult);
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName)
                    {
                        UseShellExecute = true,
                        FileName = fileName
                    }; Process.Start(processStartInfo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
        private static void TestEvalPlayer()
        {
            List<byte> data = new List<byte>() {
                //movement
                99,  99  ,99
                //health
                ,99,99,99,
                //skill
                86,  99,  63,  99,  98,
                //coolness, awareness
                99,99,
                //tackling
                95,  90,
                //flair
                99,
                //GK
                20,  28,  24,
                //misc
                99,75,43,73,99
            };

            PlayerModelDouble player=new PlayerModelDouble(data);
            player.LastName = "Test";
            player.FirstName = "Test";

            using (FormMainModel formMainModel = new FormMainModel())
            {
                lastDisplayedEvalProgress = 0;
                formMainModel.AutoTrain = true;
                formMainModel.MaxEvalAge = 99;
                formMainModel.MaxPower = true;
                formMainModel.NoAlternativeTraining = false;
                formMainModel.MaxEnergy = true;
                formMainModel.CurrentLanguage = "zh-CN";
                //formMainModel.OnFastTimer();
                formMainModel.OnEvalProgressChanged += FormMainModel_OnEvalProgressChanged;
                
                //formMainModel.MaxEvalAge = 19;
                formMainModel.TotalPlayerPositionsToEval=0;
                formMainModel.EvaluateYoungPlayer(PlayerPosition.LWB, player);
                var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".htm";

                File.WriteAllText(fileName, formMainModel.EvalYoungPlayersResult);
                ProcessStartInfo processStartInfo = new ProcessStartInfo(fileName)
                {
                    UseShellExecute = true,
                    FileName = fileName
                }; Process.Start(processStartInfo);
            }
        }

        static ProgressBar progressBar;
        private static void FormMainModel_OnEvalProgressChanged(object sender, EventArgs e)
        {
            var Model = sender as FormMainModel;

            if (Model.TotalPlayerPositionsToEval > 0)
            {
                if (progressBar == null)
                    progressBar = new ProgressBar(PbStyle.DoubleLine, Model.TotalPlayerPositionsToEval);
            }
            var progress = Model.EvalProgress < Model.TotalPlayerPositionsToEval ? Model.EvalProgress : Model.TotalPlayerPositionsToEval;
            if (Model.TotalPlayerPositionsToEval > 0)
            {
                if (progress - lastDisplayedEvalProgress > Model.TotalPlayerPositionsToEval / 100)
                {
                    progressBar.Refresh(progress, string.Format("{0}/{1}", Model.EvalProgress, Model.TotalPlayerPositionsToEval));
                    lastDisplayedEvalProgress = progress;
                }
            }
        }
    }
}
