using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;
using System.Text;

namespace FSM97Lib
{
    public static class PositionRatings
    {
        static byte[][] ratings;
        public static byte[][] Ratings
        { get { return ratings; } }
        static PositionRatings()
        {
            ratings = new byte[(int)PlayerPosition.Count][];
            var gkRatings = new byte[(int)PlayerAttribute.Count];
            gkRatings[(int)PlayerAttribute.Speed] = 2;
            gkRatings[(int)PlayerAttribute.Agility] = 25;
            gkRatings[(int)PlayerAttribute.Passing] = 2;
            gkRatings[(int)PlayerAttribute.Control] = 4;
            gkRatings[(int)PlayerAttribute.Coolness] = 7;
            gkRatings[(int)PlayerAttribute.Awareness] = 10;
            gkRatings[(int)PlayerAttribute.Kicking] = 8;
            gkRatings[(int)PlayerAttribute.Throwing] = 6;
            gkRatings[(int)PlayerAttribute.Handling] = 30;
            gkRatings[(int)PlayerAttribute.Consistency] = 6;
            ratings[(int)PlayerPosition.GK] = gkRatings;
            var lrbRatings = new byte[(int)PlayerAttribute.Count];
            lrbRatings[(int)PlayerAttribute.Speed] = 3;
            lrbRatings[(int)PlayerAttribute.Passing] = 7;
            lrbRatings[(int)PlayerAttribute.Heading] = 8;
            lrbRatings[(int)PlayerAttribute.TackleDetermination] = 10;
            lrbRatings[(int)PlayerAttribute.TackleSkill] = 44;
            lrbRatings[(int)PlayerAttribute.Coolness] = 7;
            lrbRatings[(int)PlayerAttribute.Awareness] = 15;
            lrbRatings[(int)PlayerAttribute.Consistency] = 4;
            lrbRatings[(int)PlayerAttribute.Determination] = 2;
            ratings[(int)PlayerPosition.LB] = lrbRatings;
            ratings[(int)PlayerPosition.RB] = lrbRatings;
            var cdRatings = new byte[(int)PlayerAttribute.Count];
            cdRatings[(int)PlayerAttribute.Speed] = 3;
            cdRatings[(int)PlayerAttribute.Passing] = 3;
            cdRatings[(int)PlayerAttribute.Heading] = 14;
            cdRatings[(int)PlayerAttribute.TackleDetermination] = 10;
            cdRatings[(int)PlayerAttribute.TackleSkill] = 50;
            cdRatings[(int)PlayerAttribute.Coolness] = 7;
            cdRatings[(int)PlayerAttribute.Awareness] = 8;
            cdRatings[(int)PlayerAttribute.Consistency] = 2;
            cdRatings[(int)PlayerAttribute.Leadership] = 3;
            ratings[(int)PlayerPosition.CD] = cdRatings;
            var lrwbRatings = new byte[(int)PlayerAttribute.Count];
            lrwbRatings[(int)PlayerAttribute.Speed] = 7;
            lrwbRatings[(int)PlayerAttribute.Agility] = 4;
            lrwbRatings[(int)PlayerAttribute.Acceleration] = 11;
            lrwbRatings[(int)PlayerAttribute.Passing] = 12;
            lrwbRatings[(int)PlayerAttribute.Dribbling] = 26;
            lrwbRatings[(int)PlayerAttribute.TackleDetermination] = 3;
            lrwbRatings[(int)PlayerAttribute.TackleSkill] = 26;
            lrwbRatings[(int)PlayerAttribute.Flair] = 5;
            lrwbRatings[(int)PlayerAttribute.Awareness] = 6;
            ratings[(int)PlayerPosition.LWB] = lrwbRatings;
            ratings[(int)PlayerPosition.RWB] = lrwbRatings;
            var swRatings = new byte[(int)PlayerAttribute.Count];
            swRatings[(int)PlayerAttribute.Speed] = 12;
            swRatings[(int)PlayerAttribute.Acceleration] = 6;
            swRatings[(int)PlayerAttribute.Passing] = 15;
            swRatings[(int)PlayerAttribute.Heading] = 3;
            swRatings[(int)PlayerAttribute.Dribbling] = 15;
            swRatings[(int)PlayerAttribute.TackleDetermination] = 3;
            swRatings[(int)PlayerAttribute.TackleSkill] = 26;
            swRatings[(int)PlayerAttribute.Awareness] = 20;
            ratings[(int)PlayerPosition.SW] = swRatings;
            var dmRatings = new byte[(int)PlayerAttribute.Count];
            dmRatings[(int)PlayerAttribute.Speed] = 5;
            dmRatings[(int)PlayerAttribute.Passing] = 40;
            dmRatings[(int)PlayerAttribute.Heading] = 5;
            dmRatings[(int)PlayerAttribute.TackleDetermination] = 3;
            dmRatings[(int)PlayerAttribute.TackleSkill] = 27;
            dmRatings[(int)PlayerAttribute.Awareness] = 20;
            ratings[(int)PlayerPosition.DM] = dmRatings;
            var lrmRatings = new byte[(int)PlayerAttribute.Count];
            lrmRatings[(int)PlayerAttribute.Speed] = 10;
            lrmRatings[(int)PlayerAttribute.Acceleration] = 5;
            lrmRatings[(int)PlayerAttribute.Shooting] = 3;
            lrmRatings[(int)PlayerAttribute.Passing] = 42;
            lrmRatings[(int)PlayerAttribute.Control] = 5;
            lrmRatings[(int)PlayerAttribute.Dribbling] = 5;
            lrmRatings[(int)PlayerAttribute.TackleSkill] = 20;
            lrmRatings[(int)PlayerAttribute.Awareness] = 5;
            lrmRatings[(int)PlayerAttribute.Flair] = 5;
            ratings[(int)PlayerPosition.LM] = lrmRatings;
            ratings[(int)PlayerPosition.RM] = lrmRatings;
            var amRatings = new byte[(int)PlayerAttribute.Count];
            amRatings[(int)PlayerAttribute.Speed] = 10;
            amRatings[(int)PlayerAttribute.Acceleration] = 5;
            amRatings[(int)PlayerAttribute.Shooting] = 5;
            amRatings[(int)PlayerAttribute.Passing] = 46;
            amRatings[(int)PlayerAttribute.Control] = 5;
            amRatings[(int)PlayerAttribute.Dribbling] = 5;
            amRatings[(int)PlayerAttribute.TackleSkill] = 14;
            amRatings[(int)PlayerAttribute.Awareness] = 5;
            amRatings[(int)PlayerAttribute.Flair] = 5;
            ratings[(int)PlayerPosition.AM] = amRatings;
            var lrwRatings = new byte[(int)PlayerAttribute.Count];
            lrwRatings[(int)PlayerAttribute.Speed] = 10;
            lrwRatings[(int)PlayerAttribute.Agility] = 3;
            lrwRatings[(int)PlayerAttribute.Acceleration] = 10;
            lrwRatings[(int)PlayerAttribute.Shooting] = 3;
            lrwRatings[(int)PlayerAttribute.Passing] = 31;
            lrwRatings[(int)PlayerAttribute.Control] = 3;
            lrwRatings[(int)PlayerAttribute.Dribbling] = 27;
            lrwRatings[(int)PlayerAttribute.TackleSkill] = 3;
            lrwRatings[(int)PlayerAttribute.Awareness] = 3;
            lrwRatings[(int)PlayerAttribute.Flair] = 7;
            ratings[(int)PlayerPosition.LW] = lrwRatings;
            ratings[(int)PlayerPosition.RW] = lrwRatings;
            var frRatings = new byte[(int)PlayerAttribute.Count];
            frRatings[(int)PlayerAttribute.Speed] = 12;
            frRatings[(int)PlayerAttribute.Agility] = 2;
            frRatings[(int)PlayerAttribute.Acceleration] = 8;
            frRatings[(int)PlayerAttribute.Shooting] = 4;
            frRatings[(int)PlayerAttribute.Passing] = 14;
            frRatings[(int)PlayerAttribute.Heading] = 1;
            frRatings[(int)PlayerAttribute.Control] = 10;
            frRatings[(int)PlayerAttribute.Dribbling] = 7;
            frRatings[(int)PlayerAttribute.Awareness] = 12;
            frRatings[(int)PlayerAttribute.Flair] = 10;
            ratings[(int)PlayerPosition.FR] = frRatings;
            var forRatings = new byte[(int)PlayerAttribute.Count];
            forRatings[(int)PlayerAttribute.Speed] = 10;
            forRatings[(int)PlayerAttribute.Agility] = 2;
            forRatings[(int)PlayerAttribute.Acceleration] = 9;
            forRatings[(int)PlayerAttribute.Shooting] = 36;
            forRatings[(int)PlayerAttribute.Passing] = 4;
            forRatings[(int)PlayerAttribute.Heading] = 10;
            forRatings[(int)PlayerAttribute.Control] = 10;
            forRatings[(int)PlayerAttribute.Dribbling] = 3;
            forRatings[(int)PlayerAttribute.Coolness] = 3;
            forRatings[(int)PlayerAttribute.Awareness] = 4;
            forRatings[(int)PlayerAttribute.Flair] = 9;
            ratings[(int)PlayerPosition.FOR] = forRatings;
            var ssRatings = new byte[(int)PlayerAttribute.Count];
            ssRatings[(int)PlayerAttribute.Speed] = 6;
            ssRatings[(int)PlayerAttribute.Agility] = 2;
            ssRatings[(int)PlayerAttribute.Acceleration] = 6;
            ssRatings[(int)PlayerAttribute.Shooting] = 29;
            ssRatings[(int)PlayerAttribute.Passing] = 16;
            ssRatings[(int)PlayerAttribute.Heading] = 7;
            ssRatings[(int)PlayerAttribute.Control] = 13;
            ssRatings[(int)PlayerAttribute.Dribbling] = 6;
            ssRatings[(int)PlayerAttribute.Coolness] = 2;
            ssRatings[(int)PlayerAttribute.Awareness] = 3;
            ssRatings[(int)PlayerAttribute.Flair] = 10;
            ratings[(int)PlayerPosition.SS] = ssRatings;
        }
        public static double GetPositionRatingDouble(int playerPosition, byte[] attributes)
        {
            double sum = 0;
            for (int i = 0; i < (int)PlayerAttribute.Count; i++)
            {
                sum += ratings[playerPosition][i] * attributes[i];
            }
            return sum / 100;
        }
            }
}
