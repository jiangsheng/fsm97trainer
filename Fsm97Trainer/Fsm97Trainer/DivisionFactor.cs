using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Fsm97Trainer
{
    public class DivisionFactor : IDivisionFactor
    {
        public DivisionFactor()
        {

        }
        public DivisionFactor(DivisionFactorStruct divisionFactorStruct) : this()
        {
            Program.CopyProperties<IDivisionFactor>(divisionFactorStruct, this);
        }

        [DisplayName("对手强度(Adversaries strength)")]
        public float perf_factor { get; set; }
        [DisplayName("传球成功系数(passing success scale)")]
        [Description("默认1.2,亦即传球能力75即可达到传球成功率上限 (default 1.2, reaches max passing success at 75 passing skill)")]
        public float pass_succ_factor { get; set; }
        [DisplayName("铲球成功系数(tackle success scale)")]
        public float tackle_succ_factor { get; set; }
        [DisplayName("犯规系数(faul occurence scale)")]
        public float foul_occur_factor { get; set; }
        [DisplayName("吃牌系数(card occurence scale)")]
        public float card_occur_factor { get; set; }
        [DisplayName("受伤系数(injury occurence scale)")]
        public float injury_occur_factor { get; set; }
        [DisplayName("能量流失系数(in-game energy loss scale)")]
        [Description("在位置特定的能量流失系数之外的额外附加值(on top of player position specific influence)")]
        public float energy_loss_factor { get; set; }
        [DisplayName("起脚率系数(shoot chance scale)")]
        public float shoot_factor { get; set; }
        [DisplayName("射正率系数(shoot on target scale)")]
        public float sot_factor { get; set; }
        [DisplayName("进球系数(goal scale)")]
        public float goal_factor { get; set; }

        [DisplayName("传球率系数(pass scale)")]
        public float pass_scale_factor { get; set; }
        [DisplayName("铲球率系数(tackle scale)")]
        public float tackle_scale_factor { get; set; }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DivisionFactorStruct : IDivisionFactor
    {
        public float perf_factor { get; set; }
        public float pass_succ_factor { get; set; }
        public float tackle_succ_factor { get; set; }
        public float foul_occur_factor { get; set; }
        public float card_occur_factor { get; set; }
        public float injury_occur_factor { get; set; }
        public float energy_loss_factor { get; set; }
        public float shoot_factor { get; set; }
        public float sot_factor { get; set; }
        public float goal_factor { get; set; }
        public float pass_scale_factor { get; set; }
        public float tackle_scale_factor { get; set; }
    }
}
