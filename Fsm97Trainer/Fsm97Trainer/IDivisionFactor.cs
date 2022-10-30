namespace Fsm97Trainer
{
    public interface IDivisionFactor
    {
        float card_occur_factor { get; set; }
        float energy_loss_factor { get; set; }
        float foul_occur_factor { get; set; }
        float goal_factor { get; set; }
        float injury_occur_factor { get; set; }
        float pass_scale_factor { get; set; }
        float pass_succ_factor { get; set; }
        float perf_factor { get; set; }
        float shoot_factor { get; set; }
        float sot_factor { get; set; }
        float tackle_scale_factor { get; set; }
        float tackle_succ_factor { get; set; }
    }
}