using CsvHelper;
using CsvHelper.Configuration;
using FSM97Lib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Fsm97Trainer.Models
{
    public class FormMainModel : IDisposable, INotifyPropertyChanged
    {
        public FormMainModel() { 
            NoAlternativeTraining = true;
        }
        public EventHandler<ErrorEventArgs> OnError;
        public EventHandler<ErrorEventArgs> OnWarning;
        public EventHandler<EventArgs> OnModalMessage;

        private bool disposedValue;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<PropertyChangingCancelEventArgs> PropertyChanging;

        MenusProcess MenusProcess { get; set; }
        public BindingList<PlayerNode> PlayerData { get; set; }
        bool autoTrain;
        /// <summary>
        /// auto adjust traing schedule
        /// </summary>
        public bool AutoTrain
        {
            get => autoTrain;
            set
            {
                if (autoTrain == value)
                {
                    return;
                }

                autoTrain = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoTrain)));
            }
        }
        bool maxEnergy;
        /// <summary>
        /// keep enegy full
        /// </summary>
        public bool MaxEnergy
        {
            get => maxEnergy;
            set
            {
                if (maxEnergy == value)
                {
                    return;
                }

                maxEnergy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxEnergy)));
            }
        }

        bool convertToGK;
        /// <summary>
        /// while on the bench, convert player to GK
        /// to minimize injury risk
        /// </summary>
        public bool ConvertToGK
        {
            get => convertToGK;
            set
            {
                if (convertToGK == value)
                {
                    return;
                }
                var e = new PropertyChangingCancelEventArgs(nameof(ConvertToGK), convertToGK, value);
                PropertyChanging?.Invoke(this, e);
                if (e.Cancel)
                    return;
                convertToGK = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConvertToGK)));
            }
        }

        bool autoResetStatus;
        /// <summary>
        /// forgive yellow/red cards after match
        /// recover from injuries after match
        /// </summary>
        public bool AutoResetStatus
        {
            get => autoResetStatus;
            set
            {
                if (autoResetStatus == value)
                {
                    return;
                }

                autoResetStatus = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoResetStatus)));
            }
        }
        bool maxForm;
        /// <summary>
        /// keep form at max
        /// </summary>
        public bool MaxForm
        {
            get => maxForm;
            set
            {
                if (maxForm == value)
                {
                    return;
                }

                maxForm = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxForm)));
            }
        }
        bool maxMorale;
        /// <summary>
        /// keep morale at max
        /// </summary>
        public bool MaxMorale
        {
            get => maxMorale;
            set
            {
                if (maxMorale == value)
                {
                    return;
                }

                maxMorale = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxMorale)));
            }
        }
        /// <summary>
        /// keep power at max
        /// </summary>
        public bool MaxPower { get; set; }
        /// <summary>
        /// do not train for alternative positions, focus on current position
        /// </summary>
        public bool NoAlternativeTraining { get; set; }
        /// <summary>
        /// automatic contract renewal
        /// </summary>
        public bool ContractAutoRenew { get; set; }
        /// <summary>
        /// remove negative training effects
        /// </summary>
        public bool RemoveNegativeTraining { get; set; }
        /// <summary>
        /// is training effect doubled
        /// </summary>
        public bool TrainingEffectX2 { get; set; }
        /// <summary>
        /// whether the throwing training also improves throw-in
        /// </summary>
        public bool ThrowingTrainThrowIn { get; set; }
        /// <summary>
        /// whether the shooting training also improves greed
        /// </summary>
        public bool ShootingTrainGreed { get; set; }
        /// <summary>
        /// whether the passing training also improves leadership
        /// </summary>
        public bool PassingTrainLeadership { get; set; }
        /// <summary>
        /// saved formation
        /// </summary>
        public Formation SavedFormation { get; set; }
        /// <summary>
        /// whether auto adjust position follows saved formation
        /// formation.
        /// </summary>
        public bool AutoPositionWithFormation { get; set; }
        string errorMessage;
        public string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                if (errorMessage == value)
                    return;
                errorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }
        string modalMessage;
        public string ModalMessage
        {
            get
            {
                return modalMessage;
            }
            set
            {
                if (modalMessage == value)
                    return;
                modalMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ModalMessage)));
            }
        }
        public string CurrentLanguage { get; set; }
        public int RestoreBoundsLeft { get; set; }
        public int RestoreBoundsTop { get; set; }
        public int RestoreBoundsRight { get; set; }
        public int RestoreBoundsBottom { get; set; }
        public bool AlwaysTrainConsistency{ get; set; }

        string evalYoungPlayersResult;

        public string EvalYoungPlayersResult
        {
            get
            {
                return evalYoungPlayersResult;
            }
            set
            {
                if (evalYoungPlayersResult == value)
                    return;
                evalYoungPlayersResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EvalYoungPlayersResult)));
            }
        }

        public int TotalPlayerPositionsToEval {get;set;}

        int evalProgress;
        object evalProgressLock=new object();
        public int EvalProgress { 
            get {
                lock (evalProgressLock)
                {
                    return evalProgress;
                }
            } set 
            {
                lock (evalProgressLock)
                {
                    evalProgress = value;
                }
            } 
        }
        public event EventHandler OnEvalProgressChanged;
        public int MaxEvalAge { get; set; }
        public bool DebugTraining { get; set; }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    MenusProcess?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~FormMainModel()
        {
            //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        void ReportWarning(Exception ex)
        {
            if (this.OnWarning != null)
            {
                ErrorEventArgs args = new ErrorEventArgs(ex);
                this.OnWarning(this, args);
            }
        }
        public void ReportError(Exception ex)
        {
            if (this.OnError != null)
            {
                ErrorEventArgs args = new ErrorEventArgs(ex);
                this.OnError(this, args);
            }
        }
        public MenusProcess GetMenusProcess()
        {
            if (MenusProcess == null)
            {
                MenusProcess = new MenusProcess();
                return MenusProcess;
            }
            else
            {
                try
                {
                    if (!MenusProcess.HasExited())
                        return MenusProcess;
                    MenusProcess = new MenusProcess();
                    return MenusProcess;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    ErrorMessage = ex.Message;
                    MenusProcess = new MenusProcess();
                    return MenusProcess;
                }
            }
        }
        public void ExportPlayerData()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                var players = menusProcess.ReadPlayers(false);
                if (players.Count > 0)
                {
                    using (var saveFileDialog1 = new SaveFileDialog())
                    {
                        saveFileDialog1.FileName = "players.csv";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            CsvConfiguration config = new CsvConfiguration();
                            config.Encoding = Encoding.UTF8;
                            config.CultureInfo = new CultureInfo("zh-hans");
                            using (var writer = new StreamWriter(saveFileDialog1.FileName, false, Encoding.UTF8))
                            using (var csv = new CsvWriter(writer, config))
                            {
                                csv.Configuration.RegisterClassMap<PlayerMap>();
                                csv.WriteHeader<PlayerModel>();
                                foreach (var playerNode in players)
                                {
                                    csv.WriteRecord<PlayerModel>(playerNode.Data);
                                }
                            }
                            ProcessStartInfo psi = new ProcessStartInfo();
                            psi.UseShellExecute = true;
                            psi.FileName = saveFileDialog1.FileName;
                            Process.Start(psi);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        void ShowModalMessage(string modelMeessage)
        {
            this.ModalMessage = modelMeessage;
            if (this.OnModalMessage != null)
            {
                EventArgs args = new EventArgs();
                this.OnModalMessage(this, args);
            }
        }
        public void CopyPlayerData()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                this.PlayerData = new BindingList<PlayerNode>();
                foreach (var playerNode in menusProcess.ReadPlayers(false))
                    this.PlayerData.Add(playerNode);
                ShowModalMessage(Properties.Resources.PlayerDataCopied);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void PastePlayerData()
        {
            if (PlayerData == null)
            {
                MessageBox.Show(Properties.Resources.PleaseCopyPlayerDataFirst);
                return;
            }

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.LoadPlayerData(this.PlayerData.Select(p=>p.Data).ToList());
                MessageBox.Show(Properties.Resources.PlayerDataPasted);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        private LinkedList<PlayerModel> LoadFromCsv()
        {
            var result = new LinkedList<PlayerModel>();
            using (var ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    ofd.FileName = "players.csv";
                    CsvConfiguration config = new CsvConfiguration();
                    config.Encoding = Encoding.UTF8;
                    config.CultureInfo = new CultureInfo("zh-hans");
                    using (var reader = new StreamReader(ofd.FileName, Encoding.UTF8))
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Configuration.RegisterClassMap<PlayerMap>();
                        csv.ReadHeader();
                        foreach (var record in csv.GetRecords<PlayerModel>())
                        {
                            result.AddLast(record);
                        }
                    }
                }
            }
            return result;
        }
        public void ImportPlayerData()
        {
            try
            {
                var players = this.LoadFromCsv();
                if (players.Count > 0)
                {
                    MenusProcess menusProcess = GetMenusProcess();
                    menusProcess.LoadPlayerData(players);
                    MessageBox.Show(Properties.Resources.PlayerDataImported);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void BoostYouthPlayers()
        {
            try
            {
                var result = MessageBox.Show(Properties.Resources.SeasonBeginningOnly, Properties.Resources.Warning, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    MenusProcess menusProcess = GetMenusProcess();
                    menusProcess.BoostYouthPlayer(false);
                    MessageBox.Show(Properties.Resources.YouthPlayerDataBoosted);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void Rotate(RotateMethod rotateMethod)
        {
            bool convertToGK = this.ConvertToGK;
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var result = MessageBox.Show(Properties.Resources.PleaseSwitchToTheTrainingSchedulePageFirstContinue,
                        Properties.Resources.Warning, MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        if (this.AutoPositionWithFormation && this.SavedFormation.IsValid())
                        {
                            menusProcess.RotatePlayer(rotateMethod, this.SavedFormation, convertToGK);
                        }
                        else
                            menusProcess.RotatePlayer(rotateMethod, null, convertToGK);

                        ShowModalMessage(Properties.Resources.PlayersRotated);
                    }
                }
                else
                {
                    ShowModalMessage(Properties.Resources.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void ImproveAllPlayersBy1()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.ImproveAllPlayersBy1();
                ShowModalMessage(Properties.Resources.AllPlayerDataBoosted);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        bool inFastTimer = false;
        public void OnFastTimer()
        {
            if (inFastTimer) return;
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.FastUpdate(AutoTrain, ConvertToGK, AutoResetStatus, MaxEnergy, MaxForm, MaxMorale, MaxPower, NoAlternativeTraining,AlwaysTrainConsistency
                    );                
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                ReportWarning(ex);
            }
            inFastTimer = false;
        }
        bool inSlowTimer = false;
        public void OnSlowTimer()
        {
            if (inSlowTimer) return;
            inSlowTimer = true;

            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                menusProcess.SlowUpdate(ContractAutoRenew, MaxPower);
            }
            catch (Exception ex)
            {
                ReportWarning(ex);
            }
            inSlowTimer = false;
        }
        public void ResetDate(uint targetYear)
        {
            MenusProcess menusProcess = GetMenusProcess();
            if (!menusProcess.HasExited())
            {
                var result = MessageBox.Show(Properties.Resources.GameDateChangeWarning, Properties.Resources.Warning, MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    try
                    {

                        menusProcess.ResetDate(targetYear);
                        ShowModalMessage(Properties.Resources.GameDateReset);
                    }
                    catch (Exception ex)
                    {
                        ReportError(ex);
                    }
                }
            }
            else
            {
                ShowModalMessage(Properties.Resources.CannotFindGameProcess);
            }
        }
        public void AutoPosition()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {

                    if (AutoPositionWithFormation && this.SavedFormation.IsValid())
                    {
                        menusProcess.AutoPosition(this.SavedFormation);
                    }
                    else
                        menusProcess.AutoPosition(null);
                    ShowModalMessage(Properties.Resources.PositionAutoReset);
                }
                else
                {
                    ShowModalMessage(Properties.Resources.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void SaveCurrentFormation()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    Formation newFormation = new Formation();
                    menusProcess.GetCurrentFormation(newFormation);
                    if (newFormation.IsValid())
                    {
                        this.SavedFormation = newFormation;
                        StringBuilder message = new StringBuilder();
                        message.Append(Properties.Resources.FormationSaved);
                        message.AppendFormat(":{0}", newFormation.GetFormationName());
                        MessageBox.Show(message.ToString());
                    }
                    else
                        ShowModalMessage(Properties.Resources.InvalidFormationForSaving);
                }
                else
                {
                    ShowModalMessage(Properties.Resources.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void ForceExit()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    menusProcess.Kill();
                }
                this.MenusProcess = null;
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void UpdateNewSpawn(string respawnCategory)
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    var newNames=menusProcess.UpdatePlayerNames(respawnCategory);
                    if (newNames != null)
                    {
                        StringBuilder stringBuilder=new StringBuilder();
                        foreach (var name in newNames)
                        {
                            if(stringBuilder.Length!=0)
                                stringBuilder.Append(", ");
                            stringBuilder.Append(name.HumanName.First);
                            stringBuilder.Append(" ");
                            stringBuilder.Append(name.HumanName.Last);
                        }

                        ShowModalMessage(string.Format("{0}: {1}", Properties.Resources.PlayerNamesUpdated, stringBuilder));
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void LandPurchase()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    menusProcess.PurchaseAllLand();
                    ShowModalMessage(Properties.Resources.LandPurchased);
                }
                else
                {
                    ShowModalMessage(Properties.Resources.CannotFindGameProcess);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void Restart()
        {
            try
            {
                MenusProcess menusProcess = GetMenusProcess();
                if (!menusProcess.HasExited())
                {
                    menusProcess.Restart();
                }
                this.MenusProcess = null;
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }

        }
        public void ChangeLanguage(string lang,Form mainForm)
        {
            ComponentResourceManager resources = new ComponentResourceManager(mainForm.GetType());
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture=CultureInfo.GetCultureInfo(lang);
            Program.ChangeLanguage(resources, Thread.CurrentThread.CurrentUICulture, lang, mainForm);
        }
        public void EvaluateYoungPlayersReportProgress(int progress)
        {
            this.EvalProgress = progress;
            OnEvalProgressChanged?.Invoke(this, EventArgs.Empty);
        }
        public void EvaluateYoungPlayersReportTotalPlayerPositions(int total)
        {
            this.TotalPlayerPositionsToEval= total;
        }

        public void EvaluateYoungPlayers(PlayerPosition playerPosition,string playerLastName,int minRating)
        {
           
            try
            {
                EvalProgress = 0;
                TotalPlayerPositionsToEval = 0;
                MenusProcess menusProcess = GetMenusProcess();
                EvalYoungPlayersResult =menusProcess.EvaluateYoungPlayers(playerPosition,MaxEvalAge, AutoResetStatus, MaxEnergy, MaxPower, NoAlternativeTraining
                    , EvaluateYoungPlayersReportProgress, EvaluateYoungPlayersReportTotalPlayerPositions,playerLastName, minRating, AlwaysTrainConsistency,DebugTraining);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
        public void EvaluateYoungPlayer(PlayerPosition playerPosition, PlayerModelDouble player)
        {
            try
            {
                EvalProgress = 0;
                TotalPlayerPositionsToEval = 0;
                MenusProcess menusProcess = GetMenusProcess();
                EvalYoungPlayersResult = menusProcess.EvaluateYoungPlayers(playerPosition, MaxEvalAge, AutoResetStatus, MaxEnergy, MaxPower, NoAlternativeTraining
                    , EvaluateYoungPlayersReportProgress, EvaluateYoungPlayersReportTotalPlayerPositions, AlwaysTrainConsistency,player, DebugTraining);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
    }
}
