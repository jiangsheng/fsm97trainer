using Fsm97Trainer;
using Fsm97Trainer.Models;
using Konsole;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SchedulingTest
{
    internal class Program
    {
        static void Main(string[] args)
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


        private static void TestEval()
        {
            using (FormMainModel formMainModel = new FormMainModel())
            {
                formMainModel.AutoTrain = true;
                //formMainModel.DebugTraining = true;
                formMainModel.MaxEvalAge = 19;
                formMainModel.MaxPower = true;
                formMainModel.NoAlternativeTraining = true;
                formMainModel.MaxEnergy = true;

                //formMainModel.OnFastTimer();
                formMainModel.OnEvalProgressChanged += FormMainModel_OnEvalProgressChanged;
                formMainModel.EvaluateYoungPlayers();
                Debug.WriteLine(formMainModel.EvalYoungPlayersResult);
            }
        }

        static ProgressBar progressBar;
        private static void FormMainModel_OnEvalProgressChanged(object sender, EventArgs e)
        {
            var Model = sender as FormMainModel;

            if (Model.TotalPlayerPositionsToEval > 0)
            {
                if(progressBar == null)
                    progressBar = new ProgressBar(PbStyle.DoubleLine, Model.TotalPlayerPositionsToEval);
            }
            var progress= Model.EvalProgress < Model.TotalPlayerPositionsToEval? Model.EvalProgress: Model.TotalPlayerPositionsToEval;
            progressBar.Refresh(progress,string.Format("{0}/{1}",Model.EvalProgress,Model.TotalPlayerPositionsToEval));
        }
    }
}
