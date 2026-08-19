using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

enum Attribute
{
    Speed,
    Agility,
    Acceleration,
    Fitness,
    Shooting,
    Passing,
    Heading,
    BallControl,
    Dribbling,
    Marking,
    Tackling,
    Coolness,
    Awareness,
    Flair,
    Kicking,
    Throwing,
    Handling,
    Leadership,
    Consistency,
    Determination
}

sealed class TrainingActivity
{
    public string Name { get; }
    public double[] Effects { get; }

    public TrainingActivity(
        string name,
        Dictionary<Attribute, double> effects)
    {
        Name = name;
        Effects = new double[AttributeCount];

        foreach (var pair in effects)
            Effects[(int)pair.Key] = pair.Value;
    }

    public const int AttributeCount = 20;
}

sealed class WeeklyState
{
    public double[] Attributes { get; }
    public string[] Schedule { get; }

    public WeeklyState(
        double[] attributes,
        string[] schedule)
    {
        Attributes = attributes;
        Schedule = schedule;
    }
}

sealed class AStarState
{
    public double[] Attributes { get; }
    public int Weeks { get; }
    public int Heuristic { get; }
    public List<string[]> History { get; }

    public AStarState(
        double[] attributes,
        int weeks,
        int heuristic,
        List<string[]> history)
    {
        Attributes = attributes;
        Weeks = weeks;
        Heuristic = heuristic;
        History = history;
    }

    public int F => Weeks + Heuristic;
}

class Program
{
    private const double MAX = 99.0*200;
    private const double EPS = 1e-9;

    /*
     * Number of attributes.
     */
    private const int ATTRIBUTE_COUNT = 20;

    /*
     * State-key precision.
     *
     * The training effects supplied by the problem are integers,
     * while the initial player values are doubles.
     *
     * Six decimal places is enough to distinguish practical states.
     */
    private const int KEY_DECIMAL_PLACES = 6;

    /*
     * Fitness target used for the secondary objective.
     *
     * Fitness is NOT allowed to increase the minimum number of weeks.
     * It is only used to choose between equally fast solutions.
     */
    private const double FITNESS_TARGET = 99.0;

    static readonly string[] AttributeNames =
    {
        "Speed",
        "Agility",
        "Acceleration",
        "Fitness",
        "Shooting",
        "Passing",
        "Heading",
        "Ball Control",
        "Dribbling",
        "Marking",
        "Tackling",
        "Coolness",
        "Awareness",
        "Flair",
        "Kicking",
        "Throwing",
        "Handling",
        "Leadership",
        "Consistency",
        "Determination"
    };

