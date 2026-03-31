using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public interface IPlayerTrainableAttributes<T> where T:struct
    {
        T[] Attributes { get;}

        #region bytes 1-3 speed
        T Speed { get;set; }
        T Agility { get; set; }
        T Acceleration { get; set; }
        #endregion 
        #region bytes 4-6 power
        T Stamina { get; set; }
        T Strength { get; set; }
        T Fitness { get; set; }
        #endregion 
        #region bytes 7-11 skills
        T Shooting { get; set; }
        T Passing { get; set; }
        T Heading { get; set; }
        T Control { get; set; }
        T Dribbling { get; set; }
        #endregion
        #region bytes 12-16 mental and taclking
        T TackleDetermination { get; set; }
        T TackleSkill { get; set; }
        T Coolness { get; set; }
        T Awareness { get; set; }
        T Flair { get; set; }
        #endregion
        #region bytes 17-19 goalkeeping

        T Kicking { get; set; }
        T Throwing { get; set; }
        T Handling { get; set; }

        #endregion

        #region bytes 20-24 other
        T ThrowIn { get; set; }
        T Leadership { get; set; }
        T Consistency { get; set; }
        T Determination { get; set; }
        T Greed { get; set; }
        #endregion

    }
}
