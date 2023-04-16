using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    public class Formation
    {
        public int[] PlayersInEachPosition { get; set; } = new int[(int)(PlayerPosition.SS)+1];

        public string GetFormationName()
        {
            if (PlayersInEachPosition[0] == 1)
            {
                if(PlayersInEachPosition[(int)PlayerPosition.LB]==1
                    && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
                    && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
                    && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                    return "442";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
                    && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
                    && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.SS] == 1
                    && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                    return "4411";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
                  && PlayersInEachPosition[(int)PlayerPosition.DM] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.FOR] ==2)
                    return "41212";
                if (PlayersInEachPosition[(int)PlayerPosition.LWB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.RWB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
                  && PlayersInEachPosition[(int)PlayerPosition.SW] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.DM] == 3
                  && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                    return "532";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
                && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
                && PlayersInEachPosition[(int)PlayerPosition.CD] == 2                
                && PlayersInEachPosition[(int)PlayerPosition.DM] == 3
                && PlayersInEachPosition[(int)PlayerPosition.LW] == 1
                && PlayersInEachPosition[(int)PlayerPosition.RW] == 1
                && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                    return "433";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
              && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
              && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
              && PlayersInEachPosition[(int)PlayerPosition.DM] == 3
              && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
              && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                    return "4312";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
            && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
            && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
            && PlayersInEachPosition[(int)PlayerPosition.DM] ==1
            && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
            && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
            && PlayersInEachPosition[(int)PlayerPosition.FOR] == 3)
                    return "4123";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
            && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
            && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
            && PlayersInEachPosition[(int)PlayerPosition.DM] == 3
            && PlayersInEachPosition[(int)PlayerPosition.AM] == 2
            && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                    return "4321";
                if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
          && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
          && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
          && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
          && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
          && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
          && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
          && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                    return "4231";
                if(PlayersInEachPosition[(int)PlayerPosition.LB] == 1
          && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
          && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
          && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
          && PlayersInEachPosition[(int)PlayerPosition.AM] == 2
          && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                    return "4222";
            }
            if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
          && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
          && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
          && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
          && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
          && PlayersInEachPosition[(int)PlayerPosition.FOR] == 3)
                return "4213";
            if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
         && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
         && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
         && PlayersInEachPosition[(int)PlayerPosition.DM] == 3
         && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
         && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
         && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                return "451";
            if (PlayersInEachPosition[(int)PlayerPosition.LB] == 1
         && PlayersInEachPosition[(int)PlayerPosition.RB] == 1
         && PlayersInEachPosition[(int)PlayerPosition.CD] == 2
         && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
         && PlayersInEachPosition[(int)PlayerPosition.FR] == 4)
                return "460";
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 3
       && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
       && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
       && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
        && PlayersInEachPosition[(int)PlayerPosition.LW] == 1
       && PlayersInEachPosition[(int)PlayerPosition.RW] == 1
       && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                return "343"; 
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 3
                  && PlayersInEachPosition[(int)PlayerPosition.LWB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.RWB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
                  && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                return "352"; 
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 3
                  && PlayersInEachPosition[(int)PlayerPosition.DM] == 4
                  && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                return "3412"; 
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 3
                && PlayersInEachPosition[(int)PlayerPosition.LWB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.RWB] == 1
                  && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
                  && PlayersInEachPosition[(int)PlayerPosition.AM] == 2
                  && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                return "361";
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 3
                && PlayersInEachPosition[(int)PlayerPosition.DM] == 3
                && PlayersInEachPosition[(int)PlayerPosition.AM] == 1
                && PlayersInEachPosition[(int)PlayerPosition.LW] == 1
                && PlayersInEachPosition[(int)PlayerPosition.RW] == 1
                && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                return "3313";
            if (PlayersInEachPosition[(int)PlayerPosition.SW] == 1
              && PlayersInEachPosition[(int)PlayerPosition.LWB] == 1
              && PlayersInEachPosition[(int)PlayerPosition.RWB] == 1
              && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
              && PlayersInEachPosition[(int)PlayerPosition.LW] == 1
              && PlayersInEachPosition[(int)PlayerPosition.RW] == 1
              && PlayersInEachPosition[(int)PlayerPosition.SS] == 1
              && PlayersInEachPosition[(int)PlayerPosition.FOR] == 2)
                return "1432";
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 3
             && PlayersInEachPosition[(int)PlayerPosition.LWB] == 1
             && PlayersInEachPosition[(int)PlayerPosition.RWB] == 1
             && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
             && PlayersInEachPosition[(int)PlayerPosition.LM] == 1
             && PlayersInEachPosition[(int)PlayerPosition.RM] == 1
             && PlayersInEachPosition[(int)PlayerPosition.FOR] == 1)
                return "541";
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 1
            && PlayersInEachPosition[(int)PlayerPosition.DM] == 2
            && PlayersInEachPosition[(int)PlayerPosition.LW] == 2
            && PlayersInEachPosition[(int)PlayerPosition.RW] == 2
            && PlayersInEachPosition[(int)PlayerPosition.FOR] == 3)
                return "127";
            if (PlayersInEachPosition[(int)PlayerPosition.CD] == 2
          && PlayersInEachPosition[(int)PlayerPosition.DM] ==3
          && PlayersInEachPosition[(int)PlayerPosition.LW] == 1
          && PlayersInEachPosition[(int)PlayerPosition.RW] == 1
          && PlayersInEachPosition[(int)PlayerPosition.FOR] == 3)
                return "235";
            return GenericFormationName();
        }

        private string GenericFormationName()
        {
            StringBuilder sb = new StringBuilder();
            bool firstPosition = true;
            for(int i=0;i<PlayersInEachPosition.Length;i++)
            {
                for (int j = 0; j < PlayersInEachPosition[i]; j++)
                {
                    if (firstPosition)
                    {
                        sb.Append("GK");
                        firstPosition = false;
                    }
                    else
                    {
                        sb.Append(", ");
                        sb.Append(Enum.GetName(typeof(PlayerPosition), i));
                    }
                }
            }
            return sb.ToString();
        }

        public bool IsValid()
        {
            int totalPlayers = 0;
            for (int i = 0; i < PlayersInEachPosition.Length; i++)
            {
                totalPlayers += PlayersInEachPosition[i];
            }
            return totalPlayers ==11 && PlayersInEachPosition[(int)PlayerPosition.GK]==1;
        }
    }
}