    static readonly Dictionary<string, Attribute> AttributeMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Speed"] = Attribute.Speed,
            ["Agility"] = Attribute.Agility,
            ["Acceleration"] = Attribute.Acceleration,
            ["Fitness"] = Attribute.Fitness,
            ["Shooting"] = Attribute.Shooting,
            ["Passing"] = Attribute.Passing,
            ["Heading"] = Attribute.Heading,
            ["Ball Control"] = Attribute.BallControl,
            ["BallControl"] = Attribute.BallControl,
            ["Dribbling"] = Attribute.Dribbling,
            ["Marking"] = Attribute.Marking,
            ["Tackling"] = Attribute.Tackling,
            ["Coolness"] = Attribute.Coolness,
            ["Awareness"] = Attribute.Awareness,
            ["Flair"] = Attribute.Flair,
            ["Kicking"] = Attribute.Kicking,
            ["Throwing"] = Attribute.Throwing,
            ["Handling"] = Attribute.Handling,
            ["Leadership"] = Attribute.Leadership,
            ["Consistency"] = Attribute.Consistency,
            ["Determination"] = Attribute.Determination
        };

    static readonly Dictionary<string, Attribute[]> PositionRequirements =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["GK"] = new[]
            {
                Attribute.Speed,
                Attribute.Agility,
                Attribute.Passing,
                Attribute.BallControl,
                Attribute.Coolness,
                Attribute.Awareness,
                Attribute.Kicking,
                Attribute.Throwing,
                Attribute.Handling,
                Attribute.Consistency
            },

            ["LB"] = new[]
            {
                Attribute.Speed,
                Attribute.Passing,
                Attribute.Heading,
                Attribute.Marking,
                Attribute.Tackling,
                Attribute.Coolness,
                Attribute.Awareness,
                Attribute.Consistency,
                Attribute.Determination
            },

            ["CD"] = new[]
            {
                Attribute.Speed,
                Attribute.Passing,
                Attribute.Heading,
                Attribute.Marking,
                Attribute.Tackling,
                Attribute.Coolness,
                Attribute.Awareness,
                Attribute.Leadership,
                Attribute.Consistency
            },

            ["LWB"] = new[]
            {
                Attribute.Speed,
                Attribute.Agility,
                Attribute.Acceleration,
                Attribute.Passing,
                Attribute.Dribbling,
                Attribute.Marking,
                Attribute.Tackling,
                Attribute.Awareness,
                Attribute.Flair
            },

            ["SW"] = new[]
            {
                Attribute.Speed,
                Attribute.Acceleration,
                Attribute.Passing,
                Attribute.Heading,
                Attribute.Dribbling,
                Attribute.Marking,
                Attribute.Tackling,
                Attribute.Awareness
            },

            ["DM"] = new[]
            {
                Attribute.Speed,
                Attribute.Passing,
                Attribute.Heading,
                Attribute.Marking,
                Attribute.Tackling,
                Attribute.Awareness
            },

            ["AM"] = new[]
            {
                Attribute.Speed,
                Attribute.Acceleration,
                Attribute.Shooting,
                Attribute.Passing,
                Attribute.BallControl,
                Attribute.Dribbling,
                Attribute.Tackling,
                Attribute.Awareness,
                Attribute.Flair
            },

            ["LW"] = new[]
            {
                Attribute.Speed,
                Attribute.Agility,
                Attribute.Acceleration,
                Attribute.Shooting,
                Attribute.Passing,
                Attribute.BallControl,
                Attribute.Dribbling,
                Attribute.Tackling,
                Attribute.Awareness,
                Attribute.Flair
            },

            ["FR"] = new[]
            {
                Attribute.Speed,
                Attribute.Agility,
                Attribute.Acceleration,
                Attribute.Shooting,
                Attribute.Passing,
                Attribute.Heading,
                Attribute.BallControl,
                Attribute.Dribbling,
                Attribute.Awareness,
                Attribute.Flair
            },

            ["FOR"] = new[]
            {
                Attribute.Speed,
                Attribute.Agility,
                Attribute.Acceleration,
                Attribute.Shooting,
                Attribute.Passing,
                Attribute.Heading,
                Attribute.BallControl,
                Attribute.Dribbling,
                Attribute.Coolness,
                Attribute.Awareness,
                Attribute.Flair
            }
        };

    static readonly List<TrainingActivity> Activities =
        new()
        {
            new TrainingActivity(
                "Sprinting",
                new()
                {
                    [Attribute.Speed] = 8,
                    [Attribute.Agility] = 4,
                    [Attribute.Acceleration] = 8,
                    [Attribute.Fitness] = 6,
                    [Attribute.Shooting] = -1,
                    [Attribute.Passing] = -1,
                    [Attribute.Heading] = -1,
                    [Attribute.BallControl] = -1,
                    [Attribute.Dribbling] = -1,
                    [Attribute.Marking] = -1,
                    [Attribute.Tackling] = -1
                }),

            new TrainingActivity(
                "Weightlifting",
                new()
                {
                    [Attribute.Speed] = -2,
                    [Attribute.Fitness] = 4,
                    [Attribute.Shooting] = -1,
                    [Attribute.Passing] = -1,
                    [Attribute.Heading] = -1,
                    [Attribute.BallControl] = -1,
                    [Attribute.Dribbling] = -1,
                    [Attribute.Determination] = 8
                }),

            new TrainingActivity(
                "Heading",
                new()
                {
                    [Attribute.Acceleration] = -2,
                    [Attribute.Heading] = 8
                }),

            new TrainingActivity(
                "Control",
                new()
                {
                    [Attribute.BallControl] = 8,
                    [Attribute.Dribbling] = 6,
                    [Attribute.Coolness] = 8,
                    [Attribute.Awareness] = 2,
                    [Attribute.Flair] = 4,
                    [Attribute.Consistency] = 6
                }),

            new TrainingActivity(
                "Marking",
                new()
                {
                    [Attribute.Shooting] = -1,
                    [Attribute.Passing] = -1,
                    [Attribute.Heading] = -1,
                    [Attribute.BallControl] = -1,
                    [Attribute.Dribbling] = -1,
                    [Attribute.Marking] = 8,
                    [Attribute.Tackling] = 4,
                    [Attribute.Awareness] = 4
                }),

            new TrainingActivity(
                "Tackling",
                new()
                {
                    [Attribute.Shooting] = -1,
                    [Attribute.Passing] = -1,
                    [Attribute.Heading] = -1,
                    [Attribute.BallControl] = -1,
                    [Attribute.Dribbling] = -1,
                    [Attribute.Marking] = 4,
                    [Attribute.Tackling] = 8,
                    [Attribute.Awareness] = 4
                }),

            new TrainingActivity(
                "Goalkeeping",
                new()
                {
                    [Attribute.Agility] = 8,
                    [Attribute.Awareness] = 6,
                    [Attribute.Kicking] = -2,
                    [Attribute.Handling] = 4
                }),

            new TrainingActivity(
                "Handling",
                new()
                {
                    [Attribute.Kicking] = -2,
                    [Attribute.Throwing] = 4,
                    [Attribute.Handling] = 8
                }),

            new TrainingActivity(
                "Throwing",
                new()
                {
                    [Attribute.Throwing] = 8
                }),

            new TrainingActivity(
                "Kicking",
                new()
                {
                    [Attribute.Kicking] = 8,
                    [Attribute.Throwing] = -2
                }),

            new TrainingActivity(
                "Zonal",
                new()
                {
                    [Attribute.Awareness] = 8,
                    [Attribute.Fitness] = -1
                }),

            new TrainingActivity(
                "Futsal",
                new()
                {
                    [Attribute.Agility] = 4,
                    [Attribute.Fitness] = 6,
                    [Attribute.Shooting] = 4,
                    [Attribute.Passing] = 8,
                    [Attribute.Heading] = 2,
                    [Attribute.BallControl] = 8,
                    [Attribute.Dribbling] = 2,
                    [Attribute.Marking] = 2,
                    [Attribute.Tackling] = 6,
                    [Attribute.Awareness] = 4,
                    [Attribute.Flair] = 8
                }),

            new TrainingActivity(
                "Training Match",
                new()
                {
                    [Attribute.Agility] = 4,
                    [Attribute.Fitness] = 4,
                    [Attribute.Shooting] = 8,
                    [Attribute.Passing] = 8,
                    [Attribute.Heading] = 4,
                    [Attribute.BallControl] = 4,
                    [Attribute.Dribbling] = 2,
                    [Attribute.Marking] = 4,
                    [Attribute.Tackling] = 4,
                    [Attribute.Coolness] = 4,
                    [Attribute.Awareness] = 6,
                    [Attribute.Flair] = 6
                })
        };

    // ------------------------------------------------------------
    // MAIN
    // ------------------------------------------------------------

    static void Main(string[] args)
    {

        string inputFile = "player.csv";
        string position = "FOR";

        string outputFile="training_plan.csv";

        if (!PositionRequirements.TryGetValue(
                position,
                out Attribute[] requirements))
        {
            Console.WriteLine(
                $"Unknown position '{position}'.");

            Console.WriteLine(
                "Valid positions:");

            Console.WriteLine(
                string.Join(
                    ", ",
                    PositionRequirements.Keys));

            return;
        }

        double[] player = ReadPlayer(inputFile);

        if (player == null)
            return;

        /*
         * Leadership cannot be trained for CD.
         *
         * It therefore cannot be a completion requirement.
         */
        if (position == "CD")
        {
            requirements =
                requirements
                    .Where(
                        a => a != Attribute.Leadership)
                    .ToArray();
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Position: {position}");

        Console.WriteLine(
            "Required attributes:");

        foreach (Attribute a in requirements)
        {
            Console.WriteLine(
                $"  {AttributeNames[(int)a]} = " +
                $"{player[(int)a].ToString(
                    "F4",
                    CultureInfo.InvariantCulture)}");
        }

        Console.WriteLine();

        if (IsFinished(player, requirements))
        {
            Console.WriteLine(
                "Player is already at the maximum possible rating.");

            WritePlan(
                outputFile,
                new List<string[]>(),
                player,
                requirements);

            return;
        }

        Console.WriteLine(
            "Starting exact A* search...");
        Console.WriteLine(
            "Generating weekly Pareto-optimal transitions as needed.");
        Console.WriteLine();

        AStarResult result =
            Solve(
                player,
                requirements);

        if (result == null)
        {
            Console.WriteLine(
                "No solution exists with the supplied training activities.");

            return;
        }

        WritePlan(
            outputFile,
            result.Schedule,
            result.FinalAttributes,
            requirements);

        Console.WriteLine();
        Console.WriteLine(
            "========================================");
        Console.WriteLine(
            "OPTIMAL TRAINING PLAN");
        Console.WriteLine(
            "========================================");

        Console.WriteLine(
            $"Minimum weeks: {result.Weeks}");

        Console.WriteLine(
            $"Final Fitness: " +
            $"{result.FinalAttributes[(int)Attribute.Fitness]:F4}");

        Console.WriteLine(
            $"Output file: {outputFile}");

        Console.WriteLine();
        Console.WriteLine(
            "Weekly schedule:");

        for (int w = 0;
             w < result.Schedule.Count;
             w++)
        {
            Console.WriteLine(
                $"Week {w + 1}: " +
                string.Join(
                    " | ",
                    result.Schedule[w]));
        }

        Console.WriteLine();
        Console.WriteLine(
            "Final required attributes:");

        foreach (Attribute a in requirements)
        {
            Console.WriteLine(
                $"  {AttributeNames[(int)a]} = " +
                $"{result.FinalAttributes[(int)a]:F6}");
        }
    }

    // ------------------------------------------------------------
    // A*
    // ------------------------------------------------------------

    sealed class AStarResult
    {
        public int Weeks { get; }
        public double[] FinalAttributes { get; }
        public List<string[]> Schedule { get; }

        public AStarResult(
            int weeks,
            double[] finalAttributes,
            List<string[]> schedule)
        {
            Weeks = weeks;
            FinalAttributes = finalAttributes;
            Schedule = schedule;
        }
    }

    static AStarResult Solve(
        double[] initial,
        Attribute[] requirements)
    {
        int initialH =
            LowerBoundWeeks(
                initial,
                requirements);

        if (initialH == int.MaxValue)
            return null;

        /*
         * Priority:
         *
         * 1. f = g + h
         * 2. Fitness deficit
         * 3. Required-attribute deficit
         *
         * The first value preserves A* optimality.
         * The remaining values only decide which equal-f state
         * is expanded first.
         */
        var open =
            new PriorityQueue<
                AStarState,
                (int F, double FitnessDeficit, double RequiredDeficit)>();

        var startHistory =
            new List<string[]>();

        var start =
            new AStarState(
                (double[])initial.Clone(),
                0,
                initialH,
                startHistory);

        open.Enqueue(
            start,
            (
                start.F,
                FitnessDeficit(start.Attributes),
                RequiredDeficit(
                    start.Attributes,
                    requirements)
            ));

        /*
         * For every exact state, remember the smallest number of weeks
         * at which we have reached it.
         */
        var bestG =
            new Dictionary<string, int>();

        bestG[StateKey(initial)] = 0;

        long expanded = 0;

        while (open.Count > 0)
        {
            AStarState current =
                open.Dequeue();

            string currentKey =
                StateKey(current.Attributes);

            if (bestG.TryGetValue(
                    currentKey,
                    out int knownG) &&
                current.Weeks > knownG)
            {
                continue;
            }

            expanded++;

            if (expanded % 100 == 0)
            {
                Console.WriteLine(
                    $"Expanded: {expanded:N0}  " +
                    $"Weeks: {current.Weeks}  " +
                    $"Open: {open.Count:N0}  " +
                    $"h: {current.Heuristic}");
            }

            /*
             * A* property:
             *
             * Because h is admissible and the priority queue is ordered
             * by f=g+h, the first goal dequeued is optimal.
             */
            if (IsFinished(
                    current.Attributes,
                    requirements))
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"A* expanded {expanded:N0} states.");

                return new AStarResult(
                    current.Weeks,
                    current.Attributes,
                    current.History);
            }

            /*
             * Generate all nondominated possible states after one
             * complete 7-day training cycle.
             */
            List<WeeklyState> successors =
                GenerateWeeklyTransitions(
                    current.Attributes,
                    requirements);

            foreach (WeeklyState successor in successors)
            {
                int newG =
                    current.Weeks + 1;

                int h =
                    LowerBoundWeeks(
                        successor.Attributes,
                        requirements);

                if (h == int.MaxValue)
                    continue;

                string key =
                    StateKey(
                        successor.Attributes);

                if (bestG.TryGetValue(
                        key,
                        out int previousG) &&
                    previousG <= newG)
                {
                    continue;
                }

                bestG[key] = newG;

                var newHistory =
                    new List<string[]>(
                        current.History)
                    {
                        successor.Schedule
                    };

                var next =
                    new AStarState(
                        successor.Attributes,
                        newG,
                        h,
                        newHistory);

                open.Enqueue(
                    next,
                    (
                        next.F,
                        FitnessDeficit(
                            next.Attributes),
                        RequiredDeficit(
                            next.Attributes,
                            requirements)
                    ));
            }
        }

        return null;
    }

    // ------------------------------------------------------------
    // ADMISSIBLE A* HEURISTIC
    // ------------------------------------------------------------

    /*
     * This is deliberately optimistic.
     *
     * For every required attribute we pretend that:
     *
     *   - we can train that attribute with its best activity
     *     every day;
     *   - there are no negative side effects;
     *   - there is no cap waste;
     *   - all other attributes can be ignored.
     *
     * Therefore this can NEVER require more weeks than the real game.
     *
     * h = maximum individual attribute lower bound.
     */
    static int LowerBoundWeeks(
        double[] attributes,
        Attribute[] requirements)
    {
        int lowerBound = 0;

        foreach (Attribute attribute in requirements)
        {
            double deficit =
                Math.Max(
                    0.0,
                    MAX - attributes[(int)attribute]);

            if (deficit <= EPS)
                continue;

            double maxDailyGain =
                Activities
                    .Select(
                        a => a.Effects[(int)attribute])
                    .Where(
                        x => x > EPS)
                    .DefaultIfEmpty(0.0)
                    .Max();

            /*
             * The attribute can never reach 99.
             */
            if (maxDailyGain <= EPS)
                return int.MaxValue;

            double days =
                Math.Ceiling(
                    deficit /
                    maxDailyGain -
                    EPS);

            int weeks =
                (int)Math.Ceiling(
                    days / 7.0);

            lowerBound =
                Math.Max(
                    lowerBound,
                    weeks);
        }

        return lowerBound;
    }

    // ------------------------------------------------------------
    // WEEKLY TRANSITION GENERATION
    // ------------------------------------------------------------

    /*
     * Generate all states reachable after exactly seven days.
     *
     * We don't simply enumerate 13^7 schedules.
     *
     * After every day, dominated states are removed.
     *
     * State A dominates State B if:
     *
     *   A >= B in every required attribute
     *   A.Fitness >= B.Fitness
     *
     * Since all future training effects are additive followed by
     * clamping at [0,99], A can never be worse than B for the
     * objective regardless of what happens later.
     *
     * Therefore B can safely be discarded without losing the
     * globally optimal solution.
     */
    static List<WeeklyState> GenerateWeeklyTransitions(
        double[] starting,
        Attribute[] requirements)
    {
        var states =
            new List<WeeklyState>
            {
                new WeeklyState(
                    (double[])starting.Clone(),
                    Array.Empty<string>())
            };

        for (int day = 0; day < 7; day++)
        {
            var next =
                new List<WeeklyState>(
                    states.Count *
                    Activities.Count);

            foreach (WeeklyState state in states)
            {
                foreach (TrainingActivity activity in Activities)
                {
                    double[] attributes =
                        Apply(
                            state.Attributes,
                            activity);

                    string[] schedule =
                        new string[day + 1];

                    if (day > 0)
                    {
                        Array.Copy(
                            state.Schedule,
                            schedule,
                            day);
                    }

                    schedule[day] =
                        activity.Name;

                    next.Add(
                        new WeeklyState(
                            attributes,
                            schedule));
                }
            }

            /*
             * Exact state deduplication.
             *
             * Different daily schedules can produce exactly the same
             * player attributes. Only one needs to survive.
             */
            next =
                DeduplicateStates(next);

            /*
             * Remove states that are provably dominated.
             */
            next =
                ParetoPrune(
                    next,
                    requirements);

            Console.WriteLine(
                $"    Day {day + 1}: " +
                $"{next.Count:N0} nondominated states");

            states = next;

            /*
             * If every required attribute is already 99, the remaining
             * days still have to occur because the game requires exactly
             * seven days before recalculation.
             *
             * We therefore continue generating the week.
             */
        }

        return states;
    }

    // ------------------------------------------------------------
    // STATE DEDUPLICATION
    // ------------------------------------------------------------

    static List<WeeklyState> DeduplicateStates(
        List<WeeklyState> states)
    {
        var dictionary =
            new Dictionary<string, WeeklyState>();

        foreach (WeeklyState state in states)
        {
            string key =
                StateKey(state.Attributes);

            if (!dictionary.TryGetValue(
                    key,
                    out WeeklyState existing))
            {
                dictionary[key] = state;
                continue;
            }

            /*
             * Same resulting attributes.
             *
             * Keep the schedule with better Fitness preference.
             *
             * Since the attributes are identical, this is really just
             * deterministic tie-breaking.
             */
            if (state.Schedule.Length > 0 &&
                existing.Schedule.Length > 0)
            {
                /*
                 * No meaningful difference exists for future optimization.
                 * Keep the lexicographically smaller schedule simply to
                 * make output deterministic.
                 */
                if (string.CompareOrdinal(
                        string.Join("|", state.Schedule),
                        string.Join("|", existing.Schedule)) < 0)
                {
                    dictionary[key] = state;
                }
            }
        }

        return dictionary.Values.ToList();
    }

    // ------------------------------------------------------------
    // PARETO PRUNING
    // ------------------------------------------------------------

    static List<WeeklyState> ParetoPrune(
        List<WeeklyState> states,
        Attribute[] requirements)
    {
        if (states.Count <= 1)
            return states;

        /*
         * The dimensions relevant to future optimization are:
         *
         *   required attributes
         *   Fitness
         *
         * Everything else can be ignored for dominance purposes because
         * none of those attributes contributes to the requested position
         * and none is used as an input to another attribute.
         */
        var dimensions =
            new List<int>();

        foreach (Attribute a in requirements)
            dimensions.Add((int)a);

        if (!dimensions.Contains((int)Attribute.Fitness))
            dimensions.Add((int)Attribute.Fitness);

        /*
         * Sort by the sum of relevant attributes descending.
         *
         * This is only an optimization for the pairwise dominance test;
         * it does NOT change which states are removed.
         */
        states =
            states
                .OrderByDescending(
                    s => DominanceScore(
                        s.Attributes,
                        dimensions))
                .ToList();

        var survivors =
            new List<WeeklyState>();

        foreach (WeeklyState candidate in states)
        {
            bool dominated = false;

            foreach (WeeklyState survivor in survivors)
            {
                if (Dominates(
                        survivor.Attributes,
                        candidate.Attributes,
                        dimensions))
                {
                    dominated = true;
                    break;
                }
            }

            if (!dominated)
                survivors.Add(candidate);
        }

        return survivors;
    }

    static double DominanceScore(
        double[] attributes,
        List<int> dimensions)
    {
        double score = 0;

        foreach (int i in dimensions)
            score += attributes[i];

        return score;
    }

    static bool Dominates(
        double[] a,
        double[] b,
        List<int> dimensions)
    {
        bool strictlyBetter = false;

        foreach (int i in dimensions)
        {
            if (a[i] < b[i] - EPS)
                return false;

            if (a[i] > b[i] + EPS)
                strictlyBetter = true;
        }

        return strictlyBetter;
    }

    // ------------------------------------------------------------
    // ATTRIBUTE SIMULATION
    // ------------------------------------------------------------

    static double[] Apply(
        double[] attributes,
        TrainingActivity activity)
    {
        double[] result =
            (double[])attributes.Clone();

        for (int i = 0;
             i < ATTRIBUTE_COUNT;
             i++)
        {
            double effect =
                activity.Effects[i];

            if (Math.Abs(effect) <= EPS)
                continue;

            result[i] =
                Math.Clamp(
                    result[i] + effect,
                    0.0,
                    MAX);
        }

        return result;
    }

    // ------------------------------------------------------------
    // GOAL / METRICS
    // ------------------------------------------------------------

    static bool IsFinished(
        double[] attributes,
        Attribute[] requirements)
    {
        foreach (Attribute a in requirements)
        {
            if (attributes[(int)a] <
                MAX - EPS)
            {
                return false;
            }
        }

        return true;
    }

    static double RequiredDeficit(
        double[] attributes,
        Attribute[] requirements)
    {
        double result = 0;

        foreach (Attribute a in requirements)
        {
            result +=
                Math.Max(
                    0,
                    MAX - attributes[(int)a]);
        }

        return result;
    }

    static double FitnessDeficit(
        double[] attributes)
    {
        return Math.Max(
            0,
            FITNESS_TARGET -
            attributes[(int)Attribute.Fitness]);
    }

    // ------------------------------------------------------------
    // STATE KEY
    // ------------------------------------------------------------

    static string StateKey(
        double[] attributes)
    {
        return string.Join(
            "|",
            attributes.Select(
                x => Math.Round(
                        x,
                        KEY_DECIMAL_PLACES)
                    .ToString(
                        $"F{KEY_DECIMAL_PLACES}",
                        CultureInfo.InvariantCulture)));
    }

    // ------------------------------------------------------------
    // CSV INPUT
    // ------------------------------------------------------------

    static double[] ReadPlayer(
        string file)
    {
        if (!File.Exists(file))
        {
            Console.WriteLine(
                $"File not found: {file}");

            return null;
        }

        double[] result =
            new double[ATTRIBUTE_COUNT];

        try
        {
            string[] lines =
                File.ReadAllLines(file);

            if (lines.Length == 0)
            {
                Console.WriteLine(
                    "The CSV file is empty.");

                return null;
            }

            foreach (string line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts =
                    line.Split(',');

                if (parts.Length < 2)
                    continue;

                string name =
                    parts[0].Trim();

                if (!AttributeMap.TryGetValue(
                        name,
                        out Attribute attribute))
                {
                    Console.WriteLine(
                        $"Warning: unknown attribute '{name}'.");

                    continue;
                }

                if (!double.TryParse(
                        parts[1].Trim(),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out double value))
                {
                    Console.WriteLine(
                        $"Invalid value for {name}: " +
                        $"'{parts[1]}'");

                    return null;
                }

                result[(int)attribute] =
                    Math.Clamp(
                        value,
                        0.0,
                        MAX)*200;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Error reading CSV: {ex.Message}");

            return null;
        }

        return result;
    }

    // ------------------------------------------------------------
    // CSV OUTPUT
    // ------------------------------------------------------------

    static void WritePlan(
        string file,
        List<string[]> schedule,
        double[] finalAttributes,
        Attribute[] requirements)
    {
        using var writer =
            new StreamWriter(file);

        writer.WriteLine(
            "Week,Day,Activity");

        for (int week = 0;
             week < schedule.Count;
             week++)
        {
            string[] days =
                schedule[week];

            for (int day = 0;
                 day < days.Length;
                 day++)
            {
                writer.WriteLine(
                    $"{week + 1}," +
                    $"{day + 1}," +
                    $"{EscapeCsv(days[day])}");
            }
        }

        writer.WriteLine();

        writer.WriteLine(
            "Final Attributes");

        writer.WriteLine(
            "Attribute,Value,Required");

        for (int i = 0;
             i < ATTRIBUTE_COUNT;
             i++)
        {
            Attribute attribute =
                (Attribute)i;

            bool required =
                requirements.Contains(attribute);

            writer.WriteLine(
                $"{EscapeCsv(AttributeNames[i])}," +
                $"{finalAttributes[i].ToString(
                    "F6",
                    CultureInfo.InvariantCulture)}," +
                $"{required}");
        }
    }

    static string EscapeCsv(
        string value)
    {
        if (value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n'))
        {
            return "\"" +
                   value.Replace(
                       "\"",
                       "\"\"") +
                   "\"";
        }

        return value;
    }
}